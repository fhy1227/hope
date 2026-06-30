using System.Collections.Generic;
using Godot;
using Hope.Core;
using Hope.Systems;

namespace Hope.Components;

/// <summary>
/// 玩家数值编排：RunStats 初始化、装备修改器、与 Numeric 组件协作。
/// </summary>
public partial class PlayerStatsComponent : Node
{
    private static readonly NumericType[] RunStatTypes =
    [
        NumericType.MaxHealth,
        NumericType.Damage,
        NumericType.AttackSpeed,
        NumericType.MoveSpeed,
        NumericType.WeaponRange,
        NumericType.ProjectileSpeed,
    ];

    [Export]
    public NodePath NumericPath { get; set; } = new("../NumericComponent");

    [Export]
    public NodePath ModifierPath { get; set; } = new("../DataModifierComponent");

    [Export]
    public NodePath HealthPath { get; set; } = new("../HealthComponent");

    [Export]
    public NodePath HealthSyncPath { get; set; } = new("../NumericHealthSyncComponent");

    private NumericComponent _numeric = null!;
    private DataModifierComponent _modifiers = null!;
    private NumericHealthSyncComponent _healthSync = null!;
    private HealthComponent _health = null!;
    private readonly List<DataModifier> _equipModifiers = [];

    public override void _Ready()
    {
        _numeric = GetNode<NumericComponent>(NumericPath);
        _modifiers = GetNode<DataModifierComponent>(ModifierPath);
        _healthSync = GetNode<NumericHealthSyncComponent>(HealthSyncPath);
        _health = GetNode<HealthComponent>(HealthPath);
    }

    public void ApplyStats(RunStats stats, bool refillHealth)
    {
        var isFirst = !_numeric.HasOriValue(NumericType.MaxHealth);
        if (isFirst)
        {
            _numeric.InitFromRunStats(stats, refillHealth);
        }
        else
        {
            _numeric.UpdateBaseFromRunStats(stats);
        }

        ApplyEquipModifiers(EquipManager.Instance?.CurrentBonus ?? NumericModifierMap.Empty);

        if (!isFirst)
        {
            RefreshRunStatsOnNumeric();
            if (refillHealth)
            {
                _numeric[NumericType.Health] = _numeric[NumericType.MaxHealth];
            }
        }

        _healthSync.SyncAll(refillHealth);
    }

    /// <summary>
    /// 将 RunStats 基础值（ori）与装备修改器合并后写回当前战斗数值。
    /// </summary>
    private void RefreshRunStatsOnNumeric()
    {
        foreach (var type in RunStatTypes)
        {
            _numeric[type] = _modifiers.GetFinalValue(type);
        }
    }

    public void ApplyEquipModifiers(NumericModifierMap bonus)
    {
        if (_equipModifiers.Count > 0)
        {
            _modifiers.RemoveModifiers(_equipModifiers);
            _equipModifiers.Clear();
        }

        if (bonus == null || bonus.IsEmpty)
        {
            return;
        }

        _equipModifiers.AddRange(bonus.ToDataModifiers());
        _modifiers.AddModifiers(_equipModifiers);
    }

    public float GetMoveSpeed() => _numeric[NumericType.MoveSpeed];

    public float GetArmor() => _numeric[NumericType.Armor];

    public NumericComponent GetNumeric() => _numeric;

    public void TakeContactDamage(int amount)
    {
        _healthSync.ApplyDamageWithArmor(amount, GetArmor());
    }
}
