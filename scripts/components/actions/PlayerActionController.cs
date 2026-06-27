using Godot;
using Hope.Core;
using Hope.Entities;

namespace Hope.Components.Actions;

/// <summary>
/// 玩家战斗行为调度：翻滚、聚气、震地、格挡。
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

    public bool BlocksMovement => _active?.BlocksMovement ?? false;
    public bool BlocksOtherActions => _active?.BlocksOtherActions ?? false;
    public bool GrantsInvincibility => _active?.GrantsInvincibility ?? false;
    public float MoveSpeedMultiplier => _active?.MoveSpeedMultiplier ?? 1f;
    public bool IsParrying => _parry.IsParryWindowOpen;

    public void Bind(Player player)
    {
        _player = player;
    }

    public void SetLastMoveDirection(Vector2 direction)
    {
        if (direction.LengthSquared() > 0.01f)
        {
            _lastMoveDirection = direction.Normalized();
        }
    }

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

    public bool TryParry(Enemy? source, out bool perfect)
    {
        perfect = false;
        if (!_parry.IsParryWindowOpen)
        {
            return false;
        }

        return _parry.TryResolveParry(BuildContext(), source, out perfect);
    }

    public void OnPlayerHit()
    {
        if (_active?.Id == PlayerActionId.Charge)
        {
            _charge.OnInterrupted(BuildContext());
            _active = null;
        }
    }

    internal void NotifyActionStarted(PlayerActionId id)
    {
        EventBus.Instance?.EmitPlayerActionStarted((int)id);
    }

    internal void NotifyActionEnded(PlayerActionId id)
    {
        EventBus.Instance?.EmitPlayerActionEnded((int)id);
    }

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
