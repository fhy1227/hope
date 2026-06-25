using Godot;
using Hope.Core;
using Hope.Systems;

namespace Hope.Components;

/// <summary>
/// 管理玩家两个武器槽，与 EquipManager 武器槽同步。
/// </summary>
public partial class PlayerWeaponController : Node2D
{
    private const int DefaultWeaponId = 1030;

    [Export]
    public PackedScene? ProjectileScene { get; set; }

    [Export]
    public NodePath ProjectileContainerPath { get; set; } = new("../../../Projectiles");

    private WeaponSlot _slot0 = null!;
    private WeaponSlot _slot1 = null!;
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

        if (EquipManager.Instance != null)
        {
            EquipManager.Instance.EquipmentChanged += RefreshFromEquipment;
        }
    }

    public override void _ExitTree()
    {
        if (EquipManager.Instance != null)
        {
            EquipManager.Instance.EquipmentChanged -= RefreshFromEquipment;
        }
    }

    public void SetupDefaultLoadout()
    {
        var equipped = EquipManager.Instance?.GetEquipped(EquipManager.WeaponSlotType);
        if (equipped == null || equipped.Count == 0)
        {
            EquipManager.Instance?.SetDefaultEquipment(
                EquipManager.WeaponSlotType,
                [DefaultWeaponId, DefaultWeaponId]);
        }
        else
        {
            RefreshFromEquipment();
        }
    }

    public void RefreshFromEquipment()
    {
        var items = EquipManager.Instance?.GetEquipped(EquipManager.WeaponSlotType);
        if (items == null)
        {
            return;
        }

        for (var i = 0; i < 2; i++)
        {
            if (i < items.Count)
            {
                var weapon = WeaponData.FromItemConfig(items[i].ConfigId, ProjectileScene);
                if (weapon != null)
                {
                    GetSlot(i).Equip(weapon);
                }
            }
        }
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
