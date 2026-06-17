using Godot;

namespace Hope;

/// <summary>
/// 全局事件总线，用于模块间解耦通信。
/// </summary>
public partial class EventBus : Node
{
    [Signal]
    public delegate void GameStateChangedEventHandler(int newState);

    [Signal]
    public delegate void PlayerDiedEventHandler();

    [Signal]
    public delegate void HealthChangedEventHandler(int current, int max);

    [Signal]
    public delegate void WaveStartedEventHandler(int wave, float duration);

    [Signal]
    public delegate void WaveTimerChangedEventHandler(float remaining);

    [Signal]
    public delegate void WaveEndedEventHandler(int wave);

    [Signal]
    public delegate void GoldChangedEventHandler(int gold);

    [Signal]
    public delegate void RunPhaseChangedEventHandler(int phase);

    public static EventBus? Instance { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void EmitGameStateChanged(int newState)
    {
        EmitSignal(SignalName.GameStateChanged, newState);
    }

    public void EmitPlayerDied()
    {
        EmitSignal(SignalName.PlayerDied);
    }

    public void EmitHealthChanged(int current, int max)
    {
        EmitSignal(SignalName.HealthChanged, current, max);
    }

    public void EmitWaveStarted(int wave, float duration)
    {
        EmitSignal(SignalName.WaveStarted, wave, duration);
    }

    public void EmitWaveTimerChanged(float remaining)
    {
        EmitSignal(SignalName.WaveTimerChanged, remaining);
    }

    public void EmitWaveEnded(int wave)
    {
        EmitSignal(SignalName.WaveEnded, wave);
    }

    public void EmitGoldChanged(int gold)
    {
        EmitSignal(SignalName.GoldChanged, gold);
    }

    public void EmitRunPhaseChanged(int phase)
    {
        EmitSignal(SignalName.RunPhaseChanged, phase);
    }
}
