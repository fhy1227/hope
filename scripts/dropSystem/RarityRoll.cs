using System.Collections.Generic;
using System.Linq;
using Godot;
using Hope.Config;

namespace Hope.DropSystem;

/// <summary>
/// D4 稀有度掷骰：加权随机 + 魔法发现偏向 + 小概率升档。
/// </summary>
public static class RarityRoll
{
    /// <summary> 升档概率（D4 近似：低概率跳高一档） </summary>
    public const float UpgradeChance = 0.05f;

    public static int Roll(DropContext ctx, int itemLevel)
    {
        if (ctx.ForcedRarity > 0)
            return ctx.ForcedRarity;

        var pool = ConfigManager.GetAll<QualityConfig>()
            .Where(q => q.MinItemLevel <= itemLevel)
            .OrderBy(q => q.Id)
            .ToList();

        if (pool.Count == 0)
            return 1;

        var weights = pool.Select(q => ApplyMagicFind(q, ctx.MagicFind)).ToArray();
        var picked = WeightedPick(pool, weights);

        if (picked.Id < pool[^1].Id && GD.Randf() < UpgradeChance)
            return picked.Id + 1;

        return picked.Id;
    }

    /// <summary>
    /// MF 越高，高稀有度权重越大（指数偏向高 tier）。
    /// </summary>
    private static float ApplyMagicFind(QualityConfig quality, float magicFind)
    {
        var tier = quality.Id - 1;
        var mfBoost = 1f + magicFind * tier * 0.5f;
        return quality.DropWeight * mfBoost;
    }

    private static QualityConfig WeightedPick(IReadOnlyList<QualityConfig> pool, float[] weights)
    {
        var total = weights.Sum();
        if (total <= 0f)
            return pool[0];

        var roll = GD.Randf() * total;
        var acc = 0f;

        for (var i = 0; i < pool.Count; i++)
        {
            acc += weights[i];
            if (roll <= acc)
                return pool[i];
        }

        return pool[^1];
    }
}
