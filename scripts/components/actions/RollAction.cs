using Godot;
using Hope.Core;

namespace Hope.Components.Actions;

public sealed class RollAction : IPlayerAction
{
    private const float Duration = 0.25f;
    private const float Speed = 480f;
    private const float CooldownTime = 1f;

    private enum Phase { Idle, Rolling, Cooldown }

    private Phase _phase = Phase.Idle;
    private float _timer;
    private float _cooldown;
    private Vector2 _direction;
    private uint _savedCollisionMask;

    public PlayerActionId Id => PlayerActionId.Roll;
    public bool IsActive => _phase == Phase.Rolling;
    public float CooldownRemaining => _cooldown;
    public bool BlocksMovement => IsActive;
    public bool BlocksOtherActions => IsActive;
    public bool GrantsInvincibility => IsActive;
    public float MoveSpeedMultiplier => 1f;

    public bool CanStart(PlayerActionContext ctx) =>
        _phase == Phase.Idle && _cooldown <= 0f;

    public void Enter(PlayerActionContext ctx)
    {
        _direction = ctx.GetRollDirection();
        _phase = Phase.Rolling;
        _timer = Duration;

        _savedCollisionMask = ctx.Player.CollisionMask;
        ctx.Player.CollisionMask &= ~CollisionLayers.Enemy;
        ctx.Player.SetActionVisual(new Color(1f, 1f, 1f, 0.55f));
        ctx.Controller.NotifyActionStarted(Id);
    }

    public void Update(PlayerActionContext ctx, double delta)
    {
        if (_phase != Phase.Rolling)
        {
            return;
        }

        _timer -= (float)delta;
        ctx.Player.Velocity = _direction * Speed;
        ctx.Player.MoveAndSlide();

        if (_timer <= 0f)
        {
            FinishRoll(ctx);
        }
    }

    public void Exit(PlayerActionContext ctx)
    {
        if (_phase == Phase.Rolling)
        {
            FinishRoll(ctx);
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

    private void FinishRoll(PlayerActionContext ctx)
    {
        ctx.Player.CollisionMask = _savedCollisionMask;
        ctx.Player.ResetActionVisual();
        ctx.Player.Velocity = Vector2.Zero;
        _phase = Phase.Cooldown;
        _cooldown = CooldownTime;
        ctx.Controller.NotifyActionEnded(Id);
    }
}
