using Godot;

namespace Hope.Systems;

/// <summary>
/// 游戏世界根：持有关卡、实体、特效等容器引用（路径契约见 docs/ARCHITECTURE.md）。
/// </summary>
public partial class GameWorld : Node2D
{
    public Node2D Levels => GetNode<Node2D>("Levels");
    public Node2D Entities => GetNode<Node2D>("Entities");
    public Node2D Enemies => GetNode<Node2D>("Enemies");
    public Node2D Effects => GetNode<Node2D>("Effects");
    public Node2D Projectiles => GetNode<Node2D>("Effects/Projectiles");
    public Node2D Pickups => GetNode<Node2D>("Pickups");
    public RunManager RunManager => GetNode<RunManager>("RunManager");
}
