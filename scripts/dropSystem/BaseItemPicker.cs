using System.Collections.Generic;
using System.Linq;
using Godot;
using Hope.Config;

namespace Hope.DropSystem;

/// <summary>
/// D4 底材选择：按槽位过滤 + Smart Loot 加权。
/// </summary>
public static class BaseItemPicker
{
    public static ItemConfig? Pick(DropContext ctx, int itemLevel, int slotType)
    {
        var pool = ConfigManager.GetAll<ItemConfig>()
            .Where(i => i.SlotType == slotType && i.SlotType > 0 && i.LevelReq <= itemLevel)
            .ToList();

        if (pool.Count == 0)
            return null;

        if (GD.Randf() < ctx.SmartLootChance && ctx.PreferredSlotTypes?.Length > 0)
        {
            var smart = pool.Where(i => ctx.PreferredSlotTypes.Contains(i.SlotType)).ToList();
            if (smart.Count > 0)
                pool = smart;
        }

        return pool[GD.RandRange(0, pool.Count - 1)];
    }

    /// <summary>
    /// D4：先掷装备槽位，再选该槽底材。
    /// </summary>
    public static int RollSlotType(DropContext ctx)
    {
        var slots = ConfigManager.GetAll<ItemConfig>()
            .Where(i => i.SlotType > 0)
            .Select(i => i.SlotType)
            .Distinct()
            .ToList();

        if (slots.Count == 0)
            return DropContext.EquipManagerWeaponSlot;

        if (GD.Randf() < ctx.SmartLootChance && ctx.PreferredSlotTypes?.Length > 0)
        {
            var preferred = ctx.PreferredSlotTypes
                .Where(slots.Contains)
                .ToList();

            if (preferred.Count > 0)
                return preferred[GD.RandRange(0, preferred.Count - 1)];
        }

        return slots[GD.RandRange(0, slots.Count - 1)];
    }
}
