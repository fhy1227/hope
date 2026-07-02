using Godot;
using Hope.Entities;

namespace Hope.Components.Actions;

/// <summary>
/// 以某节点为中心的范围伤害与击退工具。聚气、震地、格挡反制等 AOE 均复用此类，
/// 禁止在各 Action 内复制敌人遍历逻辑。
/// </summary>
public static class CombatPulse
{
    /// <summary>
    /// 对 origin 周围 radius 内的所有有效敌人造成伤害，并按需施加击退。
    /// </summary>
    /// <param name="origin">范围中心（通常为玩家 GlobalPosition）。</param>
    /// <param name="radius">伤害半径（像素）。</param>
    /// <param name="damage">对每个命中敌人造成的伤害值。</param>
    /// <param name="knockbackSpeed">击退速度；为 0 时不击退。</param>
    /// <returns>实际命中的敌人数量。</returns>
    public static int HitCount(Node2D origin, float radius, int damage, float knockbackSpeed)
    {
        var hitCount = 0;
        var enemies = origin.GetTree().GetNodesInGroup("enemies");

        foreach (var node in enemies)
        {
            if (node is not Enemy enemy || !GodotObject.IsInstanceValid(enemy))
            {
                continue;
            }

            if (origin.GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition) > radius * radius)
            {
                continue;
            }

            enemy.TakeDamage(damage);
            hitCount++;

            if (knockbackSpeed > 0f)
            {
                var direction = enemy.GlobalPosition - origin.GlobalPosition;
                if (direction.LengthSquared() < 0.01f)
                {
                    direction = Vector2.Right;
                }

                enemy.ApplyKnockback(direction.Normalized(), knockbackSpeed);
            }
        }

        return hitCount;
    }
}
