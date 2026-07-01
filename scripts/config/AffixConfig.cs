using Godot;
using GodotArray = Godot.Collections.Array;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Hope.Config;

/// <summary>
/// 自动生成的配置类 - 对应 affix.xlsx
/// </summary>
public partial class AffixConfig : IConfigData
{
    /// <summary>
    /// id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// affix_key
    /// </summary>
    public string AffixKey { get; set; }

    /// <summary>
    /// numeric_type
    /// </summary>
    public int NumericType { get; set; }

    /// <summary>
    /// modifier_type
    /// </summary>
    public int ModifierType { get; set; }

    /// <summary>
    /// min_per_level
    /// </summary>
    public float MinPerLevel { get; set; }

    /// <summary>
    /// max_per_level
    /// </summary>
    public float MaxPerLevel { get; set; }

    /// <summary>
    /// slot_mask // @comma
    /// </summary>
    public int[] SlotMask { get; set; }

    public void FromDict(GodotDictionary dict)
    {
        Id = (int)dict["id"];
        AffixKey = (string)dict["affix_key"];
        NumericType = (int)dict["numeric_type"];
        ModifierType = (int)dict["modifier_type"];
        MinPerLevel = (float)dict["min_per_level"];
        MaxPerLevel = (float)dict["max_per_level"];
        if (dict["slot_mask"].VariantType == Variant.Type.Array)
        {
            var arr = dict["slot_mask"].AsGodotArray();
            SlotMask = new int[arr.Count];
            for (int i = 0; i < arr.Count; i++)
                SlotMask[i] = (int)arr[i];
        }
    }
}
