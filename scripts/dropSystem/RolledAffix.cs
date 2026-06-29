using Hope.Core;

namespace Hope.DropSystem;

/// <summary>
/// 掷骰后的词条实例（写入 ItemInstance.Affixes）。
/// </summary>
public class RolledAffix
{
    public string AffixId { get; set; } = "";
    public NumericType NumericType { get; set; }
    public ModifierType ModifierType { get; set; } = ModifierType.Constant;
    public float Value { get; set; }
}
