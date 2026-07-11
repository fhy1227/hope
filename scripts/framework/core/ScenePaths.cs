namespace Hope.Core;

/// <summary>
/// 常用场景资源路径集中定义。切换场景或 Instantiate 时引用此处的常量，避免字符串散落。
/// 路径均相对于 <c>res://</c>。
/// </summary>
public static class ScenePaths
{
    /// <summary>主菜单 UI 场景。</summary>
    public const string MainMenu = "res://scenes/meta/main_menu.tscn";

    /// <summary>主城 Hub 场景。</summary>
    public const string Hub = "res://scenes/meta/hub.tscn";

    /// <summary>副本结算场景。</summary>
    public const string Settlement = "res://scenes/meta/settlement.tscn";

    /// <summary>战斗对局根场景，挂载 <see cref="Combat"/>。</summary>
    public const string Combat = "res://scenes/gameplay/combat.tscn";

    /// <summary>游戏世界子场景（关卡、实体容器）。</summary>
    public const string GameWorld = "res://scenes/gameplay/game_world.tscn";

    /// <summary>玩家角色；由 RunManager 生成到 Entities。</summary>
    public const string Player = "res://scenes/gameplay/characters/player.tscn";

    /// <summary>地面拾取物实体。</summary>
    public const string Pickup = "res://scenes/gameplay/entities/pickup.tscn";

    /// <summary>战斗 HUD。</summary>
    public const string GameHud = "res://scenes/gameplay/game_hud.tscn";

    /// <summary>波间商店面板。</summary>
    public const string ShopPanel = "res://scenes/gameplay/shop_panel.tscn";

    /// <summary>背包与装备栏 UI。</summary>
    public const string InventoryUi = "res://scenes/gameplay/inventory_ui.tscn";

    /// <summary>暂停菜单 UI。</summary>
    public const string PauseMenu = "res://scenes/gameplay/pause_menu.tscn";

    /// <summary>玩家死亡确认对话框。</summary>
    public const string DeathDialog = "res://scenes/gameplay/death_dialog.tscn";

    /// <summary>单位头顶血条。</summary>
    public const string UnitHealthBar = "res://scenes/gameplay/unit_health_bar.tscn";

    /// <summary>默认竞技场关卡。</summary>
    public const string ArenaLevel = "res://scenes/gameplay/levels/arena.tscn";

    /// <summary>世界空间浮动伤害数字。</summary>
    public const string DamageNumber = "res://scenes/gameplay/entities/damage_number.tscn";

    /// <summary>范围技能圆形闪光特效。</summary>
    public const string CircleFlashEffect = "res://scenes/gameplay/effects/circle_flash_effect.tscn";

    /// <summary>技能树面板（加点、强化、重置）。</summary>
    public const string SkillTreePanel = "res://scenes/gameplay/skill_tree_panel.tscn";

    /// <summary>战斗 HUD 技能栏。</summary>
    public const string SkillBar = "res://scenes/gameplay/skill_bar.tscn";

    /// <summary>主城铁匠附魔面板。</summary>
    public const string BlacksmithPanel = "res://scenes/meta/blacksmith_panel.tscn";
}
