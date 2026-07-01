using System.Collections.Generic;
using Godot;
using Hope.Config;
using Hope.Core;

namespace Hope.DropSystem;

/// <summary>
/// 对外掉落入口：结合 drop_table 与 D4 装备生成。
/// </summary>
public static class EquipDropResolver
{
    /// <summary>
    /// 按敌型与波次结算装备掉落（item_id=0 的规则走 D4 算法）。
    /// </summary>
    public static List<ItemInstance> RollEquipment(string enemyType, int wave)
    {
        var results = new List<ItemInstance>();

        foreach (var entry in ConfigManager.GetAll<DropTableConfig>())
        {
            if (entry.EnemyType != "*" && entry.EnemyType != enemyType)
                continue;

            if (entry.ItemId > 0)
                continue;

            var ctx = DropContext.FromWave(enemyType, wave, entry.DropRate, entry.Rarity);
            var drops = RollEntry(ctx, entry.MinCount, entry.MaxCount);

            foreach (var item in drops)
                results.Add(item);
        }

        return results;
    }

    private static List<ItemInstance> RollEntry(DropContext ctx, int minCount, int maxCount)
    {
        var results = new List<ItemInstance>();
        var count = (int)GD.RandRange(minCount, maxCount);

        for (var i = 0; i < count; i++)
        {
            var drop = i == 0 && ctx.EnemyType == ParamsConfig.EnemyTypeBoss
                ? EquipDropGenerator.Generate(ctx)
                : EquipDropGenerator.TryGenerate(ctx);

            if (drop != null)
                results.Add(drop.Value.Item);
        }

        return results;
    }
}
