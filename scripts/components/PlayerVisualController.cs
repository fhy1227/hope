using Godot;
using Hope.Config;
using Hope.Core;
using Hope.Entities;

namespace Hope.Components;

/// <summary>
/// 玩家序列帧动画：从 TexturePacker 图集切片构建 SpriteFrames，同步移动与战斗行为。
/// </summary>
public partial class PlayerVisualController : AnimatedSprite2D
{
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
		UpdateFacing();

		if (_actionLocked)
		{
			return;
		}

		UpdateLocomotion();
	}

	private void UpdateFacing()
	{
		var facingX = _player.FacingDirection.X;
		if (Mathf.Abs(facingX) > 0.01f)
		{
			FlipH = facingX > 0f;
		}
	}

	private void UpdateLocomotion()
	{
		var moving = _player.Velocity.LengthSquared() > ParamsConfig.PlayerFacingVelThresholdSq;
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
		AddLoop(frames, "idle", "idle", 0, 1, ParamsConfig.AnimPlayerIdleFps);
		AddLoop(frames, "walk", "run", 0, (int)ParamsConfig.AnimPlayerWalkFrames, ParamsConfig.AnimPlayerWalkFps);
		AddOnce(frames, "roll", "run", 0, (int)ParamsConfig.AnimPlayerRollFrames, ParamsConfig.AnimPlayerRollFps);
		AddLoop(frames, "charge", "skill1", 0, (int)ParamsConfig.AnimPlayerChargeFrames, ParamsConfig.AnimPlayerChargeFps);
		AddOnce(frames, "charge_release", "skill1", 8, 8, ParamsConfig.AnimPlayerChargeReleaseFps);
		AddOnce(frames, "stomp", "attack", 0, (int)ParamsConfig.AnimPlayerStompFrames, ParamsConfig.AnimPlayerStompFps);
		AddOnce(frames, "parry", "attack", 0, (int)ParamsConfig.AnimPlayerParryFrames, ParamsConfig.AnimPlayerParryFps);
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
				GD.PushWarning($"PlayerVisualController: missing frame {path}");
				continue;
			}

			frames.AddFrame(name, texture);
		}
	}

	private static string ResolveFramePath(string clip, int index)
	{
		var spriteDir = ParamsConfig.PathPlayerSpriteDir;
		if (clip == "idle")
		{
			return $"{spriteDir}archer_gold-idle_0.tres";
		}

		return $"{spriteDir}archer_gold-{clip}_{index:D2}.tres";
	}
}
