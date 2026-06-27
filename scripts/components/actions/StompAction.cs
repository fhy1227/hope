using Godot;
using Hope.Core;

namespace Hope.Components.Actions;

public sealed class StompAction : IPlayerAction
{
    private const float WindUpTime = 0.1f;
    private const float RecoveryTime = 0.05f;
    private const float Radius = 80f;
    private const float DamageMultiplier = 0.6f;
    private const float KnockbackSpeed = 120f;
    private const float CooldownTime = 3f;

    private enum Phase { Idle, WindUp, Recovery, Cooldown }

    private Phase _phase = Phase.Idle;
    private float _timer;
    private float _cooldown;
    private bool _hitApplied;

    public PlayerActionId Id => PlayerActionId.Stomp;
    public bool IsActive => _phase is Phase.WindUp or Phase.Recovery;
    public float CooldownRemaining => _cooldown;
    public bool BlocksMovement => IsActive;
    public bool BlocksOtherActions => IsActive;
    public bool GrantsInvincibility => false;
    public float MoveSpeedMultiplier => 0f;

    public bool CanStart(PlayerActionContext ctx) =>
        _phase == Phase.Idle && _cooldown <= 0f;

    public void Enter(PlayerActionContext ctx)
    {
        _phase = Phase.WindUp;
        _timer = WindUpTime;
        _hitApplied = false;
        ctx.Player.Velocity = Vector2.Zero;
        ctx.Player.SetActionVisual(new Color(0.75f, 0.85f, 1f, 1f), 0.85f);
        ctx.Controller.NotifyActionStarted(Id);
    }

    public void Update(PlayerActionContext ctx, double delta)
    {
        if (_phase == Phase.WindUp)
        {
            _timer -= (float)delta;
            if (!_hitApplied && _timer <= 0f)
            {
                ApplyStomp(ctx);
                _phase = Phase.Recovery;
                _timer = RecoveryTime;
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

    public void Exit(PlayerActionContext ctx)
    {
        if (_phase is Phase.WindUp or Phase.Recovery)
        {
            Finish(ctx);
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

    private void ApplyStomp(PlayerActionContext ctx)
    {
        _hitApplied = true;
        CombatPulse.HitCount(ctx.Player, Radius, ctx.GetDamage(DamageMultiplier), KnockbackSpeed);
        ctx.Player.FlashActionRelease(new Color(0.6f, 0.8f, 1f));
        ctx.Player.SetActionVisual(Colors.White, 1.15f);
    }

    private void Finish(PlayerActionContext ctx)
    {
        ctx.Player.ResetActionVisual();
        ctx.Player.Velocity = Vector2.Zero;
        _phase = Phase.Cooldown;
        _cooldown = CooldownTime;
        ctx.Controller.NotifyActionEnded(Id);
    }
}
