using Godot;
using Hope.Components;
using Hope.Components.Actions;
using Hope.Config;
using Hope.Core;
using Hope.Entities;

namespace Hope.Entities;

/// <summary>
/// 玩家：移动、受击、绑定本局数值与武器。
/// </summary>
public partial class Player : CharacterBody2D
{
	private StateMachine _stateMachine;
	private HealthComponent _health;
	private PlayerStatsComponent _statsComponent;
	private PlayerWeaponController _weapons;
	private PlayerActionController _actions;
	private float _invincibilityTimer;
	private Vector2 _visualBaseScale = Vector2.One;

	public bool IsInvincible => _invincibilityTimer > 0f || _actions.GrantsInvincibility;

	/// <summary>当前面朝方向：移动时取速度，否则取上次移动输入。</summary>
	public Vector2 FacingDirection =>
		Velocity.LengthSquared() > ParamsConfig.PlayerFacingVelThresholdSq
			? Velocity.Normalized()
			: _actions.LastMoveDirection;

	public override void _Ready()
	{
		AddToGroup("player");
		CollisionLayer = CollisionLayers.Player;
		CollisionMask = CollisionLayers.Enemy;

		_health = GetNode<HealthComponent>("HealthComponent");
		_health.IsPlayer = true;
		_statsComponent = GetNode<PlayerStatsComponent>("PlayerStatsComponent");
		_weapons = GetNode<PlayerWeaponController>("WeaponSlots");
		_actions = GetNode<PlayerActionController>("PlayerActionController");
		_actions.Bind(this);

		var visual = GetNodeOrNull<AnimatedSprite2D>("Visual");
		if (visual != null)
		{
			_visualBaseScale = visual.Scale;
		}
	}

	public void Initialize(RunStats stats)
	{
		_statsComponent.ApplyStats(stats, refillHealth: true);
		_weapons.SetupDefaultLoadout();

		_health.Died += OnDied;

		_stateMachine = new StateMachine();
		_stateMachine.Add("idle", new IdleState(this));
		_stateMachine.Add("move", new MoveState(this));
		_stateMachine.Change("idle");
	}

	public void ApplyStats(RunStats stats, bool refillHealth)
	{
		_statsComponent.ApplyStats(stats, refillHealth);
	}

	public float GetMoveSpeed() => _statsComponent.GetMoveSpeed();

	public float GetEffectiveMoveSpeed() => GetMoveSpeed() * _actions.MoveSpeedMultiplier;

	public int GetActionDamage(float multiplier = 1f)
	{
		var numeric = _statsComponent.GetNumeric();
		return Mathf.Max((int)ParamsConfig.PlayerMinDamage, Mathf.RoundToInt(numeric[NumericType.Damage] * multiplier));
	}

	public void SetActionVisual(Color color, float scaleMultiplier = 1f)
	{
		var visual = GetNodeOrNull<AnimatedSprite2D>("Visual");
		if (visual == null)
		{
			return;
		}

		visual.Modulate = color;
		visual.Scale = _visualBaseScale * scaleMultiplier;
	}

	public void ResetActionVisual()
	{
		var visual = GetNodeOrNull<AnimatedSprite2D>("Visual");
		if (visual == null)
		{
			return;
		}

		visual.Modulate = Colors.White;
		visual.Scale = _visualBaseScale;
	}

	public void FlashActionRelease(Color color)
	{
		var visual = GetNodeOrNull<CanvasItem>("Visual");
		if (visual == null)
		{
			return;
		}

		visual.Modulate = color;
		var tween = CreateTween();
		tween.TweenProperty(visual, "modulate", Colors.White, ParamsConfig.PlayerActionFlashDuration);
	}

	public override void _PhysicsProcess(double delta)
	{
		_invincibilityTimer = Mathf.Max(_invincibilityTimer - (float)delta, 0f);

		var input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
		_actions.SetLastMoveDirection(input);
		_actions.UpdateActions(delta);

		if (_actions.BlocksMovement)
		{
			if (!_actions.GrantsInvincibility)
			{
				Velocity = Vector2.Zero;
			}

			return;
		}

		_stateMachine?.Update(delta);
	}

	public bool TakeContactDamage(int amount, Enemy? source = null)
	{
		if (amount <= 0 || IsInvincible)
		{
			return false;
		}

		if (_actions.TryParry(source, out _))
		{
			return false;
		}

		_actions.OnPlayerHit();
		_statsComponent.TakeContactDamage(amount);
		_invincibilityTimer = ParamsConfig.PlayerHitInvincibility;
		FlashDamage();
		return true;
	}

	private void FlashDamage()
	{
		var visual = GetNodeOrNull<CanvasItem>("Visual");
		if (visual == null)
		{
			return;
		}

		visual.Modulate = ParamsConfig.ColorPlayerDamage;
		var tween = CreateTween();
		tween.TweenProperty(visual, "modulate", Colors.White, ParamsConfig.PlayerDamageFlashDuration);
	}

	private void OnDied()
	{
		SetPhysicsProcess(false);
		var visual = GetNodeOrNull<CanvasItem>("Visual");
		if (visual != null)
		{
			visual.Modulate = new Color(1f, 1f, 1f, ParamsConfig.PlayerDeathVisualAlpha);
		}
	}

	private void ChangePlayerState(string name)
	{
		_stateMachine?.Change(name);
	}

	private sealed class IdleState : IGameState
	{
		private readonly Player _player;

		public IdleState(Player player)
		{
			_player = player;
		}

		public void Enter()
		{
			_player.Velocity = Vector2.Zero;
		}

		public void Exit()
		{
		}

		public void Update(double delta)
		{
			var input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
			if (input != Vector2.Zero)
			{
				_player.ChangePlayerState("move");
			}
		}
	}

	private sealed class MoveState : IGameState
	{
		private readonly Player _player;

		public MoveState(Player player)
		{
			_player = player;
		}

		public void Enter()
		{
		}

		public void Exit()
		{
		}

		public void Update(double delta)
		{
			var input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
			if (input == Vector2.Zero)
			{
				_player.ChangePlayerState("idle");
				return;
			}

			_player.Velocity = input * _player.GetEffectiveMoveSpeed();
			_player.MoveAndSlide();
		}
	}
}
