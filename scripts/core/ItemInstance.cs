using Godot;
using Godot.Collections;
using Hope.Config;
using Hope.DropSystem;
using Hope.Systems;

namespace Hope.Core;

/// <summary>
/// 运行时物品实例（背包里的一件物品）
/// Phase2：支持随机词条、物品等级（D4 ilvl）
/// </summary>
public partial class ItemInstance : Resource
{
    /// <summary> 配置ID（对应 item.json 的 id） </summary>
    public int ConfigId { get; set; }

    /// <summary> 唯一实例ID（区分同类物品） </summary>
    public string Uid { get; set; } = System.Guid.NewGuid().ToString();

    /// <summary> 堆叠数量（可堆叠物品） </summary>
    public int Count { get; set; } = 1;

    /// <summary> 掉落物品等级（D4 Item Power） </summary>
    public int ItemLevel { get; set; } = 1;

    /// <summary> 掷骰稀有度（0 表示沿用配置底材 rarity） </summary>
    public int RolledRarity { get; set; }

    /// <summary> 随机词条 </summary>
    public List<RolledAffix> Affixes { get; set; } = [];

    /// <summary> 获取配置数据（延迟读取） </summary>
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

    /// <summary> 是否可堆叠 </summary>
    public bool IsStackable => Config != null && Config.StackLimit > 0;

    /// <summary> 是否为装备 </summary>
    public bool IsEquip => Config != null && Config.SlotType > 0;

    /// <summary> 有效稀有度（优先掷骰结果） </summary>
    public int EffectiveRarity => RolledRarity > 0 ? RolledRarity : Config?.Rarity ?? 1;

    /// <summary>
    /// 计算底材 + 词条 + 传奇加成的属性（供 EquipManager 使用）。
    /// </summary>
    public EquipStatBonus ComputeStatBonus()
    {
        var bonus = new EquipStatBonus();
        if (Config == null)
            return bonus;

        var legendaryMul = EffectiveRarity >= 4 ? EquipDropGenerator.LegendaryStatMultiplier : 1f;

        bonus.Hp = Config.StatHp;
        bonus.Damage = Config.StatDamage * legendaryMul;
        bonus.Speed = Config.StatSpeed;
        bonus.Crit = Config.StatCrit;
        bonus.Armor = Config.StatArmor;

        foreach (var affix in Affixes)
            AffixPool.ApplyAffix(ref bonus, affix);

        return bonus;
    }
}
