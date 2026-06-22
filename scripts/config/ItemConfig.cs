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
    /// name  // @text [client]
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// desc  // @text [client]
    /// </summary>
    public string Desc { get; set; }

    /// <summary>
    /// icon  // [client]
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// getway  // @table [client]
    /// </summary>
    public GodotArray Getway { get; set; }

    /// <summary>
    /// main_type
    /// </summary>
    public int MainType { get; set; }

    /// <summary>
    /// type
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// usable
    /// </summary>
    public int Usable { get; set; }

    /// <summary>
    /// use_on_sys
    /// </summary>
    public int UseOnSys { get; set; }

    /// <summary>
    /// tags  // @comma
    /// </summary>
    public int[] Tags { get; set; }

    /// <summary>
    /// value
    /// </summary>
    public int Value { get; set; }

    /// <summary>
    /// value1
    /// </summary>
    public int Value1 { get; set; }

    /// <summary>
    /// value_rewards
    /// </summary>
    public int ValueRewards { get; set; }

    /// <summary>
    /// stacklimit
    /// </summary>
    public int Stacklimit { get; set; }

    /// <summary>
    /// quality
    /// </summary>
    public int Quality { get; set; }

    /// <summary>
    /// timelimit
    /// </summary>
    public int Timelimit { get; set; }

    /// <summary>
    /// sort  // [client]
    /// </summary>
    public int Sort { get; set; }

    /// <summary>
    /// maxlv_to_money  // @comma
    /// </summary>
    public int[] MaxlvToMoney { get; set; }

    /// <summary>
    /// red_dots_cond
    /// </summary>
    public int RedDotsCond { get; set; }

    /// <summary>
    /// test_json  // @json
    /// </summary>
    public GodotDictionary TestJson { get; set; }

    public void FromDict(GodotDictionary dict)
    {
        Id = (int)dict["id"];
        Name = (string)dict["name"];
        Desc = (string)dict["desc"];
        Icon = (string)dict["icon"];
        if (dict["getway"].VariantType == Variant.Type.Array)
        {
            Getway = dict["getway"].AsGodotArray();
        }
        else if (dict["getway"].VariantType == Variant.Type.String && !string.IsNullOrEmpty((string)dict["getway"]))
        {
            var json = Godot.Json.ParseString((string)dict["getway"]);
            Getway = json.AsGodotArray();
        }
        MainType = (int)dict["main_type"];
        Type = (int)dict["type"];
        Usable = (int)dict["usable"];
        UseOnSys = (int)dict["use_on_sys"];
        if (dict["tags"].VariantType == Variant.Type.Array)
        {
            var arr = dict["tags"].AsGodotArray();
            Tags = new int[arr.Count];
            for (int i = 0; i < arr.Count; i++)
                Tags[i] = (int)arr[i];
        }
        Value = (int)dict["value"];
        Value1 = (int)dict["value1"];
        ValueRewards = (int)dict["value_rewards"];
        Stacklimit = (int)dict["stacklimit"];
        Quality = (int)dict["quality"];
        Timelimit = (int)dict["timelimit"];
        Sort = (int)dict["sort"];
        if (dict["maxlv_to_money"].VariantType == Variant.Type.Array)
        {
            var arr = dict["maxlv_to_money"].AsGodotArray();
            MaxlvToMoney = new int[arr.Count];
            for (int i = 0; i < arr.Count; i++)
                MaxlvToMoney[i] = (int)arr[i];
        }
        RedDotsCond = (int)dict["red_dots_cond"];
        if (dict["test_json"].VariantType == Variant.Type.Dictionary)
        {
            TestJson = dict["test_json"].AsGodotDictionary();
        }
        else if (dict["test_json"].VariantType == Variant.Type.String && !string.IsNullOrEmpty((string)dict["test_json"]))
        {
            var json = Godot.Json.ParseString((string)dict["test_json"]);
            TestJson = json.AsGodotDictionary();
        }
    }
}
