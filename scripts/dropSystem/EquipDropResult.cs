using Hope.Core;

namespace Hope.DropSystem;

/// <summary>
/// 暗黑4风格装备掉落结果。
/// </summary>
public readonly struct EquipDropResult
{
    public ItemInstance Item { get; init; }
    public int ItemLevel { get; init; }
    public int Rarity { get; init; }
}
