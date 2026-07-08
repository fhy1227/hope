using Godot;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Hope.Config;

/// <summary>
/// 自动生成的配置类 - 对应 dungeon.xlsx
/// </summary>
public partial class DungeonConfig : IConfigData
{
    /// <summary>
    /// id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// name_key  // @text
    /// </summary>
    public string NameKey { get; set; }

    /// <summary>
    /// desc_key  // @text
    /// </summary>
    public string DescKey { get; set; }

    /// <summary>
    /// icon
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// min_player_level
    /// </summary>
    public int MinPlayerLevel { get; set; }

    /// <summary>
    /// recommended_level
    /// </summary>
    public int RecommendedLevel { get; set; }

    /// <summary>
    /// difficulty_tier
    /// </summary>
    public int DifficultyTier { get; set; }

    /// <summary>
    /// total_waves
    /// </summary>
    public int TotalWaves { get; set; }

    /// <summary>
    /// boss_wave
    /// </summary>
    public int BossWave { get; set; }

    /// <summary>
    /// base_enemy_level
    /// </summary>
    public int BaseEnemyLevel { get; set; }

    /// <summary>
    /// wave_time_base
    /// </summary>
    public float WaveTimeBase { get; set; }

    /// <summary>
    /// wave_time_increment
    /// </summary>
    public float WaveTimeIncrement { get; set; }

    /// <summary>
    /// max_enemies_per_wave
    /// </summary>
    public int MaxEnemiesPerWave { get; set; }

    /// <summary>
    /// spawn_interval_base
    /// </summary>
    public float SpawnIntervalBase { get; set; }

    /// <summary>
    /// spawn_interval_min
    /// </summary>
    public float SpawnIntervalMin { get; set; }

    /// <summary>
    /// gold_multiplier
    /// </summary>
    public float GoldMultiplier { get; set; }

    /// <summary>
    /// exp_multiplier
    /// </summary>
    public float ExpMultiplier { get; set; }

    /// <summary>
    /// loot_quality_bonus
    /// </summary>
    public int LootQualityBonus { get; set; }

    /// <summary>
    /// boss_config_id
    /// </summary>
    public int BossConfigId { get; set; }

    /// <summary>
    /// scene_path
    /// </summary>
    public string ScenePath { get; set; }

    /// <summary>
    /// required_cleared_dungeon_id
    /// </summary>
    public int RequiredClearedDungeonId { get; set; }

    public void FromDict(GodotDictionary dict)
    {
        Id = (int)dict["id"];
        NameKey = (string)dict["name_key"];
        DescKey = (string)dict["desc_key"];
        Icon = (string)dict["icon"];
        MinPlayerLevel = (int)dict["min_player_level"];
        RecommendedLevel = (int)dict["recommended_level"];
        DifficultyTier = (int)dict["difficulty_tier"];
        TotalWaves = (int)dict["total_waves"];
        BossWave = (int)dict["boss_wave"];
        BaseEnemyLevel = (int)dict["base_enemy_level"];
        WaveTimeBase = (float)dict["wave_time_base"];
        WaveTimeIncrement = (float)dict["wave_time_increment"];
        MaxEnemiesPerWave = (int)dict["max_enemies_per_wave"];
        SpawnIntervalBase = (float)dict["spawn_interval_base"];
        SpawnIntervalMin = (float)dict["spawn_interval_min"];
        GoldMultiplier = (float)dict["gold_multiplier"];
        ExpMultiplier = (float)dict["exp_multiplier"];
        LootQualityBonus = (int)dict["loot_quality_bonus"];
        BossConfigId = (int)dict["boss_config_id"];
        ScenePath = (string)dict["scene_path"];
        RequiredClearedDungeonId = (int)dict["required_cleared_dungeon_id"];
    }
}
