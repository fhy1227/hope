using Godot;
using Hope.Systems;

namespace Hope.Core;

/// <summary>
/// 主游戏场景（<c>scenes/main.tscn</c>）根协调者：持有世界与 UI 层引用，重置局内 Autoload 状态。
/// 不处理玩家移动、攻击等玩法细节；跨模块访问入口为 <see cref="Instance"/>。
/// </summary>
public partial class Main : Node
{
    /// <summary>全局单例；场景卸载后变为 null。ProcessMode 为 Always，暂停时仍可协调。</summary>
    public static Main? Instance { get; private set; }

    /// <summary>可暂停的游戏世界节点，含关卡、实体容器与 <see cref="RunManager"/>。</summary>
    public GameWorld World => GetNode<GameWorld>("%GameWorld");

    /// <summary>对局流程管理（波次、玩家生成、商店阶段）；等价于 <c>World.RunManager</c>。</summary>
    public RunManager Run => World.RunManager;

    /// <summary>HUD 层（layer=<see cref="UiLayers.Hud"/>）：血条、波次等常驻界面。</summary>
    public CanvasLayer HudLayer => GetNode<CanvasLayer>("%UI_Hud");

    /// <summary>叠加层（layer=<see cref="UiLayers.Overlay"/>）：商店、背包等模态界面。</summary>
    public CanvasLayer OverlayLayer => GetNode<CanvasLayer>("%UI_Overlay");

    /// <summary>暂停层（layer=<see cref="UiLayers.Pause"/>）：暂停菜单。</summary>
    public CanvasLayer PauseLayer => GetNode<CanvasLayer>("%UI_Pause");

    /// <summary>转场层（layer=<see cref="UiLayers.Transition"/>）：场景切换遮罩等。</summary>
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
    /// 新增局内 Autoload 时须在此注册 Clear/Reset。
    /// </summary>
    public void ResetRunState()
    {
        InventoryManager.Instance?.Clear();
        EquipManager.Instance?.Clear();
    }
}
