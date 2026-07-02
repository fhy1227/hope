using Godot;

namespace Hope.Levels;

/// <summary>
/// 关卡基类：只负责关卡内环境与生成点，不拥有玩家。
/// 由 RunManager / Combat 实例化玩家并放到 SpawnPoint。
/// </summary>
public partial class BaseLevel : Node2D
{
    public Marker2D? SpawnPoint => GetNodeOrNull<Marker2D>("SpawnPoint");

    public Vector2 GetSpawnGlobalPosition() =>
        SpawnPoint?.GlobalPosition ?? GlobalPosition;
}
