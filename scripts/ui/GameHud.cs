using Godot;
using Hope.Core;
using Hope.Systems;

namespace Hope.UI;

/// <summary>
/// 战斗 HUD：生命、波次、倒计时、金币；右侧属性面板。
/// </summary>
public partial class GameHud : Control
{
    private Label _healthLabel = null!;
    private Label _waveLabel = null!;
    private Label _timerLabel = null!;
    private Label _goldLabel = null!;
    private Label _phaseLabel = null!;

    private Label _statHpLabel = null!;
    private Label _statDamageLabel = null!;
    private Label _statAttackSpeedLabel = null!;
    private Label _statSpeedLabel = null!;
    private Label _statRangeLabel = null!;
    private Label _statProjectileSpeedLabel = null!;
    private Label _statCritLabel = null!;
    private Label _statArmorLabel = null!;

    private RunManager? _runManager;

    public override void _Ready()
    {
        _healthLabel = GetNode<Label>("%HealthLabel");
        _waveLabel = GetNode<Label>("%WaveLabel");
        _timerLabel = GetNode<Label>("%TimerLabel");
        _goldLabel = GetNode<Label>("%GoldLabel");
        _phaseLabel = GetNode<Label>("%PhaseLabel");

        _statHpLabel = GetNode<Label>("%StatHpLabel");
        _statDamageLabel = GetNode<Label>("%StatDamageLabel");
        _statAttackSpeedLabel = GetNode<Label>("%StatAttackSpeedLabel");
        _statSpeedLabel = GetNode<Label>("%StatSpeedLabel");
        _statRangeLabel = GetNode<Label>("%StatRangeLabel");
        _statProjectileSpeedLabel = GetNode<Label>("%StatProjectileSpeedLabel");
        _statCritLabel = GetNode<Label>("%StatCritLabel");
        _statArmorLabel = GetNode<Label>("%StatArmorLabel");

        CallDeferred(MethodName.BindRunManager);

        if (EquipManager.Instance != null)
        {
            EquipManager.Instance.EquipmentChanged += OnEquipmentChanged;
        }

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
        if (EquipManager.Instance != null)
        {
            EquipManager.Instance.EquipmentChanged -= OnEquipmentChanged;
        }

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

    private void BindRunManager()
    {
        _runManager = GetTree().GetFirstNodeInGroup("run_manager") as RunManager;
        RefreshStatsPanel();
    }

    private void OnEquipmentChanged()
    {
        RefreshStatsPanel();
    }

    private void RefreshStatsPanel()
    {
        if (_runManager == null)
        {
            return;
        }

        var stats = _runManager.Stats;
        var bonus = EquipManager.Instance?.CurrentBonus ?? default;

        _statHpLabel.Text = FormatIntStat("生命上限", stats.MaxHealth + bonus.Hp, bonus.Hp);
        _statDamageLabel.Text = FormatFloatStat("伤害", stats.Damage, bonus.Damage);
        _statAttackSpeedLabel.Text = $"攻速  {stats.AttackSpeed:0.##}/s";
        _statSpeedLabel.Text = FormatFloatStat("移速", stats.Speed, bonus.Speed);
        _statRangeLabel.Text = $"射程  {stats.WeaponRange:0}";
        _statProjectileSpeedLabel.Text = $"弹道  {stats.ProjectileSpeed:0}";
        _statCritLabel.Text = bonus.Crit > 0f
            ? $"暴击  +{bonus.Crit:0.##}"
            : "暴击  0";
        _statArmorLabel.Text = bonus.Armor > 0
            ? $"护甲  +{bonus.Armor}"
            : "护甲  0";
    }

    private static string FormatIntStat(string name, int total, int bonus)
    {
        return bonus > 0 ? $"{name}  {total} (+{bonus})" : $"{name}  {total}";
    }

    private static string FormatFloatStat(string name, float baseValue, float bonus)
    {
        return bonus > 0f
            ? $"{name}  {baseValue:0.##} (+{bonus:0.##})"
            : $"{name}  {baseValue:0.##}";
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

        if ((RunPhase)phase == RunPhase.Combat)
        {
            RefreshStatsPanel();
        }
    }
}
