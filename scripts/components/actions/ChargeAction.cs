using Godot;
using Hope.Core;

namespace Hope.Components.Actions;

public sealed class ChargeAction : IPlayerAction
{
    private const float WindUpTime = 0.15f;
    private const float MaxChargeTime = 2f;
    private const float ReleaseTime = 0.2f;
    private const float CooldownTime = 2f;
    private const float MinReleasePercent = 0.25f;

    private enum Phase { Idle, WindUp, Charging, Release, Cooldown }

    private Phase _phase = Phase.Idle;
    private float _timer;
    private float _chargePercent;
    private float _cooldown;

    public PlayerActionId Id => PlayerActionId.Charge;
    public bool IsActive => _phase is Phase.WindUp or Phase.Charging or Phase.Release;
    public float CooldownRemaining => _cooldown;
    public bool BlocksMovement => _phase is Phase.WindUp or Phase.Release;
    public bool BlocksOtherActions => IsActive;
    public bool GrantsInvincibility => false;
    public float MoveSpeedMultiplier => _phase == Phase.Charging ? 0.5f : 0f;

    public bool CanStart(PlayerActionContext ctx) =>
        _phase == Phase.Idle && _cooldown <= 0f;

    public void Enter(PlayerActionContext ctx)
    {
        _phase = Phase.WindUp;
        _timer = WindUpTime;
        _chargePercent = 0f;
        ctx.Player.SetActionVisual(new Color(0.85f, 0.95f, 1f, 1f));
        ctx.Controller.NotifyActionStarted(Id);
    }

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

    public void Exit(PlayerActionContext ctx)
    {
        if (_phase is Phase.WindUp or Phase.Charging)
        {
            Cancel(ctx);
        }
    }

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

    public void OnInputReleased(PlayerActionContext ctx)
    {
        if (_phase == Phase.Charging)
        {
            BeginRelease(ctx);
        }
    }

    public void OnInterrupted(PlayerActionContext ctx)
    {
        if (_phase is Phase.WindUp or Phase.Charging)
        {
            Cancel(ctx);
        }
    }

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

    private void UpdateRelease(PlayerActionContext ctx, double delta)
    {
        _timer -= (float)delta;
        if (_timer <= 0f)
        {
            Finish(ctx);
        }
    }

    private void Cancel(PlayerActionContext ctx)
    {
        ctx.Player.ResetActionVisual();
        _phase = Phase.Idle;
        _chargePercent = 0f;
        ctx.Controller.NotifyActionEnded(Id);
    }

    private void Finish(PlayerActionContext ctx)
    {
        ctx.Player.ResetActionVisual();
        _phase = Phase.Cooldown;
        _cooldown = CooldownTime;
        _chargePercent = 0f;
        ctx.Controller.NotifyActionEnded(Id);
    }
}
