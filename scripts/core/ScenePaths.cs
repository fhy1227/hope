namespace Hope.Core;

/// <summary>
/// 常用场景资源路径集中定义。切换场景或 Instantiate 时引用此处的常量，避免字符串散落。
/// 路径均相对于 <c>res://</c>。
/// </summary>
public static class ScenePaths
{
    /// <summary>主菜单 UI 场景。</summary>
    public const string MainMenu = "res://scenes/ui/main_menu.tscn";

    /// <summary>对局根场景，挂载 <see cref="Main"/>。</summary>
    public const string Main = "res://scenes/main.tscn";

    /// <summary>游戏世界子场景（关卡、实体容器）。</summary>
    public const string GameWorld = "res://scenes/systems/game_world.tscn";

    /// <summary>玩家角色；由 RunManager 生成到 Entities。</summary>
    public const string Player = "res://scenes/characters/player.tscn";

    /// <summary>地面拾取物实体。</summary>
    public const string Pickup = "res://scenes/entities/pickup.tscn";

    /// <summary>暂停菜单 UI。</summary>
    public const string PauseMenu = "res://scenes/ui/pause_menu.tscn";

    /// <summary>默认竞技场关卡。</summary>
    public const string ArenaLevel = "res://scenes/levels/arena.tscn";
}
