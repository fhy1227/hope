using Godot;
using Hope.Core;

namespace Hope.Components.Actions;

/// <summary>
/// 聚气行为（输入 R 按住）：前摇 → 蓄力（可慢速移动）→ 松键或满蓄释放范围爆发 → 冷却。
/// 蓄力不足 25% 松键视为取消；受击打断前摇/蓄力。释放后 CD 2s。
/// </summary>
public sealed class ChargeAction : IPlayerAction
{
    private const float WindUpTime = 0.15f;
    private const float MaxChargeTime = 2f;
    private const float ReleaseTime = 0.2f;
    private const float CooldownTime = 2f;
    private const float MinReleasePercent = 0.25f;

    /// <summary>内部阶段：空闲 → 前摇 → 蓄力 → 释放 → 冷却。</summary>
    private enum Phase { Idle, WindUp, Charging, Release, Cooldown }

    private Phase _phase = Phase.Idle;
    private float _timer;
    private float _chargePercent;
    private float _cooldown;

    /// <inheritdoc />
    public PlayerActionId Id => PlayerActionId.Charge;

    /// <inheritdoc />
    public bool IsActive => _phase is Phase.WindUp or Phase.Charging or Phase.Release;

    /// <inheritdoc />
    public float CooldownRemaining => _cooldown;

    /// <inheritdoc />
    public bool BlocksMovement => _phase is Phase.WindUp or Phase.Release;

    /// <inheritdoc />
    public bool BlocksOtherActions => IsActive;

    /// <inheritdoc />
    public bool GrantsInvincibility => false;

    /// <inheritdoc />
    public float MoveSpeedMultiplier => _phase == Phase.Charging ? 0.5f : 0f;

    /// <inheritdoc />
    public bool CanStart(PlayerActionContext ctx) =>
        _phase == Phase.Idle && _cooldown <= 0f;

    /// <inheritdoc />
    public void Enter(PlayerActionContext ctx)
    {
        _phase = Phase.WindUp;
        _timer = WindUpTime;
        _chargePercent = 0f;
        ctx.Player.SetActionVisual(new Color(0.85f, 0.95f, 1f, 1f));
        ctx.Controller.NotifyActionStarted(Id);
    }

    /// <inheritdoc />
    public void Update(PlayerActionContext ctx, double delta)
    {
        switch (_phase)
        {
            case Phase.WindUp:
                UpdateWindUp(ctx, delta);
                break;
            case Phase.Charging:
                UpdateCharging(ctx, delta);
                break;
            case Phase.Release:
                UpdateRelease(ctx, delta);
                break;
        }
    }

    /// <inheritdoc />
    public void Exit(PlayerActionContext ctx)
    {
        if (_phase is Phase.WindUp or Phase.Charging)
        {
            Cancel(ctx);
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
    /// 聚气键松开时由 Controller 调用；仅在蓄力阶段有效，触发释放判定。
    /// </summary>
    public void OnInputReleased(PlayerActionContext ctx)
    {
        if (_phase == Phase.Charging)
        {
            BeginRelease(ctx);
        }
    }

    /// <summary>
    /// 玩家受击时由 Controller 调用；前摇或蓄力中取消聚气，不进入冷却（除非已释放）。
    /// </summary>
    public void OnInterrupted(PlayerActionContext ctx)
    {
        if (_phase is Phase.WindUp or Phase.Charging)
        {
            Cancel(ctx);
        }
    }

    /// <summary>前摇倒计时，结束后进入蓄力阶段。</summary>
    private void UpdateWindUp(PlayerActionContext ctx, double delta)
    {
        _timer -= (float)delta;
        if (_timer > 0f)
        {
            return;
        }

        _phase = Phase.Charging;
        _timer = 0f;
    }

    /// <summary>
    /// 蓄力中累加 charge 按住时间；松键、满蓄或键未按住时进入释放。
    /// 视觉随蓄力比例放大。
    /// </summary>
    private void UpdateCharging(PlayerActionContext ctx, double delta)
    {
        if (!Input.IsActionPressed("charge"))
        {
            BeginRelease(ctx);
            return;
        }

        _timer += (float)delta;
        _chargePercent = Mathf.Clamp(_timer / MaxChargeTime, 0f, 1f);
        ctx.Player.SetActionVisual(new Color(0.7f, 0.85f, 1f, 1f), 1f + _chargePercent * 0.15f);

        if (_chargePercent >= 1f)
        {
            BeginRelease(ctx);
        }
    }

    /// <summary>
    /// 判定蓄力是否达到最低释放阈值；不足则取消，否则进入释放并立即造成 AOE。
    /// </summary>
    private void BeginRelease(PlayerActionContext ctx)
    {
        if (_chargePercent < MinReleasePercent)
        {
            Cancel(ctx);
            return;
        }

        _phase = Phase.Release;
        _timer = ReleaseTime;
        ExecutePulse(ctx);
        ctx.Player.FlashActionRelease(new Color(1f, 0.95f, 0.7f));
    }

    /// <summary>
    /// 按蓄力档位选择半径、伤害倍率与击退，调用 <see cref="CombatPulse.HitCount"/>。
    /// 档位：&lt;50% / &lt;75% / 满蓄三档。
    /// </summary>
    private void ExecutePulse(PlayerActionContext ctx)
    {
        var (radius, damageMultiplier, knockback) = _chargePercent switch
        {
            < 0.5f => (60f, 0.8f, 80f),
            < 0.75f => (90f, 1.2f, 100f),
            _ => (120f, 2f, 140f),
        };

        CombatPulse.HitCount(ctx.Player, radius, ctx.GetDamage(damageMultiplier), knockback);
    }

    /// <summary>释放阶段倒计时，结束后进入冷却。</summary>
    private void UpdateRelease(PlayerActionContext ctx, double delta)
    {
        _timer -= (float)delta;
        if (_timer <= 0f)
        {
            Finish(ctx);
        }
    }

    /// <summary>蓄力不足或被打断：重置视觉并回到 Idle，不消耗冷却。</summary>
    private void Cancel(PlayerActionContext ctx)
    {
        ctx.Player.ResetActionVisual();
        _phase = Phase.Idle;
        _chargePercent = 0f;
        ctx.Controller.NotifyActionEnded(Id);
    }

    /// <summary>正常释放结束：进入 2s 冷却。</summary>
    private void Finish(PlayerActionContext ctx)
    {
        ctx.Player.ResetActionVisual();
        _phase = Phase.Cooldown;
        _cooldown = CooldownTime;
        _chargePercent = 0f;
        ctx.Controller.NotifyActionEnded(Id);
    }
}
