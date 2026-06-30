using Hope.Components.Actions;

namespace Hope.Components.Actions.Charge;

/// <summary>
/// 聚气释放瞬间的快照上下文，供 <see cref="IChargeReleaseEffect"/> 读取蓄力比例与玩家状态。
/// </summary>
public readonly struct ChargeReleaseContext
{
    /// <summary>本帧行为上下文（玩家、输入、伤害计算等）。</summary>
    public PlayerActionContext Action { get; init; }

    /// <summary>蓄力完成比例，范围 [0, 1]；调用释放效果时已通过最低阈值校验。</summary>
    public float ChargePercent { get; init; }
}
