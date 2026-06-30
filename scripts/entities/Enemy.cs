using Godot;
using Hope.Components;

namespace Hope.Entities;

/// <summary>
/// 追击玩家的敌人，接触造成伤害。
/// </summary>
public partial class Enemy : CharacterBody2D
{
    [Export]
    public float ContactCooldown { get; set; } = 0.6f;

    [Export]
    public int GoldDrop { get; set; } = 1;

    /// <summary> 敌人类型，对应 drop_table.enemy_type（* 表示通用规则） </summary>
    [Export]
    public string EnemyType { get; set; } = "normal";

    private HealthComponent _health;
    private EnemyStatsComponent _statsComponent;
    private Node2D _target;
    private float _contactTimer;
    private float _stunTimer;
    private Vector2 _knockbackVelocity;

    /// <summary>当前面朝方向（用于视觉朝向，优先取追击目标方向）。</summary>
    public Vector2 FacingDirection { get; private set; } = Vector2.Right;

    public override void _Ready()
    {
        AddToGroup("enemies");
        _health = GetNode<HealthComponent>("HealthComponent");
        _statsComponent = GetNode<EnemyStatsComponent>("EnemyStatsComponent");
        _health.Died += OnDied;
    }

    public void SetTarget(Node2D target)
    {
        _target = target;
    }

    public void TakeDamage(int amount) => _statsComponent.TakeDamage(amount);

    public void ApplyStun(float duration)
    {
        _stunTimer = Mathf.Max(_stunTimer, duration);
    }

    public void ApplyKnockback(Vector2 direction, float speed)
    {
        if (direction.LengthSquared() < 0.01f)
        {
            direction = Vector2.Right;
        }

        _knockbackVelocity = direction.Normalized() * speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || !GodotObject.IsInstanceValid(_target))
        {
            return;
        }

        _contactTimer = Mathf.Max(_contactTimer - (float)delta, 0f);
        _stunTimer = Mathf.Max(_stunTimer - (float)delta, 0f);

        if (_knockbackVelocity.LengthSquared() > 4f)
        {
            Velocity = _knockbackVelocity;
            _knockbackVelocity *= 0.88f;
            MoveAndSlide();
            return;
        }

        if (_stunTimer > 0f)
        {
            Velocity = Vector2.Zero;
            return;
        }

        var toTarget = _target.GlobalPosition - GlobalPosition;
        if (toTarget.LengthSquared() > 1f)
        {
            FacingDirection = toTarget.Normalized();
        }

        if (toTarget.LengthSquared() > 4f)
        {
            Velocity = toTarget.Normalized() * _statsComponent.GetMoveSpeed();
            MoveAndSlide();
        }
        else
        {
            Velocity = Vector2.Zero;
        }

        if (_contactTimer > 0f)
        {
            return;
        }

        for (var i = 0; i < GetSlideCollisionCount(); i++)
        {
            var collider = GetSlideCollision(i).GetCollider();
            if (collider is Player player)
            {
                player.TakeContactDamage((int)_statsComponent.GetContactDamage(), this);
                _contactTimer = ContactCooldown;

                var visual = GetNodeOrNull<EnemyVisualController>("Visual");
                visual?.FaceToward(_target.GlobalPosition);
                visual?.PlayAttack();
                break;
            }
        }
    }

    private void OnDied()
    {
        SetPhysicsProcess(false);
        CollisionLayer = 0;
        CollisionMask = 0;

        EmitSignal(SignalName.EnemyKilled, GoldDrop, GlobalPosition, EnemyType);

        var visual = GetNodeOrNull<EnemyVisualController>("Visual");
        if (visual != null)
        {
            visual.PlayDefeated();
            return;
        }

        QueueFree();
    }

    [Signal]
    public delegate void EnemyKilledEventHandler(int gold, Vector2 position, string enemyType);
}
