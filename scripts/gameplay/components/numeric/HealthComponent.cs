using Godot;
using Hope.Config;

namespace Hope.Components;

/// <summary>
/// 可复用的生命值组件，挂到任意 Node2D 下即可。
/// </summary>
public partial class HealthComponent : Node
{
    [Signal]
    public delegate void DiedEventHandler();

    [Signal]
    public delegate void ChangedEventHandler(int current, int max);

    [Export]
    public int MaxHealth { get; set; } = (int)ParamsConfig.HealthDefaultMax;

    [Export]
    public bool IsPlayer { get; set; }

    public int CurrentHealth { get; private set; }

    public override void _Ready()
    {
        CurrentHealth = MaxHealth;
        EmitSignal(SignalName.Changed, CurrentHealth, MaxHealth);

        if (IsPlayer)
        {
            Hope.EventBus.Instance?.EmitHealthChanged(CurrentHealth, MaxHealth);
        }
    }

    public void SetMaxHealth(int maxHealth, bool refill = false)
    {
        MaxHealth = Mathf.Max(maxHealth, 1);

        if (refill || CurrentHealth > MaxHealth)
        {
            CurrentHealth = MaxHealth;
        }

        EmitSignal(SignalName.Changed, CurrentHealth, MaxHealth);

        if (IsPlayer)
        {
            Hope.EventBus.Instance?.EmitHealthChanged(CurrentHealth, MaxHealth);
        }
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0)
        {
            return;
        }

        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        // 飘字显示攻击方造成的全额伤害，不因目标剩余生命截断
        Hope.EventBus.Instance?.EmitDamageTaken(amount, GetDamageNumberPosition(), IsPlayer);
        EmitSignal(SignalName.Changed, CurrentHealth, MaxHealth);

        if (IsPlayer)
        {
            Hope.EventBus.Instance?.EmitHealthChanged(CurrentHealth, MaxHealth);
        }

        if (CurrentHealth == 0)
        {
            EmitSignal(SignalName.Died);

            if (IsPlayer)
            {
                Hope.EventBus.Instance?.EmitPlayerDied();
            }
        }
    }

    private Vector2 GetDamageNumberPosition()
    {
        var parent = GetParent() as Node2D;
        if (parent == null)
        {
            return Vector2.Zero;
        }

        var healthBar = parent.GetNodeOrNull<UnitHealthBar>("UnitHealthBar");
        if (healthBar != null)
        {
            return healthBar.GlobalPosition + new Vector2(0f, -healthBar.BarSize.Y * 0.5f);
        }

        return parent.GlobalPosition + new Vector2(0f, ParamsConfig.HealthFloatTextOffsetY);
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || CurrentHealth <= 0)
        {
            return;
        }

        SetCurrentHealth(CurrentHealth + amount);
    }

    public void SetCurrentHealth(int current)
    {
        var clamped = Mathf.Clamp(current, 0, MaxHealth);
        if (CurrentHealth == clamped)
        {
            return;
        }

        CurrentHealth = clamped;
        EmitSignal(SignalName.Changed, CurrentHealth, MaxHealth);

        if (IsPlayer)
        {
            Hope.EventBus.Instance?.EmitHealthChanged(CurrentHealth, MaxHealth);
        }

        if (CurrentHealth == 0)
        {
            EmitSignal(SignalName.Died);

            if (IsPlayer)
            {
                Hope.EventBus.Instance?.EmitPlayerDied();
            }
        }
    }
}
