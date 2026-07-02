using Godot;
using Hope.Config;

namespace Hope.DropSystem;

/// <summary>
/// D4 物品等级（Item Power / ilvl）计算。
/// 掉落 ilvl ≈ 怪物等级，带小幅随机波动。
/// </summary>
public static class ItemLevelCalculator
{
    /// <summary>
    /// 根据怪物等级掷出最终物品等级。
    /// </summary>
    public static int Roll(DropContext ctx)
    {
        var baseLevel = System.Math.Max(System.Math.Max(ctx.MonsterLevel, ctx.AreaLevel), 1);
        var variance = (int)ParamsConfig.DropItemLevelVariance;
        var offset = GD.RandRange(-variance, variance);
        return Mathf.Max(baseLevel + offset, 1);
    }
}
