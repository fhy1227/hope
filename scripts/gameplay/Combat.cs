using Godot;
using Hope.SkillSystem;
using Hope.Systems;

namespace Hope.Core;

/// <summary>
/// 战斗对局根场景（<c>scenes/gameplay/combat/combat.tscn</c>）协调者：持有世界与 UI 层引用，清理战斗临时状态。
/// 背包与装备由 <see cref="Hope.Persistence.PersistenceMgr"/> 局外持久化，进关时不重置。
/// </summary>
public partial class Combat : Node
{
    /// <summary>全局单例；场景卸载后变为 null。ProcessMode 为 Always，暂停时仍可协调。</summary>
    public static Combat? Instance { get; private set; }

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
            Hope.Persistence.PersistenceMgr.Instance?.FlushSave();
            Instance = null;
        }
    }

    public override void _Ready()
    {
        ResetCombatState();
    }

    /// <summary>
    /// 进入战斗场景时清理临时战斗状态；不清理背包、装备（见存档方案）。
    /// </summary>
    public void ResetCombatState()
    {
        CooldownManager.Instance?.ClearAll();
        FuryResourceSystem.Instance?.Reset();
    }
}
