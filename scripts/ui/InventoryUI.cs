using Godot;
using System.Collections.Generic;
using System.Linq;
using Hope;
using Hope.Config;
using Hope.Core;
using Hope.Systems;

namespace Hope.UI;

/// <summary>
/// 背包 + 装备栏 UI（按 I 键切换）
/// 布局在 scenes/ui/inventory_ui.tscn，脚本只负责数据绑定与交互
/// </summary>
public partial class InventoryUI : Control
{
    // ── 品质颜色映射 ────────────────────────────
    private static StyleBoxFlat[]? _qualityBackgroundStyles;

    [Export] public PackedScene ItemSlotScene { get; set; } = null!;

    private GridContainer _inventoryGrid = null!;
    private Label _inventoryLabel = null!;
    private Label _emptyLabel = null!;
    private readonly Dictionary<int, List<Button>> _equipSlotButtons = new();
    private InventoryItemActionPopup _itemActionPopup = null!;
    private bool _pausedBySelf;

    public override void _Ready()
    {
        ItemSlotScene ??= GD.Load<PackedScene>(ParamsConfig.PathInventoryItemSlot);
        if (ItemSlotScene == null)
            GD.PushError("[InventoryUI] 未找到物品格子场景，请检查 inventory_item_slot.tscn");

        _inventoryGrid = GetNode<GridContainer>("%InventoryGrid");
        _inventoryLabel = GetNode<Label>("%InventoryLabel");
        _emptyLabel = GetNode<Label>("%EmptyLabel");

        BindEquipSlots();
        GetNode<Button>("%CloseButton").Pressed += () => SetOpen(false);
        GetNode<Button>("%SellLowRarityButton").Pressed += OnSellLowRarityClicked;

        _itemActionPopup = GetNode<InventoryItemActionPopup>("%ItemActionPopup");
        _itemActionPopup.EquipRequested += OnItemEquipRequested;
        _itemActionPopup.SellRequested += OnItemSellRequested;

        Visible = false;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.InventoryChanged += OnInventoryChanged;
        if (EquipManager.Instance != null)
            EquipManager.Instance.EquipmentChanged += OnEquipmentChanged;

        GD.Print("[InventoryUI] 初始化完成，按 I 键切换");
    }

    public override void _ExitTree()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.InventoryChanged -= OnInventoryChanged;
        if (EquipManager.Instance != null)
            EquipManager.Instance.EquipmentChanged -= OnEquipmentChanged;

        if (_itemActionPopup != null)
        {
            _itemActionPopup.EquipRequested -= OnItemEquipRequested;
            _itemActionPopup.SellRequested -= OnItemSellRequested;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("toggle_inventory"))
        {
            GetViewport().SetInputAsHandled();
            Toggle();
            return;
        }

