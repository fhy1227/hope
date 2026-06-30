using Godot;
using Hope.Core;

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
        if (@event.IsActionPressed("ui_cancel"))
        {
            if (State == GameState.Playing)
            {
                Pause();
            }
            else if (State == GameState.Paused)
            {
                Resume();
            }
        }
    }

    public void ChangeState(GameState state)
    {
        State = state;
        EventBus.Instance?.EmitGameStateChanged((int)state);
    }

    public void Pause()
    {
        GetTree().Paused = true;
        ChangeState(GameState.Paused);
    }

    public void Resume()
    {
        ChangeState(GameState.Playing);

        // 波间商店仍显示时保持世界暂停，避免 Esc 恢复后角色继续移动。
        if (Main.Instance?.Run?.Phase == RunPhase.Shop)
        {
            GetTree().Paused = true;
            return;
        }

        GetTree().Paused = false;
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
