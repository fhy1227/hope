using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Hope.Config;
using Hope.Core;
using Hope.Entities;
using Hope.Systems;

namespace Hope.UI;

/// <summary>
/// 背包 + 装备栏 UI（按 I 键切换）
/// 所有 UI 节点在 _Ready 中程序化创建
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

    // ── UI 节点引用 ─────────────────────────────
    private Control   _rootPanel;          // 主面板
    private GridContainer _inventoryGrid;  // 背包格子容器
    private Label     _inventoryLabel;     // "背包 (x/20)"
    private VBoxContainer _equipColumn;    // 装备列
    private ScrollContainer _scrollContainer; // 背包滚动

    // 装备槽按钮列表 [slotType][index]
    private readonly Dictionary<int, List<Button>> _equipSlotButtons = new();

    // 缓存
    private static readonly Dictionary<int, EquipSlotConfig> _slotConfigs = new();

    // ── 生命周期 ──────────────────────────────────────────────────

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        ZIndex = 110;
        MouseFilter = MouseFilterEnum.Ignore;

        // 全屏半透明背景
        var bg = new ColorRect();
        bg.Name = "Bg";
        bg.Color = new Color(0, 0, 0, 0.6f);
        bg.MouseFilter = MouseFilterEnum.Ignore;
        bg.AnchorsPreset = 15;
        bg.AnchorRight = 1;
        bg.AnchorBottom = 1;
        AddChild(bg);

        // 居中容器 + 主面板（避免手动 Position 导致点击区域错位）
        var center = new CenterContainer();
        center.Name = "Center";
        center.AnchorsPreset = 15;
        center.AnchorRight = 1;
        center.AnchorBottom = 1;
        center.MouseFilter = MouseFilterEnum.Ignore;
        AddChild(center);

        _rootPanel = new Panel();
        _rootPanel.Name = "MainPanel";
        _rootPanel.CustomMinimumSize = new Vector2(700, 450);
        _rootPanel.Size = new Vector2(700, 450);
        center.AddChild(_rootPanel);

        LoadSlotConfigs();
        BuildUI();

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

    // ── 构建 UI ──────────────────────────────────────────────────

    private void LoadSlotConfigs()
    {
        _slotConfigs.Clear();
        foreach (var slot in ConfigManager.GetAll<EquipSlotConfig>())
            _slotConfigs[slot.Id] = slot;
    }

    private void BuildUI()
    {
        var mainVBox = new VBoxContainer();
        mainVBox.Name = "MainVBox";
        mainVBox.AnchorsPreset = 15;
        mainVBox.AnchorRight = 1;
        mainVBox.AnchorBottom = 1;
        _rootPanel.AddChild(mainVBox);

        // ── 标题栏 ──
        var header = new HBoxContainer();
        var title = new Label { Text = "  背包与装备" };
        title.AddThemeFontSizeOverride("font_size", 18);
        title.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        var closeBtn = new Button { Text = "X", CustomMinimumSize = new Vector2(30, 30) };
        closeBtn.Pressed += () => SetOpen(false);
        header.AddChild(title);
        header.AddChild(closeBtn);
        mainVBox.AddChild(header);

        // ── 内容区 ──
        var content = new HBoxContainer();
        content.SizeFlagsVertical = SizeFlags.ExpandFill;

        BuildEquipColumn(content);
        BuildVSeparator(content);
        BuildInventoryGrid(content);

        mainVBox.AddChild(content);

        // ── 底部提示 ──
        var footer = new Label
        {
            Text = "点击背包物品穿戴 | 点击已装备物品卸下 | I 键关闭",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        mainVBox.AddChild(footer);
    }

    private void BuildEquipColumn(HBoxContainer parent)
    {
        _equipColumn = new VBoxContainer();
        _equipColumn.CustomMinimumSize = new Vector2(200, 0);
        _equipColumn.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        var label = new Label { Text = "  装 备" };
        label.AddThemeFontSizeOverride("font_size", 16);
        _equipColumn.AddChild(label);

        // 按 slot_type 顺序创建装备槽
        var sortedSlots = _slotConfigs.Values.OrderBy(s => s.Id).ToList();
        foreach (var slot in sortedSlots)
        {
            // 每个槽位可能有多个（如武器有2个）
            for (int i = 0; i < slot.MaxCount; i++)
            {
                var slotBox = new HBoxContainer();
                var slotLabel = new Label
                {
                    Text = $"{GetSlotName(slot.Id)} {i + 1}",
                    CustomMinimumSize = new Vector2(60, 30)
                };
                var btn = new Button
                {
                    CustomMinimumSize = new Vector2(120, 30),
                    SizeFlagsHorizontal = SizeFlags.ExpandFill,
                    Text = "",
                    ProcessMode = ProcessModeEnum.Always,
                };
                btn.Pressed += () => OnEquipSlotClicked(slot.Id, i);
                slotBox.AddChild(slotLabel);
                slotBox.AddChild(btn);

                _equipColumn.AddChild(slotBox);

                if (!_equipSlotButtons.ContainsKey(slot.Id))
                    _equipSlotButtons[slot.Id] = new List<Button>();
                _equipSlotButtons[slot.Id].Add(btn);
            }
        }

        parent.AddChild(_equipColumn);
    }

    private void BuildVSeparator(HBoxContainer parent)
    {
        var sep = new VSeparator();
        sep.SizeFlagsVertical = SizeFlags.ExpandFill;
        parent.AddChild(sep);
    }

    private void BuildInventoryGrid(HBoxContainer parent)
    {
        var invCol = new VBoxContainer();
        invCol.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _inventoryLabel = new Label
        {
            Text = "  背 包",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _inventoryLabel.AddThemeFontSizeOverride("font_size", 16);
        invCol.AddChild(_inventoryLabel);

        _scrollContainer = new ScrollContainer();
        _scrollContainer.SizeFlagsVertical = SizeFlags.ExpandFill;

        _inventoryGrid = new GridContainer();
        _inventoryGrid.Columns = 5;
        _inventoryGrid.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _inventoryGrid.AddThemeConstantOverride("h_separation", 4);
        _inventoryGrid.AddThemeConstantOverride("v_separation", 4);

        _scrollContainer.AddChild(_inventoryGrid);
        invCol.AddChild(_scrollContainer);
        parent.AddChild(invCol);
    }

    // ── 刷新 ─────────────────────────────────────────────────────

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
        // 清除旧格子
        foreach (var child in _inventoryGrid.GetChildren())
            child.QueueFree();

        var items = InventoryManager.Instance.Items;
        _inventoryLabel.Text = $"  背 包 ({items.Count}/{InventoryManager.Instance.MaxSlots})";

        if (items.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "\n  背包空空如也...\n  去打怪掉装备吧！",
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            emptyLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            _inventoryGrid.Columns = 1;
            _inventoryGrid.AddChild(emptyLabel);
        }
        else
        {
            _inventoryGrid.Columns = 5;
            foreach (var item in items)
            {
                var slotBtn = CreateItemSlot(item);
                _inventoryGrid.AddChild(slotBtn);
            }
        }
    }

    private void RefreshEquipSlots()
    {
        foreach (var kv in _equipSlotButtons)
        {
            var slotType = kv.Key;
            var buttons  = kv.Value;
            var equipped = EquipManager.Instance.GetEquipped(slotType);

            for (int i = 0; i < buttons.Count; i++)
            {
                if (i < equipped.Count)
                {
                    var item = equipped[i];
                    buttons[i].Text = $"  {item.Config.NameKey}";
                    var color = GetQualityColor(item.Config.Rarity);
                    buttons[i].AddThemeColorOverride("font_color", color);
                    buttons[i].TooltipText = BuildItemTooltip(item);
                }
                else
                {
                    buttons[i].Text = $"  （空）";
                    buttons[i].AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                    buttons[i].TooltipText = "";
                }
            }
        }
    }

    // ── 物品格子 ────────────────────────────────────────────────

    private Button CreateItemSlot(ItemInstance item)
    {
        var config = item.Config;
        var uid = item.Uid;
        var btn = new Button
        {
            CustomMinimumSize = new Vector2(90, 80),
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill,
            ProcessMode = ProcessModeEnum.Always,
        };

        // 品质色边框 + 名称
        var color = GetQualityColor(config.Rarity);
        btn.AddThemeColorOverride("font_color", color);

        // 品质 + 图标占位 + 名称
        var qName = GetQualityName(config.Rarity);
        btn.Text = $"{qName}\n{config.NameKey}";

        // 堆叠数量
        if (config.StackLimit > 1 && item.Count > 1)
            btn.Text += $" x{item.Count}";

        btn.TooltipText = BuildItemTooltip(item);

        // 点击事件（按 uid 查找，避免刷新网格后引用失效）
        if (item.IsEquip)
            btn.Pressed += () => OnInventoryItemClickedByUid(uid);
        else
            btn.Pressed += () => GD.Print($"[InventoryUI] 使用消耗品: {config.NameKey}");

        return btn;
    }

    // ── 点击事件 ─────────────────────────────────────────────────

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

    // ── 显示/隐藏 ───────────────────────────────────────────────

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

    // ── 工具函数 ─────────────────────────────────────────────────

    private static Color GetQualityColor(int rarity)
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

    private static string BuildItemTooltip(ItemInstance item)
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
