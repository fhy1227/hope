using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Hope.Components;
using Hope.Config;
using Hope.Core;
using Hope.Entities;

namespace Hope.Systems;

/// <summary>
/// 装备管理 - Autoload 单例
/// 负责：穿戴/卸下装备、计算属性加成并应用到 RunStats
/// </summary>
public partial class EquipManager : Node
{
    public static EquipManager Instance { get; private set; }

    /// <summary> 装备槽状态：slotType -> List of equipped ItemInstances </summary>
    private readonly Dictionary<int, List<Core.ItemInstance>> _equipped = new();

    /// <summary> 装备变更信号（UI 订阅） </summary>
    [Signal] public delegate void EquipmentChangedEventHandler();

    /// <summary> 当前装备带来的属性加成（缓存） </summary>
    public NumericModifierMap CurrentBonus { get; private set; } = new();

    public override void _Ready()
    {
        if (Instance != null && Instance != this)
        {
            QueueFree();
            return;
        }
        Instance = this;

        // 初始化所有装备槽
        var slots = ConfigManager.GetAll<Config.EquipSlotConfig>();
        foreach (var slot in slots)
        {
            _equipped[slot.Id] = new List<Core.ItemInstance>();
        }

        GD.Print($"[EquipManager] 初始化完成，装备槽数量: {_equipped.Count}");
    }

    /// <summary> 武器槽类型（equip_slot.json id=1） </summary>
    public const int WeaponSlotType = 1;

    // ── 公共查询 ─────────────────────────────────────────────────────

    /// <summary> 获取某槽位已装备物品列表 </summary>
    public IReadOnlyList<Core.ItemInstance> GetEquipped(int slotType)
    {
        if (_equipped.TryGetValue(slotType, out var list))
            return list.AsReadOnly();
        return System.Array.Empty<Core.ItemInstance>().ToList().AsReadOnly();
    }

    /// <summary> 获取所有已装备物品（展平） </summary>
    public List<Core.ItemInstance> GetAllEquipped()
    {
        var result = new List<Core.ItemInstance>();
        foreach (var kv in _equipped)
            result.AddRange(kv.Value);
        return result;
    }

    // ── 默认装备（不经过背包） ───────────────────────────────────────

    /// <summary>
    /// 直接设置某槽位的默认装备（新对局开局用，不从背包扣除）。
    /// </summary>
    public void SetDefaultEquipment(int slotType, IReadOnlyList<int> configIds)
    {
        if (!_equipped.ContainsKey(slotType))
        {
            return;
        }

        _equipped[slotType].Clear();
        foreach (var configId in configIds)
        {
            _equipped[slotType].Add(new Core.ItemInstance { ConfigId = configId });
        }

        RecalcBonus();
        EmitSignal(SignalName.EquipmentChanged);
    }

    // ── 穿戴 / 卸下 ─────────────────────────────────────────────────

    /// <summary>
    /// 穿戴装备（自动从背包移除；单槽位已满时自动替换旧装备）
    /// </summary>
    public bool Equip(Core.ItemInstance item)
    {
        if (!item.IsEquip)
        {
            GD.PrintErr("[EquipManager] 该物品不是装备");
            return false;
        }

        var slotType = item.Config.SlotType;
        if (!_equipped.ContainsKey(slotType))
        {
            GD.PrintErr($"[EquipManager] 未知装备槽类型: {slotType}");
            return false;
        }

        var slotConfig = ConfigManager.Get<Config.EquipSlotConfig>(slotType);
        if (slotConfig == null) return false;

        if (_equipped[slotType].Count >= slotConfig.MaxCount)
        {
            var old = _equipped[slotType][0];
            _equipped[slotType].RemoveAt(0);
            InventoryManager.Instance?.AddItemInstance(old);
            GD.Print($"[EquipManager] 替换旧装备: {old.Config.NameKey}");
        }

        _equipped[slotType].Add(item);
        InventoryManager.Instance?.RemoveItem(item.Uid);

        GD.Print($"[EquipManager] 穿戴: {item.Config.NameKey} (槽位 {slotType})");
        EmitSignal(SignalName.EquipmentChanged);
        RecalcBonus();
        return true;
    }

    /// <summary>
    /// 卸下装备（放回背包）
    /// </summary>
    public bool Unequip(string uid)
    {
        foreach (var kv in _equipped)
        {
            var item = kv.Value.Find(i => i.Uid == uid);
            if (item != null)
            {
                kv.Value.Remove(item);

                InventoryManager.Instance?.AddItemInstance(item);

                GD.Print($"[EquipManager] 卸下: {item.Config.NameKey}");
                EmitSignal(SignalName.EquipmentChanged);
                RecalcBonus();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 卸下某槽位指定索引的装备
    /// </summary>
    public bool UnequipAt(int slotType, int index)
    {
        if (!_equipped.TryGetValue(slotType, out var list) || index < 0 || index >= list.Count)
            return false;

        var item = list[index];
        list.RemoveAt(index);
        InventoryManager.Instance?.AddItemInstance(item);

        GD.Print($"[EquipManager] 卸下: {item.Config.NameKey}");
        EmitSignal(SignalName.EquipmentChanged);
        RecalcBonus();
        return true;
    }

    // ── 属性加成计算 ─────────────────────────────────────────────────

    /// <summary>
    /// 重新计算装备属性加成
    /// </summary>
    private void RecalcBonus()
    {
        var bonus = new NumericModifierMap();

        foreach (var item in GetAllEquipped())
        {
            if (item.Config == null)
            {
                continue;
            }

            bonus.MergeFrom(item.ComputeStatBonus());
        }

        CurrentBonus = bonus;
        GD.Print($"[EquipManager] 属性加成条目: {FormatBonusLog(bonus)}");

        ApplyToPlayerStats();
    }

    private static string FormatBonusLog(NumericModifierMap bonus)
    {
        if (bonus.IsEmpty)
        {
            return "无";
        }

        var parts = new List<string>();
        foreach (var (type, modType, value) in bonus.Entries)
        {
            parts.Add($"{type} {modType} {value:0.##}");
        }

        return string.Join(", ", parts);
    }

    /// <summary>
    /// 将当前加成应用到玩家 NumericComponent（经 DataModifierComponent）。
    /// </summary>
    private void ApplyToPlayerStats()
    {
        var player = Main.Instance?.Run?.Player;
        if (player == null)
        {
            GD.Print("[EquipManager] 未找到 player，暂存加成");
            return;
        }

        var stats = player.GetNodeOrNull<PlayerStatsComponent>("PlayerStatsComponent");
        if (stats == null)
        {
            GD.Print("[EquipManager] 未找到 PlayerStatsComponent，暂存加成");
            return;
        }

        stats.ApplyEquipModifiers(CurrentBonus);
    }

    // ── 对局重置 ─────────────────────────────────────────────────────

    /// <summary>
    /// 清空所有装备（新对局 / 回主菜单时调用）
    /// </summary>
    public void Clear()
    {
        foreach (var kv in _equipped)
        {
            kv.Value.Clear();
        }

        CurrentBonus = new NumericModifierMap();
        EmitSignal(SignalName.EquipmentChanged);
        GD.Print("[EquipManager] 已清空所有装备");
    }
}
