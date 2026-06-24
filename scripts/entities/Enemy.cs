using Godot;
using Hope.Components;
using Hope.Core;

namespace Hope.Entities;

/// <summary>
/// 追击玩家的敌人，接触造成伤害。
/// </summary>
public partial class Enemy : CharacterBody2D
{
    [Export]
    public float Speed { get; set; } = 90f;

    [Export]
    public int ContactDamage { get; set; } = 1;

    [Export]
    public float ContactCooldown { get; set; } = 0.6f;

    [Export]
    public int GoldDrop { get; set; } = 1;

    /// <summary> 敌人类型，对应 drop_table.enemy_type（* 表示通用规则） </summary>
    [Export]
    public string EnemyType { get; set; } = "normal";

    private HealthComponent _health;
    private Node2D _target;
    private float _contactTimer;

    public override void _Ready()
    {
        AddToGroup("enemies");
        _health = GetNode<HealthComponent>("HealthComponent");
        _health.Died += OnDied;
    }

    public void SetTarget(Node2D target)
    {
        _target = target;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || !GodotObject.IsInstanceValid(_target))
        {
            return;
        }

        _contactTimer = Mathf.Max(_contactTimer - (float)delta, 0f);

        var toTarget = _target.GlobalPosition - GlobalPosition;
        if (toTarget.LengthSquared() > 4f)
        {
            Velocity = toTarget.Normalized() * Speed;
            MoveAndSlide();
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
                player.TakeContactDamage(ContactDamage);
                _contactTimer = ContactCooldown;
                break;
            }
        }
    }

    private void OnDied()
    {
        SetPhysicsProcess(false);
        CollisionLayer = 0;
        CollisionMask = 0;

        var visual = GetNodeOrNull<CanvasItem>("Visual");
        if (visual != null)
        {
            visual.Modulate = new Color(1f, 1f, 1f, 0.2f);
        }

        EmitSignal(SignalName.EnemyKilled, GoldDrop, GlobalPosition, EnemyType);
        CallDeferred(Node.MethodName.QueueFree);
    }

    [Signal]
    public delegate void EnemyKilledEventHandler(int gold, Vector2 position, string enemyType);
}
