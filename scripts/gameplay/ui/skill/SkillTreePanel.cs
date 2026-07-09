using Godot;
using System.Collections.Generic;
using Hope.SkillSystem;
using Hope.Systems;

namespace Hope.UI;

/// <summary>技能树面板：Tab 切换、节点加点、详情与快捷键装配。</summary>
public partial class SkillTreePanel : Control
{
    private static readonly (ESkillTag Tag, string Label)[] Tabs =
    [
        (ESkillTag.Basic, "基础"),
        (ESkillTag.Core, "核心"),
        (ESkillTag.Defensive, "防御"),
        (ESkillTag.Brawling, "搏斗"),
        (ESkillTag.WeaponMastery, "武器精通"),
        (ESkillTag.Ultimate, "终极"),
        (ESkillTag.KeyPassive, "关键被动"),
    ];

    private TabBar _tabBar = null!;
    private Control _canvas = null!;
    private Label _pointsLabel = null!;
    private PanelContainer _tooltip = null!;
    private Label _tooltipName = null!;
    private RichTextLabel _tooltipDesc = null!;
    private Label _tooltipStats = null!;
    private VBoxContainer _enhancementBox = null!;
    private Button _investButton = null!;
    private Button _refundButton = null!;
    private OptionButton _slotPicker = null!;

    private ESkillTag _currentTag = ESkillTag.Basic;
    private SkillDefinition? _selectedDef;
    private readonly Dictionary<string, SkillNodeButton> _nodes = new();
    private bool _pausedBySelf;

    public override void _Ready()
    {
        _tabBar = GetNode<TabBar>("%TabBar");
        _canvas = GetNode<Control>("%Canvas");
        _pointsLabel = GetNode<Label>("%PointsLabel");
        _tooltip = GetNode<PanelContainer>("%Tooltip");
        _tooltipName = GetNode<Label>("%Tooltip/TooltipVBox/NameLabel");
        _tooltipDesc = GetNode<RichTextLabel>("%Tooltip/TooltipVBox/DescLabel");
        _tooltipStats = GetNode<Label>("%Tooltip/TooltipVBox/StatsLabel");
        _enhancementBox = GetNode<VBoxContainer>("%EnhancementBox");
        _investButton = GetNode<Button>("%InvestButton");
        _refundButton = GetNode<Button>("%RefundButton");
        _slotPicker = GetNode<OptionButton>("%SlotPicker");

        for (var i = 0; i < Tabs.Length; i++)
        {
            _tabBar.AddTab(Tabs[i].Label);
        }

        _tabBar.TabChanged += OnTabChanged;
        GetNode<Button>("%CloseButton").Pressed += () => SetOpen(false);
        GetNode<Button>("%ResetButton").Pressed += OnResetPressed;
        _investButton.Pressed += OnInvestPressed;
        _refundButton.Pressed += OnRefundPressed;
        _slotPicker.ItemSelected += OnSlotSelected;

        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.SkillStateChanged += OnStateChanged;
        }

