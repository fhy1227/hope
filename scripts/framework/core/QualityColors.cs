using Godot;
using Hope.Config;

namespace Hope.Core;

/// <summary>
/// 物品品质颜色；与 <see cref="QualityConfig"/> 及背包 UI 共用。
/// </summary>
public static class QualityColors
{
    /// <summary>地面金币拾取物颜色。</summary>
    public static readonly Color GoldPickup = new(1f, 0.82f, 0.2f, 1f);

    /// <summary>按稀有度 Id 取色；优先读 quality 表 color_hex，否则回退 ParamsConfig。</summary>
    public static Color GetColor(int rarity)
    {
        var cfg = ConfigManager.Get<QualityConfig>(rarity);
        if (cfg != null && !string.IsNullOrEmpty(cfg.ColorHex))
        {
            var hex = cfg.ColorHex.StartsWith('#') ? cfg.ColorHex : $"#{cfg.ColorHex}";
            return Color.FromHtml(hex);
        }

        return rarity switch
        {
            1 => ParamsConfig.ColorQualityWhite,
            2 => ParamsConfig.ColorQualityBlue,
            3 => ParamsConfig.ColorQualityYellow,
            4 => ParamsConfig.ColorQualityOrange,
            5 => ParamsConfig.ColorQualityGold,
            _ => Colors.White,
        };
    }
}
