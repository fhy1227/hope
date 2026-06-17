using Godot;
using Hope.Core;
using Hope.Systems;

namespace Hope.UI;

/// <summary>
/// 战斗 HUD：生命、波次、倒计时、金币。
/// </summary>
public partial class GameHud : Control
{
    private Label _healthLabel = null!;
    private Label _waveLabel = null!;
    private Label _timerLabel = null!;
    private Label _goldLabel = null!;
    private Label _phaseLabel = null!;

    public override void _Ready()
    {
        _healthLabel = GetNode<Label>("%HealthLabel");
        _waveLabel = GetNode<Label>("%WaveLabel");
        _timerLabel = GetNode<Label>("%TimerLabel");
        _goldLabel = GetNode<Label>("%GoldLabel");
        _phaseLabel = GetNode<Label>("%PhaseLabel");

        if (EventBus.Instance == null)
        {
            return;
        }

        EventBus.Instance.HealthChanged += OnHealthChanged;
        EventBus.Instance.WaveStarted += OnWaveStarted;
        EventBus.Instance.WaveTimerChanged += OnWaveTimerChanged;
        EventBus.Instance.GoldChanged += OnGoldChanged;
        EventBus.Instance.RunPhaseChanged += OnRunPhaseChanged;
    }

    public override void _ExitTree()
    {
        if (EventBus.Instance == null)
        {
            return;
        }

        EventBus.Instance.HealthChanged -= OnHealthChanged;
        EventBus.Instance.WaveStarted -= OnWaveStarted;
        EventBus.Instance.WaveTimerChanged -= OnWaveTimerChanged;
        EventBus.Instance.GoldChanged -= OnGoldChanged;
        EventBus.Instance.RunPhaseChanged -= OnRunPhaseChanged;
    }

    private void OnHealthChanged(int current, int max)
    {
        _healthLabel.Text = $"生命 {current}/{max}";
    }

    private void OnWaveStarted(int wave, float duration)
    {
        _waveLabel.Text = $"波次 {wave}";
        _timerLabel.Text = $"剩余 {duration:0}s";
    }

    private void OnWaveTimerChanged(float remaining)
    {
        _timerLabel.Text = $"剩余 {remaining:0.0}s";
    }

    private void OnGoldChanged(int gold)
    {
        _goldLabel.Text = $"金币 {gold}";
    }

    private void OnRunPhaseChanged(int phase)
    {
        _phaseLabel.Text = (RunPhase)phase switch
        {
            RunPhase.Combat => "战斗中",
            RunPhase.Shop => "商店阶段",
            RunPhase.GameOver => "游戏结束",
            _ => string.Empty,
        };
    }
}
