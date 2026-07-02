using Godot;
using Hope.Core;
using Hope.Systems;

namespace Hope;

/// <summary>
/// 游戏状态、暂停与场景切换入口。
/// </summary>
public partial class GameManager : Node
{
    public static GameManager? Instance { get; private set; }

    public GameState State { get; private set; } = GameState.Boot;

    public override void _EnterTree()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void _Ready()
    {
        ChangeState(GameState.Boot);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!@event.IsActionPressed("ui_cancel"))
        {
            return;
        }

        if (State == GameState.Combat)
        {
            Combat.Instance?.Run?.TogglePauseByEsc();
        }
    }

    public void ChangeState(GameState state)
    {
        State = state;
        EventBus.Instance?.EmitGameStateChanged((int)state);
    }

    public void Pause()
    {
        Combat.Instance?.Run?.PauseCombat();
    }

    public void Resume()
    {
        Combat.Instance?.Run?.ResumeCombat();
    }

    public void ChangeScene(string path)
    {
        GetTree().CallDeferred(SceneTree.MethodName.ChangeSceneToFile, path);
    }

    public void ReloadCurrentScene()
    {
        GetTree().CallDeferred(SceneTree.MethodName.ReloadCurrentScene);
    }
}
