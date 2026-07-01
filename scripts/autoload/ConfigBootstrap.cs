using Godot;
using Hope.Config;
using System.Collections.Generic;
using System.Text;

namespace Hope;

/// <summary>
/// 游戏启动时预加载所有配置表。
/// </summary>
public partial class ConfigBootstrap : Node
{
	public override void _Ready()
	{
		ConfigManager.LoadAll();
		// PrintAllConfigs();
	}

	/// <summary>
	/// 测试用：打印所有已加载的配置表内容。
	/// </summary>
	private void PrintAllConfigs()
	{
		GD.Print("[ConfigBootstrap] ===== 配置表测试 =====");
		PrintTable(ConfigManager.GetAll<ItemConfig>());
		PrintTable(ConfigManager.GetAll<QualityConfig>());
		PrintTable(ConfigManager.GetAll<EquipSlotConfig>());
		PrintTable(ConfigManager.GetAll<AffixConfig>());
		PrintTable(ConfigManager.GetAll<DropTableConfig>());
		GD.Print("[ConfigBootstrap] ===== 测试完毕 =====");
	}

	private static void PrintTable<T>(IReadOnlyList<T> rows) where T : IConfigData
	{
		GD.Print($"--- {typeof(T).Name} ({rows.Count} 条) ---");
		foreach (var row in rows)
			GD.Print($"  {FormatConfigRow(row)}");
	}

	private static string FormatConfigRow(IConfigData row)
	{
		var sb = new StringBuilder();
		sb.Append($"Id={row.Id}");
		foreach (var prop in row.GetType().GetProperties())
		{
			if (prop.Name == "Id") continue;
			sb.Append($", {prop.Name}={prop.GetValue(row)}");
		}
		return sb.ToString();
	}
}
