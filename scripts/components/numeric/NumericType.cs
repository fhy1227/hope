using Hope.Config;

namespace Hope.Core;

/// <summary>
/// 数值属性类型。按当前项目 RunStats / 装备加成最小可用集定义。
/// </summary>
public enum NumericType
{
    None = 0,

    MaxHealth = 100,
    Health = 101,
    Damage = 102,
    AttackSpeed = 103,
    MoveSpeed = 104,
    WeaponRange = 105,
    ProjectileSpeed = 106,

    Crit = 110,
    Armor = 111,
    DamageBonus = 112,
    DamageReduction = 113,
}

public static class NumericDefine
{
    public static float MinMoveSpeed => ParamsConfig.NumericMinMoveSpeed;
    public static float MinWeaponRange => ParamsConfig.NumericMinWeaponRange;
    public const float Epsilon = 0.00001f;
}
