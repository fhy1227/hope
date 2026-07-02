using Godot;
using Hope.Config;

namespace Hope.Components;

/// <summary>
/// 单位头顶血条，绑定同级 HealthComponent。
/// </summary>
public partial class UnitHealthBar : Node2D
{
	[Export]
	public NodePath HealthComponentPath { get; set; } = new("../HealthComponent");

	[Export]
	public Vector2 BarSize { get; set; } = new(ParamsConfig.HealthBarWidth, ParamsConfig.HealthBarHeight);

	[Export]
	public float YOffset { get; set; } = ParamsConfig.HealthBarYOffset;

	[Export]
	public Color FillColor { get; set; } = ParamsConfig.ColorHealthBarFill;

	[Export]
	public Color BackgroundColor { get; set; } = ParamsConfig.ColorHealthBarBg;

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
