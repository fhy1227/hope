namespace Hope.Core;

public enum ModifierType
{
    None = 0,
    Constant = 1,
    Percentage = 2,
}

/// <summary>
/// 对某一 NumericType 的单条修改器（值类型，内容相同即可用于 Remove）。
/// </summary>
public struct DataModifier
{
    public ModifierType ModifierType { get; set; }
    public int ModifierKey { get; set; }
    public float ModifierValue { get; set; }

    public DataModifier(ModifierType type, NumericType key, float value)
    {
        ModifierType = type;
        ModifierKey = (int)key;
        ModifierValue = value;
    }
}
