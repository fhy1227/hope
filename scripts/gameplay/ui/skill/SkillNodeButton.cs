using Godot;
using Hope.SkillSystem;
using Hope.Systems;

namespace Hope.UI;

/// <summary>技能树单个节点按钮（纯代码构建子节点）。</summary>
public partial class SkillNodeButton : PanelContainer
{
    [Signal] public delegate void NodeClickedEventHandler(string skillId);

    private SkillDefinition? _def;
    private Label _nameLabel = null!;
    private Label _rankLabel = null!;
    private ColorRect _lockOverlay = null!;

    public string SkillId => _def?.SkillId ?? "";

    public override void _Ready()
    {
        CustomMinimumSize = new Vector2(110, 56);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(vbox);

        _nameLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        _rankLabel = new Label { HorizontalAlignment = HorizontalAlignment.Center };
        vbox.AddChild(_nameLabel);
        vbox.AddChild(_rankLabel);

        _lockOverlay = new ColorRect
        {
            Color = new Color(0, 0, 0, 0.45f),
            MouseFilter = MouseFilterEnum.Ignore,
        };
        _lockOverlay.SetAnchorsAndOffsetsPreset(LayoutPreset.FullRect);
        AddChild(_lockOverlay);

        GuiInput += OnGuiInput;
    }

    public void SetSkill(SkillDefinition def)
    {
        _def = def;
        _nameLabel.Text = def.SkillName;
        RefreshState();
    }

    public void RefreshState()
    {
        if (_def == null)
        {
            return;
        }

        var manager = SkillManager.Instance;
        if (manager == null)
        {
            return;
        }

        var rank = manager.State.InvestedRanks.GetValueOrDefault(_def.SkillId, 0);
        var canLearn = manager.State.CanInvest(_def.SkillId, _def);

        _rankLabel.Text = rank > 0 ? $"{rank}/{_def.MaxRank}" : "";

        if (rank >= _def.MaxRank)
        {
            Modulate = new Color(1f, 0.85f, 0.2f);
            _lockOverlay.Visible = false;
        }
        else if (rank > 0)
        {
            Modulate = new Color(0.7f, 0.85f, 1f);
            _lockOverlay.Visible = false;
        }
        else if (canLearn)
        {
            Modulate = new Color(0.9f, 0.75f, 0.3f);
            _lockOverlay.Visible = false;
        }
        else
        {
            Modulate = new Color(0.45f, 0.45f, 0.45f);
            _lockOverlay.Visible = true;
        }
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (_def == null || @event is not InputEventMouseButton btn || !btn.Pressed)
        {
            return;
        }

        if (btn.ButtonIndex == MouseButton.Left)
        {
            EmitSignal(SignalName.NodeClicked, _def.SkillId);
        }
        else if (btn.ButtonIndex == MouseButton.Right)
        {
            SkillManager.Instance?.RefundOneRank(_def.SkillId);
            RefreshState();
        }
    }
}
