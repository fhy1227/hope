using System.Collections.Generic;
using Godot;
using Hope.Config;
using Hope.Core;
using Hope.DropSystem;

namespace Hope.Systems;

/// <summary>
/// 根据 drop_table 配置结算敌人掉落。
/// item_id=0 走 D4 装备生成；item_id&gt;0 为固定物品。
/// </summary>
public static class DropTableResolver
{
    public readonly struct DropResult
    {
        public int ItemId { get; init; }
        public int Count { get; init; }
        public ItemInstance? Instance { get; init; }
    }

    /// <summary>
    /// 按 enemy_type 匹配掉落规则并掷骰，返回应掉落的物品列表。
    /// </summary>
    public static List<DropResult> RollDrops(string enemyType, int wave = 1)
    {
        var results = new List<DropResult>();

        foreach (var entry in ConfigManager.GetAll<DropTableConfig>())
        {
            if (entry.EnemyType != "*" && entry.EnemyType != enemyType)
                continue;

            if (entry.ItemId == 0)
            {
                foreach (var instance in EquipDropResolver.RollEquipment(enemyType, wave))
                {
                    results.Add(new DropResult
                    {
                        ItemId = instance.ConfigId,
                        Count = 1,
                        Instance = instance,
                    });
                }
                continue;
            }

            if (GD.Randf() >= entry.DropRate)
                continue;

            var count = (int)GD.RandRange(entry.MinCount, entry.MaxCount);
            if (count <= 0)
                continue;

            results.Add(new DropResult { ItemId = entry.ItemId, Count = count });
        }

        return results;
    }
}
