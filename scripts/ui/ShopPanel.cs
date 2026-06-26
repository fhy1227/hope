using Godot;
using Hope.Core;
using Hope.Systems;

namespace Hope.UI;

/// <summary>
/// 波间商店：三选一升级后进入下一波。
/// </summary>
public partial class ShopPanel : PanelContainer
{
    private VBoxContainer _optionsBox = null!;
    private Label _titleLabel = null!;
    private RunManager? _runManager;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        _optionsBox = GetNode<VBoxContainer>("%OptionsBox");
        _titleLabel = GetNode<Label>("%TitleLabel");

        Visible = false;
        ZIndex = 100;
        MouseFilter = MouseFilterEnum.Ignore;

        CallDeferred(MethodName.BindRunManager);

        if (EventBus.Instance != null)
        {
            EventBus.Instance.RunPhaseChanged += OnRunPhaseChanged;
        }
    }

    public override void _ExitTree()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.RunPhaseChanged -= OnRunPhaseChanged;
        }
    }

    private void BindRunManager()
    {
        _runManager = Main.Instance?.Run;
        if (_runManager == null)
        {
            GD.PushError("ShopPanel: Main.Instance.Run not found.");
            return;
        }

        if (_runManager.Phase == RunPhase.Shop)
        {
            ShowShop();
        }
    }

    private void OnRunPhaseChanged(int phase)
    {
        if ((RunPhase)phase == RunPhase.Shop)
        {
            ShowShop();
            return;
        }

        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
    }

    private void ShowShop()
    {
        RefreshOptions();
        Visible = true;
        MouseFilter = MouseFilterEnum.Stop;
    }

    private void RefreshOptions()
    {
        if (_runManager == null)
        {
            return;
        }

        _titleLabel.Text = $"第 {_runManager.Stats.Wave} 波完成 — 选择一项升级";

        foreach (var child in _optionsBox.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var option in _runManager.GetShopOptions())
        {
            var id = option["id"].AsString();
            var label = option["label"].AsString();

            var button = new Button
            {
                Text = label,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                ProcessMode = ProcessModeEnum.Always,
            };
            button.Pressed += () => OnOptionPressed(id);
            _optionsBox.AddChild(button);
        }
    }

    private void OnOptionPressed(string optionId)
    {
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
        _runManager?.ApplyShopUpgradeById(optionId);
    }
}
