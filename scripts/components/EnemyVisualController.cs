using Godot;
using Hope.Entities;

namespace Hope.Components;

/// <summary>
/// 敌人序列帧动画：从 TexturePacker 图集切片构建 SpriteFrames，同步移动、攻击与死亡。
/// </summary>
public partial class EnemyVisualController : AnimatedSprite2D
{
	private const string SpriteDir = "res://assets/textures/enemy/enemy01.sprites/";
	private const string SpritePrefix = "long_daoke";

	private Enemy _enemy = null!;
	private bool _dying;
	private bool _attacking;
	private string _locomotionAnim = "idle";

	public override void _Ready()
	{
		_enemy = GetParent<Enemy>();
		SpriteFrames = BuildSpriteFrames();
		AnimationFinished += OnAnimationFinished;
		Play("idle");
	}

	public override void _ExitTree()
	{
		AnimationFinished -= OnAnimationFinished;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_dying)
		{
			return;
		}

		UpdateFacing();

		if (_attacking)
		{
			return;
		}

		UpdateLocomotion();
	}

	/// <summary>朝向世界坐标点（图集默认朝左，向右移动时水平翻转）。</summary>
	public void FaceToward(Vector2 worldPosition)
	{
		FlipH = worldPosition.X > GlobalPosition.X;
	}

	/// <summary>播放攻击动画，结束后恢复移动动画。</summary>
	public void PlayAttack()
	{
		if (_dying || SpriteFrames == null || !SpriteFrames.HasAnimation("attack"))
		{
			return;
		}

		_attacking = true;
		_locomotionAnim = "";
		Play("attack");
	}

	/// <summary>播放死亡动画，结束后由 <see cref="OnAnimationFinished"/> 销毁敌人节点。</summary>
	public void PlayDefeated()
	{
		if (_dying || SpriteFrames == null || !SpriteFrames.HasAnimation("defeated"))
		{
			_enemy.QueueFree();
			return;
		}

		_dying = true;
		_attacking = false;
		Play("defeated");
	}

	private void UpdateLocomotion()
	{
		var moving = _enemy.Velocity.LengthSquared() > 4f;
		var next = moving ? "walk" : "idle";
		if (next == _locomotionAnim && IsPlaying())
		{
			return;
		}

		_locomotionAnim = next;
		Play(next);
	}

	private void UpdateFacing()
	{
		var facingX = _enemy.Velocity.LengthSquared() > 4f
			? _enemy.Velocity.X
			: _enemy.FacingDirection.X;

		if (Mathf.Abs(facingX) > 0.01f)
		{
			FlipH = facingX > 0f;
		}
	}

	private void OnAnimationFinished()
	{
		if (_dying && Animation == "defeated")
		{
			_enemy.QueueFree();
			return;
		}

		if (_attacking && Animation == "attack")
		{
			_attacking = false;
			UpdateLocomotion();
		}
	}

	private static SpriteFrames BuildSpriteFrames()
	{
		var frames = new SpriteFrames();
		AddLoop(frames, "idle", "idle", 0, 1, 6f);
		AddLoop(frames, "walk", "run", 0, 10, 10f);
		AddOnce(frames, "attack", "attack", 0, 15, 12f);
		AddOnce(frames, "defeated", "defeated", 0, 14, 12f);
		return frames;
	}

	private static void AddLoop(SpriteFrames frames, string name, string clip, int start, int count, float fps)
	{
		AddFrames(frames, name, clip, start, count, fps, loop: true);
	}

	private static void AddOnce(SpriteFrames frames, string name, string clip, int start, int count, float fps)
	{
		AddFrames(frames, name, clip, start, count, fps, loop: false);
	}

	private static void AddFrames(
		SpriteFrames frames,
		string name,
		string clip,
		int start,
		int count,
		float fps,
		bool loop)
	{
		frames.AddAnimation(name);
		frames.SetAnimationSpeed(name, fps);
		frames.SetAnimationLoopMode(name, loop ? SpriteFrames.LoopMode.Linear : SpriteFrames.LoopMode.None);

		for (var i = 0; i < count; i++)
		{
			var path = ResolveFramePath(clip, start + i);
			var texture = GD.Load<Texture2D>(path);
			if (texture == null)
			{
				GD.PushWarning($"EnemyVisualController: missing frame {path}");
				continue;
			}

			frames.AddFrame(name, texture);
		}
	}

	private static string ResolveFramePath(string clip, int index)
	{
		if (clip == "idle")
		{
			return $"{SpriteDir}{SpritePrefix}-idle_0.tres";
		}

		return $"{SpriteDir}{SpritePrefix}-{clip}_{index:D2}.tres";
	}
}
