using Godot;
using Hope.Entities;

namespace Hope.Components.Actions;

/// <summary>
/// 单帧行为执行上下文（值类型快照）。由 <see cref="PlayerActionController"/> 每帧构建并传入各 Action，
/// 避免 Action 直接读全局 Input 或缓存过期引用。
/// </summary>
public readonly struct PlayerActionContext
{
    /// <summary>行为所属玩家实体；Bind 后始终非 null。</summary>
    public Player Player { get; init; }

    /// <summary>调度本行为的 Controller，用于 Notify 与格挡转发。</summary>
    public PlayerActionController Controller { get; init; }

    /// <summary>本帧移动方向（归一化）；按住鼠标左键时朝鼠标世界坐标，否则为零向量。</summary>
    public Vector2 InputDirection { get; init; }

    /// <summary>最近一次有效移动方向（归一化）；无输入时用于决定默认朝向。</summary>
    public Vector2 LastMoveDirection { get; init; }

    /// <summary>
    /// 计算翻滚/定向行为的位移方向：优先当前输入，其次上次移动方向，均无时向下。
    /// </summary>
    /// <returns>归一化方向向量。</returns>
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

    /// <summary>
    /// 按玩家当前攻击力与倍率计算行为伤害；数据来自 <see cref="Player.GetActionDamage"/>。
    /// </summary>
    /// <param name="multiplier">伤害倍率，默认 1（满额）。聚气/震地等按档位传入不同值。</param>
    /// <returns>取整后的伤害值。</returns>
    public int GetDamage(float multiplier = 1f)
    {
        return Player.GetActionDamage(multiplier);
    }
}
