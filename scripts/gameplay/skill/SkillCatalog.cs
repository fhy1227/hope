using Godot;

namespace Hope.SkillSystem;

/// <summary>
/// 野蛮人技能目录：在 SkillDB 启动时注册全部技能定义（阶段一用代码配置，后续可迁移为 .tres）。
/// </summary>
public static class SkillCatalog
{
    public static void RegisterBarbarianSkills(SkillDB db)
    {
        // ── 基础技能 ──
        db.Register(CreateSkill("barb_bash", "猛击", ESkillTag.Basic,
            "造成 {damage}% 武器伤害，产生 11 点怒气。", 0, 0,
            prereqs: [], weaponPct: 130f, furyGen: 11f, cooldown: 0f,
            enhancements: [
                ("bash_enhance_1", "怒涌", "额外产生 3 点怒气。"),
                ("bash_enhance_2", "痛击", "30% 概率造成双倍伤害。"),
            ]));

        db.Register(CreateSkill("barb_flay", "剥皮", ESkillTag.Basic,
            "造成 {damage}% 武器伤害，3 秒内额外 35% 流血伤害。", 1, 0,
            prereqs: [], weaponPct: 85f, cooldown: 0f,
            enhancements: [
                ("flay_enhance_1", "缓速", "流血同时减速 20%。"),
                ("flay_enhance_2", "致命", "对流血敌人 +10% 暴击。"),
            ]));

        db.Register(CreateSkill("barb_lunging", "突刺打击", ESkillTag.Basic,
            "造成 {damage}% 武器伤害，向目标突进。", 2, 0,
            prereqs: [], weaponPct: 150f, cooldown: 6f, radius: 60f,
            enhancements: [
                ("lunging_enhance_1", "疾风", "突进后 3 秒 +25% 攻速。"),
                ("lunging_enhance_2", "猎杀", "命中精英冷却 -3 秒。"),
            ]));

        // ── 核心技能 ──
        db.Register(CreateSkill("barb_whirlwind", "旋风斩", ESkillTag.Core,
            "每秒造成 {damage}% 武器伤害，持续消耗怒气。", 0, 1,
            prereqs: ["barb_bash"], weaponPct: 40f, resourceCost: 8f, cooldown: 0f,
            isChanneling: true, channelDuration: 3f, radius: 100f,
            enhancements: [
                ("ww_enhance_1", "血涌", "每命中一名敌人恢复 2 生命。"),
                ("ww_enhance_2", "狂风", "范围 +20%。"),
            ]));

        db.Register(CreateSkill("barb_upheaval", "扬石飞沙", ESkillTag.Core,
            "造成 {damage}% 武器伤害，扇形范围。", 1, 1,
            prereqs: ["barb_flay"], weaponPct: 300f, resourceCost: 25f, cooldown: 8f,
            hitShape: EHitShape.Cone, radius: 120f,
            enhancements: [
                ("upheaval_enhance_1", "震荡", "击退敌人。"),
                ("upheaval_enhance_2", "碎岩", "对精英 +50% 伤害。"),
            ]));

        db.Register(CreateSkill("barb_hammer", "先祖之锤", ESkillTag.Core,
            "造成 {damage}% 武器伤害，小范围重击。", 2, 1,
            prereqs: ["barb_lunging"], weaponPct: 350f, resourceCost: 30f, cooldown: 10f,
            radius: 90f, knockback: 200f,
            enhancements: [
                ("hammer_enhance_1", "震地", "眩晕 1.5 秒。"),
                ("hammer_enhance_2", "先祖", "冷却 -2 秒。"),
            ]));

        // ── 防御技能（需核心≥2 点）──
        db.Register(CreateSkill("barb_challenging", "挑战怒吼", ESkillTag.Defensive,
            "嘲讽周围敌人，获得 40% 减伤 6 秒。", 0, 2,
            prereqs: [], pointsRequired: 2, weaponPct: 0f, resourceCost: 20f, cooldown: 20f,
            targetType: ESkillTargetType.Self, radius: 150f, isPassive: false,
            enhancements: [
                ("cs_enhance_1", "坚韧", "减伤提升至 50%。"),
                ("cs_enhance_2", "威慑", "嘲讽时间 +2 秒。"),
            ]));

        db.Register(CreateSkill("barb_war_cry", "战吼", ESkillTag.Defensive,
            "自身 +25% 攻防 8 秒。", 1, 2,
            prereqs: [], pointsRequired: 2, resourceCost: 15f, cooldown: 15f,
            targetType: ESkillTargetType.Self,
            enhancements: [
                ("wc_enhance_1", "战意", "攻速 +15%。"),
                ("wc_enhance_2", "集结", "范围友军共享效果。"),
            ]));

        db.Register(CreateSkill("barb_iron_skin", "钢铁之肤", ESkillTag.Defensive,
            "获得最大生命 40% 的护盾 5 秒。", 2, 2,
            prereqs: [], pointsRequired: 2, resourceCost: 20f, cooldown: 18f,
            targetType: ESkillTargetType.Self,
            enhancements: [
                ("is_enhance_1", "铁壁", "护盾 +20%。"),
                ("is_enhance_2", "反震", "护盾期间反弹 10% 伤害。"),
            ]));

        // ── 搏斗技能 ──
        db.Register(CreateSkill("barb_leap", "跃击", ESkillTag.Brawling,
            "跳向目标区域，造成 {damage}% 伤害，减速 3 秒。", 0, 3,
            prereqs: [], pointsRequired: 4, weaponPct: 200f, resourceCost: 15f, cooldown: 12f,
            radius: 100f, targetType: ESkillTargetType.Ground,
            enhancements: [
                ("leap_enhance_1", "震击", "落地眩晕 1 秒。"),
                ("leap_enhance_2", "远跳", "射程 +30%。"),
            ]));

        db.Register(CreateSkill("barb_death_blow", "死亡之击", ESkillTag.Brawling,
            "造成 {damage}% 伤害，击杀重置冷却。", 1, 3,
            prereqs: [], pointsRequired: 4, weaponPct: 450f, resourceCost: 30f, cooldown: 16f,
            hitShape: EHitShape.SingleTarget, range: 80f,
            enhancements: [
                ("db_enhance_1", "处决", "对低血敌人 +100% 伤害。"),
                ("db_enhance_2", "连击", "未击杀时冷却 -4 秒。"),
            ]));

        db.Register(CreateSkill("barb_kick", "踢击", ESkillTag.Brawling,
            "击退单体，造成 {damage}% 伤害。", 2, 3,
            prereqs: [], pointsRequired: 4, weaponPct: 180f, resourceCost: 10f, cooldown: 8f,
            hitShape: EHitShape.SingleTarget, range: 60f, knockback: 350f,
            enhancements: [
                ("kick_enhance_1", "碎骨", "撞墙眩晕 3 秒。"),
                ("kick_enhance_2", "猛踢", "伤害 +30%。"),
            ]));

        // ── 终极技能 ──
        db.Register(CreateSkill("barb_wrath", "狂战士之怒", ESkillTag.Ultimate,
            "变身为狂战士，+50% 攻速移速，持续 15 秒。", 0, 4,
            prereqs: [], pointsRequired: 8, maxRank: 1, isUltimate: true,
            resourceCost: 0f, cooldown: 60f, targetType: ESkillTargetType.Self,
            enhancements: [
                ("wrath_enhance_1", "无尽", "持续时间 +5 秒。"),
                ("wrath_enhance_2", "狂怒", "伤害 +25%。"),
            ]));

        db.Register(CreateSkill("barb_steel_whirl", "钢铁漩涡", ESkillTag.Ultimate,
            "投掷 3 把铁链剑，每把造成 {damage}% 伤害。", 1, 4,
            prereqs: [], pointsRequired: 8, maxRank: 1, isUltimate: true,
            weaponPct: 300f, resourceCost: 0f, cooldown: 45f, radius: 120f,
            enhancements: [
                ("sw_enhance_1", "连锁", "额外 1 把剑。"),
                ("sw_enhance_2", "撕裂", "附加流血。"),
            ]));

        db.Register(CreateSkill("barb_call_of_ancients", "破地猛击", ESkillTag.Ultimate,
            "召唤 3 位先祖助战 15 秒。", 2, 4,
            prereqs: [], pointsRequired: 8, maxRank: 1, isUltimate: true,
            resourceCost: 0f, cooldown: 60f, targetType: ESkillTargetType.Self,
            enhancements: [
                ("coa_enhance_1", "先祖之力", "先祖伤害 +50%。"),
                ("coa_enhance_2", "持久", "持续时间 +5 秒。"),
            ]));

        // ── 关键被动 ──
        db.Register(CreateSkill("key_unconstrained", "无拘狂暴", ESkillTag.KeyPassive,
            "狂暴时效果增强 +25%。", 0, 5,
            prereqs: [], pointsRequired: 12, maxRank: 1, isPassive: true,
            enhancements: []));

        db.Register(CreateSkill("key_bleed_wounds", "涌血创伤", ESkillTag.KeyPassive,
            "对流血敌人暴击率 +15%，暴击伤害 +50%。", 1, 5,
            prereqs: [], pointsRequired: 12, maxRank: 1, isPassive: true,
            enhancements: []));

        db.Register(CreateSkill("key_unbridled_rage", "不羁怒火", ESkillTag.KeyPassive,
            "核心技能造成 +135% 伤害，消耗翻倍。", 2, 5,
            prereqs: [], pointsRequired: 12, maxRank: 1, isPassive: true,
            enhancements: []));

        db.Register(CreateSkill("key_power_strike", "强力重击", ESkillTag.KeyPassive,
            "压制概率 +10%，压制伤害 +50%。", 3, 5,
            prereqs: [], pointsRequired: 12, maxRank: 1, isPassive: true,
            enhancements: []));

        // ── 武器精通（被动占位）──
        db.Register(CreateSkill("wm_two_handed", "双手武器精通", ESkillTag.WeaponMastery,
            "双手武器伤害 +8%。", 0, 0,
            prereqs: [], maxRank: 3, isPassive: true, pointsRequired: 0,
            enhancements: []));

        db.Register(CreateSkill("wm_dual_wield", "双持精通", ESkillTag.WeaponMastery,
            "双持攻速 +6%。", 1, 0,
            prereqs: [], maxRank: 3, isPassive: true, pointsRequired: 0,
            enhancements: []));
    }

