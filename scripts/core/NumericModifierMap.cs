using System.Collections.Generic;
using Godot;

namespace Hope.Core;

/// <summary>
/// 通用数值修改器集合：按 (NumericType, ModifierType) 累加，可合并并转为 DataModifier 列表。
/// </summary>
public class NumericModifierMap
{
    public static NumericModifierMap Empty { get; } = new();

    private readonly Dictionary<(NumericType Type, ModifierType ModType), float> _entries = new();

    public bool IsEmpty => _entries.Count == 0;

    public IEnumerable<(NumericType Type, ModifierType ModType, float Value)> Entries
    {
        get
        {
            foreach (var kv in _entries)
            {
                yield return (kv.Key.Type, kv.Key.ModType, kv.Value);
            }
        }
    }

    public void Clear() => _entries.Clear();

    public void Add(NumericType type, ModifierType modType, float value)
    {
        if (type == NumericType.None || modType == ModifierType.None)
        {
            return;
        }

        if (Mathf.Abs(value) <= NumericDefine.Epsilon)
        {
            return;
        }

        var key = (type, modType);
        _entries[key] = _entries.GetValueOrDefault(key) + value;
    }

    public void MergeFrom(NumericModifierMap? other)
    {
        if (other == null || other.IsEmpty)
        {
            return;
        }

        foreach (var (type, modType, value) in other.Entries)
        {
            Add(type, modType, value);
        }
    }

    public float GetValue(NumericType type, ModifierType modType)
        => _entries.GetValueOrDefault((type, modType));

    public float ApplyToBase(NumericType type, float baseValue)
    {
        var constant = GetValue(type, ModifierType.Constant);
        var percent = GetValue(type, ModifierType.Percentage);
        return baseValue * (1f + percent / 100f) + constant;
    }

    public List<DataModifier> ToDataModifiers()
    {
        var list = new List<DataModifier>(_entries.Count);
        foreach (var kv in _entries)
        {
            list.Add(new DataModifier(kv.Key.ModType, kv.Key.Type, kv.Value));
        }

        return list;
    }

    public static string FormatEntry(NumericType type, ModifierType modType, float value, string format = "0.##")
    {
        if (modType == ModifierType.Percentage)
        {
            return value >= 0f
                ? $"{type}: +{value.ToString(format)}%"
                : $"{type}: {value.ToString(format)}%";
        }

        return value >= 0f
            ? $"{type}: +{value.ToString(format)}"
            : $"{type}: {value.ToString(format)}";
    }
}
