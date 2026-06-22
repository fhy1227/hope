using Godot;
using Hope.Config;

namespace Hope;

/// <summary>
/// 游戏启动时预加载所有配置表。
/// </summary>
public partial class ConfigBootstrap : Node
{
	public override void _Ready()
	{
		ConfigManager.LoadAll();

		//测试，打印所有配置表
		foreach (var tableName in ConfigManager.GetAll<ItemConfig>())
		{
			GD.Print(tableName);
			//打印出具体内容:
			GD.Print(tableName.Id);
			GD.Print(tableName.Name);
			GD.Print(tableName.Desc);
			GD.Print(tableName.Icon);
			GD.Print(tableName.Getway);
			GD.Print(tableName.MainType);
			GD.Print(tableName.Type);
			GD.Print(tableName.Usable);
			GD.Print(tableName.UseOnSys);
			GD.Print(tableName.Tags);
			GD.Print(tableName.Value);
			GD.Print(tableName.Value1);
			GD.Print(tableName.ValueRewards);
			GD.Print(tableName.Stacklimit);
			GD.Print(tableName.Quality);
			GD.Print(tableName.Timelimit);
			GD.Print(tableName.Sort);
			GD.Print(tableName.MaxlvToMoney);
			GD.Print(tableName.RedDotsCond);
			if (tableName.TestJson != null)
			{
				tableName.TestJson.TryGetValue("x", out var x);
				GD.Print(x.AsString());
				tableName.TestJson.TryGetValue("y", out var y);
				GD.Print(y.AsString());
			}
			GD.Print("--------------------------------");
		}
	}
}
