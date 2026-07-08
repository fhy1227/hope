using Hope.Config;

namespace Hope.Core;

/// <summary>
/// 跨场景传递的单局战斗统计；在切换至结算场景前由 <see cref="Hope.Systems.RunManager"/> 写入。
/// </summary>
public static class RunSessionData
{
    /// <summary>是否处于副本模式。</summary>
    public static bool IsDungeonRun { get; set; }

    /// <summary>是否通关。</summary>
    public static bool IsVictory { get; set; }

    /// <summary>到达波次。</summary>
    public static int WaveReached { get; set; }

    /// <summary>击杀数。</summary>
    public static int EnemiesKilled { get; set; }

    /// <summary>本局赚取金币。</summary>
    public static int RunGoldEarned { get; set; }

    /// <summary>本局赚取经验。</summary>
    public static int RunExpEarned { get; set; }

    /// <summary>进本前已有金币（用于死亡惩罚计算）。</summary>
    public static int GoldBeforeRun { get; set; }

    /// <summary>当前副本配置。</summary>
    public static DungeonConfig? Dungeon { get; set; }

    /// <summary>写入本局结果并标记为副本运行。</summary>
    public static void Capture(
        bool isVictory,
        int waveReached,
        int enemiesKilled,
        int runGoldEarned,
        int runExpEarned,
        int goldBeforeRun,
        DungeonConfig? dungeon)
    {
        IsDungeonRun = dungeon != null;
        IsVictory = isVictory;
        WaveReached = waveReached;
        EnemiesKilled = enemiesKilled;
        RunGoldEarned = runGoldEarned;
        RunExpEarned = runExpEarned;
        GoldBeforeRun = goldBeforeRun;
        Dungeon = dungeon;
    }

    /// <summary>离开结算或开新局时清空。</summary>
    public static void Clear()
    {
        IsDungeonRun = false;
        IsVictory = false;
        WaveReached = 0;
        EnemiesKilled = 0;
        RunGoldEarned = 0;
        RunExpEarned = 0;
        GoldBeforeRun = 0;
        Dungeon = null;
    }
}
