using Hope.Components.Actions;
using Hope.Components;

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
            < 0.5f => (60f, 0.8f, 80f),
            < 0.75f => (90f, 1.2f, 100f),
            _ => (120f, 2f, 140f),
        };

        CombatPulse.HitCount(ctx.Player, radius, ctx.GetDamage(damageMultiplier), knockback);
        CircleFlashEffect.SpawnAt(ctx.Player.GlobalPosition, radius);
    }
}
