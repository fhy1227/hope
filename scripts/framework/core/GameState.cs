namespace Hope.Core;

/// <summary>
/// 全局游戏流程状态，由 <see cref="Hope.GameManager"/> 维护并通过 EventBus 广播。
/// 与单局内状态（如 <c>CombatState</c>、<see cref="RunPhase"/>）不同，本枚举仅描述应用级场景流程。
/// </summary>
public enum GameState
{
    /// <summary>启动初始化，尚未进入菜单或主场景。</summary>
    Boot,

    /// <summary>主菜单等非对局界面。</summary>
    Menu,

    /// <summary>主城 Hub：局外成长、选副本。</summary>
    Hub,

    /// <summary>战斗场景（<c>scenes/gameplay/combat.tscn</c>）内流程。</summary>
    Combat,

    /// <summary>副本结算界面。</summary>
    Settlement,
}
