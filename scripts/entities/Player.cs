using Godot;
using Hope.Components;
using Hope.Core;

namespace Hope.Entities;

/// <summary>
/// 玩家：移动、受击、绑定本局数值与武器。
/// </summary>
public partial class Player : CharacterBody2D
{
	[Export]
	public float Speed { get; set; } = 200f;

	private StateMachine _stateMachine;
	private HealthComponent _health;
	private PlayerWeaponController _weapons;
	private RunStats _stats = new();
	private float _invincibilityTimer;

	public override void _Ready()
	{
		AddToGroup("player");
		CollisionLayer = CollisionLayers.Player;
		CollisionMask = CollisionLayers.Enemy;

		_health = GetNode<HealthComponent>("HealthComponent");
		_health.IsPlayer = true;
		_weapons = GetNode<PlayerWeaponController>("WeaponSlots");
	}

	public void Initialize(RunStats stats)
	{
		_stats = stats;
		ApplyStats(stats, refillHealth: true);
		_weapons.SetupDefaultLoadout();
		_weapons.BindStats(_stats);

		_health.Died += OnDied;

		_stateMachine = new StateMachine();
		_stateMachine.Add("idle", new IdleState(this));
		_stateMachine.Add("move", new MoveState(this));
		_stateMachine.Change("idle");
	}

	public void ApplyStats(RunStats stats, bool refillHealth)
	{
		_stats = stats;
		Speed = stats.Speed;
		_health.SetMaxHealth(stats.MaxHealth, refillHealth);
		_weapons.BindStats(_stats);
	}

	public override void _PhysicsProcess(double delta)
	{
		_invincibilityTimer = Mathf.Max(_invincibilityTimer - (float)delta, 0f);
		_stateMachine?.Update(delta);
	}

	public void TakeContactDamage(int amount)
	{
		if (_invincibilityTimer > 0f || amount <= 0)
		{
			return;
		}

		_health.TakeDamage(amount);
		_invincibilityTimer = 0.4f;
		FlashDamage();
	}

	private void FlashDamage()
	{
		var visual = GetNodeOrNull<CanvasItem>("Visual");
		if (visual == null)
		{
			return;
		}

		visual.Modulate = new Color(1f, 0.45f, 0.45f);
		var tween = CreateTween();
		tween.TweenProperty(visual, "modulate", Colors.White, 0.15);
	}

	private void OnDied()
	{
		SetPhysicsProcess(false);
		var visual = GetNodeOrNull<CanvasItem>("Visual");
		if (visual != null)
		{
			visual.Modulate = new Color(1f, 1f, 1f, 0.35f);
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

			_player.Velocity = input * _player.Speed;
			_player.MoveAndSlide();
		}
	}
}
