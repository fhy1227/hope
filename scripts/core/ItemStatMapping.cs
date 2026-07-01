using Hope.Config;

namespace Hope.Core;

/// <summary>
/// 物品配置底材属性 → NumericModifierMap 的映射；数值随物品强度缩放。
/// </summary>
public static class ItemStatMapping
{
    /// <summary>ilvl 达到此值时底材 stat 为配置原值；低于此时按比例衰减。</summary>
    public const int ReferenceItemLevel = 50;

    /// <summary>
    /// 将底材 stat 写入加成表；传奇品质对伤害类主属性有额外倍率。
    /// </summary>
    public static void ApplyBaseStats(
        ItemConfig config,
        NumericModifierMap map,
        int itemLevel,
        float legendaryMultiplier = 1f)
    {
        var scale = ItemLevelScale(itemLevel);

        if (config.StatHp != 0)
        {
            map.Add(NumericType.MaxHealth, ModifierType.Constant, config.StatHp * scale);
        }

        if (config.StatDamage != 0f)
        {
            map.Add(NumericType.Damage, ModifierType.Constant, config.StatDamage * scale * legendaryMultiplier);
        }

        if (config.StatSpeed != 0f)
        {
            map.Add(NumericType.MoveSpeed, ModifierType.Constant, config.StatSpeed * scale);
        }

        if (config.StatCrit != 0f)
        {
            map.Add(NumericType.Crit, ModifierType.Constant, config.StatCrit * scale);
        }

        if (config.StatArmor != 0)
        {
            map.Add(NumericType.Armor, ModifierType.Constant, config.StatArmor * scale);
        }
    }

    /// <summary>与词条公式对齐：底材模板按 ilvl 线性缩放，ilvl≤1 保留原值。</summary>
    public static float ItemLevelScale(int itemLevel)
    {
        if (itemLevel <= 1)
        {
            return 1f;
        }

        return itemLevel / (float)ReferenceItemLevel;
    }
}
