using Godot;
using Hope.Core;

namespace Hope.Components;

/// <summary>
/// 管理玩家两个武器槽，默认装备远程 + 近战。
/// </summary>
public partial class PlayerWeaponController : Node2D
{
    [Export]
    public PackedScene? ProjectileScene { get; set; }

    [Export]
    public NodePath ProjectileContainerPath { get; set; } = new("../../../Projectiles");

    private WeaponSlot _slot0 = null!;
    private WeaponSlot _slot1 = null!;
    private RunStats _stats = new();
    private Node2D _owner = null!;

    public WeaponSlot Slot0 => _slot0;
    public WeaponSlot Slot1 => _slot1;

    public override void _Ready()
    {
        _owner = GetParent<Node2D>();
        _slot0 = GetNode<WeaponSlot>("Slot0");
        _slot1 = GetNode<WeaponSlot>("Slot1");

        var projectileContainer = ResolveProjectileContainer();
        _slot0.Initialize(_owner, projectileContainer);
        _slot1.Initialize(_owner, projectileContainer);
    }

    public void SetupDefaultLoadout()
    {
        // _slot0.Equip(WeaponData.CreatePistol(ProjectileScene));
        _slot0.Equip(WeaponData.CreateSword());
        _slot1.Equip(WeaponData.CreateSpear());
    }

    public void BindStats(RunStats stats)
    {
        _stats = stats;
        _slot0.BindStats(_stats);
        _slot1.BindStats(_stats);
    }

    public void EquipWeapon(int slotIndex, WeaponData weapon)
    {
        GetSlot(slotIndex).Equip(weapon);
    }

    private WeaponSlot GetSlot(int slotIndex) => slotIndex switch
    {
        0 => _slot0,
        1 => _slot1,
        _ => throw new System.ArgumentOutOfRangeException(nameof(slotIndex)),
    };

    private Node2D ResolveProjectileContainer()
    {
        if (!ProjectileContainerPath.IsEmpty && HasNode(ProjectileContainerPath))
        {
            return GetNode<Node2D>(ProjectileContainerPath);
        }

        var world = _owner.GetParent()?.GetParent();
        if (world != null && world.HasNode("Projectiles"))
        {
            return world.GetNode<Node2D>("Projectiles");
        }

        GD.PushError("PlayerWeaponController: Projectiles container not found.");
        return _owner;
    }
}
