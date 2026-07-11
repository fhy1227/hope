using Godot;
using Hope.Config;
using Hope.Core;
using Hope.Persistence;
using Hope.Systems;
using Hope.UI;

namespace Hope.UI;

/// <summary>
/// 主城 Hub UI：展示角色状态、打开副本选择、背包与铁匠。
/// </summary>
public partial class HubUI : Control
{
    private Label _levelLabel = null!;
    private Label _expLabel = null!;
    private Label _goldLabel = null!;
    private Control _dungeonSelectPanel = null!;
    private VBoxContainer _dungeonList = null!;
    private InventoryUI? _inventoryUi;
    private BlacksmithPanel? _blacksmithPanel;

    public override void _Ready()
    {
        AddToGroup("hub_ui");
        GameManager.Instance?.ChangeState(GameState.Hub);

        _levelLabel = GetNode<Label>("%LevelLabel");
        _expLabel = GetNode<Label>("%ExpLabel");
        _goldLabel = GetNode<Label>("%GoldLabel");
        _dungeonSelectPanel = GetNode<Control>("%DungeonSelectPanel");
        _dungeonList = GetNode<VBoxContainer>("%DungeonList");

        GetNode<Button>("%EnterDungeonButton").Pressed += OnEnterDungeonPressed;
        GetNode<Button>("%InventoryButton").Pressed += OnInventoryPressed;
        GetNode<Button>("%BlacksmithButton").Pressed += OnBlacksmithPressed;
        GetNode<Button>("%CloseDungeonButton").Pressed += () => _dungeonSelectPanel.Visible = false;
        GetNode<Button>("%MainMenuButton").Pressed += OnMainMenuPressed;

        _dungeonSelectPanel.Visible = false;
        SetupOverlays();
        RefreshDisplay();
    }

    private void SetupOverlays()
    {
        var invScene = GD.Load<PackedScene>(ScenePaths.InventoryUi);
        _inventoryUi = invScene.Instantiate<InventoryUI>();
        _inventoryUi.Visible = false;
        AddChild(_inventoryUi);

        var bsScene = GD.Load<PackedScene>(ScenePaths.BlacksmithPanel);
        _blacksmithPanel = bsScene.Instantiate<BlacksmithPanel>();
        AddChild(_blacksmithPanel);
    }

    /// <summary>刷新等级、经验、金币显示。</summary>
    public void RefreshDisplay()
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

    private void OnInventoryPressed()
    {
        _inventoryUi?.Toggle();
    }

    private void OnBlacksmithPressed()
    {
        _blacksmithPanel?.Open();
        RefreshDisplay();
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
