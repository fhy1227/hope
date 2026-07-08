namespace Hope.Core;

/// <summary>
/// 单局结算快照，由 <see cref="RunSessionData"/> 携带至结算场景。
/// </summary>
public class SettlementResult
{
    /// <summary>是否通关（击败 Boss）。</summary>
    public bool IsVictory { get; set; }

    /// <summary>到达的最高波次。</summary>
    public int WaveReached { get; set; }

    /// <summary>本局击杀敌人数。</summary>
    public int EnemiesKilled { get; set; }

    /// <summary>本局获得金币（局内统计）。</summary>
    public int GoldEarned { get; set; }

    /// <summary>死亡时扣除的金币。</summary>
    public int GoldLost { get; set; }

    /// <summary>本局获得经验（局内统计）。</summary>
    public int ExpEarned { get; set; }

    /// <summary>结算后是否升级。</summary>
    public bool IsLevelUp { get; set; }

    /// <summary>升级后的等级（仅 <see cref="IsLevelUp"/> 为 true 时有效）。</summary>
    public int NewLevel { get; set; }

    /// <summary>副本配置 Id；0 表示非副本模式。</summary>
    public int DungeonId { get; set; }

    /// <summary>副本显示名称。</summary>
    public string DungeonName { get; set; } = "";
}
