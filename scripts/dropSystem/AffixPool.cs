using System;
using System.Collections.Generic;
using Godot;
using Hope.Core;

namespace Hope.DropSystem;

/// <summary>
/// 词条模板池与数值缩放（D4：词条数值随物品等级 ilvl 缩放）。
/// </summary>
public static class AffixPool
{
    private readonly struct AffixTemplate
    {
        public string Id { get; init; }
        public NumericType NumericType { get; init; }
        public ModifierType ModifierType { get; init; }
        public float MinPerLevel { get; init; }
        public float MaxPerLevel { get; init; }
    }

    private static readonly AffixTemplate[] Templates =
    [
        new() { Id = "affix.vitality", NumericType = NumericType.MaxHealth, ModifierType = ModifierType.Constant, MinPerLevel = 2f, MaxPerLevel = 5f },
        new() { Id = "affix.might", NumericType = NumericType.Damage, ModifierType = ModifierType.Percentage, MinPerLevel = 0.01f, MaxPerLevel = 0.03f },
        new() { Id = "affix.swiftness", NumericType = NumericType.MoveSpeed, ModifierType = ModifierType.Percentage, MinPerLevel = 0.005f, MaxPerLevel = 0.015f },
        new() { Id = "affix.precision", NumericType = NumericType.Crit, ModifierType = ModifierType.Percentage, MinPerLevel = 0.01f, MaxPerLevel = 0.04f },
        new() { Id = "affix.fortitude", NumericType = NumericType.Armor, ModifierType = ModifierType.Constant, MinPerLevel = 1f, MaxPerLevel = 3f },
    ];

    /// <summary> D4 各稀有度随机词条数量 </summary>
    public static int GetAffixCount(int rarity) => rarity switch
    {
        2 => 1,  // Magic
        3 => 4,  // Rare
        4 => 2,  // Legendary（另加底材传奇加成）
        _ => 0,
    };

    /// <summary>
    /// 为装备掷骰词条，同件装备不重复 stat 类型。
    /// </summary>
    public static List<RolledAffix> RollAffixes(int rarity, int itemLevel, int count)
    {
        var result = new List<RolledAffix>(count);
        if (count <= 0)
        {
            return result;
        }

        var usedStats = new HashSet<NumericType>();
        var candidates = new List<AffixTemplate>(Templates);

        for (var i = 0; i < count && candidates.Count > 0; i++)
        {
            var idx = GD.RandRange(0, candidates.Count - 1);
            var template = candidates[idx];
            candidates.RemoveAt(idx);

            if (!usedStats.Add(template.NumericType))
            {
                i--;
                continue;
            }

            result.Add(RollSingle(template, itemLevel));
        }

        return result;
    }

    private static RolledAffix RollSingle(AffixTemplate template, int itemLevel)
    {
        var t = GD.Randf();
        var perLevel = Mathf.Lerp(template.MinPerLevel, template.MaxPerLevel, t);
        var value = perLevel * Math.Max(itemLevel, 1);

        if (template.ModifierType == ModifierType.Percentage)
        {
            value = (float)Math.Round(value, 3);
        }

        return new RolledAffix
        {
            AffixId = template.Id,
            NumericType = template.NumericType,
            ModifierType = template.ModifierType,
            Value = value,
        };
    }
}
