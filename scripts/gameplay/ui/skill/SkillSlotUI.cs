using Godot;
using Hope.SkillSystem;

namespace Hope.UI;

/// <summary>单个技能快捷键槽位 UI。</summary>
public partial class SkillSlotUI : PanelContainer
{
    private static readonly string[] KeyNames = ["Z", "X", "C", "V", "1", "2"];

    private TextureRect _icon = null!;
    private Label _keyLabel = null!;
    private Label _rankLabel = null!;
    private ColorRect _cooldownOverlay = null!;
    private Label _cooldownText = null!;

    public int SlotIndex { get; set; }

    private string _skillId = "";
    private float _cooldownTotal;
    private float _cooldownRemaining;

    public override void _Ready()
    {
        _icon = GetNode<TextureRect>("%Icon");
        _keyLabel = GetNode<Label>("%KeyLabel");
        _rankLabel = GetNode<Label>("%RankLabel");
        _cooldownOverlay = GetNode<ColorRect>("%CooldownOverlay");
        _cooldownText = GetNode<Label>("%CooldownText");
        _cooldownOverlay.Visible = false;
        GuiInput += OnGuiInput;
    }

    public void SetKeyLabel(int slotIndex)
    {
        SlotIndex = slotIndex;
        if (slotIndex >= 0 && slotIndex < KeyNames.Length)
        {
            _keyLabel.Text = KeyNames[slotIndex];
        }
    }

    public override void _Process(double delta)
    {
        if (_cooldownRemaining <= 0f)
        {
            _cooldownOverlay.Visible = false;
            return;
        }

        _cooldownRemaining -= (float)delta;
        if (_cooldownRemaining <= 0f)
        {
            _cooldownRemaining = 0f;
            _cooldownOverlay.Visible = false;
            return;
        }

        _cooldownOverlay.Visible = true;
        _cooldownText.Text = _cooldownRemaining.ToString("0.0");
        _cooldownOverlay.Scale = new Vector2(1f, _cooldownRemaining / Mathf.Max(0.01f, _cooldownTotal));
    }

    /// <summary>刷新槽位显示。</summary>
    public void Refresh(SkillDefinition? def, int rank)
    {
        _skillId = def?.SkillId ?? "";

        if (def == null || rank <= 0)
        {
            _icon.Texture = null;
            _icon.Modulate = new Color(1f, 1f, 1f, 0.25f);
            _rankLabel.Text = "";
            return;
        }

        _icon.Texture = def.Icon;
        _icon.Modulate = Colors.White;
        _rankLabel.Text = $"{rank}/{def.MaxRank}";
        Modulate = def.IsUltimate ? new Color(1f, 0.7f, 0.5f) : Colors.White;
    }

    public void StartCooldown(float duration)
    {
        _cooldownTotal = duration;
        _cooldownRemaining = duration;
    }

    private void OnGuiInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton btn && btn.Pressed && btn.ButtonIndex == MouseButton.Right)
        {
            if (string.IsNullOrEmpty(_skillId))
            {
                return;
            }

            Systems.SkillManager.Instance?.AssignToSlot("", SlotIndex);
        }
    }
}
