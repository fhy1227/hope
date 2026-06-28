using Godot;
using Hope.Core;
using Hope.Entities;

namespace Hope.Components.Actions;

/// <summary>
/// 玩家战斗行为调度器：管理翻滚、聚气、震地、格挡的输入优先级、激活态与对 Player 的查询属性。
/// 挂载于 <c>player.tscn</c>，由 <see cref="Player"/> 在 <c>_PhysicsProcess</c> 中优先调用。
/// 不负责武器自动攻击（见 WeaponSlot）。
/// </summary>
public partial class PlayerActionController : Node
{
    private readonly RollAction _roll = new();
    private readonly ChargeAction _charge = new();
    private readonly StompAction _stomp = new();
    private readonly ParryAction _parry = new();

    private IPlayerAction? _active;
    private Player _player = null!;
    private Vector2 _lastMoveDirection = Vector2.Down;

    /// <summary>当前激活行为是否锁定移动状态机；无激活行为时为 false。</summary>
    public bool BlocksMovement => _active?.BlocksMovement ?? false;

    /// <summary>当前激活行为是否禁止开启其他行为；无激活行为时为 false。</summary>
    public bool BlocksOtherActions => _active?.BlocksOtherActions ?? false;

    /// <summary>当前激活行为是否授予无敌；无激活行为时为 false。</summary>
    public bool GrantsInvincibility => _active?.GrantsInvincibility ?? false;

    /// <summary>当前激活行为的移动速度倍率；无激活行为时为 1。</summary>
    public float MoveSpeedMultiplier => _active?.MoveSpeedMultiplier ?? 1f;

    /// <summary>格挡窗口是否仍开放（供 Player 接触伤害路径查询）。</summary>
    public bool IsParrying => _parry.IsParryWindowOpen;

    /// <summary>
    /// 绑定玩家引用；须在首次 <see cref="UpdateActions"/> 前由 Player 调用。
    /// </summary>
    /// <param name="player">本 Controller 所属的玩家节点。</param>
    public void Bind(Player player)
    {
        _player = player;
    }

    /// <summary>
    /// 记录最近一次有效移动方向，供无输入时的翻滚/默认朝向使用。
    /// </summary>
    /// <param name="direction">移动方向；长度过小则忽略不更新。</param>
    public void SetLastMoveDirection(Vector2 direction)
    {
        if (direction.LengthSquared() > 0.01f)
        {
            _lastMoveDirection = direction.Normalized();
        }
    }

    /// <summary>
    /// 每物理帧调用：tick 各行为冷却、更新激活行为、处理聚气键释放与输入启动。
    /// </summary>
    /// <param name="delta">物理帧间隔（秒）。</param>
    public void UpdateActions(double delta)
    {
        var ctx = BuildContext();

        _roll.TickInactive(delta);
        _charge.TickInactive(delta);
        _stomp.TickInactive(delta);
        _parry.TickInactive(delta);

        if (_active != null)
        {
            _active.Update(ctx, delta);

            if (_active.Id == PlayerActionId.Charge && Input.IsActionJustReleased("charge"))
            {
                _charge.OnInputReleased(ctx);
            }

            if (!_active.IsActive)
            {
                _active = null;
            }

            return;
        }

        TryStartFromInput(ctx);
    }

    /// <summary>
    /// 接触伤害路径调用：若格挡窗口开放则尝试反制。
    /// </summary>
    /// <param name="source">造成伤害的敌人；可为 null（仍可能触发范围反伤）。</param>
    /// <param name="perfect">输出：是否处于完美格挡帧内。</param>
    /// <returns>是否成功格挡（免伤）。</returns>
    public bool TryParry(Enemy? source, out bool perfect)
    {
        perfect = false;
        if (!_parry.IsParryWindowOpen)
        {
            return false;
        }

        return _parry.TryResolveParry(BuildContext(), source, out perfect);
    }

    /// <summary>
    /// 玩家受击时由 Player 调用；聚气前摇/蓄力中会被打断并取消。
    /// </summary>
    public void OnPlayerHit()
    {
        if (_active?.Id == PlayerActionId.Charge)
        {
            _charge.OnInterrupted(BuildContext());
            _active = null;
        }
    }

    /// <summary>行为进入时广播 EventBus.PlayerActionStarted，供 HUD 等订阅。</summary>
    internal void NotifyActionStarted(PlayerActionId id)
    {
        EventBus.Instance?.EmitPlayerActionStarted((int)id);
    }

    /// <summary>行为结束时广播 EventBus.PlayerActionEnded。</summary>
    internal void NotifyActionEnded(PlayerActionId id)
    {
        EventBus.Instance?.EmitPlayerActionEnded((int)id);
    }

    /// <summary>组装本帧 <see cref="PlayerActionContext"/>，含移动输入与缓存朝向。</summary>
    private PlayerActionContext BuildContext()
    {
        var input = Input.GetVector("move_left", "move_right", "move_up", "move_down");
        return new PlayerActionContext
        {
            Player = _player,
            Controller = this,
            InputDirection = input,
            LastMoveDirection = _lastMoveDirection,
        };
    }

    /// <summary>
    /// 无激活行为时按优先级尝试启动：格挡 → 翻滚 → 震地 → 聚气（均为 JustPressed）。
    /// </summary>
    private void TryStartFromInput(PlayerActionContext ctx)
    {
        if (Input.IsActionJustPressed("parry") && TryStart(_parry, ctx))
        {
            return;
        }

        if (Input.IsActionJustPressed("roll") && TryStart(_roll, ctx))
        {
            return;
        }

        if (Input.IsActionJustPressed("stomp") && TryStart(_stomp, ctx))
        {
            return;
        }

        if (Input.IsActionJustPressed("charge") && TryStart(_charge, ctx))
        {
            return;
        }
    }

    /// <summary>通过 CanStart 校验后 Enter 并设为当前激活行为。</summary>
    /// <returns>是否成功启动。</returns>
    private bool TryStart(IPlayerAction action, PlayerActionContext ctx)
    {
        if (!action.CanStart(ctx))
        {
            return false;
        }

        action.Enter(ctx);
        _active = action;
        return true;
    }
}
