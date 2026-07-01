using System;
using Hope.Config;

namespace Hope.Core;

/// <summary>
/// 波间商店可选项：展示文案与对 <see cref="RunStats"/> 的修改逻辑。
/// 由商店 UI 从 <see cref="Pool"/> 中随机抽取若干项供玩家选择。
/// </summary>
public readonly struct ShopUpgrade
{
    /// <summary>内部标识，用于去重或存档。</summary>
    public string Id { get; init; }

    /// <summary>UI 显示文本（如「生命 +3」）。</summary>
    public string Label { get; init; }

    /// <summary>购买后立即对 RunStats 执行的修改委托。</summary>
    public Action<RunStats> Apply { get; init; }

    /// <summary>全部可 roll 的升级池；商店每次从中随机子集。</summary>
    public static ShopUpgrade[] Pool { get; } =
    [
        new()
        {
            Id = "hp",
            Label = $"生命 +{(int)ParamsConfig.ShopHpBonus}",
            Apply = stats => stats.MaxHealth += (int)ParamsConfig.ShopHpBonus,
        },
        new()
        {
            Id = "damage",
            Label = $"伤害 +{(int)ParamsConfig.ShopDamageBonus}",
            Apply = stats => stats.Damage += ParamsConfig.ShopDamageBonus,
        },
        new()
        {
            Id = "attack_speed",
            Label = $"攻速 +{(int)((ParamsConfig.ShopAttackSpeedMul - 1f) * 100)}%",
            Apply = stats => stats.AttackSpeed *= ParamsConfig.ShopAttackSpeedMul,
        },
        new()
        {
            Id = "speed",
            Label = $"移速 +{(int)ParamsConfig.ShopSpeedBonus}",
            Apply = stats => stats.Speed += ParamsConfig.ShopSpeedBonus,
        },
        new()
        {
            Id = "range",
            Label = $"射程 +{(int)ParamsConfig.ShopWeaponRangeBonus}",
            Apply = stats => stats.WeaponRange += ParamsConfig.ShopWeaponRangeBonus,
        },
    ];
}