        _tooltip.Visible = false;
        Visible = false;
        RebuildTree();
        RefreshPoints();
    }

    public override void _ExitTree()
    {
        if (SkillManager.Instance != null)
        {
            SkillManager.Instance.SkillStateChanged -= OnStateChanged;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("toggle_skill_tree"))
        {
            GetViewport().SetInputAsHandled();
            Toggle();
            return;
        }

        if (Visible && @event.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            SetOpen(false);
        }
    }

    public void Toggle() => SetOpen(!Visible);

    public void SetOpen(bool open)
    {
        if (open == Visible)
        {
            return;
        }

        Visible = open;

        if (open)
        {
            _pausedBySelf = GetTree().Paused;
            GetTree().Paused = true;
            RefreshPoints();
            RefreshAllNodes();
        }
        else if (_pausedBySelf)
        {
            GetTree().Paused = false;
            _pausedBySelf = false;
            _tooltip.Visible = false;
        }
    }

    private void OnTabChanged(long index)
    {
        if (index < 0 || index >= Tabs.Length)
        {
            return;
        }

        _currentTag = Tabs[index].Tag;
        RebuildTree();
    }

    private void RebuildTree()
    {
        foreach (var node in _nodes.Values)
        {
            node.QueueFree();
        }

        _nodes.Clear();

        var skills = SkillDB.GetSkillsByTag(_currentTag);
        const float hSpacing = 140f;
        const float vSpacing = 100f;

        foreach (var def in skills)
        {
            var btn = new SkillNodeButton();
            btn.Position = new Vector2(def.TreePositionX * hSpacing + 40, def.TreePositionY * vSpacing + 20);
            btn.NodeClicked += OnNodeClicked;
            _canvas.AddChild(btn);
            btn.SetSkill(def);
            _nodes[def.SkillId] = btn;
        }
    }

    private void OnNodeClicked(string skillId)
    {
        var def = SkillDB.GetDefinition(skillId);
        if (def != null)
        {
            SelectSkill(def);
        }
    }

    private void SelectSkill(SkillDefinition def)
    {
        _selectedDef = def;
        var manager = SkillManager.Instance;
        if (manager == null)
        {
            return;
        }

        var rank = manager.State.InvestedRanks.GetValueOrDefault(def.SkillId, 0);
        _tooltipName.Text = def.SkillName;
        _tooltipDesc.Text = def.GetDescriptionAtRank(Mathf.Max(1, rank));

        if (def.EffectResource != null)
        {
            var previewRank = Mathf.Max(1, rank);
            var effect = def.EffectResource;
            _tooltipStats.Text =
                $"伤害: {effect.GetFinalDamage(previewRank, 100):F0}%\n" +
                $"冷却: {effect.GetCooldown(previewRank):F1}s\n" +
                $"消耗: {effect.GetResourceCost(previewRank):F0} 怒气";
        }
        else
        {
            _tooltipStats.Text = def.IsPassive ? "被动效果" : "";
        }

        _investButton.Text = rank > 0 ? "升级 (+1)" : "学习";
        _investButton.Disabled = !manager.State.CanInvest(def.SkillId, def);
        _refundButton.Visible = rank > 0;
        _refundButton.Disabled = !manager.State.CanRefund(def.SkillId);

        RebuildEnhancements(def, rank);
        RebuildSlotPicker(def, rank);
        _tooltip.Visible = true;
    }

    private void RebuildEnhancements(SkillDefinition def, int rank)
    {
        foreach (var child in _enhancementBox.GetChildren())
        {
            child.QueueFree();
        }

        if (rank <= 0 || def.Enhancements.Count == 0)
        {
            return;
        }

        var manager = SkillManager.Instance;
        var chosen = manager?.State.ChosenEnhancements.GetValueOrDefault(def.SkillId) ?? "";

        foreach (var enh in def.Enhancements)
        {
            var btn = new Button
            {
                Text = $"{enh.DisplayName}: {enh.Description}",
                ToggleMode = true,
                ButtonPressed = enh.EnhancementId == chosen,
            };
            var enhId = enh.EnhancementId;
            btn.Pressed += () => SkillManager.Instance?.ChooseEnhancement(def.SkillId, enhId);
            _enhancementBox.AddChild(btn);
        }
    }

    private void RebuildSlotPicker(SkillDefinition def, int rank)
    {
        _slotPicker.Clear();
        if (rank <= 0)
        {
            _slotPicker.Visible = false;
            return;
        }

        _slotPicker.Visible = true;
        _slotPicker.AddItem("装配到 Z", 0);
        _slotPicker.AddItem("装配到 X", 1);
        _slotPicker.AddItem("装配到 C", 2);
        _slotPicker.AddItem("装配到 V", 3);
        _slotPicker.AddItem("装配到 1", 4);
        _slotPicker.AddItem("装配到 2", 5);

        var manager = SkillManager.Instance;
        if (manager != null)
        {
            for (var i = 0; i < PlayerSkillState.SlotCount; i++)
            {
                if (manager.State.SkillSlots[i].SkillId == def.SkillId)
                {
                    _slotPicker.Select(i);
                    break;
                }
            }
        }
    }

    private void OnSlotSelected(long index)
    {
        if (_selectedDef == null)
        {
            return;
        }

        SkillManager.Instance?.AssignToSlot(_selectedDef.SkillId, (int)index);
    }

    private void OnInvestPressed()
    {
        if (_selectedDef == null)
        {
            return;
        }

        SkillManager.Instance?.Invest(_selectedDef.SkillId);
        SelectSkill(_selectedDef);
    }

    private void OnRefundPressed()
    {
        if (_selectedDef == null)
        {
            return;
        }

        SkillManager.Instance?.Refund(_selectedDef.SkillId);
        SelectSkill(_selectedDef);
    }

    private void OnResetPressed()
    {
        SkillManager.Instance?.RefundAll();
        if (_selectedDef != null)
        {
            SelectSkill(_selectedDef);
        }
    }

    private void OnStateChanged()
    {
        RefreshPoints();
        RefreshAllNodes();
        if (_selectedDef != null)
        {
            SelectSkill(_selectedDef);
        }
    }

    private void RefreshPoints()
    {
        var manager = SkillManager.Instance;
        if (manager == null)
        {
            _pointsLabel.Text = "可用点数: -";
            return;
        }

        var total = manager.State.TotalPointsSpent + manager.State.AvailablePoints;
        _pointsLabel.Text = $"可用点数: {manager.State.AvailablePoints}  |  已用: {manager.State.TotalPointsSpent}/{total}";
    }

    private void RefreshAllNodes()
    {
        foreach (var btn in _nodes.Values)
        {
            btn.RefreshState();
        }
    }
}
