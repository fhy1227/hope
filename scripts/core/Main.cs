using Godot;
using Hope.Systems;

namespace Hope.Core;

/// <summary>
/// 主游戏场景根协调者：持有世界与 UI 层引用，重置局内系统，不微操玩法细节。
/// </summary>
public partial class Main : Node
{
    public static Main? Instance { get; private set; }

    public GameWorld World => GetNode<GameWorld>("%GameWorld");
    public RunManager Run => World.RunManager;

    public CanvasLayer HudLayer => GetNode<CanvasLayer>("%UI_Hud");
    public CanvasLayer OverlayLayer => GetNode<CanvasLayer>("%UI_Overlay");
    public CanvasLayer PauseLayer => GetNode<CanvasLayer>("%UI_Pause");
    public CanvasLayer TransitionLayer => GetNode<CanvasLayer>("%UI_Transition");

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
        ResetRunState();
    }

    /// <summary>
    /// 新对局进入主场景时清空背包与装备（Autoload 局内状态重置入口）。
    /// </summary>
    public void ResetRunState()
    {
        InventoryManager.Instance?.Clear();
        EquipManager.Instance?.Clear();
    }
}
