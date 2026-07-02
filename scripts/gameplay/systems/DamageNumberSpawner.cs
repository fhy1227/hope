using Godot;
using Hope.Components;
using Hope.Core;

namespace Hope.Systems;

/// <summary>
/// 监听全局伤害事件，在 Effects 容器中生成浮动伤害数字。
/// </summary>
public partial class DamageNumberSpawner : Node
{
	[Export]
	public PackedScene DamageNumberScene { get; set; } = null!;

	public override void _Ready()
	{
		if (EventBus.Instance != null)
		{
			EventBus.Instance.DamageTaken += OnDamageTaken;
		}
	}

	public override void _ExitTree()
	{
		if (EventBus.Instance != null)
		{
			EventBus.Instance.DamageTaken -= OnDamageTaken;
		}
	}

	private void OnDamageTaken(int amount, Vector2 worldPosition, bool isPlayer)
	{
		if (DamageNumberScene == null || amount <= 0)
		{
			return;
		}

		var number = DamageNumberScene.Instantiate<DamageNumber>();
		AddChild(number);
		number.GlobalPosition = worldPosition;
		number.Setup(amount, isPlayer);
	}
}
