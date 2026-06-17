using Godot;
using Hope.Components;
using Hope.Core;

namespace Hope.Entities;

/// <summary>
/// 直线飞行的子弹，命中敌人后销毁。
/// </summary>
public partial class Projectile : Area2D
{
    [Export]
    public float Speed { get; set; } = 450f;

    [Export]
    public int Damage { get; set; } = 1;

    [Export]
    public float Lifetime { get; set; } = 2f;

    private Vector2 _direction = Vector2.Right;
    private float _life;

    public override void _Ready()
    {
        CollisionLayer = CollisionLayers.Projectile;
        CollisionMask = CollisionLayers.Enemy;
        Monitoring = true;
        BodyEntered += OnBodyEntered;
        SetPhysicsProcess(true);
    }

    public void Launch(Vector2 direction, float speed, int damage)
    {
        _direction = direction.Normalized();
        Speed = speed;
        Damage = damage;
        Rotation = _direction.Angle();
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += _direction * Speed * (float)delta;

        _life += (float)delta;
        if (_life >= Lifetime)
        {
            QueueFree();
        }
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is not Enemy)
        {
            return;
        }

        var health = body.GetNodeOrNull<HealthComponent>("HealthComponent");
        health?.TakeDamage(Damage);
        QueueFree();
    }
}
