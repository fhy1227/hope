using Godot;
using GodotArray = Godot.Collections.Array;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Hope.Config;

/// <summary>
/// 自动生成的配置类 - 对应 quality.xlsx
/// </summary>
public partial class QualityConfig : IConfigData
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
    /// color_hex
    /// </summary>
    public string ColorHex { get; set; }

    /// <summary>
    /// drop_weight
    /// </summary>
    public int DropWeight { get; set; }

    /// <summary>
    /// min_item_level
    /// </summary>
    public int MinItemLevel { get; set; }

    /// <summary>
    /// affix_count
    /// </summary>
    public int AffixCount { get; set; }

    /// <summary>
    /// has_aspect
    /// </summary>
    public bool HasAspect { get; set; }

    public void FromDict(GodotDictionary dict)
    {
        Id = (int)dict["id"];
        NameKey = (string)dict["name_key"];
        ColorHex = (string)dict["color_hex"];
        DropWeight = (int)dict["drop_weight"];
        MinItemLevel = (int)dict["min_item_level"];
        AffixCount = (int)dict["affix_count"];
        HasAspect = (bool)dict["has_aspect"];
    }
}
