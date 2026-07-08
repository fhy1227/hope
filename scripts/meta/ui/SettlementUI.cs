using Godot;
using Hope.Core;
using Hope.Persistence;
using Hope.Systems;

namespace Hope.UI;

/// <summary>
/// 副本结算界面：应用经验/金币、展示统计并返回主城或重试。
/// </summary>
public partial class SettlementUI : Control
{
    private Label _titleLabel = null!;
    private Label _statsLabel = null!;
    private Label _rewardLabel = null!;
    private Button _hubButton = null!;
    private Button _retryButton = null!;

    private SettlementResult _result = new();

    public override void _Ready()
    {
        GameManager.Instance?.ChangeState(GameState.Settlement);

        _titleLabel = GetNode<Label>("%TitleLabel");
        _statsLabel = GetNode<Label>("%StatsLabel");
        _rewardLabel = GetNode<Label>("%RewardLabel");
        _hubButton = GetNode<Button>("%HubButton");
        _retryButton = GetNode<Button>("%RetryButton");

        _hubButton.Pressed += OnHubPressed;
        _retryButton.Pressed += OnRetryPressed;

        _result = ApplySettlement();
        RefreshDisplay();
    }

    private SettlementResult ApplySettlement()
    {
        var result = new SettlementResult
        {
            IsVictory = RunSessionData.IsVictory,
            WaveReached = RunSessionData.WaveReached,
            EnemiesKilled = RunSessionData.EnemiesKilled,
            GoldEarned = RunSessionData.RunGoldEarned,
            ExpEarned = RunSessionData.RunExpEarned,
            DungeonId = RunSessionData.Dungeon?.Id ?? 0,
            DungeonName = RunSessionData.Dungeon?.NameKey ?? "",
        };

        var save = PersistenceMgr.Instance?.ActiveCharacter;
        if (save == null)
        {
            return result;
        }

        if (result.IsVictory)
        {
            save.Gold = RunSessionData.GoldBeforeRun + RunSessionData.RunGoldEarned;
            save.TotalRunsCompleted += 1;

            if (result.DungeonId > 0 && !save.ClearedDungeons.Contains(result.DungeonId))
            {
                save.ClearedDungeons.Add(result.DungeonId);
            }
        }
        else
        {
            save.TotalDeaths += 1;
            var penalty = CalculateDeathPenalty(RunSessionData.GoldBeforeRun, RunSessionData.RunGoldEarned);
            save.Gold = Mathf.Max(
                RunSessionData.GoldBeforeRun + RunSessionData.RunGoldEarned - penalty,
                0);
            result.GoldLost = penalty;
        }

        if (result.WaveReached > save.HighestWaveReached)
        {
            save.HighestWaveReached = result.WaveReached;
        }

        save.Experience += result.ExpEarned;
        while (save.Level < 50 && save.Experience >= ExpSystem.GetExpForNextLevel(save.Level))
        {
            save.Experience -= ExpSystem.GetExpForNextLevel(save.Level);
            save.Level += 1;
            ApplyLevelUpReward(save, save.Level);
            result.IsLevelUp = true;
            result.NewLevel = save.Level;
        }

        if (result.IsLevelUp && result.NewLevel == 0)
        {
            result.NewLevel = save.Level;
        }

        PersistenceMgr.Instance?.MarkDirty();
        PersistenceMgr.Instance?.FlushSave();
        RunSessionData.Clear();

        return result;
    }

    private static int CalculateDeathPenalty(int goldBeforeRun, int runGoldEarned)
    {
        return Mathf.FloorToInt(runGoldEarned * 0.5f) + Mathf.FloorToInt(goldBeforeRun * 0.1f);
    }

    private static void ApplyLevelUpReward(CharacterSaveData save, int newLevel)
    {
        var reward = ExpSystem.GetLevelUpReward(newLevel);
        save.Gold += reward.GoldBonus;
        save.BaseMaxHealth += reward.MaxHealthBonus;
        save.BaseDamage += reward.DamageBonus;
        save.BaseSpeed += reward.SpeedBonus;
        GD.Print($"[Settlement] 升级至 Lv.{newLevel} HP+{reward.MaxHealthBonus}");
    }

    private void RefreshDisplay()
    {
        _titleLabel.Text = _result.IsVictory ? "通关！" : "阵亡";
        _statsLabel.Text =
            $"副本：{_result.DungeonName}\n" +
            $"波次：{_result.WaveReached}\n" +
            $"击杀：{_result.EnemiesKilled}\n" +
            $"金币：+{_result.GoldEarned}" +
            (_result.GoldLost > 0 ? $"（损失 {_result.GoldLost}）" : "");

        var expLine = $"经验：+{_result.ExpEarned}";
        if (_result.IsLevelUp)
        {
            expLine += $"  升级！Lv.{_result.NewLevel}";
        }

        _rewardLabel.Text = expLine;
        _retryButton.Visible = _result.DungeonId > 0;
    }

    private void OnHubPressed()
    {
        GameManager.Instance?.ChangeScene(ScenePaths.Hub);
    }

    private void OnRetryPressed()
    {
        if (_result.DungeonId > 0)
        {
            DungeonManager.Instance?.SelectDungeon(_result.DungeonId);
        }

        DungeonManager.Instance?.EnterDungeon();
    }
}
