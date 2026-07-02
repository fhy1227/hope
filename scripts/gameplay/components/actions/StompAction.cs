using Godot;
using Hope.Config;
using Hope.Core;

namespace Hope.Components.Actions;

/// <summary>
/// 震地行为（输入 Q）：短前摇后瞬发脚下 AOE 与击退，随后极短恢复帧，CD 3s。
/// 前摇与恢复期间锁定移动，无无敌。伤害倍率 0.6，半径 80px。
/// </summary>
public sealed class StompAction : IPlayerAction
{
    /// <summary>内部阶段：空闲 → 前摇 → 恢复 → 冷却。</summary>
    private enum Phase { Idle, WindUp, Recovery, Cooldown }

    private Phase _phase = Phase.Idle;
    private float _timer;
    private float _cooldown;

    /// <summary>防止前摇跨帧重复结算伤害。</summary>
    private bool _hitApplied;

    /// <inheritdoc />
    public PlayerActionId Id => PlayerActionId.Stomp;

    /// <inheritdoc />
    public bool IsActive => _phase is Phase.WindUp or Phase.Recovery;

    /// <inheritdoc />
    public float CooldownRemaining => _cooldown;

    /// <inheritdoc />
    public bool BlocksMovement => IsActive;

    /// <inheritdoc />
    public bool BlocksOtherActions => IsActive;

    /// <inheritdoc />
    public bool GrantsInvincibility => false;

    /// <inheritdoc />
    public float MoveSpeedMultiplier => 0f;

    /// <inheritdoc />
    public bool CanStart(PlayerActionContext ctx) =>
        _phase == Phase.Idle && _cooldown <= 0f;

    /// <inheritdoc />
    public void Enter(PlayerActionContext ctx)
    {
        _phase = Phase.WindUp;
        _timer = ParamsConfig.StompWindupTime;
        _hitApplied = false;
        ctx.Player.Velocity = Vector2.Zero;
        ctx.Player.SetActionVisual(ParamsConfig.ColorStompEnter, ParamsConfig.StompVisualScale);
        ctx.Controller.NotifyActionStarted(Id);
    }

    /// <inheritdoc />
    public void Update(PlayerActionContext ctx, double delta)
    {
        if (_phase == Phase.WindUp)
        {
            _timer -= (float)delta;
            if (!_hitApplied && _timer <= 0f)
            {
                ApplyStomp(ctx);
                _phase = Phase.Recovery;
                _timer = ParamsConfig.StompRecoveryTime;
            }

            return;
        }

        if (_phase == Phase.Recovery)
        {
            _timer -= (float)delta;
            if (_timer <= 0f)
            {
                Finish(ctx);
            }
        }
    }

    /// <inheritdoc />
    public void Exit(PlayerActionContext ctx)
    {
        if (_phase is Phase.WindUp or Phase.Recovery)
        {
            Finish(ctx);
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

    /// <summary>前摇结束时执行一次范围伤害、闪光与视觉反馈。</summary>
    private void ApplyStomp(PlayerActionContext ctx)
    {
        _hitApplied = true;
        CombatPulse.HitCount(
            ctx.Player,
            ParamsConfig.StompRadius,
            ctx.GetDamage(ParamsConfig.StompDamageMul),
            ParamsConfig.StompKnockbackSpeed);
        ctx.Player.FlashActionRelease(ParamsConfig.ColorStompRelease);
        ctx.Player.SetActionVisual(Colors.White, ParamsConfig.StompReleaseVisualScale);
    }

    /// <summary>恢复结束：重置视觉与速度，进入冷却。</summary>
    private void Finish(PlayerActionContext ctx)
    {
        ctx.Player.ResetActionVisual();
        ctx.Player.Velocity = Vector2.Zero;
        _phase = Phase.Cooldown;
        _cooldown = ParamsConfig.StompCooldown;
        ctx.Controller.NotifyActionEnded(Id);
    }
}
