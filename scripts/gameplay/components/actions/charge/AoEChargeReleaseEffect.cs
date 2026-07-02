using Hope.Components.Actions;
using Hope.Components;
using Hope.Config;

namespace Hope.Components.Actions.Charge;

/// <summary>
/// 默认聚气释放：以玩家为中心的范围伤害与击退，蓄力比例决定半径、倍率与击退三档。
/// </summary>
public sealed class AoEChargeReleaseEffect : IChargeReleaseEffect
{
    /// <inheritdoc />
    public void Execute(in ChargeReleaseContext release)
    {
        var ctx = release.Action;
        var (radius, damageMultiplier, knockback) = release.ChargePercent switch
        {
            var p when p < ParamsConfig.ChargeAoeThresholdMid => (
                ParamsConfig.ChargeAoeRadiusLow,
                ParamsConfig.ChargeAoeDamageMulLow,
                ParamsConfig.ChargeAoeKnockbackLow),
            var p when p < ParamsConfig.ChargeAoeThresholdHigh => (
                ParamsConfig.ChargeAoeRadiusMid,
                ParamsConfig.ChargeAoeDamageMulMid,
                ParamsConfig.ChargeAoeKnockbackMid),
            _ => (
                ParamsConfig.ChargeAoeRadiusHigh,
                ParamsConfig.ChargeAoeDamageMulHigh,
                ParamsConfig.ChargeAoeKnockbackHigh),
        };

        CombatPulse.HitCount(ctx.Player, radius, ctx.GetDamage(damageMultiplier), knockback);
        CircleFlashEffect.SpawnAt(ctx.Player.GlobalPosition, radius);
    }
}
