using Godot;
using Hope.Core;
using Hope.Entities;

namespace Hope.Components.Actions;

public sealed class ParryAction : IPlayerAction
{
    private const float WindowDuration = 0.25f;
    private const float PerfectWindow = 0.08f;
    private const float SuccessCooldown = 0.8f;
    private const float FailCooldown = 1.5f;
    private const float CounterRadius = 70f;
    private const float CounterDamageMultiplier = 1f;
    private const float StunDuration = 0.4f;

    private enum Phase { Idle, Active, Cooldown }

    private Phase _phase = Phase.Idle;
    private float _timer;
    private float _cooldown;
    private bool _resolved;

    public PlayerActionId Id => PlayerActionId.Parry;
    public bool IsActive => _phase == Phase.Active;
    public float CooldownRemaining => _cooldown;
    public bool BlocksMovement => false;
    public bool BlocksOtherActions => IsActive;
    public bool GrantsInvincibility => false;
    public float MoveSpeedMultiplier => IsActive ? 0.3f : 1f;

    public bool IsParryWindowOpen => _phase == Phase.Active && !_resolved;

    public bool CanStart(PlayerActionContext ctx) =>
        _phase == Phase.Idle && _cooldown <= 0f;

    public void Enter(PlayerActionContext ctx)
    {
        _phase = Phase.Active;
        _timer = WindowDuration;
        _resolved = false;
        ctx.Player.SetActionVisual(new Color(0.9f, 1f, 0.95f, 1f));
        ctx.Controller.NotifyActionStarted(Id);
    }

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

    public void Exit(PlayerActionContext ctx)
    {
        if (_phase == Phase.Active && !_resolved)
        {
            Finish(ctx, succeeded: false);
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

    public bool TryResolveParry(PlayerActionContext ctx, Enemy? source, out bool perfect)
    {
        perfect = false;
        if (!IsParryWindowOpen)
        {
            return false;
        }

        perfect = _timer >= WindowDuration - PerfectWindow;
        _resolved = true;

        if (source != null && GodotObject.IsInstanceValid(source))
        {
            source.ApplyStun(StunDuration);
        }

        CombatPulse.HitCount(ctx.Player, CounterRadius, ctx.GetDamage(CounterDamageMultiplier), 60f);
        ctx.Player.FlashActionRelease(perfect ? new Color(1f, 1f, 0.6f) : new Color(0.85f, 1f, 0.85f));
        Finish(ctx, succeeded: true, perfect: perfect);
        return true;
    }

    private void Finish(PlayerActionContext ctx, bool succeeded, bool perfect = false)
    {
        ctx.Player.ResetActionVisual();
        _phase = Phase.Cooldown;
        _cooldown = succeeded ? (perfect ? 0f : SuccessCooldown) : FailCooldown;
        _resolved = true;
        ctx.Controller.NotifyActionEnded(Id);
    }
}
