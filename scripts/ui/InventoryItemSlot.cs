using System;
using Godot;
using Hope.Core;

namespace Hope.UI;

/// <summary>
/// 背包物品格子（场景定义外观，脚本只负责绑定数据）
/// </summary>
public partial class InventoryItemSlot : Button
{
    private string _itemUid = "";
    private ItemInstance? _item;
    private bool _isEquip;
    private Action<string>? _onEquipClicked;
    private Action<string>? _onEquipRightClicked;
    private Action<ItemInstance>? _onUseClicked;

    public void Bind(
        ItemInstance item,
        Action<string> onEquipItemClicked,
        Action<string> onEquipItemRightClicked,
        Action<ItemInstance> onUseClicked)
    {
        _itemUid = item.Uid;
        _item = item;
        _isEquip = item.IsEquip;
        _onEquipClicked = onEquipItemClicked;
        _onEquipRightClicked = onEquipItemRightClicked;
        _onUseClicked = onUseClicked;

        var config = item.Config;

        InventoryUI.ApplyItemIcon(this, item);

        var color = InventoryUI.GetQualityColor(item.EffectiveRarity);
        AddThemeColorOverride("font_color", color);
        InventoryUI.ApplyQualityBackground(this, item.EffectiveRarity);
        Text = config.StackLimit > 1 && item.Count > 1 ? $"x{item.Count}" : "";
        TooltipText = InventoryUI.BuildItemTooltip(item);

        Pressed += OnPressed;
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (!_isEquip || _onEquipRightClicked == null)
        {
            return;
        }

        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Right })
        {
            _onEquipRightClicked.Invoke(_itemUid);
            AcceptEvent();
        }
    }

    private void OnPressed()
    {
        if (_item == null)
        {
            return;
        }

        if (_isEquip)
        {
            _onEquipClicked?.Invoke(_itemUid);
            return;
        }

        _onUseClicked?.Invoke(_item);
    }
}
