using Godot;

namespace Hope.Components;

/// <summary>
/// 单位头顶血条，绑定同级 HealthComponent。
/// </summary>
public partial class UnitHealthBar : Node2D
{
    [Export]
    public NodePath HealthComponentPath { get; set; } = new("../HealthComponent");

    [Export]
    public Vector2 BarSize { get; set; } = new(28f, 4f);

    [Export]
    public float YOffset { get; set; } = -18f;

    [Export]
    public Color FillColor { get; set; } = new(0.35f, 0.9f, 0.45f);

    [Export]
    public Color BackgroundColor { get; set; } = new(0.1f, 0.1f, 0.12f, 0.9f);

    [Export]
    public bool HideWhenFull { get; set; }

    private HealthComponent? _health;
    private float _ratio = 1f;

    public override void _Ready()
    {
        Position = new Vector2(0f, YOffset);
        CallDeferred(MethodName.BindHealth);
    }

    public override void _ExitTree()
    {
        if (_health != null)
        {
            _health.Changed -= OnHealthChanged;
            _health.Died -= OnDied;
        }
    }

    private void BindHealth()
    {
        if (!HasNode(HealthComponentPath))
        {
            GD.PushError("UnitHealthBar: HealthComponent not found.");
            return;
        }

        _health = GetNode<HealthComponent>(HealthComponentPath);
        _health.Changed += OnHealthChanged;
        _health.Died += OnDied;
        OnHealthChanged(_health.CurrentHealth, _health.MaxHealth);
    }

    private void OnHealthChanged(int current, int max)
    {
        _ratio = max > 0 ? (float)current / max : 0f;
        Visible = !HideWhenFull || current < max;
        QueueRedraw();
    }

    private void OnDied()
    {
        Visible = false;
    }

    public override void _Draw()
    {
        if (!Visible)
        {
            return;
        }

        var half = BarSize / 2f;
        var background = new Rect2(-half.X, -half.Y, BarSize.X, BarSize.Y);
        DrawRect(background, BackgroundColor);

        if (_ratio <= 0f)
        {
            return;
        }

        var fill = new Rect2(-half.X, -half.Y, BarSize.X * _ratio, BarSize.Y);
        DrawRect(fill, FillColor);
    }
}
