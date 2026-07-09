using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Hope.Persistence;

/// <summary>技能快捷键槽位存档 DTO。</summary>
public class SkillSlotSaveData
{
    [JsonPropertyName("slot")]
    public int Slot { get; set; }

    [JsonPropertyName("skill_id")]
    public string SkillId { get; set; } = "";
}

/// <summary>玩家技能状态存档 DTO。</summary>
public class SkillSaveData
{
    [JsonPropertyName("available_points")]
    public int AvailablePoints { get; set; }

    [JsonPropertyName("total_points_spent")]
    public int TotalPointsSpent { get; set; }

    [JsonPropertyName("invested_ranks")]
    public Dictionary<string, int> InvestedRanks { get; set; } = new();

    [JsonPropertyName("chosen_enhancements")]
    public Dictionary<string, string> ChosenEnhancements { get; set; } = new();

    [JsonPropertyName("chosen_key_passive_id")]
    public string ChosenKeyPassiveId { get; set; } = "";

    [JsonPropertyName("skill_slots")]
    public List<SkillSlotSaveData> SkillSlots { get; set; } = [];
}
