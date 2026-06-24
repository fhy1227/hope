using Godot;
using Godot.Collections;
using Hope.Config;

namespace Hope.Core;

/// <summary>
/// 运行时物品实例（背包里的一件物品）
/// Phase1：只引用配置ID
/// Phase2+：可扩展随机词条、强化等级等
/// </summary>
public partial class ItemInstance : Resource
{
    /// <summary> 配置ID（对应 item.json 的 id） </summary>
    public int ConfigId { get; set; }

    /// <summary> 唯一实例ID（区分同类物品） </summary>
    public string Uid { get; set; } = System.Guid.NewGuid().ToString();

    /// <summary> 堆叠数量（可堆叠物品） </summary>
    public int Count { get; set; } = 1;

    /// <summary> 获取配置数据（延迟读取） </summary>
    public Config.ItemConfig Config
    {
        get
        {
            var config = ConfigManager.Get<Config.ItemConfig>(ConfigId);
            if (config == null)
                GD.PrintErr($"[ItemInstance] 找不到配置: {ConfigId}");
            return config;
        }
    }

    /// <summary> 是否可堆叠 </summary>
    public bool IsStackable => Config != null && Config.StackLimit > 0;

    /// <summary> 是否为装备 </summary>
    public bool IsEquip => Config != null && Config.SlotType > 0;
}
