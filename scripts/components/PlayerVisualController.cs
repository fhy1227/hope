using Godot;
using Hope.Core;
using Hope.Entities;

namespace Hope.Components;

/// <summary>
/// 玩家序列帧动画：从 frames 目录构建 SpriteFrames，同步移动与战斗行为。
/// </summary>
public partial class PlayerVisualController : AnimatedSprite2D
{
	private const string FrameDir = "res://assets/sprites/characters/player/frames/";

	private Player _player = null!;
	private bool _actionLocked;
	private string _locomotionAnim = "idle";

	public override void _Ready()
	{
		_player = GetParent<Player>();

		SpriteFrames = BuildSpriteFrames();
		AnimationFinished += OnAnimationFinished;

		if (EventBus.Instance != null)
		{
			EventBus.Instance.PlayerActionStarted += OnActionStarted;
			EventBus.Instance.PlayerActionEnded += OnActionEnded;
		}

		Play("idle");
	}

	public override void _ExitTree()
	{
		AnimationFinished -= OnAnimationFinished;
		if (EventBus.Instance != null)
		{
			EventBus.Instance.PlayerActionStarted -= OnActionStarted;
			EventBus.Instance.PlayerActionEnded -= OnActionEnded;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_actionLocked)
		{
			return;
		}

		UpdateLocomotion();
	}

	private void UpdateLocomotion()
	{
		var moving = _player.Velocity.LengthSquared() > 4f;
		var next = moving ? "walk" : "idle";
		if (next == _locomotionAnim && IsPlaying())
		{
			return;
		}

		_locomotionAnim = next;
		Play(next);
	}

	private void OnActionStarted(int actionId)
	{
		var anim = ((PlayerActionId)actionId) switch
		{
			PlayerActionId.Roll => "roll",
			PlayerActionId.Charge => "charge",
			PlayerActionId.Stomp => "stomp",
			PlayerActionId.Parry => "parry",
			_ => null,
		};

		if (anim == null || SpriteFrames == null || !SpriteFrames.HasAnimation(anim))
		{
			return;
		}

		_actionLocked = true;
		Play(anim);
	}

	private void OnActionEnded(int actionId)
	{
		if ((PlayerActionId)actionId == PlayerActionId.Charge && Animation == "charge"
			&& SpriteFrames?.HasAnimation("charge_release") == true)
		{
			Play("charge_release");
			return;
		}
	}

	private void OnAnimationFinished()
	{
		if (!_actionLocked)
		{
			return;
		}

		var anim = Animation;
		if (anim == "roll" || anim == "stomp" || anim == "parry" || anim == "charge" || anim == "charge_release")
		{
			_actionLocked = false;
			UpdateLocomotion();
		}
	}

	private static SpriteFrames BuildSpriteFrames()
	{
		var frames = new SpriteFrames();
		AddLoop(frames, "idle", "idle_", 5, 6f);
		AddLoop(frames, "walk", "walk_", 5, 8f);
		AddOnce(frames, "roll", "walk_", 5, 20f);
		AddLoop(frames, "charge", "action_b_", 3, 8f);
		AddOnce(frames, "charge_release", "action_b_", 3, 2, 10f);
		AddOnce(frames, "stomp", "action_c_", 5, 14f);
		AddOnce(frames, "parry", "action_d_", 5, 12f);
		return frames;
	}

	private static void AddLoop(SpriteFrames frames, string name, string prefix, int count, float fps)
	{
		AddFrames(frames, name, prefix, 0, count, fps, loop: true);
	}

	private static void AddOnce(SpriteFrames frames, string name, string prefix, int count, float fps)
	{
		AddFrames(frames, name, prefix, 0, count, fps, loop: false);
	}

	private static void AddOnce(SpriteFrames frames, string name, string prefix, int start, int count, float fps)
	{
		AddFrames(frames, name, prefix, start, count, fps, loop: false);
	}

	private static void AddFrames(
		SpriteFrames frames,
		string name,
		string prefix,
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
			var path = $"{FrameDir}{prefix}{(start + i):D2}.png";
			var texture = GD.Load<Texture2D>(path);
			if (texture == null)
			{
				GD.PushWarning($"PlayerVisualController: missing frame {path}");
				continue;
			}

			frames.AddFrame(name, texture);
		}
	}
}
