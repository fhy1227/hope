using Hope.Core;

namespace Hope.Components.Actions;

/// <summary>
/// 玩家主动战斗行为契约。实现类由 <see cref="PlayerActionController"/> 统一调度，
/// 与 <see cref="Hope.Entities.Player"/> 的移动状态机分离，互不塞进 StateMachine。
/// </summary>
public interface IPlayerAction
{
    /// <summary>行为唯一标识，对应 <see cref="PlayerActionId"/>，用于 EventBus 广播与 HUD。</summary>
    PlayerActionId Id { get; }

    /// <summary>是否处于进行中阶段（非 Idle / 非纯 Cooldown 等待）。Controller 据此决定是否清空 <c>_active</c>。</summary>
    bool IsActive { get; }

    /// <summary>剩余冷却秒数；为 0 表示可再次 <see cref="CanStart"/>。</summary>
    float CooldownRemaining { get; }

    /// <summary>
    /// 是否允许启动本行为。通常检查阶段、冷却，以及与其他行为的互斥。
    /// </summary>
    /// <param name="ctx">当前帧玩家输入与引用快照。</param>
    /// <returns>为 true 时 Controller 会调用 <see cref="Enter"/>。</returns>
    bool CanStart(PlayerActionContext ctx);

    /// <summary>
    /// 行为开始：设置阶段、视觉反馈，并应调用 <see cref="PlayerActionController.NotifyActionStarted"/>。
    /// </summary>
    /// <param name="ctx">当前帧上下文。</param>
    void Enter(PlayerActionContext ctx);

    /// <summary>
    /// 行为进行中每帧更新；仅在 <see cref="IsActive"/> 为 true 时由 Controller 调用。
    /// </summary>
    /// <param name="ctx">当前帧上下文。</param>
    /// <param name="delta">物理帧间隔（秒）。</param>
    void Update(PlayerActionContext ctx, double delta);

    /// <summary>
    /// 行为被强制结束或切换时清理：恢复碰撞、视觉，并应调用 <see cref="PlayerActionController.NotifyActionEnded"/>。
    /// </summary>
    /// <param name="ctx">当前帧上下文。</param>
    void Exit(PlayerActionContext ctx);

    /// <summary>
    /// 非激活状态下的每帧 tick，主要用于冷却倒计时；无论是否有 <c>_active</c> 都会调用。
    /// </summary>
    /// <param name="delta">物理帧间隔（秒）。</param>
    void TickInactive(double delta);

    /// <summary>
    /// 为 true 时 Player 跳过 idle/move 状态机，由本行为自行控制位移（如翻滚）。
    /// </summary>
    bool BlocksMovement { get; }

    /// <summary>
    /// 为 true 时进行中不可通过输入启动其他行为。
    /// </summary>
    bool BlocksOtherActions { get; }

    /// <summary>
    /// 为 true 时 Player 视为无敌，接触伤害会被忽略。
    /// </summary>
    bool GrantsInvincibility { get; }

    /// <summary>
    /// 移动速度倍率；1 为正常，小于 1 表示蓄力/格挡减速但仍可走 StateMachine 移动。
    /// </summary>
    float MoveSpeedMultiplier { get; }
}
