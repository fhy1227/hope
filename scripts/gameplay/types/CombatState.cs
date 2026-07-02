namespace Hope.Systems;

/// <summary>
/// 战斗场景内状态，由 <see cref="RunManager"/> 维护。
/// </summary>
public enum CombatState
{
	/// <summary>战斗进行中，可正常输入与推进波次。</summary>
	Playing,

	/// <summary>战斗内暂停（例如 Esc 打开暂停菜单）。</summary>
	Paused,

	/// <summary>战斗结束（玩家死亡或结算中）。</summary>
	GameOver,
}
