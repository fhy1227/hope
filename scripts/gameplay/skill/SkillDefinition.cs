using Godot;

namespace Hope.SkillSystem;

/// <summary>技能每级数值表。</summary>
[GlobalClass]
public partial class SkillRankData : Resource
{
    [Export] public int Rank { get; set; } = 1;
    [Export] public float DamageMultiplier { get; set; } = 1f;
    [Export] public float CooldownReduction { get; set; }
    [Export] public float ResourceCostModifier { get; set; } = 1f;
    [Export] public float AdditionalRange { get; set; }
    [Export] public float AdditionalRadius { get; set; }
    [Export] public string DescriptionOverride { get; set; } = "";
}

/// <summary>强化分支选项。</summary>
[GlobalClass]
public partial class SkillEnhancement : Resource
{
    [Export] public string EnhancementId { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    [Export] public string Description { get; set; } = "";
    [Export] public string EffectModifierId { get; set; } = "";
    [Export] public Texture2D? Icon { get; set; }
}

/// <summary>技能定义资源：静态模板，可在 Inspector 或代码中配置。</summary>
[GlobalClass]
public partial class SkillDefinition : Resource
{
    [ExportGroup("基础信息")]
    [Export] public string SkillId { get; set; } = "";
    [Export] public string SkillName { get; set; } = "";
    [Export] public string Description { get; set; } = "";
    [Export] public string FlavorText { get; set; } = "";
    [Export] public Texture2D? Icon { get; set; }

    [ExportGroup("分类")]
    [Export] public ESkillTag Tag { get; set; }
    [Export] public ESkillTargetType TargetType { get; set; }
    [Export] public EDamageType DamageType { get; set; }

    [ExportGroup("等级")]
    [Export] public int MaxRank { get; set; } = 5;
    [Export] public int PointsRequired { get; set; }
    [Export] public Godot.Collections.Array<string> PrerequisiteIds { get; set; } = [];
    [Export] public Godot.Collections.Array<SkillRankData> RankData { get; set; } = [];

    [ExportGroup("强化分支")]
    [Export] public Godot.Collections.Array<SkillEnhancement> Enhancements { get; set; } = [];

    [ExportGroup("效果数据")]
    [Export] public SkillEffectResource? EffectResource { get; set; }
    [Export] public bool IsPassive { get; set; }
    [Export] public bool IsUltimate { get; set; }

    [ExportGroup("视觉/音效")]
    [Export] public string CastAnimation { get; set; } = "";
    [Export] public string CastVfxPath { get; set; } = "";
    [Export] public string HitVfxPath { get; set; } = "";

    [ExportGroup("技能树布局")]
    [Export] public int TreePositionX { get; set; }
    [Export] public int TreePositionY { get; set; }

    /// <summary>获取指定等级的伤害倍率。</summary>
    public float GetDamageAtRank(int rank)
    {
        if (RankData == null || RankData.Count == 0)
        {
            return EffectResource?.WeaponDamagePercent / 100f ?? 1f;
        }

        if (rank < 1 || rank > RankData.Count)
        {
            return 0f;
        }

        return RankData[rank - 1].DamageMultiplier;
    }

    /// <summary>获取指定等级的描述文本（支持 {damage}、{rank} 模板变量）。</summary>
    public string GetDescriptionAtRank(int rank)
    {
        if (RankData != null && rank >= 1 && rank <= RankData.Count)
        {
            var descOverride = RankData[rank - 1].DescriptionOverride;
            if (!string.IsNullOrEmpty(descOverride))
            {
                return descOverride.Replace("{rank}", rank.ToString());
            }
        }

        return Description
            .Replace("{damage}", (GetDamageAtRank(rank) * 100f).ToString("F0"))
            .Replace("{rank}", rank.ToString());
    }
}