    private static SkillDefinition CreateSkill(
        string id, string name, ESkillTag tag, string desc,
        int treeX, int treeY,
        Godot.Collections.Array<string>? prereqs = null,
        int pointsRequired = 0,
        int maxRank = 5,
        bool isUltimate = false,
        bool isPassive = false,
        bool isChanneling = false,
        float channelDuration = 0f,
        float weaponPct = 100f,
        float furyGen = 0f,
        float resourceCost = 0f,
        float cooldown = 1f,
        float radius = 80f,
        float range = 0f,
        float knockback = 0f,
        EHitShape hitShape = EHitShape.Circle,
        ESkillTargetType targetType = ESkillTargetType.Area,
        (string id, string name, string desc)[]? enhancements = null)
    {
        var effect = new SkillEffectResource
        {
            UseWeaponDamage = weaponPct > 0f,
            WeaponDamagePercent = weaponPct,
            FuryGenerated = furyGen,
            ResourceCost = resourceCost,
            Cooldown = cooldown,
            Radius = radius,
            Range = range > 0 ? range : radius,
            HitShape = hitShape,
            KnockbackForce = knockback,
            IsChanneling = isChanneling,
            ChannelDuration = channelDuration,
        };

        var def = new SkillDefinition
        {
            SkillId = id,
            SkillName = name,
            Description = desc,
            Tag = tag,
            TargetType = isPassive ? ESkillTargetType.None : targetType,
            DamageType = EDamageType.Physical,
            MaxRank = maxRank,
            PointsRequired = pointsRequired,
            EffectResource = effect,
            IsPassive = isPassive,
            IsUltimate = isUltimate,
            TreePositionX = treeX,
            TreePositionY = treeY,
        };

        if (prereqs != null)
        {
            foreach (var p in prereqs)
            {
                def.PrerequisiteIds.Add(p);
            }
        }

        for (var rank = 1; rank <= maxRank; rank++)
        {
            def.RankData.Add(new SkillRankData
            {
                Rank = rank,
                DamageMultiplier = weaponPct / 100f * (1f + (rank - 1) * 0.08f),
            });
        }

        if (enhancements != null)
        {
            foreach (var (enhId, enhName, enhDesc) in enhancements)
            {
                def.Enhancements.Add(new SkillEnhancement
                {
                    EnhancementId = enhId,
                    DisplayName = enhName,
                    Description = enhDesc,
                    EffectModifierId = enhId,
                });
            }
        }

        return def;
    }
}
