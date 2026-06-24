using Godot;
using Hope.Config;

namespace Hope.Entities;

/// <summary>
/// 敌人掉落的拾取物。
///  ItemConfigId == 0  → 金币拾取
///  ItemConfigId >  0  → 物品拾取（自动入背包）
/// </summary>
public partial class Pickup : Area2D
{
    /// <summary> 金币数量（ItemConfigId == 0 时生效） </summary>
    [Export]
    public int GoldAmount { get; set; } = 1;

    /// <summary> 物品配置ID（>0 时为物品拾取） </summary>
    [Export]
    public int ItemConfigId { get; set; } = 0;

    /// <summary> 物品数量（ItemConfigId > 0 时生效） </summary>
    [Export]
    public int ItemCount { get; set; } = 1;

    [Export] public float MagnetRange  { get; set; } = 80f;
    [Export] public float MagnetSpeed  { get; set; } = 280f;

    private Node2D _target;
    private bool   _magnetized;

    public override void _Ready()
    {
        CollisionLayer = Hope.Core.CollisionLayers.Pickup;
        CollisionMask = Hope.Core.CollisionLayers.Player;
        BodyEntered += OnBodyEntered;
    }

    public void SetTarget(Node2D target) => _target = target;

    public override void _PhysicsProcess(double delta)
    {
        if (_target == null || !GodotObject.IsInstanceValid(_target))
            return;

        var offset   = _target.GlobalPosition - GlobalPosition;
        var distance = offset.Length();

        if (!_magnetized && distance <= MagnetRange)
            _magnetized = true;

        if (!_magnetized)
            return;

        if (distance <= 8f)
        {
            Collect();
            return;
        }

        GlobalPosition += offset.Normalized() * MagnetSpeed * (float)delta;
    }

    private void OnBodyEntered(Node2D body)
    {
        if (body is Hope.Entities.Player)
            Collect();
    }

    private void Collect()
    {
        // ── 物品拾取 ──────────────────────────────
        if (ItemConfigId > 0)
        {
            var ok = Hope.Systems.InventoryManager.Instance?.AddItem(ItemConfigId, ItemCount) ?? false;
            if (ok)
            {
                var cfg = ConfigManager.Get<ItemConfig>(ItemConfigId);
                GD.Print($"[Pickup] 拾取物品: {cfg?.NameKey ?? ItemConfigId.ToString()}");
                QueueFree();
            }
            else
            {
                GD.Print("[Pickup] 背包已满，无法拾取物品");
                // 不销毁，等背包有空位再拾取
            }
            return;
        }

        // ── 金币拾取（原逻辑）────────────────────
        EmitSignal(SignalName.Collected, GoldAmount);
        QueueFree();
    }

    [Signal]
    public delegate void CollectedEventHandler(int amount);
}
