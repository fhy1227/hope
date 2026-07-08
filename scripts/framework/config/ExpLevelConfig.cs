using Godot;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Hope.Config;

/// <summary>
/// 自动生成的配置类 - 对应 exp_level.xlsx
/// </summary>
public partial class ExpLevelConfig : IConfigData
{
    /// <summary>
    /// id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// exp_required
    /// </summary>
    public int ExpRequired { get; set; }

    /// <summary>
    /// reward_hp
    /// </summary>
    public int RewardHp { get; set; }

    /// <summary>
    /// reward_damage
    /// </summary>
    public int RewardDamage { get; set; }

    /// <summary>
    /// reward_speed
    /// </summary>
    public int RewardSpeed { get; set; }

    /// <summary>
    /// reward_gold
    /// </summary>
    public int RewardGold { get; set; }

    public void FromDict(GodotDictionary dict)
    {
        Id = (int)dict["id"];
        ExpRequired = (int)dict["exp_required"];
        RewardHp = (int)dict["reward_hp"];
        RewardDamage = (int)dict["reward_damage"];
        RewardSpeed = (int)dict["reward_speed"];
        RewardGold = (int)dict["reward_gold"];
    }
}
