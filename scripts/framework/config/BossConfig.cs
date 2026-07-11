using Godot;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Hope.Config;

/// <summary>
/// 自动生成的配置类 - 对应 boss.xlsx
/// </summary>
public partial class BossConfig : IConfigData
{
    /// <summary>
    /// id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// name_key  // @text
    /// </summary>
    public string NameKey { get; set; } = string.Empty;

    /// <summary>
    /// hp_mult
    /// </summary>
    public float HpMult { get; set; }

    /// <summary>
    /// damage_mult
    /// </summary>
    public float DamageMult { get; set; }

    /// <summary>
    /// scale
    /// </summary>
    public float Scale { get; set; } = 1f;

    /// <summary>
    /// gold_mult
    /// </summary>
    public float GoldMult { get; set; } = 1f;

    /// <summary>
    /// tint_color
    /// </summary>
    public string TintColor { get; set; } = "#FFFFFF";

    public void FromDict(GodotDictionary dict)
    {
        Id = (int)dict["id"];
        NameKey = (string)dict["name_key"];
        HpMult = (float)dict["hp_mult"];
        DamageMult = (float)dict["damage_mult"];
        Scale = (float)dict["scale"];
        GoldMult = (float)dict["gold_mult"];
        TintColor = dict.ContainsKey("tint_color") ? (string)dict["tint_color"] : "#FFFFFF";
    }
}
