using System;
using Godot;
using Hope.Core;

namespace Hope.UI;

/// <summary>
/// 背包物品操作弹窗：装备 / 卖出。
/// </summary>
public partial class InventoryItemActionPopup : PanelContainer
{
    private Label _itemLabel = null!;
    private Button _equipButton = null!;
    private Button _sellButton = null!;
    private string _itemUid = "";

    public event Action<string>? EquipRequested;
    public event Action<string>? SellRequested;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;

        _itemLabel = GetNode<Label>("%ItemLabel");
        _equipButton = GetNode<Button>("%EquipButton");
        _sellButton = GetNode<Button>("%SellButton");

        _equipButton.Pressed += OnEquipPressed;
        _sellButton.Pressed += OnSellPressed;
        GetNode<Button>("%CloseButton").Pressed += HidePopup;

        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
    }

    public bool IsOpen => Visible;

    public void ShowFor(ItemInstance item)
    {
        _itemUid = item.Uid;
        var cfg = item.Config;
        _itemLabel.Text = cfg?.NameKey ?? "未知物品";

        var sellPrice = cfg?.SellPrice ?? 0;
        if (sellPrice > 0)
        {
            _sellButton.Text = $"卖出 ({sellPrice} 金币)";
            _sellButton.Disabled = false;
        }
        else
        {
            _sellButton.Text = "不可卖出";
            _sellButton.Disabled = true;
        }

        Visible = true;
        MouseFilter = MouseFilterEnum.Stop;
    }

    public void HidePopup()
    {
        Visible = false;
        MouseFilter = MouseFilterEnum.Ignore;
        _itemUid = "";
    }

    private void OnEquipPressed()
    {
        var uid = _itemUid;
        HidePopup();
        EquipRequested?.Invoke(uid);
    }

    private void OnSellPressed()
    {
        if (_sellButton.Disabled)
            return;

        var uid = _itemUid;
        HidePopup();
        SellRequested?.Invoke(uid);
    }
}
