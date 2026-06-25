using Godot;
using System.Collections.Generic;
using System.Linq;
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
    private static readonly Color[] QualityColors = {
        new(0, 0, 0, 0),                  // 0 - 未使用
        new(0.627f, 0.627f, 0.627f, 1),   // 1 白 #A0A0A0
        new(0.188f, 0.447f, 0.953f, 1),   // 2 蓝 #3072F3
        new(0.961f, 0.651f, 0.137f, 1),   // 3 黄 #F5A623
        new(0.639f, 0.208f, 0.933f, 1),   // 4 橙 #A335EE
        new(1,     0.843f, 0,      1),    // 5 暗金 #FFD700
    };

    private const string ItemSlotScenePath = "res://scenes/ui/inventory_item_slot.tscn";

    [Export] public PackedScene ItemSlotScene { get; set; } = null!;

    private GridContainer _inventoryGrid = null!;
    private Label _inventoryLabel = null!;
    private Label _emptyLabel = null!;
    private readonly Dictionary<int, List<Button>> _equipSlotButtons = new();

    public override void _Ready()
    {
        ItemSlotScene ??= GD.Load<PackedScene>(ItemSlotScenePath);
        if (ItemSlotScene == null)
            GD.PushError("[InventoryUI] 未找到物品格子场景，请检查 inventory_item_slot.tscn");

        _inventoryGrid = GetNode<GridContainer>("%InventoryGrid");
        _inventoryLabel = GetNode<Label>("%InventoryLabel");
        _emptyLabel = GetNode<Label>("%EmptyLabel");

        BindEquipSlots();
        GetNode<Button>("%CloseButton").Pressed += () => SetOpen(false);

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
    }

    public override void _Input(InputEvent @event)
    {
        if (!@event.IsActionPressed("toggle_inventory"))
            return;

        GetViewport().SetInputAsHandled();
        Toggle();
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

        _inventoryGrid.Columns = 5;
        _emptyLabel.Visible = false;

        if (ItemSlotScene == null)
            return;

        foreach (var item in items)
        {
            var slot = ItemSlotScene.Instantiate<InventoryItemSlot>();
            slot.Bind(item, OnInventoryItemClickedByUid, OnConsumableClicked);
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
                {
                    var item = equipped[i];
                    ApplyItemIcon(buttons[i], item);
                    buttons[i].Text = item.Count > 1 ? $"x{item.Count}" : "";
                    var color = GetQualityColor(item.Config.Rarity);
                    buttons[i].AddThemeColorOverride("font_color", color);
                    buttons[i].TooltipText = BuildItemTooltip(item);
                }
                else
                {
                    buttons[i].Icon = null;
                    buttons[i].Text = "（空）";
                    buttons[i].AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                    buttons[i].TooltipText = "";
                }
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

    private static Texture2D? LoadItemIcon(string iconPath)
    {
        if (string.IsNullOrEmpty(iconPath))
            return null;

        return GD.Load<Texture2D>(iconPath);
    }

    private void OnInventoryItemClickedByUid(string uid)
    {
        var item = InventoryManager.Instance?.Items.FirstOrDefault(i => i.Uid == uid);
        if (item == null)
        {
            GD.Print("[InventoryUI] 物品已不在背包中");
            return;
        }

        if (!item.IsEquip)
            return;

        bool ok = EquipManager.Instance.Equip(item);
        if (ok)
            GD.Print($"[InventoryUI] 已穿戴: {item.Config.NameKey}");
        else
            GD.Print($"[InventoryUI] 穿戴失败: {item.Config.NameKey}");
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
        Visible = open;
        MouseFilter = open ? MouseFilterEnum.Stop : MouseFilterEnum.Ignore;

        if (open)
        {
            RefreshInventoryGrid();
            RefreshEquipSlots();
        }
    }

    public static Color GetQualityColor(int rarity)
    {
        if (rarity >= 1 && rarity < QualityColors.Length)
            return QualityColors[rarity];
        return Colors.White;
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

        var lines = new List<string>
        {
            $"[{GetQualityName(cfg.Rarity)}] {cfg.NameKey}",
            cfg.DescKey,
            "---",
            $"类型: {GetSlotName(cfg.SlotType)}"
        };
        if (cfg.StatHp     > 0) lines.Add($"生命: +{cfg.StatHp}");
        if (cfg.StatDamage > 0) lines.Add($"伤害: x{cfg.StatDamage:F2}");
        if (cfg.StatSpeed  > 0) lines.Add($"速度: x{cfg.StatSpeed:F2}");
        if (cfg.StatCrit   > 0) lines.Add($"暴击: +{cfg.StatCrit:F2}");
        if (cfg.StatArmor  > 0) lines.Add($"护甲: +{cfg.StatArmor}");
        if (cfg.SellPrice  > 0) lines.Add($"售价: {cfg.SellPrice} 金币");

        return string.Join("\n", lines);
    }
}
