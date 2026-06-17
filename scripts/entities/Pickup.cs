using Godot;

namespace Hope.Entities;

/// <summary>
/// 敌人掉落的金币，玩家靠近自动吸附。
/// </summary>
public partial class Pickup : Area2D
{
    [Export]
    public int GoldAmount { get; set; } = 1;

    [Export]
    public float MagnetRange { get; set; } = 80f;

    [Export]
    public float MagnetSpeed { get; set; } = 280f;

    private Node2D _target;
    private bool _magnetized;

    public override void _Ready()
    {
        CollisionLayer = Hope.Core.CollisionLayers.Pickup;
        CollisionMask = Hope.Core.CollisionLayers.Player;
        BodyEntered += OnBodyEntered;
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

        var offset = _target.GlobalPosition - GlobalPosition;
        var distance = offset.Length();

        if (!_magnetized && distance <= MagnetRange)
        {
            _magnetized = true;
        }

        if (!_magnetized)
        {
            return;
        }

        if (distance <= 8f)
        {
            Collect();
            return;
        }

        GlobalPosition += offset.Normalized() * MagnetSpeed * (float)delta;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Player)
        {
            Collect();
        }
    }

    private void Collect()
    {
        EmitSignal(SignalName.Collected, GoldAmount);
        QueueFree();
    }

    [Signal]
    public delegate void CollectedEventHandler(int amount);
}
