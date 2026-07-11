using Godot;
using Hope.Config;
using Hope.Core;
using Hope.Entities;
using Hope.Levels;
using Hope.Persistence;
using Hope.SkillSystem;

namespace Hope.Systems;

/// <summary>
/// 土豆兄弟式单局循环；副本模式下为有限波次 + Boss + 结算。
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
	private FateCardManager? _fateCardManager;
	private Node2D _playerContainer;
	private Node2D _pickupContainer;
	private Player _player;
	private RunStats _stats = new();
	private RunPhase _phase = RunPhase.Combat;
	private CombatState _combatState = CombatState.Playing;

	private DungeonConfig? _dungeon;
	private bool _isDungeonMode;
	private bool _isBossWaveActive;
	private int _goldBeforeRun;
	private int _runGoldEarned;
	private int _runExpEarned;
	private int _totalKills;
	private float _goldMultiplier = 1f;
	private float _expMultiplier = 1f;

	/// <summary>本局是否通关（仅副本模式有意义）。</summary>
	public bool IsVictory { get; private set; }

	/// <summary>本局击杀总数。</summary>
	public int TotalKills => _totalKills;

	/// <summary>本局赚取金币（不含进本前已有）。</summary>
	public int RunGoldEarned => _runGoldEarned;

	/// <summary>本局赚取经验。</summary>
	public int RunExpEarned => _runExpEarned;

	public RunStats Stats => _stats;
	public RunPhase Phase => _phase;
	/// <summary>当前战斗状态（进行中/暂停/结束）。</summary>
	public CombatState State => _combatState;
	public Player? Player => IsInstanceValid(_player) ? _player : null;

	/// <summary>当前活动关卡；无关卡时为 null。</summary>
	public BaseLevel? CurrentLevel => _levelManager?.Current;

	public override void _Ready()
	{
		GameManager.Instance?.ChangeState(GameState.Combat);

		AddToGroup("run_manager");

		_waveManager = GetNode<WaveManager>("WaveManager");
		_enemySpawner = GetNode<EnemySpawner>("EnemySpawner");
		_levelManager = GetNode<LevelManager>(LevelManagerPath);
		_fateCardManager = GetNodeOrNull<FateCardManager>("FateCardManager");
		_playerContainer = GetNode<Node2D>(PlayerContainerPath);
		_pickupContainer = GetNode<Node2D>(PickupContainerPath);

		_waveManager.WaveCompleted += OnWaveCompleted;
		_enemySpawner.EnemyDefeated += OnEnemyDefeated;
		Hope.EventBus.Instance!.PlayerDied += OnPlayerDied;

		_dungeon = DungeonManager.Instance?.CurrentDungeon;
		_isDungeonMode = _dungeon != null;

		if (_isDungeonMode)
		{
			RunSessionData.IsDungeonRun = true;
			ApplyDungeonSetup();
		}
		else
		{
			InitStatsFromSave();
		}

		SetCombatState(CombatState.Playing);
		SpawnPlayer();
		StartNextWave();
	}

	public override void _ExitTree()
	{
		if (Hope.EventBus.Instance != null)
		{
			Hope.EventBus.Instance.PlayerDied -= OnPlayerDied;
		}
	}

	private void ApplyDungeonSetup()
	{
		if (_dungeon == null)
		{
			return;
		}

		_goldMultiplier = _dungeon.GoldMultiplier;
		_expMultiplier = _dungeon.ExpMultiplier;

		_waveManager.Initialize(_dungeon);
		_enemySpawner.ApplyDungeonSettings(_dungeon);

		if (!string.IsNullOrEmpty(_dungeon.ScenePath))
		{
			_levelManager?.LoadLevel(_dungeon.ScenePath);
		}

		InitStatsFromSave();
	}

	private void InitStatsFromSave()
	{
		var save = PersistenceMgr.Instance?.ActiveCharacter;
		if (save == null)
		{
			return;
		}

		_goldBeforeRun = save.Gold;
		_stats.MaxHealth = save.BaseMaxHealth;
		_stats.Damage = save.BaseDamage;
		_stats.Speed = save.BaseSpeed;
		_stats.Gold = save.Gold;
		Hope.EventBus.Instance?.EmitGoldChanged(_stats.Gold);
	}

	/// <summary>Esc 切换战斗暂停状态；仅在进行中与暂停间切换。</summary>
	public void TogglePauseByEsc()
	{
		if (_combatState == CombatState.Playing)
		{
			PauseCombat();
			return;
		}

		if (_combatState == CombatState.Paused)
			ResumeCombat();
	}

	/// <summary>将战斗置为暂停；非进行中状态下静默忽略。</summary>
	public void PauseCombat()
	{
		if (_combatState != CombatState.Playing)
			return;

		SetCombatState(CombatState.Paused);
	}

	/// <summary>从暂停恢复战斗；仅暂停态可恢复。</summary>
	public void ResumeCombat()
	{
		if (_combatState != CombatState.Paused)
			return;

		SetCombatState(CombatState.Playing);
	}

	public IReadOnlyList<ShopUpgrade> RollShopOptions(int count = -1)
	{
		if (count < 0)
			count = (int)ParamsConfig.ShopOptionCount;
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
	/// 获取当前命运织机三选一候选卡牌（供 UI 展示）。
	/// </summary>
	/// <returns>卡牌字典列表；每项包含 id/card_code/name/desc/rarity。</returns>
	public Godot.Collections.Array<Godot.Collections.Dictionary> GetFateCardOptions()
	{
		var result = new Godot.Collections.Array<Godot.Collections.Dictionary>();
		if (_fateCardManager == null || !_fateCardManager.IsEnabled)
		{
			return result;
		}

		foreach (var card in _fateCardManager.DrawCards(_stats.Wave))
		{
			result.Add(new Godot.Collections.Dictionary
			{
				["id"] = card.Id,
				["card_code"] = card.CardCode,
				["name"] = card.Name,
				["desc"] = card.Desc,
				["rarity"] = card.Rarity,
			});
		}

		return result;
	}

	/// <summary>
	/// 在命运织机阶段选择卡牌并进入商店。
	/// </summary>
	/// <param name="cardId">被选择的卡牌配置 Id。</param>
	public void SelectFateCardAndEnterShop(int cardId)
	{
		if (_fateCardManager?.SelectCard(cardId, _stats) == true)
		{
			SyncPlayerStats(refillHealth: false);
		}

		SetPhase(RunPhase.Shop);
	}

	/// <summary>
	/// 当命运织机不可用时跳过该阶段并直接进入商店。
	/// </summary>
	public void SkipFateCardAndEnterShop()
	{
		SetPhase(RunPhase.Shop);
	}

	/// <summary>获取本局已持有命运卡牌数量。</summary>
	public int GetFateOwnedCount() => _fateCardManager?.OwnedCardIds.Count ?? 0;

	/// <summary>获取本局已激活连锁数量。</summary>
	public int GetFateChainCount() => _fateCardManager?.ActiveChainIds.Count ?? 0;

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

	public Godot.Collections.Array<Godot.Collections.Dictionary> GetShopOptions(int count = -1)
	{
		if (count < 0)
			count = (int)ParamsConfig.ShopOptionCount;
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
		SkillCastingSystem.Instance?.BindPlayer(_player);
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
		_stats.Wave += 1;

		if (_isDungeonMode && _dungeon != null && _stats.Wave >= _dungeon.BossWave)
		{
			StartBossWave();
			return;
		}

		SetPhase(RunPhase.Combat);
		_isBossWaveActive = false;
		_waveManager.StartWave(_stats.Wave);
		_enemySpawner.BeginWave(_stats.Wave);
	}

	private void StartBossWave()
	{
		SetPhase(RunPhase.Combat);
		_isBossWaveActive = true;
		_waveManager.StartBossWave(_stats.Wave);
		_enemySpawner.SpawnBoss(_dungeon!.BossConfigId, _dungeon.BaseEnemyLevel);
	}

	private void OnWaveCompleted(int wave)
	{
		_enemySpawner?.Stop();

		if (_isDungeonMode && _dungeon != null && wave >= _dungeon.TotalWaves)
		{
			return;
		}

		if (_fateCardManager != null && _fateCardManager.IsEnabled)
		{
			SetPhase(RunPhase.FateCard);
			return;
		}

		SetPhase(RunPhase.Shop);
	}

	private void OnEnemyDefeated(int gold, Vector2 position, string enemyType)
	{
		_totalKills += 1;

		var scaledGold = Mathf.RoundToInt(gold * _goldMultiplier);
		var fateGoldMult = _fateCardManager?.GetEffectValue("gold_gain_mult") ?? 0f;
		if (fateGoldMult > 0f)
		{
			scaledGold = Mathf.RoundToInt(scaledGold * (1f + fateGoldMult / 100f));
		}
		if (_isDungeonMode)
		{
			var enemyLevel = _dungeon?.BaseEnemyLevel ?? 1;
			_runExpEarned += ExpSystem.CalculateKillExp(enemyType, enemyLevel, _expMultiplier);
		}

		if (enemyType == ParamsConfig.EnemyTypeBoss && _isBossWaveActive)
		{
			OnBossDefeated();
		}

		if (PickupScene == null)
		{
			AddGold(scaledGold);
			ApplyDropTable(enemyType, position);
			return;
		}

		SpawnGoldPickup(scaledGold, position);
		ApplyDropTable(enemyType, position);
	}

	private void OnBossDefeated()
	{
		_waveManager?.Stop();
		_enemySpawner?.Stop();
		IsVictory = true;
		SetPhase(RunPhase.Victory);
		SetCombatState(CombatState.GameOver);
		Hope.EventBus.Instance?.EmitBossDefeated();
		Hope.EventBus.Instance?.EmitDungeonCompleted();
		ScheduleSettlementTransition();
	}

	private void SpawnGoldPickup(int gold, Vector2 position)
	{
		var pickup = PickupScene!.Instantiate<Pickup>();
		pickup.GlobalPosition = position;
		pickup.GoldAmount = gold;
		pickup.RefreshAppearance();
		pickup.SetTarget(_player);
		pickup.Collected += OnPickupCollected;
		Callable.From(() => _pickupContainer.AddChild(pickup)).CallDeferred();
	}

	private void ApplyDropTable(string enemyType, Vector2 position)
	{
		if (PickupScene == null)
			return;

		var wave = _waveManager?.CurrentWave ?? _stats.Wave;
		var lootBonus = _dungeon?.LootQualityBonus ?? 0;
		foreach (var drop in DropTableResolver.RollDrops(enemyType, wave, lootBonus))
			SpawnItemPickup(drop, position);
	}

	private void SpawnItemPickup(DropTableResolver.DropResult drop, Vector2 position)
	{
		var pickup = PickupScene!.Instantiate<Pickup>();
		pickup.GlobalPosition = position;
		pickup.ItemConfigId = drop.ItemId;
		pickup.ItemCount = drop.Count;
		pickup.DropInstance = drop.Instance;
		pickup.RefreshAppearance();
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
		_runGoldEarned += amount;
		Hope.EventBus.Instance?.EmitGoldChanged(_stats.Gold);
	}

	private void OnPlayerDied()
	{
		_waveManager.Stop();
		_enemySpawner.Stop();
		SetPhase(RunPhase.GameOver);
		SetCombatState(CombatState.GameOver);

		if (_isDungeonMode)
		{
			ScheduleSettlementTransition();
		}
	}

	private void ScheduleSettlementTransition()
	{
		GetTree().CreateTimer(1.5f).Timeout += GoToSettlement;
	}

	private void GoToSettlement()
	{
		RunSessionData.Capture(
			IsVictory,
			_waveManager?.CurrentWave ?? _stats.Wave,
			_totalKills,
			_runGoldEarned,
			_runExpEarned,
			_goldBeforeRun,
			_dungeon);

		GetTree().Paused = false;
		GameManager.Instance?.ChangeScene(ScenePaths.Settlement);
	}

	private void SetPhase(RunPhase phase)
	{
		_phase = phase;
		Hope.EventBus.Instance?.EmitRunPhaseChanged((int)phase);
		SyncWorldPause();
	}

	/// <summary>
	/// 统一根据战斗态与阶段冻结/恢复世界（<c>GameWorld</c> 为 Pausable）。
	/// </summary>
	private void SyncWorldPause()
	{
		var shouldPause =
			_combatState == CombatState.Paused
			|| _combatState == CombatState.GameOver
			|| _phase == RunPhase.FateCard
			|| _phase == RunPhase.Shop
			|| _phase == RunPhase.GameOver
			|| _phase == RunPhase.Victory;

		if (shouldPause)
		{
			GetTree().Paused = true;
			_waveManager?.SetPaused(true);
			_enemySpawner?.SetPaused(true);
			return;
		}

		if (GameManager.Instance?.State == GameState.Combat)
		{
			GetTree().Paused = false;
			_waveManager?.SetPaused(false);
			_enemySpawner?.SetPaused(false);
		}
	}

	private void SetCombatState(CombatState state)
	{
		_combatState = state;
		Hope.EventBus.Instance?.EmitCombatStateChanged((int)state);
		SyncWorldPause();
	}

	private void SyncPlayerStats(bool refillHealth)
	{
		_player?.ApplyStats(_stats, refillHealth);
	}
}
