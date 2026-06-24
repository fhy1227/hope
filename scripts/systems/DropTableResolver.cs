using System.Collections.Generic;
using System.Linq;
using Godot;
using Hope.Config;

namespace Hope.Systems;

/// <summary>
/// 根据 drop_table 配置结算敌人掉落。
/// </summary>
public static class DropTableResolver
{
    public readonly struct DropResult
    {
        public int ItemId { get; init; }
        public int Count { get; init; }
    }

    /// <summary>
    /// 按 enemy_type 匹配掉落规则并掷骰，返回应掉落的物品列表。
    /// </summary>
    public static List<DropResult> RollDrops(string enemyType)
    {
        var results = new List<DropResult>();

        foreach (var entry in ConfigManager.GetAll<DropTableConfig>())
        {
            if (entry.EnemyType != "*" && entry.EnemyType != enemyType)
                continue;

            if (GD.Randf() >= entry.DropRate)
                continue;

            var itemId = entry.ItemId > 0 ? entry.ItemId : PickRandomItem(entry.Rarity);
            if (itemId <= 0)
                continue;

            var count = (int)GD.RandRange(entry.MinCount, entry.MaxCount);
            if (count <= 0)
                continue;

            results.Add(new DropResult { ItemId = itemId, Count = count });
        }

        return results;
    }

    private static int PickRandomItem(int rarity)
    {
        var pool = ConfigManager.GetAll<ItemConfig>()
            .Where(item => rarity <= 0 || item.Rarity == rarity)
            .ToList();

        if (pool.Count == 0)
            return 0;

        return pool[GD.RandRange(0, pool.Count - 1)].Id;
    }
}
