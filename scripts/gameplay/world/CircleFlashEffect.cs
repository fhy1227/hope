using Godot;
using Hope.Config;
using Hope.Core;

namespace Hope.Components;

/// <summary>
/// 世界空间圆形闪光：半透明填充圆快速放大并淡出，用于聚气爆发等范围技能反馈。
/// 由 <see cref="SpawnAt"/> 实例化到 <see cref="GameWorld.Effects"/>，动画结束后自毁。
/// </summary>
public partial class CircleFlashEffect : Node2D
{
	private float _radius;
	private Color _baseColor = Colors.White;
	private float _alpha;
	private float _scale;

	/// <summary>
	/// 在指定世界坐标生成一次圆形闪光。
	/// </summary>
	/// <param name="globalPosition">圆心世界坐标。</param>
	/// <param name="radius">圆半径（像素），与伤害范围一致。</param>
	/// <param name="color">填充色（不含透明度，透明度由动画控制）。</param>
	public static void SpawnAt(Vector2 globalPosition, float radius, Color? color = null)
	{
		var effects = Combat.Instance?.World.Effects;
		if (effects == null)
		{
			return;
		}

		var scene = GD.Load<PackedScene>(ScenePaths.CircleFlashEffect);
		var flash = scene.Instantiate<CircleFlashEffect>();
		effects.AddChild(flash);
		flash.GlobalPosition = globalPosition;
		flash.Play(radius, color ?? ParamsConfig.ColorCircleFlashDefault);
	}

	/// <summary>配置半径与颜色并播放淡出动画。</summary>
	public void Play(float radius, Color color)
	{
		_radius = radius;
		_baseColor = color;
		_alpha = ParamsConfig.CircleFlashStartAlpha;
		_scale = ParamsConfig.CircleFlashStartScale;
		QueueRedraw();

		var tween = CreateTween();
		tween.SetParallel(true);
		tween.TweenMethod(Callable.From<float>(SetAlpha), ParamsConfig.CircleFlashStartAlpha, 0f, ParamsConfig.CircleFlashDuration)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Quad);
		tween.TweenMethod(Callable.From<float>(SetScale), ParamsConfig.CircleFlashStartScale, ParamsConfig.CircleFlashEndScale, ParamsConfig.CircleFlashDuration)
			.SetEase(Tween.EaseType.Out)
			.SetTrans(Tween.TransitionType.Quad);
		tween.Chain().TweenCallback(Callable.From(QueueFree));
	}

	public override void _Draw()
	{
		var fill = new Color(_baseColor.R, _baseColor.G, _baseColor.B, _alpha);
		DrawCircle(Vector2.Zero, _radius * _scale, fill);
	}

	private void SetAlpha(float alpha)
	{
		_alpha = alpha;
		QueueRedraw();
	}

	private void SetScale(float scale)
	{
		_scale = scale;
		QueueRedraw();
	}
}
