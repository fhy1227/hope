using Godot;
using Hope.Components;
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
    private Label _fateOwnedLabel = null!;
    private Label _fateChainLabel = null!;

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
        _fateOwnedLabel = GetNode<Label>("%FateOwnedLabel");
        _fateChainLabel = GetNode<Label>("%FateChainLabel");

        _statHpLabel = GetNode<Label>("%StatHpLabel");
        _statDamageLabel = GetNode<Label>("%StatDamageLabel");
        _statAttackSpeedLabel = GetNode<Label>("%StatAttackSpeedLabel");
        _statSpeedLabel = GetNode<Label>("%StatSpeedLabel");
        _statRangeLabel = GetNode<Label>("%StatRangeLabel");
        _statProjectileSpeedLabel = GetNode<Label>("%StatProjectileSpeedLabel");
        _statCritLabel = GetNode<Label>("%StatCritLabel");
        _statArmorLabel = GetNode<Label>("%StatArmorLabel");

        CallDeferred(MethodName.BindRunManager);

        GetNode<Button>("%InventoryButton").Pressed += OnInventoryButtonPressed;
        GetNode<Button>("%SkillTreeButton").Pressed += OnSkillTreeButtonPressed;

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
        EventBus.Instance.FateCardSelected += OnFateProgressChanged;
        EventBus.Instance.FateChainActivated += OnFateChainActivated;
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
        EventBus.Instance.FateCardSelected -= OnFateProgressChanged;
        EventBus.Instance.FateChainActivated -= OnFateChainActivated;
    }

    private void BindRunManager()
    {
        _runManager = GetTree().GetFirstNodeInGroup("run_manager") as RunManager;
        RefreshStatsPanel();
        RefreshFateSummary();
    }

    private void OnInventoryButtonPressed()
    {
        Combat.Instance?.OverlayLayer.GetNodeOrNull<InventoryUI>("InventoryUI")?.Toggle();
    }

    private void OnSkillTreeButtonPressed()
    {
        Combat.Instance?.OverlayLayer.GetNodeOrNull<SkillTreePanel>("SkillTreePanel")?.Toggle();
    }

    private void OnEquipmentChanged()
    {
        RefreshStatsPanel();
    }

    private void RefreshFateSummary()
    {
        if (_runManager == null)
        {
            _fateOwnedLabel.Text = "卡牌 x0";
            _fateChainLabel.Text = "连锁 x0";
            return;
        }

        _fateOwnedLabel.Text = $"卡牌 x{_runManager.GetFateOwnedCount()}";
        _fateChainLabel.Text = $"连锁 x{_runManager.GetFateChainCount()}";
    }

    private void RefreshStatsPanel()
    {
        if (_runManager == null)
        {
            return;
        }

        var stats = _runManager.Stats;
        var bonus = EquipManager.Instance?.CurrentBonus ?? NumericModifierMap.Empty;

        _statHpLabel.Text = FormatStatWithBonus("生命上限", stats.MaxHealth, bonus, NumericType.MaxHealth, "0");
        _statDamageLabel.Text = FormatStatWithBonus("伤害", stats.Damage, bonus, NumericType.Damage);
        _statAttackSpeedLabel.Text = FormatStatWithBonus("攻速", stats.AttackSpeed, bonus, NumericType.AttackSpeed, "0.##") + "/s";
        _statSpeedLabel.Text = FormatStatWithBonus("移速", stats.Speed, bonus, NumericType.MoveSpeed);
        _statRangeLabel.Text = FormatStatWithBonus("射程", stats.WeaponRange, bonus, NumericType.WeaponRange, "0");
        _statProjectileSpeedLabel.Text = FormatStatWithBonus("弹道", stats.ProjectileSpeed, bonus, NumericType.ProjectileSpeed, "0");
        _statCritLabel.Text = FormatBonusOnlyStat("暴击", bonus, NumericType.Crit);
        _statArmorLabel.Text = FormatBonusOnlyStat("护甲", bonus, NumericType.Armor, "0");
    }

    private static string FormatStatWithBonus(
        string name,
        float baseValue,
        NumericModifierMap bonus,
        NumericType type,
        string format = "0.##")
    {
        var constant = bonus.GetValue(type, ModifierType.Constant);
        var percent = bonus.GetValue(type, ModifierType.Percentage);
        if (Mathf.Abs(constant) <= NumericDefine.Epsilon && Mathf.Abs(percent) <= NumericDefine.Epsilon)
        {
            return $"{name}  {baseValue.ToString(format)}";
        }

        var total = bonus.ApplyToBase(type, baseValue);
        return $"{name}  {total.ToString(format)} ({FormatBonusSuffix(constant, percent, format)})";
    }

    private static string FormatBonusOnlyStat(
        string name,
        NumericModifierMap bonus,
        NumericType type,
        string format = "0.##")
    {
        var constant = bonus.GetValue(type, ModifierType.Constant);
        var percent = bonus.GetValue(type, ModifierType.Percentage);
        if (Mathf.Abs(constant) <= NumericDefine.Epsilon && Mathf.Abs(percent) <= NumericDefine.Epsilon)
        {
            return $"{name}  0";
        }

        return $"{name}  {FormatBonusSuffix(constant, percent, format)}";
    }

    private static string FormatBonusSuffix(float constant, float percent, string format)
    {
        var parts = new System.Collections.Generic.List<string>();
        if (Mathf.Abs(constant) > NumericDefine.Epsilon)
        {
            parts.Add(constant > 0f
                ? $"+{constant.ToString(format)}"
                : constant.ToString(format));
        }

        if (Mathf.Abs(percent) > NumericDefine.Epsilon)
        {
            parts.Add(percent > 0f
                ? $"+{percent.ToString(format)}%"
                : $"{percent.ToString(format)}%");
        }

        return string.Join(", ", parts);
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
            RunPhase.FateCard => "命运织机",
            RunPhase.Shop => "商店阶段",
            RunPhase.GameOver => "游戏结束",
            _ => string.Empty,
        };

        if ((RunPhase)phase == RunPhase.Combat || (RunPhase)phase == RunPhase.Shop)
        {
            RefreshStatsPanel();
        }

        RefreshFateSummary();
    }

    private void OnFateProgressChanged(int cardId, string cardCode)
    {
        RefreshFateSummary();
    }

    private void OnFateChainActivated(int chainId, string chainName)
    {
        RefreshFateSummary();
    }
}
