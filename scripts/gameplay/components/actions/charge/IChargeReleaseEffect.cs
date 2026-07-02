namespace Hope.Components.Actions.Charge;

/// <summary>
/// 聚气释放效果契约。蓄力阶段由 <see cref="ChargeAction"/> 管理，达标松键后委托本接口执行具体效果。
/// 装备词条或角色差异可通过替换实现类扩展，无需改动聚气状态机。
/// </summary>
public interface IChargeReleaseEffect
{
    /// <summary>
    /// 在蓄力达标、进入释放阶段时立即调用一次。
    /// </summary>
    /// <param name="release">释放快照，含蓄力比例与玩家上下文。</param>
    void Execute(in ChargeReleaseContext release);
}
