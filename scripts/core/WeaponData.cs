using Godot;
using Hope.Config;

namespace Hope.Core;

/// <summary>
/// 单把武器的运行时配置：类型、射程、攻速倍率、弹道场景与视觉色。
/// 可由 item 配置表生成（<see cref="FromItemConfig"/>），或使用内置工厂方法创建默认武器。
/// </summary>
public class WeaponData
{
    /// <summary>来源物品配置 ID；非表驱动武器可为 0。</summary>
    public int ItemConfigId { get; set; }

    /// <summary>武器逻辑 ID 字符串，用于存档或调试。</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>显示名称（或本地化 key）。</summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>UI 图标资源路径。</summary>
    public string IconPath { get; set; } = string.Empty;

    /// <summary>远程或近战，决定 WeaponSlot 攻击分支。</summary>
    public WeaponType Type { get; set; }

    /// <summary>近战子类型；远程武器忽略。</summary>
    public MeleeStyle MeleeStyle { get; set; } = MeleeStyle.Swing;

    /// <summary>相对 RunStats.Damage 的伤害倍率。</summary>
    public float DamageScale { get; set; } = ParamsConfig.WeaponDamageScaleDefault;

    /// <summary>相对 RunStats.AttackSpeed 的攻速倍率。</summary>
    public float AttackSpeedScale { get; set; } = ParamsConfig.WeaponAttackSpeedScaleDefault;

    /// <summary>攻击/索敌距离（像素）。</summary>
    public float Range { get; set; } = ParamsConfig.WeaponRangeDefault;

    /// <summary>远程弹道速度（像素/秒）；近战忽略。</summary>
    public float ProjectileSpeed { get; set; } = ParamsConfig.WeaponProjectileSpeedDefault;

    /// <summary>远程攻击实例化的弹道场景；未设置则无法发射。</summary>
    public PackedScene? ProjectileScene { get; set; }

    /// <summary>武器精灵或特效的 tint 颜色。</summary>
    public Color VisualColor { get; set; } = Colors.White;

    /// <summary>
    /// 从 item 配置行构建 WeaponData；按 ItemConfigId 分支设置远程/近战参数。
    /// </summary>
    /// <param name="itemConfigId">item.json 中的 id（如 1031 短剑、1032/1033 远程）。</param>
    /// <param name="projectileScene">远程武器必填的弹道 PackedScene。</param>
    /// <returns>构建成功返回实例；配置不存在时 PushError 并返回 null。</returns>
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
            DamageScale = config.StatDamage > 0f ? config.StatDamage : ParamsConfig.WeaponDamageScaleDefault,
            AttackSpeedScale = Mathf.Max(ParamsConfig.WeaponMinAttackSpeedScale, 1f + config.StatSpeed),
        };

        var rangedId1 = (int)ParamsConfig.WeaponRangedConfigId1;
        var rangedId2 = (int)ParamsConfig.WeaponRangedConfigId2;
        var swordId = (int)ParamsConfig.WeaponSwordConfigId;

        switch (itemConfigId)
        {
            case var id when id == rangedId1 || id == rangedId2:
                data.Type = WeaponType.Ranged;
                data.Range = ParamsConfig.WeaponRangeRanged;
                data.ProjectileSpeed = ParamsConfig.WeaponProjectileSpeedRanged;
                data.ProjectileScene = projectileScene;
                break;
            case var id when id == swordId:
                data.Type = WeaponType.Melee;
                data.MeleeStyle = MeleeStyle.Swing;
                data.Range = ParamsConfig.WeaponRangeSword;
                break;
            default:
                data.Type = WeaponType.Melee;
                data.MeleeStyle = MeleeStyle.Swing;
                data.Range = ParamsConfig.WeaponRangeMeleeDefault;
                break;
        }

        return data;
    }

    /// <summary>创建默认手枪（远程）数据，用于测试或未配表时的 fallback。</summary>
    /// <param name="projectileScene">弹道场景。</param>
    public static WeaponData CreatePistol(PackedScene? projectileScene) => new()
    {
        Id = "pistol",
        DisplayName = "手枪",
        Type = WeaponType.Ranged,
        DamageScale = ParamsConfig.WeaponDamageScaleDefault,
        AttackSpeedScale = ParamsConfig.WeaponPistolAttackSpeedScale,
        Range = ParamsConfig.WeaponRangeRanged,
        ProjectileSpeed = ParamsConfig.WeaponProjectileSpeedRanged,
        ProjectileScene = projectileScene,
        VisualColor = ParamsConfig.ColorWeaponPistol,
    };

    /// <summary>创建默认短剑（挥砍近战）数据。</summary>
    public static WeaponData CreateSword() => new()
    {
        Id = "sword",
        DisplayName = "短剑",
        Type = WeaponType.Melee,
        MeleeStyle = MeleeStyle.Swing,
        DamageScale = ParamsConfig.WeaponSwordDamageScale,
        AttackSpeedScale = ParamsConfig.WeaponSwordAttackSpeedScale,
        Range = ParamsConfig.WeaponRangeMeleeDefault,
        VisualColor = ParamsConfig.ColorWeaponSword,
    };

    /// <summary>创建默认长矛（刺击近战）数据，射程略长于短剑。</summary>
    public static WeaponData CreateSpear() => new()
    {
        Id = "spear",
        DisplayName = "长矛",
        Type = WeaponType.Melee,
        MeleeStyle = MeleeStyle.Thrust,
        DamageScale = ParamsConfig.WeaponSpearDamageScale,
        AttackSpeedScale = ParamsConfig.WeaponAttackSpeedScaleDefault,
        Range = ParamsConfig.WeaponSpearRange,
        VisualColor = ParamsConfig.ColorWeaponSpear,
    };
}
