using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Hope.Config;
using Hope.Core;

namespace Hope.DropSystem;

/// <summary>
/// 词条模板池与数值缩放（配置驱动，按装备槽 slot_mask 过滤）。
/// </summary>
public static class AffixPool
{
    /// <summary>从品质配置读取随机词缀数量。</summary>
    public static int GetAffixCount(int rarity)
    {
        var quality = ConfigManager.Get<QualityConfig>(rarity);
        return quality?.AffixCount ?? 0;
    }

    /// <summary>
    /// 为装备掷骰词条，同件装备不重复 stat 类型；按 slotType 过滤词缀池。
    /// </summary>
    public static List<RolledAffix> RollAffixes(int slotType, int itemLevel, int count)
    {
        var result = new List<RolledAffix>(count);
        if (count <= 0)
        {
            return result;
        }

        var usedStats = new HashSet<NumericType>();
        var candidates = ConfigManager.GetAll<AffixConfig>()
            .Where(a => a.SlotMask != null && a.SlotMask.Contains(slotType))
            .ToList();

        for (var i = 0; i < count && candidates.Count > 0; i++)
        {
            var idx = GD.RandRange(0, candidates.Count - 1);
            var template = candidates[idx];
            candidates.RemoveAt(idx);

            var numericType = (NumericType)template.NumericType;
            if (!usedStats.Add(numericType))
            {
                i--;
                continue;
            }

            result.Add(RollSingle(template, itemLevel));
        }

        return result;
    }

    private static RolledAffix RollSingle(AffixConfig template, int itemLevel)
    {
        var t = GD.Randf();
        var perLevel = Mathf.Lerp(template.MinPerLevel, template.MaxPerLevel, t);
        var value = perLevel * Math.Max(itemLevel, 1);
        var modifierType = (ModifierType)template.ModifierType;

        if (modifierType == ModifierType.Percentage)
        {
            value = (float)Math.Round(value, 3);
        }

        return new RolledAffix
        {
            AffixId = template.AffixKey,
            NumericType = (NumericType)template.NumericType,
            ModifierType = modifierType,
            Value = value,
        };
    }
}
