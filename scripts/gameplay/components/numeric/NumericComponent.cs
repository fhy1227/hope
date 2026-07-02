using System;
using System.Collections.Generic;
using Godot;
using Hope.Core;

namespace Hope.Components;

/// <summary>
/// 数值容器节点：存储基础值与当前值，支持合并、快照与变更通知。
/// </summary>
public partial class NumericComponent : Node
{
    private readonly Dictionary<NumericType, float> _numeric = new();
    private readonly Dictionary<NumericType, float> _oriNumeric = new();

    [Signal]
    public delegate void ValueChangedEventHandler(int numericType, float value);

    public event Action<NumericType, float>? Changed;

    public void Clear()
    {
        _numeric.Clear();
        _oriNumeric.Clear();
    }

    public bool HasValue(NumericType key) => _numeric.ContainsKey(key);

    public bool HasOriValue(NumericType key) => _oriNumeric.ContainsKey(key);

    public float GetOriValue(NumericType key) => _oriNumeric.GetValueOrDefault(key);

    public void InitOriNumeric()
    {
        _oriNumeric.Clear();
        foreach (var kv in _numeric)
        {
            _oriNumeric[kv.Key] = kv.Value;
        }
    }

    public void ResetToOri()
    {
        _numeric.Clear();
        foreach (var kv in _oriNumeric)
        {
            _numeric[kv.Key] = kv.Value;
        }
    }

    public void CloneFrom(NumericComponent? other)
    {
        Clear();
        if (other == null)
        {
            return;
        }

        foreach (var kv in other._numeric)
        {
            _numeric[kv.Key] = kv.Value;
        }

        foreach (var kv in other._oriNumeric)
        {
            _oriNumeric[kv.Key] = kv.Value;
        }
    }

    public void AddOtherNumeric(NumericComponent other)
    {
        foreach (var kv in other._numeric)
        {
            this[kv.Key] += kv.Value;
        }
    }

    /// <summary>合并多个 NumericComponent 的加总（返回未入树的临时节点，仅用于计算）。</summary>
    public static NumericComponent GetTotal(params NumericComponent[] args)
    {
        var ret = new NumericComponent();
        foreach (var com in args)
        {
            ret.AddOtherNumeric(com);
        }

        return ret;
    }

    public float this[NumericType numericType]
    {
        get => _numeric.GetValueOrDefault(numericType);
        set
        {
            var current = _numeric.GetValueOrDefault(numericType);
            if (Mathf.Abs(current - value) <= NumericDefine.Epsilon)
            {
                return;
            }

            if (numericType == NumericType.MaxHealth)
            {
                ApplyMaxHealthChange(value);
            }
            else if (numericType is NumericType.Damage or NumericType.AttackSpeed or NumericType.Crit)
            {
                _numeric[numericType] = Mathf.Max(value, 0f);
            }
            else if (numericType == NumericType.MoveSpeed)
            {
                _numeric[numericType] = value > NumericDefine.MinMoveSpeed
                    ? value
                    : NumericDefine.MinMoveSpeed;
            }
            else if (numericType == NumericType.WeaponRange)
            {
                _numeric[numericType] = value > NumericDefine.MinWeaponRange
                    ? value
                    : NumericDefine.MinWeaponRange;
            }
            else
            {
                _numeric[numericType] = value;
            }

            NotifyChanged(numericType, this[numericType]);

            if (numericType == NumericType.MaxHealth)
            {
                NotifyChanged(NumericType.Health, this[NumericType.Health]);
            }
        }
    }

    private void SetRaw(NumericType numericType, float value) => _numeric[numericType] = value;

    private void ApplyMaxHealthChange(float newMaxHealth)
    {
        var maxHealth = Mathf.Max(newMaxHealth, 1f);
        var oldMaxHealth = _numeric.GetValueOrDefault(NumericType.MaxHealth);
        var maxChange = maxHealth - oldMaxHealth;
        var oldHealth = _numeric.GetValueOrDefault(NumericType.Health);

        float newHealth;
        if (maxChange > 0f)
        {
            // 上限提高：当前生命同步增加等量，不超过新上限
            newHealth = Mathf.Min(oldHealth + maxChange, maxHealth);
        }
        else
        {
            // 上限降低：当前生命不超过新上限
            newHealth = Mathf.Min(oldHealth, maxHealth);
        }

        if (Mathf.Abs(newHealth - oldHealth) > NumericDefine.Epsilon)
        {
            SetRaw(NumericType.Health, newHealth);
        }

        SetRaw(NumericType.MaxHealth, maxHealth);
    }

    public void InitFromRunStats(RunStats stats, bool refillHealth = true)
    {
        Clear();
        var maxHealth = Mathf.Max(stats.MaxHealth, 1f);
        SetRaw(NumericType.MaxHealth, maxHealth);
        SetRaw(NumericType.Health, refillHealth ? maxHealth : 0f);
        SetRaw(NumericType.Damage, stats.Damage);
        SetRaw(NumericType.AttackSpeed, stats.AttackSpeed);
        SetRaw(NumericType.MoveSpeed, stats.Speed);
        SetRaw(NumericType.WeaponRange, stats.WeaponRange);
        SetRaw(NumericType.ProjectileSpeed, stats.ProjectileSpeed);
        InitOriNumeric();
    }

    public void ApplyToRunStats(RunStats stats)
    {
        stats.MaxHealth = (int)this[NumericType.MaxHealth];
        stats.Damage = this[NumericType.Damage];
        stats.AttackSpeed = this[NumericType.AttackSpeed];
        stats.Speed = this[NumericType.MoveSpeed];
        stats.WeaponRange = this[NumericType.WeaponRange];
        stats.ProjectileSpeed = this[NumericType.ProjectileSpeed];
    }

    public void UpdateBaseFromRunStats(RunStats stats)
    {
        _oriNumeric[NumericType.MaxHealth] = stats.MaxHealth;
        _oriNumeric[NumericType.Damage] = stats.Damage;
        _oriNumeric[NumericType.AttackSpeed] = stats.AttackSpeed;
        _oriNumeric[NumericType.MoveSpeed] = stats.Speed;
        _oriNumeric[NumericType.WeaponRange] = stats.WeaponRange;
        _oriNumeric[NumericType.ProjectileSpeed] = stats.ProjectileSpeed;
    }

    public void InitFromConfig(int maxHealth, float moveSpeed, float damage, bool refillHealth = true)
    {
        Clear();
        _numeric[NumericType.MaxHealth] = maxHealth;
        _numeric[NumericType.Health] = refillHealth ? maxHealth : 0f;
        _numeric[NumericType.MoveSpeed] = moveSpeed;
        _numeric[NumericType.Damage] = damage;
        InitOriNumeric();
    }

    private void NotifyChanged(NumericType type, float value)
    {
        EmitSignal(SignalName.ValueChanged, (int)type, value);
        Changed?.Invoke(type, value);
    }
}
