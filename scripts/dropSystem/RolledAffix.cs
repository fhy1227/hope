namespace Hope.DropSystem;

/// <summary>
/// 掷骰后的词条实例（写入 ItemInstance.Affixes）。
/// </summary>
public class RolledAffix
{
    public string AffixId { get; set; } = "";
    public AffixStat Stat { get; set; }
    public float Value { get; set; }
}

/// <summary>
/// 词条可影响的属性类型。
/// </summary>
public enum AffixStat
{
    Hp = 1,
    Damage = 2,
    Speed = 3,
    Crit = 4,
    Armor = 5,
}
