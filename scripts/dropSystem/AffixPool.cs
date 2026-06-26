using System;
using System.Collections.Generic;
using Godot;
using Hope.Systems;

namespace Hope.DropSystem;

/// <summary>
/// 词条模板池与数值缩放（D4：词条数值随物品等级 ilvl 缩放）。
/// </summary>
public static class AffixPool
{
    private readonly struct AffixTemplate
    {
        public string Id { get; init; }
        public AffixStat Stat { get; init; }
        public float MinPerLevel { get; init; }
        public float MaxPerLevel { get; init; }
        public bool IsPercent { get; init; }
    }

    private static readonly AffixTemplate[] Templates =
    [
        new() { Id = "affix.vitality", Stat = AffixStat.Hp, MinPerLevel = 2f, MaxPerLevel = 5f },
        new() { Id = "affix.might", Stat = AffixStat.Damage, MinPerLevel = 0.01f, MaxPerLevel = 0.03f, IsPercent = true },
        new() { Id = "affix.swiftness", Stat = AffixStat.Speed, MinPerLevel = 0.005f, MaxPerLevel = 0.015f, IsPercent = true },
        new() { Id = "affix.precision", Stat = AffixStat.Crit, MinPerLevel = 0.01f, MaxPerLevel = 0.04f, IsPercent = true },
        new() { Id = "affix.fortitude", Stat = AffixStat.Armor, MinPerLevel = 1f, MaxPerLevel = 3f },
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
            return result;

        var usedStats = new HashSet<AffixStat>();
        var candidates = new List<AffixTemplate>(Templates);

        for (var i = 0; i < count && candidates.Count > 0; i++)
        {
            var idx = GD.RandRange(0, candidates.Count - 1);
            var template = candidates[idx];
            candidates.RemoveAt(idx);

            if (!usedStats.Add(template.Stat))
            {
                i--;
                continue;
            }

            result.Add(RollSingle(template, itemLevel));
        }

        return result;
    }

    public static void ApplyAffix(ref EquipStatBonus bonus, RolledAffix affix)
    {
        switch (affix.Stat)
        {
            case AffixStat.Hp:
                bonus.Hp += (int)affix.Value;
                break;
            case AffixStat.Damage:
                bonus.Damage += affix.Value;
                break;
            case AffixStat.Speed:
                bonus.Speed += affix.Value;
                break;
            case AffixStat.Crit:
                bonus.Crit += affix.Value;
                break;
            case AffixStat.Armor:
                bonus.Armor += (int)affix.Value;
                break;
        }
    }

    private static RolledAffix RollSingle(AffixTemplate template, int itemLevel)
    {
        var t = GD.Randf();
        var perLevel = Mathf.Lerp(template.MinPerLevel, template.MaxPerLevel, t);
        var value = perLevel * Math.Max(itemLevel, 1);

        if (template.IsPercent)
            value = (float)Math.Round(value, 3);

        return new RolledAffix
        {
            AffixId = template.Id,
            Stat = template.Stat,
            Value = value,
        };
    }
}
