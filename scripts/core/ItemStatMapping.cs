using Hope.Config;

namespace Hope.Core;

/// <summary>
/// 物品配置底材属性 → NumericModifierMap 的映射。
/// </summary>
public static class ItemStatMapping
{
    public static void ApplyBaseStats(ItemConfig config, NumericModifierMap map, float damageMultiplier = 1f)
    {
        if (config.StatHp != 0)
        {
            map.Add(NumericType.MaxHealth, ModifierType.Constant, config.StatHp);
        }

        if (config.StatDamage != 0f)
        {
            map.Add(NumericType.Damage, ModifierType.Constant, config.StatDamage * damageMultiplier);
        }

        if (config.StatSpeed != 0f)
        {
            map.Add(NumericType.MoveSpeed, ModifierType.Constant, config.StatSpeed);
        }

        if (config.StatCrit != 0f)
        {
            map.Add(NumericType.Crit, ModifierType.Constant, config.StatCrit);
        }

        if (config.StatArmor != 0)
        {
            map.Add(NumericType.Armor, ModifierType.Constant, config.StatArmor);
        }
    }
}
