using Godot;

namespace Hope.Components;

/// <summary>
/// 玩家移动输入：按住鼠标左键时朝鼠标世界坐标持续移动（暗黑式 hold-to-move）。
/// 由 <see cref="Entities.Player"/> 与 <see cref="Actions.PlayerActionController"/> 每物理帧查询。
/// </summary>
public static class PlayerMoveInput
{
	/// <summary>距目标点小于此距离时视为到达，停止移动（避免光标压在角色上时抖动）。</summary>
	private const float StopDistanceSq = 64f;

	/// <summary>
	/// 读取本帧移动方向：未按住移动键、或距鼠标过近时返回零向量。
	/// </summary>
	/// <param name="player">用于坐标换算的玩家节点。</param>
	/// <returns>归一化朝向鼠标的方向；无移动意图时 <see cref="Vector2.Zero"/>。</returns>
	public static Vector2 GetDirection(Node2D player)
	{
		if (!Input.IsActionPressed("move_hold"))
		{
			return Vector2.Zero;
		}

		var toMouse = player.GetGlobalMousePosition() - player.GlobalPosition;
		if (toMouse.LengthSquared() <= StopDistanceSq)
		{
			return Vector2.Zero;
		}

		return toMouse.Normalized();
	}
}
