using Godot;
using Hope.Core;
using Hope.Persistence;
using Hope.Systems;

namespace Hope.UI;

/// <summary>
/// 暂停菜单：Esc 暂停时显示，需在 Always 处理的 UI 层。
/// </summary>
public partial class PauseMenu : Control
{
    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;

        GetNode<Button>("%ResumeButton").Pressed += OnResumePressed;
        GetNode<Button>("%MainMenuButton").Pressed += OnMainMenuPressed;

        if (EventBus.Instance != null)
        {
            EventBus.Instance.CombatStateChanged += OnCombatStateChanged;
        }
    }

    public override void _ExitTree()
    {
        if (EventBus.Instance != null)
        {
            EventBus.Instance.CombatStateChanged -= OnCombatStateChanged;
        }
    }

    private void OnCombatStateChanged(int state)
    {
        var paused = (CombatState)state == CombatState.Paused;
        Visible = paused;
        MouseFilter = paused ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
    }

    private void OnResumePressed()
    {
        GameManager.Instance?.Resume();
    }

    private void OnMainMenuPressed()
    {
        GameManager.Instance?.Resume();
        PersistenceMgr.Instance?.FlushSave();
        GameManager.Instance?.ChangeScene(ScenePaths.MainMenu);
    }
}
