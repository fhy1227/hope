using Godot;
using Hope.Config;
using Hope.Core;

namespace Hope.Components;

/// <summary>
/// 将 NumericComponent 的生命值与 HealthComponent 双向同步。
/// </summary>
public partial class NumericHealthSyncComponent : Node
{
    [Export]
    public NodePath NumericPath { get; set; } = new("../NumericComponent");

    [Export]
    public NodePath HealthPath { get; set; } = new("../HealthComponent");

    private NumericComponent _numeric = null!;
    private HealthComponent _health = null!;

    public override void _Ready()
    {
        _numeric = GetNode<NumericComponent>(NumericPath);
        _health = GetNode<HealthComponent>(HealthPath);
        _numeric.Changed += OnNumericChanged;
    }

    public override void _ExitTree()
    {
        if (_numeric != null)
        {
            _numeric.Changed -= OnNumericChanged;
        }
    }

    public void SyncHealthToNumeric()
    {
        _numeric[NumericType.Health] = _health.CurrentHealth;
    }

    public void SyncAll(bool refillHealth = false)
    {
        _health.SetMaxHealth((int)_numeric[NumericType.MaxHealth], refill: refillHealth);
        _health.SetCurrentHealth((int)_numeric[NumericType.Health]);
    }

    public void ApplyDamage(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _health.TakeDamage(amount);
        SyncHealthToNumeric();
    }

    public void ApplyDamageWithArmor(int amount, float armor)
    {
        if (amount <= 0)
        {
            return;
        }

        var damage = Mathf.Max((int)ParamsConfig.PlayerMinDamage, amount - (int)armor);
        ApplyDamage(damage);
    }

    private void OnNumericChanged(NumericType type, float value)
    {
        switch (type)
        {
            case NumericType.MaxHealth:
                _health.SetMaxHealth((int)value, refill: false);
                break;
            case NumericType.Health:
                _health.SetCurrentHealth((int)value);
                break;
        }
    }
}
