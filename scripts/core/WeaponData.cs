using Godot;
using Hope.Config;

namespace Hope.Core;

/// <summary>
/// 单把武器的配置数据。
/// </summary>
public class WeaponData
{
    public int ItemConfigId { get; set; }
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public WeaponType Type { get; set; }
    public MeleeStyle MeleeStyle { get; set; } = MeleeStyle.Swing;
    public float DamageScale { get; set; } = 1f;
    public float AttackSpeedScale { get; set; } = 1f;
    public float Range { get; set; } = 300f;
    public float ProjectileSpeed { get; set; } = 450f;
    public PackedScene? ProjectileScene { get; set; }
    public Color VisualColor { get; set; } = Colors.White;

    /// <summary>从 item.json 配置生成武器数据。</summary>
    public static WeaponData? FromItemConfig(int itemConfigId, PackedScene? projectileScene = null)
    {
        var config = ConfigManager.Get<ItemConfig>(itemConfigId);
        if (config == null)
        {
            GD.PushError($"WeaponData: 找不到物品配置 {itemConfigId}");
            return null;
        }

        var data = new WeaponData
        {
            ItemConfigId = itemConfigId,
            Id = itemConfigId.ToString(),
            DisplayName = config.NameKey,
            IconPath = config.Icon,
            DamageScale = config.StatDamage > 0f ? config.StatDamage : 1f,
            AttackSpeedScale = Mathf.Max(0.1f, 1f + config.StatSpeed),
        };

        switch (itemConfigId)
        {
            case 1032:
            case 1033:
                data.Type = WeaponType.Ranged;
                data.Range = 340f;
                data.ProjectileSpeed = 480f;
                data.ProjectileScene = projectileScene;
                break;
            case 1031:
                data.Type = WeaponType.Melee;
                data.MeleeStyle = MeleeStyle.Swing;
                data.Range = 65f;
                break;
            default:
                data.Type = WeaponType.Melee;
                data.MeleeStyle = MeleeStyle.Swing;
                data.Range = 58f;
                break;
        }

        return data;
    }

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
