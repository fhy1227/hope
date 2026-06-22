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
	}
}
