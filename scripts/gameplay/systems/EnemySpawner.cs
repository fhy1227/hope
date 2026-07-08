using Godot;
using Hope.Components;
using Hope.Config;
using Hope.Entities;

namespace Hope.Systems;

/// <summary>
/// 在玩家周围环形刷怪，随波次提高密度；副本模式下支持 Boss 生成。
/// </summary>
public partial class EnemySpawner : Node
{
	[Export]
	public PackedScene? EnemyScene { get; set; }

	[Export]
	public NodePath EnemyContainerPath { get; set; } = new("../Enemies");

	[Export]
	public float SpawnRadiusMin { get; set; } = ParamsConfig.SpawnRadiusMin;

	[Export]
	public float SpawnRadiusMax { get; set; } = ParamsConfig.SpawnRadiusMax;

	[Export]
	public float BaseSpawnInterval { get; set; } = ParamsConfig.SpawnBaseInterval;

	[Export]
	public int MaxAliveEnemies { get; set; } = (int)ParamsConfig.SpawnMaxAlive;

	private Node2D _enemyContainer;
	private Node2D _player;
	private float _spawnTimer;
	private int _wave;
	private bool _active;
	private bool _gamePaused;
	private float _spawnIntervalMin = ParamsConfig.SpawnIntervalMin;

	public override void _Ready()
	{
		_enemyContainer = GetNode<Node2D>(EnemyContainerPath);
	}

	public void BindPlayer(Node2D player)
	{
		_player = player;
	}

	/// <summary>应用副本刷怪参数。</summary>
	public void ApplyDungeonSettings(DungeonConfig dungeon)
	{
		BaseSpawnInterval = dungeon.SpawnIntervalBase;
		_spawnIntervalMin = dungeon.SpawnIntervalMin;
		MaxAliveEnemies = dungeon.MaxEnemiesPerWave;
	}

	public void BeginWave(int wave)
	{
		_wave = wave;
		_active = true;
		_spawnTimer = ParamsConfig.SpawnBeginTimer;
	}

	public void Stop()
	{
		_active = false;
	}

	public void SetPaused(bool paused)
	{
		_gamePaused = paused;
	}

	/// <summary>生成 Boss 实体（单次，不持续刷怪）。</summary>
	public void SpawnBoss(int bossConfigId, int baseLevel)
	{
		_active = false;
		if (EnemyScene == null || _player == null)
		{
			return;
		}

		var enemy = EnemyScene.Instantiate<Enemy>();
		enemy.GlobalPosition = GetSpawnPosition();
		enemy.EnemyType = ParamsConfig.EnemyTypeBoss;
		enemy.GoldDrop = (int)(ParamsConfig.EnemyGoldDropDefault * 5f * (1 + baseLevel * 0.2f));
		enemy.EnemyLevel = baseLevel;
		enemy.SetTarget(_player);
		enemy.EnemyKilled += OnEnemyKilled;
		_enemyContainer.AddChild(enemy);

		var health = enemy.GetNode<HealthComponent>("HealthComponent");
		var bossHp = (int)(ParamsConfig.HealthDefaultMax * (5 + baseLevel * 2));
		health.SetMaxHealth(bossHp, refill: true);

		GD.Print($"[EnemySpawner] Boss spawned (config={bossConfigId}, hp={bossHp})");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_active || _gamePaused || EnemyScene == null || _player == null)
		{
			return;
		}

		_spawnTimer -= (float)delta;
		if (_spawnTimer > 0f)
		{
			return;
		}

		if (CountAliveEnemies() >= MaxAliveEnemies)
		{
			_spawnTimer = ParamsConfig.SpawnCapRetryTimer;
			return;
		}

		SpawnEnemy();
		_spawnTimer = GetSpawnInterval();
	}

	private float GetSpawnInterval()
	{
		return Mathf.Max(
			BaseSpawnInterval - _wave * ParamsConfig.SpawnIntervalWaveReduce,
			_spawnIntervalMin);
	}

	private int CountAliveEnemies()
	{
		return GetTree().GetNodesInGroup("enemies").Count;
	}

	private void SpawnEnemy()
	{
		var enemy = EnemyScene.Instantiate<Enemy>();
		enemy.GlobalPosition = GetSpawnPosition();
		enemy.SetTarget(_player);
		enemy.EnemyKilled += OnEnemyKilled;
		_enemyContainer.AddChild(enemy);
	}

	private Vector2 GetSpawnPosition()
	{
		var angle = (float)GD.RandRange(0d, Mathf.Tau);
		var radius = (float)GD.RandRange(SpawnRadiusMin, SpawnRadiusMax);
		var offset = Vector2.Right.Rotated(angle) * radius;
		return _player.GlobalPosition + offset;
	}

	private void OnEnemyKilled(int gold, Vector2 position, string enemyType)
	{
		EmitSignal(SignalName.EnemyDefeated, gold, position, enemyType);
	}

	[Signal]
	public delegate void EnemyDefeatedEventHandler(int gold, Vector2 position, string enemyType);
}
