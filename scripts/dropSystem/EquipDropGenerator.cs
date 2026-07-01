using Godot;
using Hope.Config;
using Hope.Core;

namespace Hope.DropSystem;

/// <summary>
/// 暗黑4装备掉落生成器。
///
/// 流程（对齐 D4 公开机制简化版）：
/// 1. 掉落门禁 — drop_table.drop_rate × 敌型倍率
/// 2. 物品等级 — ilvl ≈ 怪物等级 ± 波动
/// 3. 稀有度掷骰 — quality.drop_weight + MF 偏向 + 5% 升档
/// 4. 槽位掷骰 — Smart Loot 倾向玩家可用槽
/// 5. 底材选择 — is_drop_base 池，level_req ≤ ilvl
/// 6. 词条生成 — quality.affix_count，按 slot_mask 过滤
/// 7. 威能 — 预设底材 aspect_id 或品质 has_aspect（随机池待 aspect.xlsx）
/// </summary>
public static class EquipDropGenerator
{
    /// <summary> 传奇底材对主属性的额外倍率 </summary>
    public const float LegendaryStatMultiplier = 1.35f;

    /// <summary>
    /// 尝试生成一件装备；未通过掉落门禁时返回 null。
    /// </summary>
    public static EquipDropResult? TryGenerate(DropContext ctx)
    {
        var dropChance = ctx.TableDropRate * ctx.DropRateMultiplier;
        if (GD.Randf() >= dropChance)
            return null;

        return Generate(ctx);
    }

    /// <summary>
    /// 强制生成一件装备（Boss 保底等）。
    /// </summary>
    public static EquipDropResult? Generate(DropContext ctx)
    {
        var itemLevel = ItemLevelCalculator.Roll(ctx);
        var rarity = RarityRoll.Roll(ctx, itemLevel);
        var slotType = BaseItemPicker.RollSlotType(ctx);
        var baseItem = BaseItemPicker.Pick(ctx, itemLevel, slotType);

        if (baseItem == null)
        {
            GD.PrintErr($"[EquipDropGenerator] 无可用底材: slot={slotType} ilvl={itemLevel}");
            return null;
        }

        var affixCount = AffixPool.GetAffixCount(rarity);
        var affixes = AffixPool.RollAffixes(slotType, itemLevel, affixCount);
        var aspectId = ResolveAspectId(baseItem, rarity);

        var instance = new ItemInstance
        {
            ConfigId = baseItem.Id,
            ItemLevel = itemLevel,
            RolledRarity = rarity,
            Affixes = affixes,
            AspectId = aspectId,
        };

        GD.Print($"[EquipDropGenerator] 掉落: {baseItem.NameKey} ilvl={itemLevel} rarity={rarity} affixes={affixes.Count} aspect={aspectId}");

        return new EquipDropResult
        {
            Item = instance,
            ItemLevel = itemLevel,
            Rarity = rarity,
        };
    }

    /// <summary>预设传奇底材使用配置 aspect_id；随机传奇在 has_aspect 时留空待 aspect 池掷骰。</summary>
    private static string ResolveAspectId(ItemConfig baseItem, int rarity)
    {
        if (!string.IsNullOrEmpty(baseItem.AspectId))
        {
            return baseItem.AspectId;
        }

        // 随机底材 + 传奇品质：威能池掷骰待 aspect.xlsx 接入
        var quality = ConfigManager.Get<QualityConfig>(rarity);
        return quality is { HasAspect: true } ? "" : "";
    }
}