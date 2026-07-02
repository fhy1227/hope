using Godot;
using GodotArray = Godot.Collections.Array;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Hope.Config;

/// <summary>
/// 自动生成的配置类 - 对应 equip_slot.xlsx
/// </summary>
public partial class EquipSlotConfig : IConfigData
{
    /// <summary>
    /// id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// name_key
    /// </summary>
    public string NameKey { get; set; }

    /// <summary>
    /// icon
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// max_count
    /// </summary>
    public int MaxCount { get; set; }

    public void FromDict(GodotDictionary dict)
    {
        Id = (int)dict["id"];
        NameKey = (string)dict["name_key"];
        Icon = (string)dict["icon"];
        MaxCount = (int)dict["max_count"];
    }
}
