using Godot;
using Hope.Core;
using Hope.Levels;

namespace Hope.Systems;

/// <summary>
/// 关卡加载器：在 <c>GameWorld/Levels</c> 下实例化/卸载 <see cref="BaseLevel"/>，不负责玩家生成。
/// </summary>
public partial class LevelManager : Node
{
	[Export]
	public PackedScene? InitialLevelScene { get; set; }

	[Export]
	public NodePath LevelsContainerPath { get; set; } = new("../Levels");

	private Node2D _levelsContainer;

	/// <summary>当前活动关卡；无关卡或切换中为 null。</summary>
	public BaseLevel? Current { get; private set; }

	public override void _Ready()
	{
		_levelsContainer = GetNode<Node2D>(LevelsContainerPath);

		if (InitialLevelScene != null)
			LoadLevel(InitialLevelScene);
	}

	/// <summary>按场景路径加载关卡，替换当前关卡。</summary>
	/// <param name="path"><see cref="ScenePaths"/> 中的关卡路径。</param>
	public void LoadLevel(string path)
	{
		var scene = GD.Load<PackedScene>(path);
		if (scene == null)
		{
			GD.PushError($"LevelManager: failed to load level scene '{path}'.");
			return;
		}

		LoadLevel(scene);
	}

	/// <summary>用预制体加载关卡，替换当前关卡。</summary>
	public void LoadLevel(PackedScene scene)
	{
		ClearLevels();

		var level = scene.Instantiate<BaseLevel>();
		_levelsContainer.AddChild(level);
		Current = level;

		EventBus.Instance?.EmitLevelChanged(scene.ResourcePath);
	}

	private void ClearLevels()
	{
		foreach (var child in _levelsContainer.GetChildren())
		{
			_levelsContainer.RemoveChild(child);
			child.Free();
		}

		Current = null;
	}
}
