using Godot;
using Hope.Config;

namespace Hope.Components;

/// <summary>
/// 世界空间浮动伤害数字：上浮、淡出后自毁。
/// </summary>
public partial class DamageNumber : Node2D
{
	private string _text = "";
	private Color _color;
	private Vector2 _velocity;
	private float _elapsed;

	public void Setup(int amount, bool isPlayer)
	{
		_text = amount.ToString();
		_color = isPlayer
			? ParamsConfig.ColorDamageNumberPlayer
			: ParamsConfig.ColorDamageNumberEnemy;

		Position += new Vector2(
			(float)GD.RandRange(-ParamsConfig.DamageNumberJitterX, ParamsConfig.DamageNumberJitterX),
			(float)GD.RandRange(-ParamsConfig.DamageNumberJitterY, ParamsConfig.DamageNumberJitterY));
		_velocity = new Vector2(
			(float)GD.RandRange(-ParamsConfig.DamageNumberVelJitterX, ParamsConfig.DamageNumberVelJitterX),
			-ParamsConfig.DamageNumberRiseSpeed);
		Scale = Vector2.One * ParamsConfig.DamageNumberInitialScale;

		var tween = CreateTween();
		tween.TweenProperty(this, "scale", Vector2.One, ParamsConfig.DamageNumberScaleTween)
			.SetTrans(Tween.TransitionType.Back)
			.SetEase(Tween.EaseType.Out);
	}

	public override void _Process(double delta)
	{
		var dt = (float)delta;
		_elapsed += dt;
		Position += _velocity * dt;
		_velocity *= ParamsConfig.DamageNumberVelocityDecay;
		QueueRedraw();

		if (_elapsed >= ParamsConfig.DamageNumberDuration)
		{
			QueueFree();
		}
	}

	public override void _Draw()
	{
		var font = ThemeDB.FallbackFont;
		var fontSize = (int)ParamsConfig.DamageNumberFontSize;
		var size = font.GetStringSize(_text, HorizontalAlignment.Center, -1, fontSize);
		var alpha = Mathf.Clamp(1f - _elapsed / ParamsConfig.DamageNumberDuration, 0f, 1f);
		var color = new Color(_color.R, _color.G, _color.B, alpha);
		DrawString(font, new Vector2(-size.X / 2f, size.Y / 4f), _text, HorizontalAlignment.Center, -1, fontSize, color);
	}
}
