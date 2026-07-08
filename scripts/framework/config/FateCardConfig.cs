using Godot;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Hope.Config;

/// <summary>
/// 命运卡牌配置行；描述单张卡的展示信息、稀有度与效果参数。
/// 由 <see cref="Hope.Systems.FateCardManager"/> 在战斗内读取与抽取。
/// </summary>
public partial class FateCardConfig : IConfigData
{
    /// <summary>数值主键；用于配置索引与序列化。</summary>
    public int Id { get; set; }

    /// <summary>卡牌业务 ID（如 C01/R12/E03）。</summary>
    public string CardCode { get; set; } = string.Empty;

    /// <summary>卡牌名称（直接展示）。</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>卡牌描述文本。</summary>
    public string Desc { get; set; } = string.Empty;

    /// <summary>稀有度：1=基础，2=稀有，3=史诗。</summary>
    public int Rarity { get; set; }

    /// <summary>基础抽取权重。</summary>
    public int Weight { get; set; }

    /// <summary>效果类型关键字（如 stat_damage、gold_gain_mult）。</summary>
    public string EffectType { get; set; } = string.Empty;

    /// <summary>效果主数值参数。</summary>
    public float EffectValue { get; set; }

    /// <summary>效果附加参数（JSON 字符串或空）。</summary>
    public string ExtraParams { get; set; } = string.Empty;

    /// <summary>
    /// 从配置字典反序列化当前行。
    /// </summary>
    /// <param name="dict">配置表中的单行字典。</param>
    public void FromDict(GodotDictionary dict)
    {
        Id = (int)dict["id"];
        CardCode = (string)dict["card_code"];
        Name = (string)dict["name"];
        Desc = (string)dict["desc"];
        Rarity = (int)dict["rarity"];
        Weight = (int)dict["weight"];
        EffectType = (string)dict["effect_type"];
        EffectValue = (float)dict["effect_value"];
        ExtraParams = dict.ContainsKey("extra_params") ? (string)dict["extra_params"] : string.Empty;
    }
}
