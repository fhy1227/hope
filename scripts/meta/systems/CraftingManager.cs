using System;
using Godot;
using Hope.Config;
using Hope.Core;
using Hope.DropSystem;
using Hope.Persistence;

namespace Hope.Systems;

/// <summary>
/// 局外锻造服务：铁匠附魔等（meta 层，修改持久化物品与金币）。
/// </summary>
public static class CraftingManager
{
    /// <summary>每件装备最大附魔次数。</summary>
    public const int MaxEnchantPerItem = 1;

    /// <summary>附魔基础金币费用。</summary>
    public const int EnchantBaseCost = 500;

    /// <summary>附魔每物品等级额外费用。</summary>
    public const int EnchantCostPerItemLevel = 50;

    /// <summary>可附魔的最低稀有度（3=稀有）。</summary>
    public const int MinEnchantRarity = 3;

    public enum EnchantResult
    {
        Success,
        ItemNotFound,
        NotEnchantable,
        MaxEnchantReached,
        NoAffixes,
        InsufficientGold,
    }

    /// <summary>计算附魔费用。</summary>
    public static int GetEnchantCost(ItemInstance item)
        => EnchantBaseCost + item.ItemLevel * EnchantCostPerItemLevel;

    /// <summary>
    /// 消耗金币，随机替换装备一条随机词缀。
    /// </summary>
    public static EnchantResult TryEnchant(string itemUid, out string message)
    {
        message = string.Empty;
        var item = FindItem(itemUid);
        if (item == null)
        {
            message = "未找到该装备。";
            return EnchantResult.ItemNotFound;
        }

        if (!item.IsEquip || item.EffectiveRarity < MinEnchantRarity)
        {
            message = "仅稀有及以上装备可附魔。";
            return EnchantResult.NotEnchantable;
        }

        if (item.EnchantCount >= MaxEnchantPerItem)
        {
            message = "该装备附魔次数已用尽。";
            return EnchantResult.MaxEnchantReached;
        }

        if (item.Affixes.Count == 0)
        {
            message = "该装备没有可替换的词缀。";
            return EnchantResult.NoAffixes;
        }

        var cost = GetEnchantCost(item);
        var save = PersistenceMgr.Instance?.ActiveCharacter;
        if (save == null || save.Gold < cost)
        {
            message = $"金币不足（需要 {cost} G）。";
            return EnchantResult.InsufficientGold;
        }

        var slotType = item.Config?.SlotType ?? 0;
        var replaceIndex = GD.RandRange(0, item.Affixes.Count - 1);
        var rolled = AffixPool.RollAffixes(slotType, item.ItemLevel, 1);
        if (rolled.Count == 0)
        {
            message = "词缀池为空，无法附魔。";
            return EnchantResult.NotEnchantable;
        }

        item.Affixes[replaceIndex] = rolled[0];
        item.EnchantCount += 1;
        save.Gold -= cost;

        EquipManager.Instance?.RecalculateBonus();
        EquipManager.Instance?.EmitSignal(EquipManager.SignalName.EquipmentChanged);
        InventoryManager.Instance?.EmitSignal(InventoryManager.SignalName.InventoryChanged);
        PersistenceMgr.Instance?.MarkDirty();

        message = $"附魔成功，消耗 {cost} G。";
        return EnchantResult.Success;
    }

    /// <summary>枚举背包与已穿戴的可附魔装备。</summary>
    public static System.Collections.Generic.List<ItemInstance> GetEnchantableItems()
    {
        var result = new System.Collections.Generic.List<ItemInstance>();
        var seen = new System.Collections.Generic.HashSet<string>();

        if (InventoryManager.Instance != null)
        {
            foreach (var item in InventoryManager.Instance.Items)
            {
                if (item.IsEquip && item.EffectiveRarity >= MinEnchantRarity && seen.Add(item.Uid))
                {
                    result.Add(item);
                }
            }
        }

        if (EquipManager.Instance != null)
        {
            foreach (var item in EquipManager.Instance.GetAllEquipped())
            {
                if (item.EffectiveRarity >= MinEnchantRarity && seen.Add(item.Uid))
                {
                    result.Add(item);
                }
            }
        }

        return result;
    }

    private static ItemInstance? FindItem(string uid)
    {
        if (InventoryManager.Instance != null)
        {
            foreach (var item in InventoryManager.Instance.Items)
            {
                if (item.Uid == uid)
                {
                    return item;
                }
            }
        }

        if (EquipManager.Instance != null)
        {
            foreach (var item in EquipManager.Instance.GetAllEquipped())
            {
                if (item.Uid == uid)
                {
                    return item;
                }
            }
        }

        return null;
    }
}
