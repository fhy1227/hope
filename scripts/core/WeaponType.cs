namespace Hope.Core;

/// <summary>
/// 武器大类，决定攻击逻辑走远程弹道还是近战判定。
/// </summary>
public enum WeaponType
{
    /// <summary>远程：生成弹道，依赖 ProjectileSpeed 与 Range。</summary>
    Ranged,

    /// <summary>近战：无弹道，在 Range 内直接判定命中。</summary>
    Melee,
}
