using System;
using System.Collections.Generic;
using Godot;
using Hope.Core;
using Hope.Entities;
using Hope.Levels;

namespace Hope.Systems;

/// <summary>
/// 土豆兄弟式单局循环：战斗波次 -> 商店 -> 下一波。
/// </summary>
public partial class RunManager : Node
{
	[Export]
	public PackedScene? PlayerScene { get; set; }

	[Export]
	public PackedScene? PickupScene { get; set; }

	[Export]
	public NodePath PlayerContainerPath { get; set; } = new("../Entities");

	[Export]
	public NodePath PickupContainerPath { get; set; } = new("../Pickups");

	[Export]
	public NodePath LevelManagerPath { get; set; } = new("../LevelManager");

	private WaveManager? _waveManager;
	private EnemySpawner? _enemySpawner;
	private LevelManager? _levelManager;
	private Node2D _playerContainer;
	private Node2D _pickupContainer;
	private Player _player;
	private RunStats _stats = new();
	private RunPhase _phase = RunPhase.Combat;

	public RunStats Stats => _stats;
	public RunPhase Phase => _phase;
	public Player? Player => IsInstanceValid(_player) ? _player : null;

	/// <summary>当前活动关卡；无关卡时为 null。</summary>
	public BaseLevel? CurrentLevel => _levelManager?.Current;

	public override void _Ready()
	{
		GameManager.Instance?.ChangeState(GameState.Playing);

		AddToGroup("run_manager");

		_waveManager = GetNode<WaveManager>("WaveManager");
		_enemySpawner = GetNode<EnemySpawner>("EnemySpawner");
		_levelManager = GetNode<LevelManager>(LevelManagerPath);
		_playerContainer = GetNode<Node2D>(PlayerContainerPath);
		_pickupContainer = GetNode<Node2D>(PickupContainerPath);

		_waveManager.WaveCompleted += OnWaveCompleted;
		_enemySpawner.EnemyDefeated += OnEnemyDefeated;
		Hope.EventBus.Instance!.PlayerDied += OnPlayerDied;
		Hope.EventBus.Instance.GameStateChanged += OnGameStateChanged;

		SpawnPlayer();
		StartNextWave();
	}

	public override void _ExitTree()
	{
		if (Hope.EventBus.Instance != null)
		{
			Hope.EventBus.Instance.PlayerDied -= OnPlayerDied;
			Hope.EventBus.Instance.GameStateChanged -= OnGameStateChanged;
		}
	}

	private void OnGameStateChanged(int state)
	{
		var paused = (GameState)state == GameState.Paused;
		_waveManager?.SetPaused(paused);
		_enemySpawner?.SetPaused(paused);
	}

	public IReadOnlyList<ShopUpgrade> RollShopOptions(int count = 3)
	{
		var pool = new List<ShopUpgrade>(ShopUpgrade.Pool);
		var result = new List<ShopUpgrade>(count);

		for (var i = 0; i < count && pool.Count > 0; i++)
		{
			var index = GD.RandRange(0, pool.Count - 1);
			result.Add(pool[index]);
			pool.RemoveAt(index);
		}

		return result;
	}

	public void ApplyShopUpgrade(ShopUpgrade upgrade)
	{
		upgrade.Apply(_stats);
		SyncPlayerStats(refillHealth: false);
		Hope.EventBus.Instance?.EmitGoldChanged(_stats.Gold);
		StartNextWave();
	}

	public void SkipShop()
	{
		StartNextWave();
	}

	/// <summary>
	/// 运行时切换关卡：卸载旧关卡、加载新关卡，并将玩家移到新生成点。
	/// 会停止刷怪并清空当前敌人。
	/// </summary>
	/// <param name="levelPath">关卡场景路径，如 <see cref="ScenePaths.ArenaLevel"/>。</param>
	public void SwitchLevel(string levelPath)
	{
		if (_levelManager == null)
		{
			GD.PushError("RunManager: LevelManager is not available.");
			return;
		}

		_waveManager?.Stop();
		_enemySpawner?.Stop();
		ClearEnemies();

		_levelManager.LoadLevel(levelPath);
		RelocatePlayerToSpawn();
	}

	/// <summary>将玩家移到当前关卡的 <see cref="BaseLevel.SpawnPoint"/>。</summary>
	public void RelocatePlayerToSpawn()
	{
		if (!IsInstanceValid(_player))
			return;

		_player.GlobalPosition = ResolveSpawnPosition();
	}

