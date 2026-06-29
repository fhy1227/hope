using Godot;

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
    public int MaxHealth { get; set; } = 3;

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

        var actualDamage = Mathf.Min(amount, CurrentHealth);
        CurrentHealth = Mathf.Max(CurrentHealth - amount, 0);
        Hope.EventBus.Instance?.EmitDamageTaken(actualDamage, GetDamageNumberPosition(), IsPlayer);
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
        return parent?.GlobalPosition + new Vector2(0f, -16f) ?? Vector2.Zero;
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
