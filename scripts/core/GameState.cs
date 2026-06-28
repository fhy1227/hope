namespace Hope.Core;

/// <summary>
/// 全局游戏流程状态，由 <see cref="Hope.GameManager"/> 维护并通过 EventBus 广播。
/// 与单局内 <see cref="RunPhase"/>（战斗/商店）不同，本枚举描述应用级暂停、菜单等。
/// </summary>
public enum GameState
{
    /// <summary>启动初始化，尚未进入菜单或主场景。</summary>
    Boot,

    /// <summary>主菜单等非对局界面。</summary>
    Menu,

    /// <summary>对局进行中（<c>GetTree().Paused == false</c>）。</summary>
    Playing,

    /// <summary>对局暂停（Esc 打开暂停菜单）。</summary>
    Paused,

    /// <summary>玩家死亡或对局结束。</summary>
    GameOver,
}
