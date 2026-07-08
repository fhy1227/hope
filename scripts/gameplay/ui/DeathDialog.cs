using Godot;
using Hope.Core;
using Hope.Persistence;
using Hope.Systems;

namespace Hope.UI;

/// <summary>
/// 玩家死亡提示：显示「已死亡」，点确定后回到主菜单。
/// 挂于 <c>UI_Pause</c> 层，<see cref="ProcessModeEnum.Always"/> 以便世界暂停时仍可交互。
/// </summary>
public partial class DeathDialog : Control
{
	public override void _Ready()
	{
		ProcessMode = ProcessModeEnum.Always;
		Visible = false;
		MouseFilter = MouseFilterEnum.Ignore;

		GetNode<Button>("%ConfirmButton").Pressed += OnConfirmPressed;

		if (EventBus.Instance != null)
		{
			EventBus.Instance.RunPhaseChanged += OnRunPhaseChanged;
		}
	}

	public override void _ExitTree()
	{
		if (EventBus.Instance != null)
		{
			EventBus.Instance.RunPhaseChanged -= OnRunPhaseChanged;
		}
	}

	private void OnRunPhaseChanged(int phase)
	{
		var runPhase = (RunPhase)phase;
		var show = runPhase == RunPhase.GameOver
		           && DungeonManager.Instance?.CurrentDungeon == null;
		Visible = show;
		MouseFilter = show ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;
	}

	private void OnConfirmPressed()
	{
		GetTree().Paused = false;
		PersistenceMgr.Instance?.FlushSave();
		GameManager.Instance?.ChangeScene(ScenePaths.MainMenu);
	}
}
