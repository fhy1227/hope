using Godot;
using Hope.Config;
using Hope.Core;
using Hope.Persistence;

namespace Hope.UI;

/// <summary>
/// 主菜单：继续游戏、新游戏、设置、退出。
/// </summary>
public partial class MainMenu : Control
{
    private static int DefaultSlotIndex => (int)ParamsConfig.SaveDefaultSlotIndex;

    private SettingsPanel _settingsPanel = null!;
    private Button _continueButton = null!;
    private Button _newGameButton = null!;

    public override void _Ready()
    {
        _settingsPanel = GetNode<SettingsPanel>("%SettingsPanel");
        _continueButton = GetNode<Button>("%ContinueButton");
        _newGameButton = GetNode<Button>("%NewGameButton");

        _continueButton.Pressed += OnContinuePressed;
        _newGameButton.Pressed += OnNewGamePressed;
        GetNode<Button>("%SettingsButton").Pressed += OnSettingsPressed;
        GetNode<Button>("%ExitButton").Pressed += OnExitPressed;

        RefreshContinueButton();
        GameManager.Instance?.ChangeState(GameState.Menu);
    }

    private void RefreshContinueButton()
    {
        var save = PersistenceMgr.Instance;
        if (save == null)
        {
            _continueButton.Disabled = true;
            return;
        }

        var slot = save.GetLastPlayedSlotIndex();
        if (slot < 0)
        {
            slot = DefaultSlotIndex;
        }

        _continueButton.Disabled = !save.HasProfile(slot);
    }

    private void OnContinuePressed()
    {
        var save = PersistenceMgr.Instance;
        if (save == null)
        {
            return;
        }

        var slot = save.GetLastPlayedSlotIndex();
        if (slot < 0)
        {
            slot = DefaultSlotIndex;
        }

        if (!save.Load(slot))
        {
            GD.PrintErr("[MainMenu] 读档失败");
            RefreshContinueButton();
            return;
        }

        GameManager.Instance?.ChangeScene(ScenePaths.Combat);
    }

    private void OnNewGamePressed()
    {
        var save = PersistenceMgr.Instance;
        if (save == null)
        {
            return;
        }

        if (!save.CreateProfile(DefaultSlotIndex, "冒险者"))
        {
            GD.PrintErr("[MainMenu] 创建角色失败");
            return;
        }

        RefreshContinueButton();
        GameManager.Instance?.ChangeScene(ScenePaths.Combat);
    }

    private void OnSettingsPressed()
    {
        _settingsPanel.ShowSettings();
    }

    private void OnExitPressed()
    {
        PersistenceMgr.Instance?.FlushSave();
        GetTree().Quit();
    }
}
