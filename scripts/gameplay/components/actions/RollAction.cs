using Godot;
using Hope.Config;
using Hope.Core;

namespace Hope.Components.Actions;

/// <summary>
/// 翻滚行为（输入 Space）：定向位移约 0.25s，全程无敌并临时忽略与敌人的碰撞（穿敌）。
/// 结束后进入 1s 冷却。自行调用 MoveAndSlide，锁定移动状态机。
/// </summary>
public sealed class RollAction : IPlayerAction
{
    /// <summary>内部阶段：空闲 → 翻滚中 → 冷却。</summary>
    private enum Phase { Idle, Rolling, Cooldown }

    private Phase _phase = Phase.Idle;
    private float _timer;
    private float _cooldown;
    private Vector2 _direction;
    private uint _savedCollisionMask;

    /// <inheritdoc />
    public PlayerActionId Id => PlayerActionId.Roll;

    /// <inheritdoc />
    public bool IsActive => _phase == Phase.Rolling;

    /// <inheritdoc />
    public float CooldownRemaining => _cooldown;

    /// <inheritdoc />
    public bool BlocksMovement => IsActive;

    /// <inheritdoc />
    public bool BlocksOtherActions => IsActive;

    /// <inheritdoc />
    public bool GrantsInvincibility => IsActive;

    /// <inheritdoc />
    public float MoveSpeedMultiplier => 1f;

    /// <inheritdoc />
    public bool CanStart(PlayerActionContext ctx) =>
        _phase == Phase.Idle && _cooldown <= 0f;

    /// <inheritdoc />
    public void Enter(PlayerActionContext ctx)
    {
        _direction = ctx.GetRollDirection();
        _phase = Phase.Rolling;
        _timer = ParamsConfig.RollDuration;

        _savedCollisionMask = ctx.Player.CollisionMask;
        ctx.Player.CollisionMask &= ~CollisionLayers.Enemy;
        ctx.Player.SetActionVisual(ParamsConfig.ColorRollVisual);
        ctx.Controller.NotifyActionStarted(Id);
    }

    /// <inheritdoc />
    public void Update(PlayerActionContext ctx, double delta)
    {
        if (_phase != Phase.Rolling)
        {
            return;
        }

        _timer -= (float)delta;
        ctx.Player.Velocity = _direction * ParamsConfig.RollSpeed;
        ctx.Player.MoveAndSlide();

        if (_timer <= 0f)
        {
            FinishRoll(ctx);
        }
    }

    /// <inheritdoc />
    public void Exit(PlayerActionContext ctx)
    {
        if (_phase == Phase.Rolling)
        {
            FinishRoll(ctx);
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

    /// <summary>翻滚结束：恢复碰撞掩码、视觉与速度，进入冷却并通知 Controller。</summary>
    private void FinishRoll(PlayerActionContext ctx)
    {
        ctx.Player.CollisionMask = _savedCollisionMask;
        ctx.Player.ResetActionVisual();
        ctx.Player.Velocity = Vector2.Zero;
        _phase = Phase.Cooldown;
        _cooldown = ParamsConfig.RollCooldown;
        ctx.Controller.NotifyActionEnded(Id);
    }
}
