namespace Hope.Core;

/// <summary>
/// 近战武器攻击动画/判定样式，影响 <see cref="WeaponData"/> 与 WeaponSlot 的表现与 hitbox 形状。
/// </summary>
public enum MeleeStyle
{
    /// <summary>挥砍：弧形或扇形攻击。</summary>
    Swing,

    /// <summary>刺击：直线或突刺方向攻击。</summary>
    Thrust,
}
