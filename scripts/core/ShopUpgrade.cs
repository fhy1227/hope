using System;

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
            Label = "生命 +3",
            Apply = stats => stats.MaxHealth += 3,
        },
        new()
        {
            Id = "damage",
            Label = "伤害 +2",
            Apply = stats => stats.Damage += 2f,
        },
        new()
        {
            Id = "attack_speed",
            Label = "攻速 +15%",
            Apply = stats => stats.AttackSpeed *= 1.15f,
        },
        new()
        {
            Id = "speed",
            Label = "移速 +20",
            Apply = stats => stats.Speed += 20f,
        },
        new()
        {
            Id = "range",
            Label = "射程 +40",
            Apply = stats => stats.WeaponRange += 40f,
        },
    ];
}
