using Hope.Config;

namespace Hope.Systems;

/// <summary>
/// 角色等级与经验计算；读取 <see cref="ExpLevelConfig"/> 配置表。
/// </summary>
public static class ExpSystem
{
    /// <summary>升到下一级所需经验；无配置时返回极大值。</summary>
    public static long GetExpForNextLevel(int level)
    {
        var cfg = ConfigManager.Get<ExpLevelConfig>(level);
        return cfg?.ExpRequired ?? 999_999;
    }

    /// <summary>获取指定等级升级奖励。</summary>
    public static LevelUpReward GetLevelUpReward(int newLevel)
    {
        var cfg = ConfigManager.Get<ExpLevelConfig>(newLevel);
        if (cfg == null)
        {
            return new LevelUpReward();
        }

        return new LevelUpReward
        {
            MaxHealthBonus = cfg.RewardHp,
            DamageBonus = cfg.RewardDamage,
            SpeedBonus = cfg.RewardSpeed,
            GoldBonus = cfg.RewardGold,
        };
    }

    /// <summary>根据敌人类型与副本倍率计算击杀经验。</summary>
    public static int CalculateKillExp(string enemyType, int enemyLevel, float expMultiplier)
    {
        var baseExp = enemyLevel * 10;
        if (enemyType == ParamsConfig.EnemyTypeElite)
        {
            baseExp *= 3;
        }
        else if (enemyType == ParamsConfig.EnemyTypeBoss)
        {
            baseExp *= 10;
        }

        return (int)System.MathF.Round(baseExp * expMultiplier);
    }
}

/// <summary>升级时发放的属性与金币奖励。</summary>
public struct LevelUpReward
{
    public int MaxHealthBonus;
    public int DamageBonus;
    public int SpeedBonus;
    public int GoldBonus;
}
