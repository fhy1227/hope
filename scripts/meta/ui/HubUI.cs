using Godot;
using Hope.Config;
using Hope.Core;
using Hope.Persistence;
using Hope.Systems;

namespace Hope.UI;

/// <summary>
/// 主城 Hub UI：展示角色状态、打开副本选择与背包。
/// </summary>
public partial class HubUI : Control
{
    private Label _levelLabel = null!;
    private Label _expLabel = null!;
    private Label _goldLabel = null!;
    private Control _dungeonSelectPanel = null!;
    private VBoxContainer _dungeonList = null!;

    public override void _Ready()
    {
        GameManager.Instance?.ChangeState(GameState.Hub);

        _levelLabel = GetNode<Label>("%LevelLabel");
        _expLabel = GetNode<Label>("%ExpLabel");
        _goldLabel = GetNode<Label>("%GoldLabel");
        _dungeonSelectPanel = GetNode<Control>("%DungeonSelectPanel");
        _dungeonList = GetNode<VBoxContainer>("%DungeonList");

        GetNode<Button>("%EnterDungeonButton").Pressed += OnEnterDungeonPressed;
        GetNode<Button>("%CloseDungeonButton").Pressed += () => _dungeonSelectPanel.Visible = false;
        GetNode<Button>("%MainMenuButton").Pressed += OnMainMenuPressed;

        _dungeonSelectPanel.Visible = false;
        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        var save = PersistenceMgr.Instance?.ActiveCharacter;
        if (save == null)
        {
            return;
        }

        _levelLabel.Text = $"Lv.{save.Level}";
        var nextExp = ExpSystem.GetExpForNextLevel(save.Level);
        _expLabel.Text = $"经验 {save.Experience}/{nextExp}";
        _goldLabel.Text = $"{save.Gold} G";
    }

    private void OnEnterDungeonPressed()
    {
        BuildDungeonList();
        _dungeonSelectPanel.Visible = true;
    }

    private void BuildDungeonList()
    {
        foreach (var child in _dungeonList.GetChildren())
        {
            child.QueueFree();
        }

        var manager = DungeonManager.Instance;
        if (manager == null)
        {
            return;
        }

        foreach (var dungeon in manager.GetAllDungeons())
        {
            var unlocked = manager.IsDungeonUnlocked(dungeon);
            var card = new VBoxContainer();
            card.AddThemeConstantOverride("separation", 4);

            var title = new Label
            {
                Text = $"{dungeon.NameKey}  Lv.{dungeon.MinPlayerLevel}+  {dungeon.TotalWaves}波",
            };
            card.AddChild(title);

            var desc = new Label
            {
                Text = dungeon.DescKey,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            card.AddChild(desc);

            var enterBtn = new Button
            {
                Text = unlocked ? "进入" : "未解锁",
                Disabled = !unlocked,
            };
            var dungeonId = dungeon.Id;
            enterBtn.Pressed += () => OnDungeonSelected(dungeonId);
            card.AddChild(enterBtn);

            _dungeonList.AddChild(card);
        }
    }

    private void OnDungeonSelected(int dungeonId)
    {
        if (DungeonManager.Instance?.SelectDungeon(dungeonId) != true)
        {
            return;
        }

        _dungeonSelectPanel.Visible = false;
        DungeonManager.Instance.EnterDungeon();
    }

    private void OnMainMenuPressed()
    {
        PersistenceMgr.Instance?.FlushSave();
        GameManager.Instance?.ChangeScene(ScenePaths.MainMenu);
    }
}
