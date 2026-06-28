namespace Hope.Core;

/// <summary>
/// 玩家主动战斗行为标识，与 <c>Hope.Components.Actions</c> 下各 Action 一一对应。
/// 经 EventBus 以 int 广播，供 HUD 显示 CD 或图标。
/// </summary>
public enum PlayerActionId
{
    /// <summary>翻滚：定向位移 + 无敌（Space）。</summary>
    Roll,

    /// <summary>聚气：按住蓄力后范围爆发（R）。</summary>
    Charge,

    /// <summary>震地：瞬发脚下 AOE（Q）。</summary>
    Stomp,

    /// <summary>格挡：短时窗口免伤反制（右键）。</summary>
    Parry,
}
