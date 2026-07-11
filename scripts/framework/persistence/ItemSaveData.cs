using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using Hope.Core;
using Hope.DropSystem;

namespace Hope.Persistence;

/// <summary>
/// 物品实例的 JSON 存档形态，与 <see cref="ItemInstance"/> 互转。
/// </summary>
public class ItemSaveData
{
    [JsonPropertyName("uid")]
    public string Uid { get; set; } = "";

    [JsonPropertyName("config_id")]
    public int ConfigId { get; set; }

    [JsonPropertyName("count")]
    public int Count { get; set; } = 1;

    [JsonPropertyName("item_level")]
    public int ItemLevel { get; set; } = 1;

    [JsonPropertyName("rolled_rarity")]
    public int RolledRarity { get; set; }

    [JsonPropertyName("affixes")]
    public List<AffixSaveData> Affixes { get; set; } = [];

    [JsonPropertyName("tempered_affixes")]
    public List<AffixSaveData> TemperedAffixes { get; set; } = [];

    [JsonPropertyName("temper_durability")]
    public int TemperDurability { get; set; }

    [JsonPropertyName("masterwork_level")]
    public int MasterworkLevel { get; set; }

    [JsonPropertyName("masterwork_bonus_indices")]
    public List<int> MasterworkBonusIndices { get; set; } = [];

    [JsonPropertyName("socketed_gem_ids")]
    public List<int> SocketedGemIds { get; set; } = [];

    [JsonPropertyName("max_sockets")]
    public int MaxSockets { get; set; }

    [JsonPropertyName("aspect_id")]
    public string AspectId { get; set; } = "";

    [JsonPropertyName("enchant_count")]
    public int EnchantCount { get; set; }

    public static ItemSaveData FromInstance(ItemInstance item)
    {
        return new ItemSaveData
        {
            Uid = item.Uid,
            ConfigId = item.ConfigId,
            Count = item.Count,
            ItemLevel = item.ItemLevel,
            RolledRarity = item.RolledRarity,
            Affixes = item.Affixes.Select(AffixSaveData.FromRolled).ToList(),
            TemperedAffixes = item.TemperedAffixes.Select(AffixSaveData.FromRolled).ToList(),
            MasterworkLevel = item.MasterworkLevel,
            EnchantCount = item.EnchantCount,
            AspectId = item.AspectId,
        };
    }

    public ItemInstance ToInstance()
    {
        return new ItemInstance
        {
            Uid = string.IsNullOrEmpty(Uid) ? System.Guid.NewGuid().ToString() : Uid,
            ConfigId = ConfigId,
            Count = Count,
            ItemLevel = ItemLevel,
            RolledRarity = RolledRarity,
            Affixes = Affixes.Select(a => a.ToRolled()).ToList(),
            TemperedAffixes = TemperedAffixes.Select(a => a.ToRolled()).ToList(),
            MasterworkLevel = MasterworkLevel,
            EnchantCount = EnchantCount,
            AspectId = AspectId,
        };
    }
}

/// <summary>词条存档条目。</summary>
public class AffixSaveData
{
    [JsonPropertyName("affix_id")]
    public string AffixId { get; set; } = "";

    [JsonPropertyName("numeric_type")]
    public int NumericTypeValue { get; set; }

    [JsonPropertyName("modifier_type")]
    public int ModifierTypeValue { get; set; }

    [JsonPropertyName("value")]
    public float Value { get; set; }

    public static AffixSaveData FromRolled(RolledAffix affix) => new()
    {
        AffixId = affix.AffixId,
        NumericTypeValue = (int)affix.NumericType,
        ModifierTypeValue = (int)affix.ModifierType,
        Value = affix.Value,
    };

    public RolledAffix ToRolled() => new()
    {
        AffixId = AffixId,
        NumericType = (NumericType)NumericTypeValue,
        ModifierType = (ModifierType)ModifierTypeValue,
        Value = Value,
    };
}
