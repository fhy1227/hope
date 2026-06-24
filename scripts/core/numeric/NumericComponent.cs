using System;
using System.Collections.Generic;
using Godot;

namespace Hope.Core;

/// <summary>
/// 数值容器：存储基础值与当前值，支持合并、快照与变更通知。
/// </summary>
public class NumericComponent
{
    private readonly Dictionary<NumericType, float> _numeric = new();
    private readonly Dictionary<NumericType, float> _oriNumeric = new();

    /// <summary>数值变更时触发（type, 新值）。</summary>
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

    /// <summary>将 other 的所有属性累加到当前组件。</summary>
    public void AddOtherNumeric(NumericComponent other)
    {
        foreach (var kv in other._numeric)
        {
            this[kv.Key] += kv.Value;
        }
    }

    /// <summary>合并多个 NumericComponent 的加总（返回新实例）。</summary>
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

            Changed?.Invoke(numericType, this[numericType]);

            if (numericType == NumericType.MaxHealth)
            {
                Changed?.Invoke(NumericType.Health, this[NumericType.Health]);
            }
        }
    }

    private void ApplyMaxHealthChange(float newMaxHealth)
    {
        var maxHealth = Mathf.Max(newMaxHealth, 1f);
        var oldMaxHealth = _numeric.GetValueOrDefault(NumericType.MaxHealth);
        var maxChange = maxHealth - oldMaxHealth;
        var oldHealth = _numeric.GetValueOrDefault(NumericType.Health);

        if (maxChange > 0f)
        {
            var newHealth = Mathf.Min(oldHealth + maxChange, maxHealth);
            _numeric[NumericType.Health] = newHealth;
        }
        else
        {
            _numeric[NumericType.Health] = Mathf.Min(oldHealth, maxHealth);
        }

        _numeric[NumericType.MaxHealth] = maxHealth;
    }

    /// <summary>从 RunStats 写入基础数值并建立原始快照（不触发 Changed）。</summary>
    public void InitFromRunStats(RunStats stats, bool refillHealth = true)
    {
        Clear();
        _numeric[NumericType.MaxHealth] = stats.MaxHealth;
        _numeric[NumericType.Health] = refillHealth ? stats.MaxHealth : 0f;
        _numeric[NumericType.Damage] = stats.Damage;
        _numeric[NumericType.AttackSpeed] = stats.AttackSpeed;
        _numeric[NumericType.MoveSpeed] = stats.Speed;
        _numeric[NumericType.WeaponRange] = stats.WeaponRange;
        _numeric[NumericType.ProjectileSpeed] = stats.ProjectileSpeed;
        InitOriNumeric();
    }

    /// <summary>将当前数值回写到 RunStats（不含 Health）。</summary>
    public void ApplyToRunStats(RunStats stats)
    {
        stats.MaxHealth = (int)this[NumericType.MaxHealth];
        stats.Damage = this[NumericType.Damage];
        stats.AttackSpeed = this[NumericType.AttackSpeed];
        stats.Speed = this[NumericType.MoveSpeed];
        stats.WeaponRange = this[NumericType.WeaponRange];
        stats.ProjectileSpeed = this[NumericType.ProjectileSpeed];
    }
}
