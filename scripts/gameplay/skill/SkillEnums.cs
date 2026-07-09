namespace Hope.SkillSystem;

/// <summary>技能标签，决定技能在树中的分组页。</summary>
public enum ESkillTag
{
    Basic = 0,
    Core = 1,
    Defensive = 2,
    Brawling = 3,
    WeaponMastery = 4,
    Ultimate = 5,
    KeyPassive = 6,
}

/// <summary>技能目标类型，决定释放时的选取逻辑。</summary>
public enum ESkillTargetType
{
    None,
    Single,
    Area,
    Direction,
    Self,
    Ground,
    Projectile,
}

/// <summary>伤害类型。</summary>
public enum EDamageType
{
    Physical = 0,
    Fire = 1,
    Cold = 2,
    Lightning = 3,
    Poison = 4,
    Shadow = 5,
    Holy = 6,
}

/// <summary>被动触发时机。</summary>
public enum EPassiveTrigger
{
    OnHit,
    OnKill,
    OnDamageTaken,
    OnSkillCast,
    OnSkillEnd,
    OnCooldownEnd,
    OnResourceChange,
    OnDodge,
    OnCrit,
    OnLuckyHit,
    OnOverpower,
    OnVulnerableApply,
    OnFortify,
    Periodic,
    OnHealthBelow,
    OnEliteHit,
    OnCrowdControl,
}

/// <summary>控制状态类型。</summary>
public enum ECrowdControlType
{
    Stun,
    Freeze,
    Slow,
    Fear,
    Taunt,
    Root,
    Daze,
    Knockback,
    Knockdown,
    Tether,
}

/// <summary>技能释放结果。</summary>
public enum ESkillCastResult
{
    Success,
    OnCooldown,
    InsufficientResource,
    NoValidTarget,
    Interrupted,
    AlreadyActive,
    OutOfRange,
    Silenced,
    Rooted,
    Stunned,
    NotLearned,
}

/// <summary>命中区域形状。</summary>
public enum EHitShape
{
    SingleTarget,
    Circle,
    Cone,
    Rectangle,
    Arc,
    Projectile,
    Chain,
}

/// <summary>职业资源类型。</summary>
public enum EResourceType
{
    Fury,
    Mana,
    Essence,
    Energy,
    Spirit,
    None,
}
