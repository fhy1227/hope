using Godot;

namespace Hope.Core;

/// <summary>
/// 单把武器的配置数据。
/// </summary>
public class WeaponData
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public WeaponType Type { get; set; }
    public MeleeStyle MeleeStyle { get; set; } = MeleeStyle.Swing;
    public float DamageScale { get; set; } = 1f;
    public float AttackSpeedScale { get; set; } = 1f;
    public float Range { get; set; } = 300f;
    public float ProjectileSpeed { get; set; } = 450f;
    public PackedScene? ProjectileScene { get; set; }
    public Color VisualColor { get; set; } = Colors.White;

    public static WeaponData CreatePistol(PackedScene? projectileScene) => new()
    {
        Id = "pistol",
        DisplayName = "手枪",
        Type = WeaponType.Ranged,
        DamageScale = 1f,
        AttackSpeedScale = 1.2f,
        Range = 340f,
        ProjectileSpeed = 480f,
        ProjectileScene = projectileScene,
        VisualColor = new Color(1f, 0.85f, 0.3f),
    };

    public static WeaponData CreateSword() => new()
    {
        Id = "sword",
        DisplayName = "短剑",
        Type = WeaponType.Melee,
        MeleeStyle = MeleeStyle.Swing,
        DamageScale = 1.4f,
        AttackSpeedScale = 0.85f,
        Range = 58f,
        VisualColor = new Color(0.85f, 0.9f, 1f),
    };

    public static WeaponData CreateSpear() => new()
    {
        Id = "spear",
        DisplayName = "长矛",
        Type = WeaponType.Melee,
        MeleeStyle = MeleeStyle.Thrust,
        DamageScale = 1.2f,
        AttackSpeedScale = 1f,
        Range = 72f,
        VisualColor = new Color(0.75f, 0.8f, 0.95f),
    };
}
