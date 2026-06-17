using System;

namespace Hope.Core;

public readonly struct ShopUpgrade
{
    public string Id { get; init; }
    public string Label { get; init; }
    public Action<RunStats> Apply { get; init; }

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
