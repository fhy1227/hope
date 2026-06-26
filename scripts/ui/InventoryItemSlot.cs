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

    public void Bind(ItemInstance item, Action<string> onEquipClicked, Action<ItemInstance> onUseClicked)
    {
        _itemUid = item.Uid;
        var config = item.Config;

        InventoryUI.ApplyItemIcon(this, item);

        var color = InventoryUI.GetQualityColor(item.EffectiveRarity);
        AddThemeColorOverride("font_color", color);
        Text = config.StackLimit > 1 && item.Count > 1 ? $"x{item.Count}" : "";
        TooltipText = InventoryUI.BuildItemTooltip(item);

        Pressed += OnPressed;

        void OnPressed()
        {
            if (item.IsEquip)
                onEquipClicked(_itemUid);
            else
                onUseClicked(item);
        }
    }
}
