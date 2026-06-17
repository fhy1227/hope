using Godot;
using Hope.Core;
using Hope.UI.Framework;

namespace Hope.UI;

/// <summary>
/// UI 根节点：通过 GodotUIFramework 管理 HUD 与弹窗。
/// </summary>
public partial class UiRoot : CanvasLayer
{
    private Node _hudGroup = null!;
    private Node _popupGroup = null!;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        AddToGroup("ui_root");

        _hudGroup = GetNode("HudGroup");
        _popupGroup = GetNode("PopupGroup");

        UiGroupBridge.ShowScene(_hudGroup, "game_hud");

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

    public void ShowShop()
    {
        UiGroupBridge.ShowScene(_popupGroup, "shop_panel");
    }

    private void OnRunPhaseChanged(int phase)
    {
        switch ((RunPhase)phase)
        {
            case RunPhase.Shop:
                ShowShop();
                break;
            case RunPhase.Combat:
            case RunPhase.GameOver:
                UiGroupBridge.CloseCurrentScene(_popupGroup);
                break;
        }
    }
}
