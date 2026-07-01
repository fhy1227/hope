using Godot;
using Hope.Config;
using Hope.Core;
using Hope.Entities;

namespace Hope.Components.Actions;

/// <summary>
/// 格挡行为（输入右键）：开启 0.25s 判定窗口，窗口内受击则免伤并范围反制；
/// 前 0.08s 为完美格挡（更短 CD、金色闪光）。可移动但减速至 30%。
/// 成功 CD 0.8s，失败 1.5s，完美格挡无 CD。
/// </summary>
public sealed class ParryAction : IPlayerAction
{
    /// <summary>内部阶段：空闲 → 窗口激活 → 冷却。</summary>
    private enum Phase { Idle, Active, Cooldown }

    private Phase _phase = Phase.Idle;
    private float _timer;
    private float _cooldown;

    /// <summary>本窗口是否已结算（成功或超时失败），防止重复判定。</summary>
    private bool _resolved;

    /// <inheritdoc />
    public PlayerActionId Id => PlayerActionId.Parry;

    /// <inheritdoc />
    public bool IsActive => _phase == Phase.Active;

    /// <inheritdoc />
    public float CooldownRemaining => _cooldown;

    /// <inheritdoc />
    public bool BlocksMovement => false;

    /// <inheritdoc />
    public bool BlocksOtherActions => IsActive;

    /// <inheritdoc />
    public bool GrantsInvincibility => false;

    /// <inheritdoc />
    public float MoveSpeedMultiplier => IsActive ? ParamsConfig.ParryMoveSpeedMul : 1f;

    /// <summary>
    /// 格挡窗口是否仍开放且未结算；Player 接触伤害与 Controller.TryParry 据此判断。
    /// </summary>
    public bool IsParryWindowOpen => _phase == Phase.Active && !_resolved;

    /// <inheritdoc />
    public bool CanStart(PlayerActionContext ctx) =>
        _phase == Phase.Idle && _cooldown <= 0f;

    /// <inheritdoc />
    public void Enter(PlayerActionContext ctx)
    {
        _phase = Phase.Active;
        _timer = ParamsConfig.ParryWindowDuration;
        _resolved = false;
        ctx.Player.SetActionVisual(ParamsConfig.ColorParryEnter);
        ctx.Controller.NotifyActionStarted(Id);
    }

    /// <inheritdoc />
    public void Update(PlayerActionContext ctx, double delta)
    {
        if (_phase != Phase.Active)
        {
            return;
        }

        _timer -= (float)delta;
        if (_timer <= 0f && !_resolved)
        {
            Finish(ctx, succeeded: false);
        }
    }

    /// <inheritdoc />
    public void Exit(PlayerActionContext ctx)
    {
        if (_phase == Phase.Active && !_resolved)
        {
            Finish(ctx, succeeded: false);
        }
    }

    /// <inheritdoc />
    public void TickInactive(double delta)
    {
        if (_phase != Phase.Cooldown)
        {
            return;
        }

        _cooldown -= (float)delta;
        if (_cooldown <= 0f)
        {
            _cooldown = 0f;
            _phase = Phase.Idle;
        }
    }

    /// <summary>
    /// 在窗口内收到敌人接触伤害时调用：眩晕来源敌人、范围反伤，并按是否完美格挡结算 CD。
    /// </summary>
    /// <param name="ctx">当前帧上下文。</param>
    /// <param name="source">接触伤害的敌人；有效时施加眩晕。</param>
    /// <param name="perfect">输出：剩余窗口时间是否在完美帧内（窗口末尾 PerfectWindow 秒）。</param>
    /// <returns>始终为 true（调用方已确认窗口开放）。</returns>
    public bool TryResolveParry(PlayerActionContext ctx, Enemy? source, out bool perfect)
    {
        perfect = false;
        if (!IsParryWindowOpen)
        {
            return false;
        }

        perfect = _timer >= ParamsConfig.ParryWindowDuration - ParamsConfig.ParryPerfectWindow;
        _resolved = true;

        if (source != null && GodotObject.IsInstanceValid(source))
        {
            source.ApplyStun(ParamsConfig.ParryStunDuration);
        }

        CombatPulse.HitCount(
            ctx.Player,
            ParamsConfig.ParryCounterRadius,
            ctx.GetDamage(ParamsConfig.ParryCounterDamageMul),
            ParamsConfig.ParryCounterKnockback);
        ctx.Player.FlashActionRelease(perfect ? ParamsConfig.ColorParryPerfect : ParamsConfig.ColorParryNormal);
        Finish(ctx, succeeded: true, perfect: perfect);
        return true;
    }

    /// <summary>
    /// 结束格挡窗口：重置视觉，按成功/失败/完美设置冷却并通知 Controller。
    /// </summary>
    /// <param name="succeeded">是否在窗口内成功格挡到伤害。</param>
    /// <param name="perfect">成功时是否为完美格挡（仅 succeeded 为 true 时有效）。</param>
    private void Finish(PlayerActionContext ctx, bool succeeded, bool perfect = false)
    {
        ctx.Player.ResetActionVisual();
        _phase = Phase.Cooldown;
        _cooldown = succeeded
            ? (perfect ? 0f : ParamsConfig.ParrySuccessCooldown)
            : ParamsConfig.ParryFailCooldown;
        _resolved = true;
        ctx.Controller.NotifyActionEnded(Id);
    }
}
