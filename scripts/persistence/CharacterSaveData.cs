using System.Collections.Generic;
using System.Text.Json.Serialization;
using Hope.Config;

namespace Hope.Persistence;

/// <summary>
/// 角色局外存档根对象，对应 <c>user://saves/slot_N/character.json</c>。
/// </summary>
public class CharacterSaveData
{
    [JsonPropertyName("schema_version")]
    public int SchemaVersion { get; set; } = SaveSchema.CurrentVersion;

    [JsonPropertyName("character_name")]
    public string CharacterName { get; set; } = "";

    [JsonPropertyName("last_saved_unix_ms")]
    public long LastSavedUnixMs { get; set; }

    [JsonPropertyName("level")]
    public int Level { get; set; } = 1;

    [JsonPropertyName("experience")]
    public long Experience { get; set; }

    [JsonPropertyName("gold")]
    public int Gold { get; set; }

    [JsonPropertyName("difficulty_tier")]
    public int DifficultyTier { get; set; }

    [JsonPropertyName("last_hub_scene")]
    public string LastHubScene { get; set; } = "";

    [JsonPropertyName("last_area_id")]
    public string LastAreaId { get; set; } = "";

    [JsonPropertyName("inventory")]
    public List<ItemSaveData> Inventory { get; set; } = [];

    [JsonPropertyName("equipped")]
    public Dictionary<int, List<ItemSaveData>> Equipped { get; set; } = [];

    [JsonPropertyName("materials")]
    public Dictionary<int, int> Materials { get; set; } = [];

    [JsonPropertyName("aspect_codex")]
    public Dictionary<string, int> AspectCodex { get; set; } = [];

    [JsonPropertyName("defeated_bosses")]
    public List<string> DefeatedBosses { get; set; } = [];

    /// <summary>新建角色时的默认装备（双持武士刀）。</summary>
    public static CharacterSaveData CreateDefault(string characterName)
    {
        var weaponId = (int)ParamsConfig.WeaponDefaultConfigId;
        var katana = new ItemSaveData { ConfigId = weaponId, Count = 1 };
        return new CharacterSaveData
        {
            CharacterName = characterName,
            Equipped = new Dictionary<int, List<ItemSaveData>>
            {
                [EquipManagerWeaponSlotType] = [katana, new ItemSaveData { ConfigId = weaponId, Count = 1 }],
            },
        };
    }

    /// <summary>与 <see cref="Systems.EquipManager.WeaponSlotType"/> 一致，避免循环引用。</summary>
    private static int EquipManagerWeaponSlotType => (int)ParamsConfig.DropWeaponSlotType;
}
