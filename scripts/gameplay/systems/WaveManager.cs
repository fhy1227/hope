using Godot;
using Hope.Config;

namespace Hope.Systems;

/// <summary>
/// 单波倒计时，时间结束后发出 WaveCompleted；副本 Boss 波无倒计时。
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
	private bool _bossWaveMode;
	private DungeonConfig? _dungeon;

	public void SetPaused(bool paused)
	{
		_gamePaused = paused;
	}

	/// <summary>应用副本波次参数。</summary>
	public void Initialize(DungeonConfig dungeon)
	{
		_dungeon = dungeon;
		WaveDuration = dungeon.WaveTimeBase;
		DurationGrowthPerWave = dungeon.WaveTimeIncrement;
	}

	public override void _Process(double delta)
	{
		if (!IsRunning || _gamePaused || _bossWaveMode)
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
		_bossWaveMode = false;
		CurrentWave = wave;
		TimeRemaining = WaveDuration + DurationGrowthPerWave * (wave - 1);
		IsRunning = true;
		Hope.EventBus.Instance?.EmitWaveStarted(wave, TimeRemaining);
		Hope.EventBus.Instance?.EmitWaveTimerChanged(TimeRemaining);
	}

	/// <summary>Boss 波：无倒计时，由 Boss 击杀触发通关。</summary>
	public void StartBossWave(int wave)
	{
		_bossWaveMode = true;
		CurrentWave = wave;
		TimeRemaining = 0f;
		IsRunning = false;
		Hope.EventBus.Instance?.EmitWaveStarted(wave, 0f);
		Hope.EventBus.Instance?.EmitWaveTimerChanged(0f);
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
