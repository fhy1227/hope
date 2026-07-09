using Godot;

namespace Hope.SkillSystem;

/// <summary>野蛮人怒气资源系统（战斗内临时状态）。</summary>
public partial class FuryResourceSystem : Node
{
    public static FuryResourceSystem? Instance { get; private set; }

    [Export] public float Max { get; set; } = 100f;
    [Export] public float RegenPerSecond { get; set; } = 2f;
    [Export] public float RegenDelay { get; set; } = 3f;

    public float Current { get; private set; }
    public EResourceType ResourceType => EResourceType.Fury;

    private float _timeSinceLastSpend;

    public override void _EnterTree()
    {
        Instance = this;
        Current = Max * 0.3f;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void _Process(double delta)
    {
        var dt = (float)delta;
        _timeSinceLastSpend += dt;

        if (_timeSinceLastSpend >= RegenDelay && Current < Max)
        {
            Generate(RegenPerSecond * dt);
        }
    }

    public bool CanAfford(float amount) => Current >= amount;

    public bool Spend(float amount)
    {
        if (!CanAfford(amount))
        {
            return false;
        }

        Current -= amount;
        _timeSinceLastSpend = 0f;
        EventBus.Instance?.EmitSkillResourceChanged(Current, Max);
        return true;
    }

    public void Generate(float amount)
    {
        Current = Mathf.Min(Max, Current + amount);
        EventBus.Instance?.EmitSkillResourceChanged(Current, Max);
    }

    public void Reset()
    {
        Current = Max * 0.3f;
        _timeSinceLastSpend = 0f;
    }
}
