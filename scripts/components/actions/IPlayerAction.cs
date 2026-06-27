using Hope.Core;

namespace Hope.Components.Actions;

public interface IPlayerAction
{
    PlayerActionId Id { get; }
    bool IsActive { get; }
    float CooldownRemaining { get; }

    bool CanStart(PlayerActionContext ctx);
    void Enter(PlayerActionContext ctx);
    void Update(PlayerActionContext ctx, double delta);
    void Exit(PlayerActionContext ctx);
    void TickInactive(double delta);

    bool BlocksMovement { get; }
    bool BlocksOtherActions { get; }
    bool GrantsInvincibility { get; }
    float MoveSpeedMultiplier { get; }
}
