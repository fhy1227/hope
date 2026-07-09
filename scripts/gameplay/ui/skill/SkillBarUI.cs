using Godot;
using Hope.Core;
using Hope.SkillSystem;
using Hope.Systems;

namespace Hope.UI;

/// <summary>战斗 HUD 技能栏：6 个快捷键槽位 + 怒气条。</summary>
public partial class SkillBarUI : Control
{
    private readonly SkillSlotUI[] _slots = new SkillSlotUI[PlayerSkillState.SlotCount];
    private ProgressBar _furyBar = null!;
    private Label _furyLabel = null!;

    public override void _Ready()
    {
        for (var i = 0; i < PlayerSkillState.SlotCount; i++)
        {
            _slots[i] = GetNode<SkillSlotUI>($"%Slot{i}");
            _slots[i].SlotIndex = i;
            _slots[i].SetKeyLabel(i);
        }

        _furyBar = GetNode<ProgressBar>("%FuryBar");
        _furyLabel = GetNode<Label>("%FuryLabel");

        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.SkillStateChanged += RefreshAll;
        }

        if (EventBus.Instance != null)
        {
            EventBus.Instance.SkillCooldownStarted += OnCooldownStarted;
            EventBus.Instance.SkillResourceChanged += OnResourceChanged;
            EventBus.Instance.SkillLearned += OnSkillLearned;
            EventBus.Instance.SkillRefunded += OnSkillRefunded;
            EventBus.Instance.SkillTreeReset += OnTreeReset;
        }

        RefreshAll();
        RefreshFury();
    }

    public override void _ExitTree()
    {
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.SkillStateChanged -= RefreshAll;
        }

        if (EventBus.Instance != null)
        {
            EventBus.Instance.SkillCooldownStarted -= OnCooldownStarted;
            EventBus.Instance.SkillResourceChanged -= OnResourceChanged;
            EventBus.Instance.SkillLearned -= OnSkillLearned;
            EventBus.Instance.SkillRefunded -= OnSkillRefunded;
            EventBus.Instance.SkillTreeReset -= OnTreeReset;
        }
    }

    private void RefreshAll()
    {
        var manager = SkillManager.Instance;
        if (manager == null)
        {
            return;
        }

        for (var i = 0; i < PlayerSkillState.SlotCount; i++)
        {
            var slot = manager.State.SkillSlots[i];
            var def = string.IsNullOrEmpty(slot.SkillId) ? null : SkillDB.GetDefinition(slot.SkillId);
            var rank = manager.State.InvestedRanks.GetValueOrDefault(slot.SkillId, 0);
            _slots[i].Refresh(def, rank);
        }
    }

    private void RefreshFury()
    {
        var fury = FuryResourceSystem.Instance;
        if (fury == null)
        {
            return;
        }

        _furyBar.MaxValue = fury.Max;
        _furyBar.Value = fury.Current;
        _furyLabel.Text = $"怒气 {fury.Current:0}/{fury.Max:0}";
    }

    private void OnCooldownStarted(string skillId, float duration)
    {
        var manager = SkillManager.Instance;
        if (manager == null)
        {
            return;
        }

        for (var i = 0; i < PlayerSkillState.SlotCount; i++)
        {
            if (manager.State.SkillSlots[i].SkillId == skillId)
            {
                _slots[i].StartCooldown(duration);
            }
        }
    }

    private void OnResourceChanged(float current, float max)
    {
        _furyBar.MaxValue = max;
        _furyBar.Value = current;
        _furyLabel.Text = $"怒气 {current:0}/{max:0}";
    }

    private void OnSkillLearned(string skillId, int newRank) => RefreshAll();
    private void OnSkillRefunded(string skillId) => RefreshAll();
    private void OnTreeReset() => RefreshAll();
}
