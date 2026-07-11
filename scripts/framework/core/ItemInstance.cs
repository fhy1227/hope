using Godot;
using Godot.Collections;
using Hope.Config;
using Hope.DropSystem;
using Hope.Systems;

namespace Hope.Core;

/// <summary>
/// 运行时物品实例（背包/装备槽中的一条记录），继承 Godot <see cref="Resource"/> 便于序列化。
/// 配置底材来自 item.json；随机掉落可附带 ilvl、掷骰稀有度与词条列表。
/// </summary>
public partial class ItemInstance : Resource
{
    /// <summary>配置表 ID，对应 <see cref="Config.ItemConfig"/> 的 id 字段。</summary>
    public int ConfigId { get; set; }

    /// <summary>实例唯一标识，区分同 ConfigId 的多件装备；默认新建 GUID。</summary>
    public string Uid { get; set; } = System.Guid.NewGuid().ToString();

    /// <summary>堆叠数量；不可堆叠装备恒为 1。</summary>
    public int Count { get; set; } = 1;

    /// <summary>掉落物品等级（Item Power），影响词条档位等。</summary>
    public int ItemLevel { get; set; } = 1;

    /// <summary>掷骰得到的稀有度；为 0 时沿用配置底材 <see cref="Config.ItemConfig.Rarity"/>。</summary>
    public int RolledRarity { get; set; }

    /// <summary>随机附加词条列表，由 <see cref="EquipDropGenerator"/> 生成。</summary>
    public List<RolledAffix> Affixes { get; set; } = [];

    /// <summary>威能 ID；预设传奇底材来自 item.aspect_id，随机传奇待铭印。</summary>
    public string AspectId { get; set; } = "";

    /// <summary>淬炼词缀（与随机词缀独立计数）。</summary>
    public List<RolledAffix> TemperedAffixes { get; set; } = [];

    /// <summary>精工等级。</summary>
    public int MasterworkLevel { get; set; }

    /// <summary>已附魔次数（每件装备上限见 CraftingManager）。</summary>
    public int EnchantCount { get; set; }

    /// <summary>
    /// 延迟从 <see cref="ConfigManager"/> 读取的配置；ConfigId 无效时 PrintErr 并可能返回 null。
    /// </summary>
    public Config.ItemConfig Config
    {
        get
        {
            var config = ConfigManager.Get<Config.ItemConfig>(ConfigId);
            if (config == null)
                GD.PrintErr($"[ItemInstance] 找不到配置: {ConfigId}");
            return config;
        }
    }

    /// <summary>配置中 StackLimit &gt; 0 时可与其他同类实例堆叠。</summary>
    public bool IsStackable => Config != null && Config.StackLimit > 0;

    /// <summary>SlotType &gt; 0 表示可装备到对应槽位。</summary>
    public bool IsEquip => Config != null && Config.SlotType > 0;

    /// <summary>展示与数值计算用的稀有度：优先 <see cref="RolledRarity"/>，否则底材 rarity，默认 1。</summary>
    public int EffectiveRarity => RolledRarity > 0 ? RolledRarity : Config?.Rarity ?? 1;

    /// <summary>
    /// 汇总底材属性、传奇倍率与全部词条，供 <see cref="EquipManager"/> 合并到玩家数值。
    /// </summary>
    /// <returns>可叠加的 <see cref="NumericModifierMap"/>；Config 缺失时返回空 map。</returns>
    public NumericModifierMap ComputeStatBonus()
    {
        var bonus = new NumericModifierMap();
        if (Config == null)
        {
            return bonus;
        }

        var legendaryMul = EffectiveRarity >= (int)ParamsConfig.DropLegendaryRarityThreshold
            ? EquipDropGenerator.LegendaryStatMultiplier
            : 1f;
        ItemStatMapping.ApplyBaseStats(Config, bonus, ItemLevel, legendaryMul);

        foreach (var affix in Affixes)
        {
            bonus.Add(affix.NumericType, affix.ModifierType, affix.Value);
        }

        foreach (var affix in TemperedAffixes)
        {
            bonus.Add(affix.NumericType, affix.ModifierType, affix.Value);
        }

        return bonus;
    }
}
