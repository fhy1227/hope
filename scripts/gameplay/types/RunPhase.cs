namespace Hope.Core;

/// <summary>
/// 单局（Run）内的阶段划分，由 <see cref="Hope.Systems.RunManager"/> 驱动。
/// 决定当前是否刷怪、显示商店或结束界面。
/// </summary>
public enum RunPhase
{
    /// <summary>波次战斗：敌人生成、玩家可移动与战斗。</summary>
    Combat,

    /// <summary>波间商店：选择升级、购买后进入下一波。</summary>
    Shop,

    /// <summary>本局结束（胜利或失败），不再刷怪。</summary>
    GameOver,
}
