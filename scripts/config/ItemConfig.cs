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
    /// id  // [!mode]
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
    /// type
    /// </summary>
    public int Type { get; set; }

    /// <summary>
    /// usable
    /// </summary>
    public int Usable { get; set; }

    /// <summary>
    /// stacklimit
    /// </summary>
    public int Stacklimit { get; set; }

    /// <summary>
    /// quality
    /// </summary>
    public int Quality { get; set; }

    /// <summary>
    /// sort  // [client]
    /// </summary>
    public int Sort { get; set; }

    public void FromDict(GodotDictionary dict)
    {
        Id = (int)dict["id"];
        Name = (string)dict["name"];
        Desc = (string)dict["desc"];
        Icon = (string)dict["icon"];
        Type = (int)dict["type"];
        Usable = (int)dict["usable"];
        Stacklimit = (int)dict["stacklimit"];
        Quality = (int)dict["quality"];
        Sort = (int)dict["sort"];
    }
}
