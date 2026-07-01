using Godot;
using GodotArray = Godot.Collections.Array;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Hope.Config;

/// <summary>
/// 自动生成的配置类 - 对应 item.xlsx
/// </summary>
public partial class ItemConfig : IConfigData
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
    /// desc_key
    /// </summary>
    public string DescKey { get; set; }

    /// <summary>
    /// icon
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// type
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// rarity
    /// </summary>
    public int Rarity { get; set; }

    /// <summary>
    /// slot_type
    /// </summary>
    public int SlotType { get; set; }

    /// <summary>
    /// level_req
    /// </summary>
    public int LevelReq { get; set; }

    /// <summary>
    /// stat_hp
    /// </summary>
    public int StatHp { get; set; }

    /// <summary>
    /// stat_damage
    /// </summary>
    public float StatDamage { get; set; }

    /// <summary>
    /// stat_speed
    /// </summary>
    public float StatSpeed { get; set; }

    /// <summary>
    /// stat_crit
    /// </summary>
    public float StatCrit { get; set; }

    /// <summary>
    /// stat_armor
    /// </summary>
    public int StatArmor { get; set; }

    /// <summary>
    /// stack_limit
    /// </summary>
    public int StackLimit { get; set; }

    /// <summary>
    /// sell_price
    /// </summary>
    public int SellPrice { get; set; }

    /// <summary>
    /// aspect_id
    /// </summary>
    public string AspectId { get; set; }

    /// <summary>
    /// is_drop_base
    /// </summary>
    public bool IsDropBase { get; set; }

    public void FromDict(GodotDictionary dict)
    {
        Id = (int)dict["id"];
        NameKey = (string)dict["name_key"];
        DescKey = (string)dict["desc_key"];
        Icon = (string)dict["icon"];
        Type = (int)dict["type"];
        Rarity = (int)dict["rarity"];
        SlotType = (int)dict["slot_type"];
        LevelReq = (int)dict["level_req"];
        StatHp = (int)dict["stat_hp"];
        StatDamage = (float)dict["stat_damage"];
        StatSpeed = (float)dict["stat_speed"];
        StatCrit = (float)dict["stat_crit"];
        StatArmor = (int)dict["stat_armor"];
        StackLimit = (int)dict["stack_limit"];
        SellPrice = (int)dict["sell_price"];
        AspectId = dict["aspect_id"].VariantType == Variant.Type.Nil
            ? ""
            : (string)dict["aspect_id"];
        IsDropBase = (bool)dict["is_drop_base"];
    }
}
