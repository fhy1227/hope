using Godot;

namespace Hope;

/// <summary>
/// 全局事件总线，用于模块间解耦通信。
/// </summary>
public partial class EventBus : Node
{
    [Signal]
    public delegate void GameStateChangedEventHandler(int newState);

    [Signal]
    public delegate void PlayerDiedEventHandler();

    [Signal]
    public delegate void HealthChangedEventHandler(int current, int max);

    [Signal]
    public delegate void WaveStartedEventHandler(int wave, float duration);

    [Signal]
    public delegate void WaveTimerChangedEventHandler(float remaining);

    [Signal]
    public delegate void WaveEndedEventHandler(int wave);

    [Signal]
    public delegate void GoldChangedEventHandler(int gold);

    [Signal]
    public delegate void RunPhaseChangedEventHandler(int phase);

    [Signal]
    public delegate void CombatStateChangedEventHandler(int state);

    [Signal]
    public delegate void PlayerActionStartedEventHandler(int actionId);

    [Signal]
    public delegate void PlayerActionEndedEventHandler(int actionId);

    [Signal]
    public delegate void DamageTakenEventHandler(int amount, Vector2 worldPosition, bool isPlayer);

    [Signal]
    public delegate void LevelChangedEventHandler(string levelPath);

    [Signal]
    public delegate void DungeonCompletedEventHandler();

    [Signal]
    public delegate void BossDefeatedEventHandler();

    [Signal]
    public delegate void FateCardSelectedEventHandler(int cardId, string cardCode);

    [Signal]
    public delegate void FateChainActivatedEventHandler(int chainId, string chainName);

    [Signal]
    public delegate void SkillCastEventHandler(string skillId, int rank, Vector2 position);

    [Signal]
    public delegate void SkillHitEventHandler(string skillId, Node2D target, int damage);

    [Signal]
    public delegate void SkillEndedEventHandler(string skillId);

    [Signal]
    public delegate void SkillLearnedEventHandler(string skillId, int newRank);

    [Signal]
    public delegate void SkillRefundedEventHandler(string skillId);

    [Signal]
    public delegate void SkillEnhancementChosenEventHandler(string skillId, string enhancementId);

    [Signal]
    public delegate void SkillTreeResetEventHandler();

    [Signal]
    public delegate void SkillCooldownStartedEventHandler(string skillId, float duration);

    [Signal]
    public delegate void SkillCooldownEndedEventHandler(string skillId);

    [Signal]
    public delegate void SkillResourceChangedEventHandler(float current, float max);

    public static EventBus? Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void EmitGameStateChanged(int newState)
    {
        EmitSignal(SignalName.GameStateChanged, newState);
    }

    public void EmitPlayerDied()
    {
        EmitSignal(SignalName.PlayerDied);
    }

    public void EmitHealthChanged(int current, int max)
    {
        EmitSignal(SignalName.HealthChanged, current, max);
    }

    public void EmitWaveStarted(int wave, float duration)
    {
        EmitSignal(SignalName.WaveStarted, wave, duration);
    }

    public void EmitWaveTimerChanged(float remaining)
    {
        EmitSignal(SignalName.WaveTimerChanged, remaining);
    }

    public void EmitWaveEnded(int wave)
    {
        EmitSignal(SignalName.WaveEnded, wave);
    }

    public void EmitGoldChanged(int gold)
    {
        EmitSignal(SignalName.GoldChanged, gold);
    }

    public void EmitRunPhaseChanged(int phase)
    {
        EmitSignal(SignalName.RunPhaseChanged, phase);
    }

    public void EmitCombatStateChanged(int state)
    {
        EmitSignal(SignalName.CombatStateChanged, state);
    }

    public void EmitPlayerActionStarted(int actionId)
    {
        EmitSignal(SignalName.PlayerActionStarted, actionId);
    }

    public void EmitPlayerActionEnded(int actionId)
    {
        EmitSignal(SignalName.PlayerActionEnded, actionId);
    }

    public void EmitDamageTaken(int amount, Vector2 worldPosition, bool isPlayer)
    {
        EmitSignal(SignalName.DamageTaken, amount, worldPosition, isPlayer);
    }

    public void EmitLevelChanged(string levelPath)
    {
        EmitSignal(SignalName.LevelChanged, levelPath);
    }

    public void EmitDungeonCompleted()
    {
        EmitSignal(SignalName.DungeonCompleted);
    }

    public void EmitBossDefeated()
    {
        EmitSignal(SignalName.BossDefeated);
    }

    /// <summary>广播命运卡牌被选择事件。</summary>
    /// <param name="cardId">卡牌配置主键。</param>
    /// <param name="cardCode">卡牌业务 ID（如 C01）。</param>
    public void EmitFateCardSelected(int cardId, string cardCode)
    {
        EmitSignal(SignalName.FateCardSelected, cardId, cardCode);
    }

    /// <summary>广播命运连锁激活事件。</summary>
    /// <param name="chainId">连锁配置主键。</param>
    /// <param name="chainName">连锁显示名。</param>
    public void EmitFateChainActivated(int chainId, string chainName)
    {
        EmitSignal(SignalName.FateChainActivated, chainId, chainName);
    }

    public void EmitSkillCast(string skillId, int rank, Vector2 position)
    {
        EmitSignal(SignalName.SkillCast, skillId, rank, position);
    }

    public void EmitSkillHit(string skillId, Node2D target, int damage)
    {
        EmitSignal(SignalName.SkillHit, skillId, target, damage);
    }

    public void EmitSkillEnded(string skillId)
    {
        EmitSignal(SignalName.SkillEnded, skillId);
    }

    public void EmitSkillLearned(string skillId, int newRank)
    {
        EmitSignal(SignalName.SkillLearned, skillId, newRank);
    }

    public void EmitSkillRefunded(string skillId)
    {
        EmitSignal(SignalName.SkillRefunded, skillId);
    }

    public void EmitSkillEnhancementChosen(string skillId, string enhancementId)
    {
        EmitSignal(SignalName.SkillEnhancementChosen, skillId, enhancementId);
    }

    public void EmitSkillTreeReset()
    {
        EmitSignal(SignalName.SkillTreeReset);
    }

    public void EmitSkillCooldownStarted(string skillId, float duration)
    {
        EmitSignal(SignalName.SkillCooldownStarted, skillId, duration);
    }

    public void EmitSkillCooldownEnded(string skillId)
    {
        EmitSignal(SignalName.SkillCooldownEnded, skillId);
    }

    public void EmitSkillResourceChanged(float current, float max)
    {
        EmitSignal(SignalName.SkillResourceChanged, current, max);
    }
}
