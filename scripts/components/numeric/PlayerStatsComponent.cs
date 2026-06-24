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
        var savedHealth = refillHealth ? -1 : _health.CurrentHealth;

        _numeric.InitFromRunStats(stats, refillHealth);
        if (!refillHealth && savedHealth >= 0)
        {
            _numeric[NumericType.Health] = savedHealth;
        }

        ApplyEquipModifiers(EquipManager.Instance?.CurrentBonus ?? default);
        _healthSync.SyncAll(refillHealth);
    }

    public void ApplyEquipModifiers(EquipStatBonus bonus)
    {
        if (_equipModifiers.Count > 0)
        {
            _modifiers.RemoveModifiers(_equipModifiers);
            _equipModifiers.Clear();
        }

        if (bonus.Hp != 0)
        {
            _equipModifiers.Add(new DataModifier(ModifierType.Constant, NumericType.MaxHealth, bonus.Hp));
        }

        if (bonus.Damage != 0f)
        {
            _equipModifiers.Add(new DataModifier(ModifierType.Constant, NumericType.Damage, bonus.Damage));
        }

        if (bonus.Speed != 0f)
        {
            _equipModifiers.Add(new DataModifier(ModifierType.Constant, NumericType.MoveSpeed, bonus.Speed));
        }

        if (bonus.Crit != 0f)
        {
            _equipModifiers.Add(new DataModifier(ModifierType.Constant, NumericType.Crit, bonus.Crit));
        }

        if (bonus.Armor != 0)
        {
            _equipModifiers.Add(new DataModifier(ModifierType.Constant, NumericType.Armor, bonus.Armor));
        }

        if (_equipModifiers.Count > 0)
        {
            _modifiers.AddModifiers(_equipModifiers);
        }
    }

    public float GetMoveSpeed() => _numeric[NumericType.MoveSpeed];

    public float GetArmor() => _numeric[NumericType.Armor];

    public NumericComponent GetNumeric() => _numeric;

    public void TakeContactDamage(int amount)
    {
        _healthSync.ApplyDamageWithArmor(amount, GetArmor());
    }
}
