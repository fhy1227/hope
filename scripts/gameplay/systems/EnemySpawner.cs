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

	/// <summary>生成 Boss 实体（单次，不持续刷怪）；数值由 <see cref="BossConfig"/> 驱动。</summary>
	public void SpawnBoss(int bossConfigId, int baseLevel)
	{
		_active = false;
		if (EnemyScene == null || _player == null)
		{
			return;
		}

		var cfg = ConfigManager.Get<BossConfig>(bossConfigId);
		var hpMult = cfg?.HpMult ?? 5f;
		var damageMult = cfg?.DamageMult ?? 1f;
		var scale = cfg?.Scale ?? 1f;
		var goldMult = cfg?.GoldMult ?? 5f;
		var tint = cfg?.TintColor ?? "#FFFFFF";
		var bossName = cfg?.NameKey ?? $"Boss#{bossConfigId}";

		var enemy = EnemyScene.Instantiate<Enemy>();
		enemy.GlobalPosition = GetSpawnPosition();
		enemy.EnemyType = ParamsConfig.EnemyTypeBoss;
		enemy.GoldDrop = (int)(ParamsConfig.EnemyGoldDropDefault * goldMult * (1 + baseLevel * 0.2f));
		enemy.EnemyLevel = baseLevel;
		enemy.Scale = Vector2.One * scale;
		enemy.Modulate = ParseHexColor(tint);
		enemy.AddToGroup("bosses");
		enemy.SetTarget(_player);
		enemy.EnemyKilled += OnEnemyKilled;

		var stats = enemy.GetNode<EnemyStatsComponent>("EnemyStatsComponent");
		stats.ContactDamage = ParamsConfig.EnemyContactDamageDefault * damageMult;

		_enemyContainer.AddChild(enemy);

		var health = enemy.GetNode<HealthComponent>("HealthComponent");
		var bossHp = (int)(ParamsConfig.HealthDefaultMax * hpMult * (1 + baseLevel * 0.15f));
		health.SetMaxHealth(bossHp, refill: true);

		GD.Print($"[EnemySpawner] Boss spawned: {bossName} (config={bossConfigId}, hp={bossHp})");
	}

	private static Color ParseHexColor(string hex)
	{
		if (string.IsNullOrWhiteSpace(hex))
		{
			return Colors.White;
		}

		var s = hex.TrimStart('#');
		if (s.Length == 6 && Color.HtmlIsValid($"#{s}"))
		{
			return Color.FromHtml($"#{s}");
		}

		return Colors.White;
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

		var eliteChance = ParamsConfig.EliteSpawnChanceBase + _wave * ParamsConfig.EliteSpawnChancePerWave;
		var isElite = GD.Randf() < eliteChance;
		if (isElite)
		{
			enemy.EnemyType = ParamsConfig.EnemyTypeElite;
			enemy.GoldDrop = Mathf.RoundToInt(enemy.GoldDrop * ParamsConfig.EliteGoldMult);
			enemy.Scale = Vector2.One * ParamsConfig.EliteScale;
			enemy.Modulate = new Color(1.25f, 1.1f, 0.65f);
		}

		_enemyContainer.AddChild(enemy);

		if (isElite)
		{
			var health = enemy.GetNode<HealthComponent>("HealthComponent");
			var eliteHp = Mathf.Max(1, Mathf.RoundToInt(health.MaxHealth * ParamsConfig.EliteHpMult));
			health.SetMaxHealth(eliteHp, refill: true);
		}
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
