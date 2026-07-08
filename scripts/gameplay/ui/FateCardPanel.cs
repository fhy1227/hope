using Godot;
using Hope.Config;
using Hope.Core;
using Hope.Systems;

namespace Hope.UI;

/// <summary>
/// 命运织机三选一面板：监听阶段变化并展示候选卡牌，选择后交给 RunManager 应用。
/// 挂载在战斗 Overlay 层，始终使用 Always 模式确保暂停时可交互。
/// </summary>
public partial class FateCardPanel : PanelContainer
{
    private Label _titleLabel = null!;
    private Label _summaryLabel = null!;
    private HBoxContainer _cardsBox = null!;
    private RunManager? _runManager;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        _titleLabel = GetNode<Label>("%TitleLabel");
        _summaryLabel = GetNode<Label>("%SummaryLabel");
        _cardsBox = GetNode<HBoxContainer>("%CardsBox");
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
        CallDeferred(MethodName.BindRunManager);

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

    private void BindRunManager()
    {
        _runManager = Combat.Instance?.Run;
        if (_runManager != null && _runManager.Phase == RunPhase.FateCard)
        {
            ShowChoices();
        }
    }

    private void OnRunPhaseChanged(int phase)
    {
        if ((RunPhase)phase == RunPhase.FateCard)
        {
            ShowChoices();
            return;
        }

        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
    }

    private void ShowChoices()
    {
        if (_runManager == null)
        {
            return;
        }

        var cards = _runManager.GetFateCardOptions();
        if (cards.Count == 0)
        {
            _runManager.SkipFateCardAndEnterShop();
            return;
        }

        _titleLabel.Text = $"第 {_runManager.Stats.Wave} 波结束 - 命运织机";
        _summaryLabel.Text = $"已持有 {_runManager.GetFateOwnedCount()} 张，连锁 {_runManager.GetFateChainCount()} 个";
        BuildCardButtons(cards);
        Visible = true;
        MouseFilter = MouseFilterEnum.Stop;
    }

    private void BuildCardButtons(Godot.Collections.Array<Godot.Collections.Dictionary> cards)
    {
        foreach (var child in _cardsBox.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var row in cards)
        {
            var id = row["id"].AsInt32();
            var cardCode = row["card_code"].AsString();
            var name = row["name"].AsString();
            var desc = row["desc"].AsString();
            var rarity = row["rarity"].AsInt32();

            var button = new Button
            {
                Text = $"{name}\n{desc}\n[{cardCode}]",
                CustomMinimumSize = new Vector2(220f, 128f),
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
            };
            button.AddThemeColorOverride("font_color", QualityColors.GetColor(rarity));
            button.Pressed += () => OnCardPressed(id);
            _cardsBox.AddChild(button);
        }
    }

    private void OnCardPressed(int cardId)
    {
        if (_runManager == null)
        {
            return;
        }

        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
        _runManager.SelectFateCardAndEnterShop(cardId);
    }
}
