using Godot;
using Hope.Config;
using Hope.Core;

namespace Hope.Components;

/// <summary>
/// 敌人数值编排：从 Export 初始化 Numeric，并提供战斗查询接口。
/// </summary>
public partial class EnemyStatsComponent : Node
{
    [Export]
    public float MoveSpeed { get; set; } = ParamsConfig.EnemyMoveSpeedDefault;

    [Export]
    public float ContactDamage { get; set; } = ParamsConfig.EnemyContactDamageDefault;

    [Export]
    public NodePath NumericPath { get; set; } = new("../NumericComponent");

    [Export]
    public NodePath HealthPath { get; set; } = new("../HealthComponent");

    [Export]
    public NodePath HealthSyncPath { get; set; } = new("../NumericHealthSyncComponent");

    private NumericComponent _numeric = null!;
    private HealthComponent _health = null!;
    private NumericHealthSyncComponent _healthSync = null!;

    public override void _Ready()
    {
        _numeric = GetNode<NumericComponent>(NumericPath);
        _health = GetNode<HealthComponent>(HealthPath);
        _healthSync = GetNode<NumericHealthSyncComponent>(HealthSyncPath);

        _numeric.InitFromConfig(_health.MaxHealth, MoveSpeed, ContactDamage);
        _healthSync.SyncAll(refillHealth: true);
    }

    public float GetMoveSpeed() => _numeric[NumericType.MoveSpeed];

    public float GetContactDamage() => _numeric[NumericType.Damage];

    public void TakeDamage(int amount) => _healthSync.ApplyDamage(amount);
}
