using Godot;

namespace Hope.SkillSystem;

/// <summary>状态效果定义。</summary>
[GlobalClass]
public partial class StatusEffectDef : Resource
{
    [Export] public string EffectId { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    [Export] public EPassiveTrigger Trigger { get; set; }
    [Export] public float Duration { get; set; }
    [Export] public float TickInterval { get; set; }
    [Export] public float Magnitude { get; set; }
    [Export] public float MaxStacks { get; set; } = 1f;
    [Export] public bool IsDebuff { get; set; }
    [Export] public bool RemoveOnHit { get; set; }
    [Export] public Texture2D? Icon { get; set; }
    [Export] public string VfxPath { get; set; } = "";
}

/// <summary>技能效果资源：伤害、范围、冷却、状态等运行时参数模板。</summary>
[GlobalClass]
public partial class SkillEffectResource : Resource
{
    [ExportGroup("伤害参数")]
    [Export] public float BaseDamage { get; set; }
    [Export] public float DamagePerRank { get; set; }
    [Export] public float WeaponDamagePercent { get; set; } = 100f;
    [Export] public bool UseWeaponDamage { get; set; } = true;
    [Export] public float FuryGenerated { get; set; }

    [ExportGroup("范围参数")]
    [Export] public EHitShape HitShape { get; set; } = EHitShape.Circle;
    [Export] public float Radius { get; set; } = 80f;
    [Export] public float ConeAngle { get; set; } = 90f;
    [Export] public float Range { get; set; } = 200f;
    [Export] public float Width { get; set; }
    [Export] public float Height { get; set; }
    [Export] public int MaxTargets { get; set; }
    [Export] public bool Pierce { get; set; }
    [Export] public int MaxBounces { get; set; }

    [ExportGroup("消耗与冷却")]
    [Export] public float Cooldown { get; set; } = 1f;
    [Export] public float ResourceCost { get; set; }
    [Export] public float ResourceCostPerSecond { get; set; }
    [Export] public float CastTime { get; set; }
    [Export] public float ChannelDuration { get; set; }
    [Export] public bool IsChanneling { get; set; }
    [Export] public bool IsInterruptible { get; set; } = true;

    [ExportGroup("弹道参数")]
    [Export] public string ProjectileScenePath { get; set; } = "";
    [Export] public float ProjectileSpeed { get; set; } = 400f;
    [Export] public float ProjectileLifetime { get; set; } = 2f;
    [Export] public bool ProjectileHoming { get; set; }
    [Export] public float HomingStrength { get; set; }

    [ExportGroup("特效")]
    [Export] public string CastEffectPath { get; set; } = "";
    [Export] public string HitEffectPath { get; set; } = "";
    [Export] public float HitEffectScale { get; set; } = 1f;

    [ExportGroup("状态效果")]
    [Export] public Godot.Collections.Array<StatusEffectDef> StatusEffects { get; set; } = [];
    [Export] public Godot.Collections.Array<ECrowdControlType> CrowdControls { get; set; } = [];
    [Export] public float CrowdControlDuration { get; set; }
    [Export] public float CrowdControlChance { get; set; } = 1f;

    [ExportGroup("特殊机制")]
    [Export] public float LuckyHitChance { get; set; }
    [Export] public float KnockbackForce { get; set; }
    [Export] public float PullForce { get; set; }
    [Export] public Godot.Collections.Array<string> Tags { get; set; } = [];

    /// <summary>按技能等级计算最终伤害（武器伤害或固定值）。</summary>
    public float GetFinalDamage(int rank, float weaponDamage)
    {
        var rankBonus = 1f + (rank - 1) * 0.1f;
        if (UseWeaponDamage)
        {
            return weaponDamage * (WeaponDamagePercent / 100f) * rankBonus;
        }

        return (BaseDamage + DamagePerRank * (rank - 1)) * rankBonus;
    }

    /// <summary>按技能等级计算冷却时间。</summary>
    public float GetCooldown(int rank)
    {
        return Mathf.Max(0.1f, Cooldown - (rank - 1) * 0.5f);
    }

    /// <summary>按技能等级计算资源消耗。</summary>
    public float GetResourceCost(int rank)
    {
        return ResourceCost + (rank - 1) * ResourceCost * 0.05f;
    }
}
