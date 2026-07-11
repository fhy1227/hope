using Hope.Config;

namespace Hope.DropSystem;

/// <summary>
/// 单次装备掉落的输入上下文（怪物等级、区域、MF、Smart Loot 等）。
/// </summary>
public readonly struct DropContext
{
    /// <summary> 敌人类型，对应 drop_table.enemy_type </summary>
    public string EnemyType { get; init; }

    /// <summary> 怪物等级（D4：决定物品等级上限） </summary>
    public int MonsterLevel { get; init; }

    /// <summary> 区域等级（D4：区域 ilvl 基准） </summary>
    public int AreaLevel { get; init; }

    /// <summary> 魔法发现（0~1，提高高稀有度权重） </summary>
    public float MagicFind { get; init; }

    /// <summary> 掉落率倍率（精英/Boss 加成） </summary>
    public float DropRateMultiplier { get; init; }

    /// <summary> Smart Loot 倾向的装备槽（如武器槽 1） </summary>
    public int[] PreferredSlotTypes { get; init; }

    /// <summary> Smart Loot 触发概率（D4 约 85% 掉可用装备） </summary>
    public float SmartLootChance { get; init; }

    /// <summary> drop_table 单条规则的掉落率 </summary>
    public float TableDropRate { get; init; }

    /// <summary> drop_table 指定稀有度（0=随机） </summary>
    public int ForcedRarity { get; init; }

    /// <summary> 副本掉落品质加成（提高 MF） </summary>
    public int LootQualityBonus { get; init; }

    private const float LootQualityMfPerPoint = 0.05f;

    public static DropContext FromWave(
        string enemyType,
        int wave,
        float tableDropRate,
        int forcedRarity = 0,
        int lootQualityBonus = 0)
    {
        var areaLevel = (int)(ParamsConfig.DropAreaLevelBase + (wave - 1) * ParamsConfig.DropAreaLevelPerWave);
        var (mf, rateMul, levelBonus) = EnemyTypeModifiers(enemyType);

        return new DropContext
        {
            EnemyType = enemyType,
            MonsterLevel = areaLevel + levelBonus,
            AreaLevel = areaLevel,
            MagicFind = mf + lootQualityBonus * LootQualityMfPerPoint,
            DropRateMultiplier = rateMul,
            PreferredSlotTypes = [(int)ParamsConfig.DropWeaponSlotType],
            SmartLootChance = ParamsConfig.DropSmartLootChance,
            TableDropRate = tableDropRate,
            ForcedRarity = forcedRarity,
            LootQualityBonus = lootQualityBonus,
        };
    }

    private static (float mf, float rateMul, int levelBonus) EnemyTypeModifiers(string enemyType) =>
        enemyType switch
        {
            var t when t == ParamsConfig.EnemyTypeElite => (
                ParamsConfig.DropEliteMagicFind,
                ParamsConfig.DropEliteRateMul,
                (int)ParamsConfig.DropEliteLevelBonus),
            var t when t == ParamsConfig.EnemyTypeBoss => (
                ParamsConfig.DropBossMagicFind,
                ParamsConfig.DropBossRateMul,
                (int)ParamsConfig.DropBossLevelBonus),
            _ => (0f, 1f, 0),
        };
}
