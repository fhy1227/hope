using Hope.Config;

namespace Hope.Core;

/// <summary>
/// 单局运行时数值快照。由 RunManager 持有；波间商店升级通过 <see cref="ShopUpgrade"/> 修改本对象。
/// 与玩家组件上的最终属性（含装备加成）分离，此处为对局级基础成长。
/// </summary>
public class RunStats
{
    /// <summary>本局生命上限（商店可叠加）。</summary>
    public int MaxHealth { get; set; } = (int)ParamsConfig.RunMaxHealth;

    /// <summary>移动速度（像素/秒）。</summary>
    public float Speed { get; set; } = ParamsConfig.RunSpeed;

    /// <summary>基础攻击伤害。</summary>
    public float Damage { get; set; } = ParamsConfig.RunDamage;

    /// <summary>攻击间隔倍率；越大攻速越快。</summary>
    public float AttackSpeed { get; set; } = ParamsConfig.RunAttackSpeed;

    /// <summary>远程弹道速度（像素/秒）。</summary>
    public float ProjectileSpeed { get; set; } = ParamsConfig.RunProjectileSpeed;

    /// <summary>武器有效射程/索敌距离。</summary>
    public float WeaponRange { get; set; } = ParamsConfig.RunWeaponRange;

    /// <summary>本局累计金币。</summary>
    public int Gold { get; set; }

    /// <summary>护甲（命运织机等对局成长；装备护甲另行叠加）。</summary>
    public int Armor { get; set; }

    /// <summary>当前波次，从 1 起计。</summary>
    public int Wave { get; set; }

    /// <summary>浅拷贝全部字段，用于 UI 预览或存档快照而不共享引用。</summary>
    /// <returns>新的 <see cref="RunStats"/> 实例，数值与当前相同。</returns>
    public RunStats Clone()
    {
        return new RunStats
        {
            MaxHealth = MaxHealth,
            Speed = Speed,
            Damage = Damage,
            AttackSpeed = AttackSpeed,
            ProjectileSpeed = ProjectileSpeed,
            WeaponRange = WeaponRange,
            Gold = Gold,
            Armor = Armor,
            Wave = Wave,
        };
    }
}
