using Godot;
using Hope.Core;
using Hope.Systems;

namespace Hope.UI;

/// <summary>
/// 主城铁匠面板：选择装备进行附魔（消耗金币随机替换一条词缀）。
/// </summary>
public partial class BlacksmithPanel : PanelContainer
{
    private ItemList _itemList = null!;
    private Label _detailLabel = null!;
    private Label _statusLabel = null!;
    private Button _enchantButton = null!;

    private string? _selectedUid;

    public override void _Ready()
    {
        _itemList = GetNode<ItemList>("%ItemList");
        _detailLabel = GetNode<Label>("%DetailLabel");
        _statusLabel = GetNode<Label>("%StatusLabel");
        _enchantButton = GetNode<Button>("%EnchantButton");

        GetNode<Button>("%CloseButton").Pressed += () => Visible = false;
        _enchantButton.Pressed += OnEnchantPressed;
        _itemList.ItemSelected += OnItemSelected;

        Visible = false;
    }

    public void Open()
    {
        Visible = true;
        _statusLabel.Text = string.Empty;
        RefreshList();
    }

    private void RefreshList()
    {
        _itemList.Clear();
        _selectedUid = null;
        _detailLabel.Text = "选择一件稀有及以上装备。";
        _enchantButton.Disabled = true;

        foreach (var item in CraftingManager.GetEnchantableItems())
        {
            var name = item.Config?.NameKey ?? $"#{item.ConfigId}";
            var cost = CraftingManager.GetEnchantCost(item);
            var idx = _itemList.ItemCount;
            _itemList.AddItem($"{name}  Lv.{item.ItemLevel}  ({cost} G)");
            _itemList.SetItemMetadata(idx, item.Uid);
        }

        if (_itemList.ItemCount == 0)
        {
            _detailLabel.Text = "背包中没有可附魔的装备。";
        }
    }

    private void OnItemSelected(long index)
    {
        _selectedUid = _itemList.GetItemMetadata((int)index).AsString();
        var item = CraftingManager.GetEnchantableItems()
            .Find(i => i.Uid == _selectedUid);
        if (item == null)
        {
            return;
        }

        var cost = CraftingManager.GetEnchantCost(item);
        var remaining = CraftingManager.MaxEnchantPerItem - item.EnchantCount;
        _detailLabel.Text =
            $"{item.Config?.NameKey}\n词缀数: {item.Affixes.Count}  已附魔: {item.EnchantCount}/{CraftingManager.MaxEnchantPerItem}\n费用: {cost} G";
        _enchantButton.Disabled = remaining <= 0;
        _enchantButton.Text = remaining > 0 ? "附魔" : "已达上限";
    }

    private void OnEnchantPressed()
    {
        if (string.IsNullOrEmpty(_selectedUid))
        {
            return;
        }

        var result = CraftingManager.TryEnchant(_selectedUid, out var message);
        _statusLabel.Text = message;

        if (result == CraftingManager.EnchantResult.Success
            && GetTree().GetFirstNodeInGroup("hub_ui") is HubUI hub)
        {
            hub.RefreshDisplay();
        }

        RefreshList();
    }
}
