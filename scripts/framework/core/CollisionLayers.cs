namespace Hope.Core;

/// <summary>
/// 物理碰撞层位掩码常量。设置 CollisionLayer / CollisionMask 时使用，禁止魔法数字。
/// 层号与 project.godot 中 layer_names 对应（Player=1, Enemy=2, …）。
/// </summary>
public static class CollisionLayers
{
    /// <summary>玩家 CharacterBody 所在层。</summary>
    public const uint Player = 1;

    /// <summary>敌人所在层；翻滚等行为可临时从 Mask 中移除此层以实现穿敌。</summary>
    public const uint Enemy = 2;

    /// <summary>玩家/敌人发射的弹道。</summary>
    public const uint Projectile = 4;

    /// <summary>可拾取掉落物。</summary>
    public const uint Pickup = 8;
}
