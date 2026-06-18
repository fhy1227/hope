using Godot;
using Hope.Entities;

namespace Hope.Systems;

/// <summary>
/// 在玩家周围环形刷怪，随波次提高密度。
/// </summary>
public partial class EnemySpawner : Node
{
    [Export]
    public PackedScene? EnemyScene { get; set; }

    [Export]
    public NodePath EnemyContainerPath { get; set; } = new("../Enemies");

    [Export]
    public float SpawnRadiusMin { get; set; } = 360f;

    [Export]
    public float SpawnRadiusMax { get; set; } = 480f;

    [Export]
    public float BaseSpawnInterval { get; set; } = 2.2f;

    [Export]
    public int MaxAliveEnemies { get; set; } = 30;

    private Node2D _enemyContainer;
    private Node2D _player;
    private float _spawnTimer;
    private int _wave;
    private bool _active;
    private bool _gamePaused;

    public override void _Ready()
    {
        _enemyContainer = GetNode<Node2D>(EnemyContainerPath);
    }

    public void BindPlayer(Node2D player)
    {
        _player = player;
    }

    public void BeginWave(int wave)
    {
        _wave = wave;
        _active = true;
        _spawnTimer = 0.5f;
    }

    public void Stop()
    {
        _active = false;
    }

    public void SetPaused(bool paused)
    {
        _gamePaused = paused;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_active || _gamePaused || EnemyScene == null || _player == null)
        {
            return;
        }

        _spawnTimer -= (float)delta;
        if (_spawnTimer > 0f)
        {
            return;
        }

        if (CountAliveEnemies() >= MaxAliveEnemies)
        {
            _spawnTimer = 0.5f;
            return;
        }

        SpawnEnemy();
        _spawnTimer = GetSpawnInterval();
    }

    private float GetSpawnInterval()
    {
        return Mathf.Max(BaseSpawnInterval - _wave * 0.12f, 0.45f);
    }

    private int CountAliveEnemies()
    {
        return GetTree().GetNodesInGroup("enemies").Count;
    }

    private void SpawnEnemy()
    {
        var enemy = EnemyScene.Instantiate<Enemy>();
        enemy.GlobalPosition = GetSpawnPosition();
        enemy.SetTarget(_player);
        enemy.EnemyKilled += OnEnemyKilled;
        _enemyContainer.AddChild(enemy);
    }

    private Vector2 GetSpawnPosition()
    {
        var angle = (float)GD.RandRange(0d, Mathf.Tau);
        var radius = (float)GD.RandRange(SpawnRadiusMin, SpawnRadiusMax);
        var offset = Vector2.Right.Rotated(angle) * radius;
        return _player.GlobalPosition + offset;
    }

    private void OnEnemyKilled(int gold, Vector2 position)
    {
        EmitSignal(SignalName.EnemyDefeated, gold, position);
    }

    [Signal]
    public delegate void EnemyDefeatedEventHandler(int gold, Vector2 position);
}