	public Godot.Collections.Array<Godot.Collections.Dictionary> GetShopOptions(int count = 3)
	{
		var result = new Godot.Collections.Array<Godot.Collections.Dictionary>();
		foreach (var option in RollShopOptions(count))
		{
			result.Add(new Godot.Collections.Dictionary
			{
				["id"] = option.Id,
				["label"] = option.Label,
			});
		}

		return result;
	}

	public void ApplyShopUpgradeById(string id)
	{
		foreach (var option in ShopUpgrade.Pool)
		{
			if (option.Id == id)
			{
				ApplyShopUpgrade(option);
				return;
			}
		}

		GD.PushWarning($"RunManager: unknown shop upgrade '{id}'.");
	}

	private void SpawnPlayer()
	{
		if (PlayerScene == null)
		{
			GD.PushError("RunManager: PlayerScene is not set.");
			return;
		}

		_player = PlayerScene.Instantiate<Player>();
		_player.GlobalPosition = ResolveSpawnPosition();
		_playerContainer.AddChild(_player);
		_player.Initialize(_stats);
		_enemySpawner.BindPlayer(_player);
	}

	private Vector2 ResolveSpawnPosition() =>
		_levelManager?.Current?.GetSpawnGlobalPosition() ?? Vector2.Zero;

	private void ClearEnemies()
	{
		var enemies = GetParent().GetNode<Node2D>("Enemies");
		foreach (var child in enemies.GetChildren())
		{
			enemies.RemoveChild(child);
			child.Free();
		}
	}

	private void StartNextWave()
	{
		SetPhase(RunPhase.Combat);
		_stats.Wave += 1;
		_waveManager.StartWave(_stats.Wave);
		_enemySpawner.BeginWave(_stats.Wave);
	}

	private void OnWaveCompleted(int wave)
	{
		_enemySpawner?.Stop();
		SetPhase(RunPhase.Shop);
	}

	private void OnEnemyDefeated(int gold, Vector2 position, string enemyType)
	{
		if (PickupScene == null)
		{
			AddGold(gold);
			ApplyDropTable(enemyType, position);
			return;
		}

		SpawnGoldPickup(gold, position);
		ApplyDropTable(enemyType, position);
	}

	private void SpawnGoldPickup(int gold, Vector2 position)
	{
		var pickup = PickupScene!.Instantiate<Pickup>();
		pickup.GlobalPosition = position;
		pickup.GoldAmount = gold;
		pickup.SetTarget(_player);
		pickup.Collected += OnPickupCollected;
		Callable.From(() => _pickupContainer.AddChild(pickup)).CallDeferred();
	}

	private void ApplyDropTable(string enemyType, Vector2 position)
	{
		if (PickupScene == null)
			return;

		var wave = _waveManager?.CurrentWave ?? _stats.Wave;
		foreach (var drop in DropTableResolver.RollDrops(enemyType, wave))
			SpawnItemPickup(drop, position);
	}

	private void SpawnItemPickup(DropTableResolver.DropResult drop, Vector2 position)
	{
		var pickup = PickupScene!.Instantiate<Pickup>();
		pickup.GlobalPosition = position;
		pickup.ItemConfigId = drop.ItemId;
		pickup.ItemCount = drop.Count;
		pickup.DropInstance = drop.Instance;
		pickup.SetTarget(_player);
		Callable.From(() => _pickupContainer.AddChild(pickup)).CallDeferred();
	}

	private void OnPickupCollected(int amount)
	{
		AddGold(amount);
	}

	public void AddGold(int amount)
	{
		_stats.Gold += amount;
		Hope.EventBus.Instance?.EmitGoldChanged(_stats.Gold);
	}

	private void OnPlayerDied()
	{
		_waveManager.Stop();
		_enemySpawner.Stop();
		SetPhase(RunPhase.GameOver);
		GameManager.Instance?.ChangeState(GameState.GameOver);
	}

	private void SetPhase(RunPhase phase)
	{
		_phase = phase;
		Hope.EventBus.Instance?.EmitRunPhaseChanged((int)phase);
		SyncRunPhasePause();
	}

	/// <summary>
	/// 波间商店等阶段需冻结世界（<c>GameWorld</c> 为 Pausable），但不切换 <see cref="GameState.Paused"/> 以免弹出暂停菜单。
	/// </summary>
	private void SyncRunPhasePause()
	{
		if (_phase == RunPhase.Shop || _phase == RunPhase.GameOver)
		{
			GetTree().Paused = true;
			return;
		}

		if (GameManager.Instance?.State == GameState.Playing)
		{
			GetTree().Paused = false;
		}
	}

	private void SyncPlayerStats(bool refillHealth)
	{
		_player?.ApplyStats(_stats, refillHealth);
	}
}
