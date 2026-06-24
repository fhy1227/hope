using Godot;
using GodotArray = Godot.Collections.Array;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Hope.Config;

/// <summary>
/// 自动生成的配置类 - 对应 drop_table.xlsx
/// </summary>
public partial class DropTableConfig : IConfigData
{
    /// <summary>
    /// id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// enemy_type
    /// </summary>
    public string EnemyType { get; set; }

    /// <summary>
    /// item_id
    /// </summary>
    public int ItemId { get; set; }

    /// <summary>
    /// rarity
    /// </summary>
    public int Rarity { get; set; }

    /// <summary>
    /// drop_rate
    /// </summary>
    public float DropRate { get; set; }

    /// <summary>
    /// min_count
    /// </summary>
    public int MinCount { get; set; }

    /// <summary>
    /// max_count
    /// </summary>
    public int MaxCount { get; set; }

    public void FromDict(GodotDictionary dict)
    {
        Id = (int)dict["id"];
        EnemyType = (string)dict["enemy_type"];
        ItemId = (int)dict["item_id"];
        Rarity = (int)dict["rarity"];
        DropRate = (float)dict["drop_rate"];
        MinCount = (int)dict["min_count"];
        MaxCount = (int)dict["max_count"];
    }
}
