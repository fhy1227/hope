using Godot;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Hope.Config;

/// <summary>
/// 命运卡牌连锁配置；定义触发所需卡组与连锁生效效果。
/// 仅对当前 Run 生效，不进入局外存档。
/// </summary>
public partial class FateChainConfig : IConfigData
{
    /// <summary>连锁数值主键。</summary>
    public int Id { get; set; }

    /// <summary>连锁名称。</summary>
    public string ChainName { get; set; } = string.Empty;

    /// <summary>连锁效果描述。</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>触发所需卡牌 1（CardCode）。</summary>
    public string CardCode1 { get; set; } = string.Empty;

    /// <summary>触发所需卡牌 2（CardCode）。</summary>
    public string CardCode2 { get; set; } = string.Empty;

    /// <summary>触发所需卡牌 3（CardCode）。</summary>
    public string CardCode3 { get; set; } = string.Empty;

    /// <summary>连锁效果类型关键字。</summary>
    public string EffectType { get; set; } = string.Empty;

    /// <summary>连锁效果数值参数。</summary>
    public float EffectValue { get; set; }

    /// <summary>
    /// 从配置字典反序列化当前行。
    /// </summary>
    /// <param name="dict">配置表中的单行字典。</param>
    public void FromDict(GodotDictionary dict)
    {
        Id = (int)dict["id"];
        ChainName = (string)dict["chain_name"];
        Description = (string)dict["description"];
        CardCode1 = (string)dict["card_code_1"];
        CardCode2 = (string)dict["card_code_2"];
        CardCode3 = (string)dict["card_code_3"];
        EffectType = (string)dict["effect_type"];
        EffectValue = (float)dict["effect_value"];
    }
}