        if (Visible && @event.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            if (_itemActionPopup.IsOpen)
            {
                _itemActionPopup.HidePopup();
                return;
            }

            SetOpen(false);
        }
    }

    private void BindEquipSlots()
    {
        _equipSlotButtons[1] = new List<Button>
        {
            GetNode<Button>("%EquipSlot_1_0"),
            GetNode<Button>("%EquipSlot_1_1"),
        };
        _equipSlotButtons[2] = new List<Button> { GetNode<Button>("%EquipSlot_2_0") };
        _equipSlotButtons[3] = new List<Button> { GetNode<Button>("%EquipSlot_3_0") };
        _equipSlotButtons[4] = new List<Button> { GetNode<Button>("%EquipSlot_4_0") };
        _equipSlotButtons[5] = new List<Button> { GetNode<Button>("%EquipSlot_5_0") };

        foreach (var (slotType, buttons) in _equipSlotButtons)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                var index = i;
                buttons[i].Pressed += () => OnEquipSlotClicked(slotType, index);
            }
        }
    }

    private void OnInventoryChanged()
    {
        RefreshInventoryGrid();
        RefreshEquipSlots();
    }

    private void OnEquipmentChanged()
    {
        RefreshInventoryGrid();
        RefreshEquipSlots();
    }

    private void RefreshInventoryGrid()
    {
        foreach (var child in _inventoryGrid.GetChildren())
        {
            if (child == _emptyLabel)
                continue;
            child.QueueFree();
        }

        var items = InventoryManager.Instance.Items;
        _inventoryLabel.Text = $"  背 包 ({items.Count}/{InventoryManager.Instance.MaxSlots})";

        if (items.Count == 0)
        {
            _inventoryGrid.Columns = 1;
            _emptyLabel.Visible = true;
            return;
        }

        _inventoryGrid.Columns = (int)ParamsConfig.InventoryGridColumns;
        _emptyLabel.Visible = false;

        if (ItemSlotScene == null)
            return;

        foreach (var item in items)
        {
            var slot = ItemSlotScene.Instantiate<InventoryItemSlot>();
            slot.Bind(item, OnInventoryEquipClicked, OnInventoryEquipRightClicked, OnConsumableClicked);
            _inventoryGrid.AddChild(slot);
        }
    }

    private void RefreshEquipSlots()
    {
        foreach (var (slotType, buttons) in _equipSlotButtons)
        {
            var equipped = EquipManager.Instance.GetEquipped(slotType);

            for (int i = 0; i < buttons.Count; i++)
            {
                if (i < equipped.Count)
                    ApplyEquipSlotAppearance(buttons[i], equipped[i]);
                else
                    ClearEquipSlotAppearance(buttons[i]);
            }
        }
    }

    public static void ApplyItemIcon(Button btn, ItemInstance item)
    {
        var config = item.Config;
        if (config == null)
        {
            btn.Icon = null;
            return;
        }

        btn.Icon = LoadItemIcon(config.Icon);
    }

    public static void ApplyEquipSlotAppearance(Button btn, ItemInstance item)
    {
        ApplyItemIcon(btn, item);
        btn.Text = item.Count > 1 ? $"x{item.Count}" : "";
        btn.AddThemeColorOverride("font_color", GetQualityColor(item.EffectiveRarity));
        btn.TooltipText = BuildItemTooltip(item);
        ApplyQualityBackground(btn, item.EffectiveRarity);
    }

    public static void ClearEquipSlotAppearance(Button btn)
    {
        btn.Icon = null;
        btn.Text = "（空）";
        btn.AddThemeColorOverride("font_color", ParamsConfig.ColorInventoryEmptySlot);
        btn.TooltipText = "";
        ClearQualityBackground(btn);
    }

    public static void ApplyQualityBackground(Button btn, int rarity)
    {
        var style = GetQualityBackgroundStyle(rarity);
        btn.AddThemeStyleboxOverride("normal", style);
        btn.AddThemeStyleboxOverride("hover", style);
        btn.AddThemeStyleboxOverride("pressed", style);
        btn.AddThemeStyleboxOverride("focus", style);
    }

    public static void ClearQualityBackground(Button btn)
    {
        btn.RemoveThemeStyleboxOverride("normal");
        btn.RemoveThemeStyleboxOverride("hover");
        btn.RemoveThemeStyleboxOverride("pressed");
        btn.RemoveThemeStyleboxOverride("focus");
    }

    private static Texture2D? LoadItemIcon(string iconPath)
    {
        if (string.IsNullOrEmpty(iconPath))
            return null;

        return GD.Load<Texture2D>(iconPath);
    }

    private void OnInventoryEquipClicked(string uid)
    {
        var item = InventoryManager.Instance?.Items.FirstOrDefault(i => i.Uid == uid);
        if (item == null)
        {
            GD.Print("[InventoryUI] 物品已不在背包中");
            return;
        }

        if (!item.IsEquip)
        {
            return;
        }

        var slotType = item.Config.SlotType;
        if (!HasEquippedInSlot(slotType))
        {
            OnItemEquipRequested(uid);
            return;
        }

        ShowItemActionPopup(uid);
    }

    private void OnInventoryEquipRightClicked(string uid)
    {
        _itemActionPopup.HidePopup();
        OnItemSellRequested(uid);
    }

    private static bool HasEquippedInSlot(int slotType) =>
        EquipManager.Instance?.GetEquipped(slotType).Count > 0;

    private void ShowItemActionPopup(string uid)
    {
        var item = InventoryManager.Instance?.Items.FirstOrDefault(i => i.Uid == uid);
        if (item == null)
        {
            GD.Print("[InventoryUI] 物品已不在背包中");
            return;
        }

        if (!item.IsEquip)
            return;

        _itemActionPopup.ShowFor(item);
    }

    private void OnItemEquipRequested(string uid)
    {
        var item = InventoryManager.Instance?.Items.FirstOrDefault(i => i.Uid == uid);
        if (item == null)
        {
            GD.Print("[InventoryUI] 物品已不在背包中");
            return;
        }

        bool ok = EquipManager.Instance.Equip(item);
        if (ok)
            GD.Print($"[InventoryUI] 已穿戴: {item.Config.NameKey}");
        else
            GD.Print($"[InventoryUI] 穿戴失败: {item.Config.NameKey}");
    }

    private void OnItemSellRequested(string uid)
    {
        var item = InventoryManager.Instance?.Items.FirstOrDefault(i => i.Uid == uid);
        if (item == null)
        {
            GD.Print("[InventoryUI] 物品已不在背包中");
            return;
        }

        if (!TrySellItem(item, out var gold))
            return;

        Main.Instance?.Run?.AddGold(gold);
        GD.Print($"[InventoryUI] 已卖出: {item.Config?.NameKey} +{gold} 金币");
    }

    private void OnSellLowRarityClicked()
    {
        var inventory = InventoryManager.Instance;
        if (inventory == null)
            return;

        var toSell = inventory.Items
            .Where(i => IsLowRarity(i.EffectiveRarity))
            .Where(i => GetSellGold(i) > 0)
            .ToList();

        if (toSell.Count == 0)
        {
            GD.Print("[InventoryUI] 背包中没有可卖出的普通/魔法物品");
            return;
        }

        var removals = toSell.Select(i => (i.Uid, i.Count)).ToList();
        var totalGold = toSell.Sum(GetSellGold);

        if (!inventory.RemoveItems(removals))
            return;

        Main.Instance?.Run?.AddGold(totalGold);
        GD.Print($"[InventoryUI] 批量卖出 {toSell.Count} 件普通/魔法物品，获得 {totalGold} 金币");
    }

    private static bool IsLowRarity(int rarity) => rarity <= (int)ParamsConfig.InventoryLowRarityMax;

    private static int GetSellGold(ItemInstance item)
    {
        var unitPrice = item.Config?.SellPrice ?? 0;
        return unitPrice > 0 ? unitPrice * item.Count : 0;
    }

    private static bool TrySellItem(ItemInstance item, out int gold)
    {
        gold = GetSellGold(item);
        if (gold <= 0)
        {
            GD.Print($"[InventoryUI] 不可卖出: {item.Config?.NameKey}");
            return false;
        }

        return InventoryManager.Instance?.RemoveItem(item.Uid) == true;
    }

    private static void OnConsumableClicked(ItemInstance item)
    {
        GD.Print($"[InventoryUI] 使用消耗品: {item.Config.NameKey}");
    }

    private void OnEquipSlotClicked(int slotType, int index)
    {
        var equipped = EquipManager.Instance.GetEquipped(slotType);
        if (index < equipped.Count)
        {
            var item = equipped[index];
            EquipManager.Instance.Unequip(item.Uid);
            GD.Print($"[InventoryUI] 已卸下: {item.Config.NameKey}");
        }
    }

    public void Toggle() => SetOpen(!Visible);

    private void SetOpen(bool open)
    {
        if (open)
        {
            if (!Visible && GameManager.Instance?.State == GameState.Playing)
            {
                GetTree().Paused = true;
                _pausedBySelf = true;
            }

            Visible = true;
            MouseFilter = MouseFilterEnum.Stop;
            RefreshInventoryGrid();
            RefreshEquipSlots();
            return;
        }

        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
        _itemActionPopup.HidePopup();

        if (_pausedBySelf)
        {
            if (Main.Instance?.Run?.Phase != RunPhase.Shop)
                GetTree().Paused = false;
            _pausedBySelf = false;
        }
    }

    public static Color GetQualityColor(int rarity) => rarity switch
    {
        1 => ParamsConfig.ColorQualityWhite,
        2 => ParamsConfig.ColorQualityBlue,
        3 => ParamsConfig.ColorQualityYellow,
        4 => ParamsConfig.ColorQualityOrange,
        5 => ParamsConfig.ColorQualityGold,
        _ => Colors.White,
    };

    public static Color GetQualityBackgroundColor(int rarity)
    {
        var color = GetQualityColor(rarity);
        color.A = ParamsConfig.InventoryQualityBgAlpha;
        return color;
    }

    private static StyleBoxFlat GetQualityBackgroundStyle(int rarity)
    {
        const int qualitySlotCount = 6;
        _qualityBackgroundStyles ??= new StyleBoxFlat[qualitySlotCount];

        if (rarity < 1 || rarity >= _qualityBackgroundStyles.Length)
            rarity = 1;

        if (_qualityBackgroundStyles[rarity] == null)
        {
            var radius = (int)ParamsConfig.InventorySlotCornerRadius;
            _qualityBackgroundStyles[rarity] = new StyleBoxFlat
            {
                BgColor = GetQualityBackgroundColor(rarity),
                CornerRadiusTopLeft = radius,
                CornerRadiusTopRight = radius,
                CornerRadiusBottomLeft = radius,
                CornerRadiusBottomRight = radius,
            };
        }

        return _qualityBackgroundStyles[rarity];
    }

    private static string GetQualityName(int rarity) => rarity switch
    {
        1 => "普通",
        2 => "魔法",
        3 => "稀有",
        4 => "传奇",
        5 => "暗金",
        _ => ""
    };

    private static string GetSlotName(int slotType) => slotType switch
    {
        1 => "武器",
        2 => "头盔",
        3 => "护甲",
        4 => "靴子",
        5 => "饰品",
        _ => $"槽位{slotType}"
    };

    public static string BuildItemTooltip(ItemInstance item)
    {
        var cfg = item.Config;
        if (cfg == null) return "未知物品";

        var rarity = item.EffectiveRarity;
        var bonus = item.ComputeStatBonus();

        var lines = new List<string>
        {
            $"[{GetQualityName(rarity)}] {cfg.NameKey}",
            cfg.DescKey,
            "---",
            $"物品等级: {item.ItemLevel}",
            $"类型: {GetSlotName(cfg.SlotType)}"
        };
        foreach (var (type, modType, value) in bonus.Entries)
        {
            lines.Add(NumericModifierMap.FormatEntry(type, modType, value));
        }

        foreach (var affix in item.Affixes)
        {
            lines.Add($"+ {affix.AffixId}: {NumericModifierMap.FormatEntry(affix.NumericType, affix.ModifierType, affix.Value)}");
        }

        if (cfg.SellPrice  > 0) lines.Add($"售价: {cfg.SellPrice} 金币");

        return string.Join("\n", lines);
    }
}
