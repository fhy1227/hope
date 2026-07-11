using Hope.Config;

namespace Hope.SkillSystem;

/// <summary>强化分支对单次施放的数值修饰。</summary>
public readonly struct SkillEffectModifiers
{
    public float DamageMult { get; init; }
    public float RadiusMult { get; init; }
    public float RangeMult { get; init; }
    public float CooldownDelta { get; init; }
    public float FuryBonus { get; init; }
    public float KnockbackBonus { get; init; }
    public float EliteDamageMult { get; init; }
    public float DoubleDamageChance { get; init; }

    public static SkillEffectModifiers Identity => new()
    {
        DamageMult = 1f,
        RadiusMult = 1f,
        RangeMult = 1f,
        EliteDamageMult = 1f,
    };
}

/// <summary>将已选强化分支映射为施放修饰符。</summary>
public static class SkillEnhancementResolver
{
    /// <summary>解析技能当前选用的强化修饰符。</summary>
    public static SkillEffectModifiers Resolve(string skillId, PlayerSkillState state)
    {
        if (!state.ChosenEnhancements.TryGetValue(skillId, out var enhancementId))
        {
            return SkillEffectModifiers.Identity;
        }

        return ResolveEnhancement(enhancementId);
    }

    private static SkillEffectModifiers ResolveEnhancement(string enhancementId) =>
        enhancementId switch
        {
            "bash_enhance_1" => new SkillEffectModifiers { FuryBonus = 3f },
            "bash_enhance_2" => new SkillEffectModifiers { DoubleDamageChance = 0.3f },
            "ww_enhance_2" => new SkillEffectModifiers { RadiusMult = 1.2f },
            "hammer_enhance_2" => new SkillEffectModifiers { CooldownDelta = -2f },
            "upheaval_enhance_1" => new SkillEffectModifiers { KnockbackBonus = 150f },
            "upheaval_enhance_2" => new SkillEffectModifiers { EliteDamageMult = 1.5f },
            "kick_enhance_2" => new SkillEffectModifiers { DamageMult = 1.3f },
            "leap_enhance_2" => new SkillEffectModifiers { RadiusMult = 1.3f, RangeMult = 1.3f },
            _ => SkillEffectModifiers.Identity,
        };
}
