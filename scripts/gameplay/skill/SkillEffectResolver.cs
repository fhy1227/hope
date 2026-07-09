using Godot;
using System.Collections.Generic;
using System.Linq;
using Hope.Entities;

namespace Hope.SkillSystem;

/// <summary>技能目标解析：根据命中形状在场景中选取敌人。</summary>
public static class SkillEffectResolver
{
    /// <summary>解析技能可命中的敌人列表。</summary>
    public static List<Enemy> ResolveTargets(
        SkillDefinition def,
        int rank,
        Node2D caster,
        Vector2? aimPosition = null)
    {
        var effect = def.EffectResource;
        if (effect == null || caster == null)
        {
            return [];
        }

        var origin = caster.GlobalPosition;
        var aim = aimPosition ?? origin + Vector2.Right * 80f;
        var radius = effect.Radius + (rank - 1) * 5f;

        var enemies = caster.GetTree().GetNodesInGroup("enemies")
            .OfType<Enemy>()
            .Where(e => GodotObject.IsInstanceValid(e))
            .ToList();

        List<Enemy> hits = effect.HitShape switch
        {
            EHitShape.SingleTarget => PickNearest(enemies, aim, effect.Range),
            EHitShape.Cone => FilterCone(enemies, origin, aim - origin, radius, effect.ConeAngle),
            _ => FilterCircle(enemies, origin, radius),
        };

        if (effect.MaxTargets > 0 && hits.Count > effect.MaxTargets)
        {
            hits = hits.Take(effect.MaxTargets).ToList();
        }

        return hits;
    }

    private static List<Enemy> PickNearest(List<Enemy> enemies, Vector2 point, float range)
    {
        Enemy? nearest = null;
        var bestDist = range * range;

        foreach (var enemy in enemies)
        {
            var dist = point.DistanceSquaredTo(enemy.GlobalPosition);
            if (dist <= bestDist)
            {
                bestDist = dist;
                nearest = enemy;
            }
        }

        return nearest != null ? [nearest] : [];
    }

    private static List<Enemy> FilterCircle(List<Enemy> enemies, Vector2 center, float radius)
    {
        var r2 = radius * radius;
        return enemies.Where(e => center.DistanceSquaredTo(e.GlobalPosition) <= r2).ToList();
    }

    private static List<Enemy> FilterCone(
        List<Enemy> enemies, Vector2 origin, Vector2 direction, float radius, float angleDeg)
    {
        if (direction.LengthSquared() < 0.01f)
        {
            direction = Vector2.Right;
        }

        direction = direction.Normalized();
        var halfAngle = Mathf.DegToRad(angleDeg * 0.5f);
        var r2 = radius * radius;

        return enemies.Where(e =>
        {
            var toEnemy = e.GlobalPosition - origin;
            if (toEnemy.LengthSquared() > r2)
            {
                return false;
            }

            toEnemy = toEnemy.Normalized();
            return direction.Dot(toEnemy) >= Mathf.Cos(halfAngle);
        }).ToList();
    }
}
