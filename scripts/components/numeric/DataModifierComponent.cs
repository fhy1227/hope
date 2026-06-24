using System.Collections.Generic;
using Godot;
using Hope.Core;

namespace Hope.Components;

/// <summary>
/// 数据修改器节点：对 NumericComponent 施加常量/百分比加成。
/// 公式：final = ori * (1 + sumPercent / 100) + sumConstant
/// </summary>
public partial class DataModifierComponent : Node
{
    [Export]
    public NodePath NumericPath { get; set; } = new("../NumericComponent");

    private NumericComponent _numeric = null!;
    private readonly Dictionary<int, List<DataModifier>> _modifiers = new();

    public override void _Ready()
    {
        _numeric = GetNode<NumericComponent>(NumericPath);
    }

    public void Clear() => _modifiers.Clear();

    public void AddModifier(DataModifier modifier)
    {
        if (modifier.ModifierKey == (int)NumericType.None)
        {
            return;
        }

        if (!_modifiers.TryGetValue(modifier.ModifierKey, out var list))
        {
            list = new List<DataModifier>();
            _modifiers[modifier.ModifierKey] = list;
        }

        list.Add(modifier);
        UpdateToNumeric((NumericType)modifier.ModifierKey);
    }

    public void AddModifiers(IReadOnlyList<DataModifier> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
        {
            return;
        }

        var changedKeys = new HashSet<int>();
        foreach (var modifier in modifiers)
        {
            if (modifier.ModifierKey == (int)NumericType.None)
            {
                continue;
            }

            if (!_modifiers.TryGetValue(modifier.ModifierKey, out var list))
            {
                list = new List<DataModifier>();
                _modifiers[modifier.ModifierKey] = list;
            }

            list.Add(modifier);
            changedKeys.Add(modifier.ModifierKey);
        }

        foreach (var key in changedKeys)
        {
            UpdateToNumeric((NumericType)key);
        }
    }

    public void RemoveModifier(DataModifier modifier)
    {
        if (modifier.ModifierKey == (int)NumericType.None)
        {
            return;
        }

        if (!_modifiers.TryGetValue(modifier.ModifierKey, out var list))
        {
            GD.PushError($"[DataModifierComponent] 删除修改器失败: 不存在 key={modifier.ModifierKey}");
            return;
        }

        list.Remove(modifier);
        UpdateToNumeric((NumericType)modifier.ModifierKey);
    }

    public void RemoveModifiers(IReadOnlyList<DataModifier> modifiers)
    {
        if (modifiers == null || modifiers.Count == 0)
        {
            return;
        }

        var changedKeys = new HashSet<int>();
        foreach (var modifier in modifiers)
        {
            if (!_modifiers.TryGetValue(modifier.ModifierKey, out var list))
            {
                GD.PushError($"[DataModifierComponent] 删除修改器失败: 不存在 key={modifier.ModifierKey}");
                continue;
            }

            list.Remove(modifier);
            changedKeys.Add(modifier.ModifierKey);
        }

        foreach (var key in changedKeys)
        {
            UpdateToNumeric((NumericType)key);
        }
    }

    public DataModifier UpdateModifier(DataModifier modifier, float newValue)
    {
        if (!_modifiers.TryGetValue(modifier.ModifierKey, out var list))
        {
            GD.PushError($"[DataModifierComponent] 更新修改器失败: 不存在 key={modifier.ModifierKey}");
            return default;
        }

        list.Remove(modifier);
        var updated = new DataModifier(modifier.ModifierType, (NumericType)modifier.ModifierKey, newValue);
        AddModifier(updated);
        return updated;
    }

    public void RecalculateAll()
    {
        foreach (var key in _modifiers.Keys)
        {
            UpdateToNumeric((NumericType)key);
        }
    }

    private void UpdateToNumeric(NumericType numericType)
    {
        if (numericType == NumericType.None)
        {
            return;
        }

        var ori = _numeric.GetOriValue(numericType);
        _numeric[numericType] = ApplyModifiers((int)numericType, ori);
    }

    private float ApplyModifiers(int modifierKey, float value)
    {
        if (!_modifiers.TryGetValue(modifierKey, out var modifiers))
        {
            return value;
        }

        var constModifier = 0f;
        var perModifier = 0f;
        foreach (var modifier in modifiers)
        {
            if (modifier.ModifierType == ModifierType.Constant)
            {
                constModifier += modifier.ModifierValue;
            }
            else if (modifier.ModifierType == ModifierType.Percentage)
            {
                perModifier += modifier.ModifierValue;
            }
        }

        return value * (1f + perModifier / 100f) + constModifier;
    }
}
