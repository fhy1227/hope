using Godot;
using Hope.Core;

namespace Hope.UI;

/// <summary>
/// 主菜单：开始游戏、设置、退出。
/// </summary>
public partial class MainMenu : Control
{
    private SettingsPanel _settingsPanel = null!;

    public override void _Ready()
    {
        _settingsPanel = GetNode<SettingsPanel>("%SettingsPanel");

        GetNode<Button>("%StartButton").Pressed += OnStartPressed;
        GetNode<Button>("%SettingsButton").Pressed += OnSettingsPressed;
        GetNode<Button>("%ExitButton").Pressed += OnExitPressed;

        GameManager.Instance?.ChangeState(GameState.Menu);
    }

    private void OnStartPressed()
    {
        GameManager.Instance?.ChangeScene(ScenePaths.Main);
    }

    private void OnSettingsPressed()
    {
        _settingsPanel.ShowSettings();
    }

    private void OnExitPressed()
    {
        GetTree().Quit();
    }
}
