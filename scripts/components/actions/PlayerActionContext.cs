using Godot;
using Hope.Entities;

namespace Hope.Components.Actions;

public readonly struct PlayerActionContext
{
    public Player Player { get; init; }
    public PlayerActionController Controller { get; init; }
    public Vector2 InputDirection { get; init; }
    public Vector2 LastMoveDirection { get; init; }

    public Vector2 GetRollDirection()
    {
        if (InputDirection.LengthSquared() > 0.01f)
        {
            return InputDirection.Normalized();
        }

        if (LastMoveDirection.LengthSquared() > 0.01f)
        {
            return LastMoveDirection.Normalized();
        }

        return Vector2.Down;
    }

    public int GetDamage(float multiplier = 1f)
    {
        return Player.GetActionDamage(multiplier);
    }
}
