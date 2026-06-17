using Godot;
using Hope.Core;
using Hope.Entities;

namespace Hope.Components;

/// <summary>
/// 自动瞄准最近敌人并发射子弹。
/// </summary>
public partial class WeaponComponent : Node
{
    [Export]
    public PackedScene? ProjectileScene { get; set; }

    [Export]
    public NodePath ProjectileContainerPath { get; set; } = new("../Projectiles");

    private Node2D _owner;
    private Node2D _projectileContainer;
    private RunStats _stats = new();
    private float _cooldown;

    public override void _Ready()
    {
        _owner = GetParent<Node2D>();
        _projectileContainer = ResolveProjectileContainer();
        SetPhysicsProcess(true);
    }

    private Node2D ResolveProjectileContainer()
    {
        if (!ProjectileContainerPath.IsEmpty && HasNode(ProjectileContainerPath))
        {
            return GetNode<Node2D>(ProjectileContainerPath);
        }

        var world = _owner.GetParent()?.GetParent();
        if (world != null && world.HasNode("Projectiles"))
        {
            return world.GetNode<Node2D>("Projectiles");
        }

        GD.PushError("WeaponComponent: Projectiles container not found.");
        return _owner;
    }

    public void BindStats(RunStats stats)
    {
        _stats = stats;
    }

    public override void _PhysicsProcess(double delta)
    {
        _cooldown -= (float)delta;
        if (_cooldown > 0f || ProjectileScene == null)
        {
            return;
        }

        var target = FindNearestEnemy();
        if (target == null)
        {
            return;
        }

        Fire(target);
        _cooldown = 1f / Mathf.Max(_stats.AttackSpeed, 0.1f);
    }

    private Node2D? FindNearestEnemy()
    {
        var enemies = GetTree().GetNodesInGroup("enemies");
        Node2D? nearest = null;
        var bestDistance = _stats.WeaponRange * _stats.WeaponRange;

        foreach (var node in enemies)
        {
            if (node is not Node2D enemy || !GodotObject.IsInstanceValid(enemy))
            {
                continue;
            }

            var distance = _owner.GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
            if (distance > bestDistance)
            {
                continue;
            }

            bestDistance = distance;
            nearest = enemy;
        }

        return nearest;
    }

    private void Fire(Node2D target)
    {
        var projectile = ProjectileScene.Instantiate<Projectile>();
        _projectileContainer.AddChild(projectile);
        projectile.GlobalPosition = _owner.GlobalPosition;

        var direction = target.GlobalPosition - _owner.GlobalPosition;
        projectile.Launch(direction, _stats.ProjectileSpeed, Mathf.RoundToInt(_stats.Damage));
    }
}
