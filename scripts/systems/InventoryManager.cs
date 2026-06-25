using Godot;
using System;
using System.Collections.Generic;
using Hope.Config;

namespace Hope.Systems;

/// <summary>
/// 背包管理 - Autoload 单例
/// 负责：添加/移除物品、背包容量、通知 UI 更新
/// </summary>
public partial class InventoryManager : Node
{
    private static InventoryManager _instance;
    public static InventoryManager Instance => _instance;

    /// <summary> 背包最大容量（格数） </summary>
    [Export] public int MaxSlots { get; set; } = 20;

    /// <summary> 背包物品列表 </summary>
    private readonly List<Core.ItemInstance> _items = new();

    public IReadOnlyList<Core.ItemInstance> Items => _items.AsReadOnly();

    /// <summary> 背包变更信号（UI 订阅） </summary>
    [Signal] public delegate void InventoryChangedEventHandler();

    /// <summary> 获得新物品信号（用于飘字提示） </summary>
    [Signal] public delegate void ItemObtainedEventHandler(Core.ItemInstance item);

    public override void _Ready()
    {
        if (_instance != null && _instance != this)
        {
            QueueFree();
            return;
        }
        _instance = this;
        GD.Print("[InventoryManager] 初始化完成");
    }

    /// <summary>
    /// 将已有物品实例放回背包（卸下装备时使用）
    /// </summary>
    public bool AddItemInstance(Core.ItemInstance item)
    {
        if (item == null) return false;

        if (_items.Count >= MaxSlots)
        {
            GD.Print("[InventoryManager] 背包已满，无法放入物品");
            return false;
        }

        _items.Add(item);
        EmitSignal(SignalName.InventoryChanged);
        return true;
    }

    /// <summary>
    /// 添加物品到背包（自动堆叠）
    /// </summary>
    public bool AddItem(int configId, int count = 1)
    {
        var config = ConfigManager.Get<Config.ItemConfig>(configId);
        if (config == null)
        {
            GD.PrintErr($"[InventoryManager] 无效物品ID: {configId}");
            return false;
        }

        // 可堆叠物品：先找已有堆叠
        if (config.StackLimit > 0)
        {
            foreach (var existing in _items)
            {
                if (existing.ConfigId == configId && existing.Count < config.StackLimit)
                {
                    var canAdd = Math.Min(count, config.StackLimit - existing.Count);
                    existing.Count += canAdd;
                    count -= canAdd;
                    if (count <= 0)
                    {
                        EmitSignal(SignalName.InventoryChanged);
                        EmitSignal(SignalName.ItemObtained, existing);
                        return true;
                    }
                }
            }
        }

        // 剩余数量开新槽位
        var modified = false;
        while (count > 0)
        {
            if (_items.Count >= MaxSlots)
            {
                GD.Print("[InventoryManager] 背包已满！");
                if (modified)
                    EmitSignal(SignalName.InventoryChanged);
                return false;
            }

            var newItem = new Core.ItemInstance { ConfigId = configId };
            if (config.StackLimit > 0)
            {
                newItem.Count = Math.Min(count, config.StackLimit);
                count -= newItem.Count;
            }
            else
            {
                newItem.Count = 1;
                count--;
            }

            _items.Add(newItem);
            modified = true;
            EmitSignal(SignalName.ItemObtained, newItem);
        }

        if (modified)
            EmitSignal(SignalName.InventoryChanged);
        return true;
    }

    /// <summary>
    /// 移除物品（按实例UID）
    /// </summary>
    public bool RemoveItem(string uid, int count = 1)
    {
        var item = _items.Find(i => i.Uid == uid);
        if (item == null) return false;

        if (item.IsStackable && item.Count > count)
        {
            item.Count -= count;
        }
        else
        {
            _items.Remove(item);
        }

        EmitSignal(SignalName.InventoryChanged);
        return true;
    }

    /// <summary>
    /// 按配置ID移除物品
    /// </summary>
    public bool RemoveItemByConfigId(int configId, int count = 1)
    {
        for (int i = _items.Count - 1; i >= 0 && count > 0; i--)
        {
            if (_items[i].ConfigId == configId)
            {
                if (_items[i].IsStackable && _items[i].Count > count)
                {
                    _items[i].Count -= count;
                    count = 0;
                }
                else
                {
                    count -= _items[i].Count;
                    _items.RemoveAt(i);
                }
            }
        }

        EmitSignal(SignalName.InventoryChanged);
        return count <= 0;
    }

    /// <summary>
    /// 获取背包物品（按槽位索引）
    /// </summary>
    public Core.ItemInstance GetItemAt(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= _items.Count) return null;
        return _items[slotIndex];
    }

    /// <summary>
    /// 背包空格数
    /// </summary>
    public int GetFreeSlots()
    {
        // 计算可释放的槽位（非堆叠物品每个占1槽，堆叠物品看是否已满）
        int usedSlots = 0;
        foreach (var item in _items)
        {
            if (!item.IsStackable) usedSlots++;
            else
            {
                // 堆叠物品：如果还能继续堆叠，不算占满槽位
                // 简化：每个堆叠物品占1槽
                usedSlots++;
            }
        }
        return MaxSlots - usedSlots;
    }

    /// <summary>
    /// 清空背包（新对局时调用）
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        EmitSignal(SignalName.InventoryChanged);
    }
}
