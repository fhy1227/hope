using Godot;
using Hope.Entities;

namespace Hope.Components.Actions;

/// <summary>
/// 以玩家为中心的范围伤害与击退。
/// </summary>
public static class CombatPulse
{
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
