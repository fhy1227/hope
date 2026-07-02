using Godot;
using Hope.Config;

namespace Hope.Systems;

/// <summary>
/// 单波倒计时，时间结束后发出 WaveCompleted。
/// </summary>
public partial class WaveManager : Node
{
	[Export]
	public float WaveDuration { get; set; } = ParamsConfig.WaveDuration;

	[Export]
	public float DurationGrowthPerWave { get; set; } = ParamsConfig.WaveDurationGrowth;

	public int CurrentWave { get; private set; }
	public float TimeRemaining { get; private set; }
	public bool IsRunning { get; private set; }

	private bool _gamePaused;

	public void SetPaused(bool paused)
	{
		_gamePaused = paused;
	}

	public override void _Process(double delta)
	{
		if (!IsRunning || _gamePaused)
		{
			return;
		}

		TimeRemaining = Mathf.Max(TimeRemaining - (float)delta, 0f);
		Hope.EventBus.Instance?.EmitWaveTimerChanged(TimeRemaining);

		if (TimeRemaining <= 0f)
		{
			CompleteWave();
		}
	}

	public void StartWave(int wave)
	{
		CurrentWave = wave;
		TimeRemaining = WaveDuration + DurationGrowthPerWave * (wave - 1);
		IsRunning = true;
		Hope.EventBus.Instance?.EmitWaveStarted(wave, TimeRemaining);
		Hope.EventBus.Instance?.EmitWaveTimerChanged(TimeRemaining);
	}

	public void Stop()
	{
		IsRunning = false;
	}

	private void CompleteWave()
	{
		IsRunning = false;
		EmitSignal(SignalName.WaveCompleted, CurrentWave);
		Hope.EventBus.Instance?.EmitWaveEnded(CurrentWave);
	}

	[Signal]
	public delegate void WaveCompletedEventHandler(int wave);
}
