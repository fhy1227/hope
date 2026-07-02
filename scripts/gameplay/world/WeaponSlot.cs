using Godot;
using Hope.Config;
using Hope.Core;
using Hope.Entities;

namespace Hope.Components;

/// <summary>
/// 单个武器槽：视图朝向目标，远程开火 / 近战挥砍或直刺。
/// </summary>
public partial class WeaponSlot : Node2D
{
	[Export]
	public Vector2 SlotOffset { get; set; } = Vector2.Right * ParamsConfig.WeaponSlotOffset;

	private Node2D _pivot = null!;
	private Sprite2D _iconVisual = null!;
	private Polygon2D _rangedVisual = null!;
	private Polygon2D _meleeVisual = null!;
	private Marker2D _muzzle = null!;
	private Area2D _meleeHitbox = null!;
	private CollisionShape2D _meleeShape = null!;

	private Node2D _owner = null!;
	private Node2D _projectileContainer = null!;
	private NumericComponent? _numeric;
	private WeaponData? _weapon;
	private float _cooldown;
	private bool _attacking;
	private readonly HashSet<ulong> _hitThisSwing = [];

	public WeaponData? EquippedWeapon => _weapon;

	public override void _Ready()
	{
		_pivot = GetNode<Node2D>("Pivot");
		_iconVisual = GetNode<Sprite2D>("Pivot/IconVisual");
		_rangedVisual = GetNode<Polygon2D>("Pivot/RangedVisual");
		_meleeVisual = GetNode<Polygon2D>("Pivot/MeleeVisual");
		_muzzle = GetNode<Marker2D>("Pivot/Muzzle");
		_meleeHitbox = GetNode<Area2D>("Pivot/MeleeHitbox");
		_meleeShape = GetNode<CollisionShape2D>("Pivot/MeleeHitbox/CollisionShape2D");

		_meleeHitbox.Monitoring = false;
		_meleeHitbox.BodyEntered += OnMeleeBodyEntered;
		_meleeHitbox.AreaEntered += OnMeleeAreaEntered;

		Position = SlotOffset;
		SetPhysicsProcess(true);
	}

	public void Initialize(Node2D owner, Node2D projectileContainer)
	{
		_owner = owner;
		_projectileContainer = projectileContainer;
		_numeric = owner.GetNodeOrNull<NumericComponent>("NumericComponent");
	}

	public void Equip(WeaponData weapon)
	{
		_weapon = weapon;
		_cooldown = 0f;
		_attacking = false;
		_meleeHitbox.Monitoring = false;

		UpdateVisual(weapon);

		if (weapon.Type == WeaponType.Melee)
		{
			ConfigureMeleeHitbox(weapon, _numeric != null ? GetMeleeRange() : weapon.Range);
		}
	}

	private void UpdateVisual(WeaponData weapon)
	{
		var hasIcon = !string.IsNullOrEmpty(weapon.IconPath);
		if (hasIcon)
		{
			var texture = GD.Load<Texture2D>(weapon.IconPath);
			_iconVisual.Texture = texture;
			_iconVisual.Visible = texture != null;
			_rangedVisual.Visible = false;
			_meleeVisual.Visible = false;
			return;
		}

		_iconVisual.Visible = false;
		_rangedVisual.Visible = weapon.Type == WeaponType.Ranged;
		_meleeVisual.Visible = weapon.Type == WeaponType.Melee;

		var visual = weapon.Type == WeaponType.Ranged ? (CanvasItem)_rangedVisual : _meleeVisual;
		visual.Modulate = weapon.VisualColor;
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_weapon == null || _owner == null || _numeric == null)
		{
			return;
		}

		if (!_attacking)
		{
			AimAtNearestTarget();
		}

		if (_attacking)
		{
			return;
		}

		_cooldown -= (float)delta;
		if (_cooldown > 0f)
		{
			return;
		}

		var target = FindNearestEnemy();
		if (target == null)
		{
			return;
		}

		if (_weapon.Type == WeaponType.Ranged)
		{
			FireRanged(target);
		}
		else
		{
			StartMeleeAttack(target);
		}

		_cooldown = GetAttackInterval();
	}

	private void AimAtNearestTarget()
	{
		var target = FindNearestEnemy();
		if (target == null)
		{
			return;
		}

		var direction = target.GlobalPosition - _pivot.GlobalPosition;
		if (direction.LengthSquared() < 0.01f)
		{
			return;
		}

		_pivot.Rotation = direction.Angle();
	}

	private float GetAttackRange()
	{
		if (_weapon == null)
		{
			return 0f;
		}

		return _weapon.Type == WeaponType.Ranged
			? _numeric![NumericType.WeaponRange] * (_weapon.Range / ParamsConfig.WeaponRangedRangeRef)
			: GetMeleeRange();
	}

	/// <summary>近战射程随 RunStats.WeaponRange 同比缩放（默认 320 为基准）。</summary>
	private float GetMeleeRange()
	{
		return _weapon!.Range * (_numeric![NumericType.WeaponRange] / ParamsConfig.WeaponMeleeRangeRef);
	}

	private float GetAttackInterval()
	{
		var speed = Mathf.Max(
			_numeric![NumericType.AttackSpeed] * _weapon!.AttackSpeedScale,
			ParamsConfig.WeaponMinAttackSpeed);
		return 1f / speed;
	}

	private int GetDamage()
	{
		return Mathf.Max((int)ParamsConfig.PlayerMinDamage, Mathf.RoundToInt(_numeric![NumericType.Damage] * _weapon!.DamageScale));
	}

	private Node2D? FindNearestEnemy()
	{
		var enemies = GetTree().GetNodesInGroup("enemies");
		Node2D? nearest = null;
		var range = GetAttackRange();
		var bestDistance = range * range;

		foreach (var node in enemies)
		{
			if (node is not Node2D enemy || !GodotObject.IsInstanceValid(enemy))
			{
				continue;
			}

			var distance = _owner.GlobalPosition.DistanceSquaredTo(enemy.GlobalPosition);
			if (distance > bestDistance)
			{
				continue;
			}

			bestDistance = distance;
			nearest = enemy;
		}

		return nearest;
	}

	private void FireRanged(Node2D target)
	{
		if (_weapon?.ProjectileScene == null)
		{
			return;
		}

		var direction = target.GlobalPosition - _muzzle.GlobalPosition;
		if (direction.LengthSquared() < 0.01f)
		{
			return;
		}

		var projectile = _weapon.ProjectileScene.Instantiate<Projectile>();
		_projectileContainer.AddChild(projectile);
		projectile.GlobalPosition = _muzzle.GlobalPosition;
		projectile.Launch(direction, _numeric![NumericType.ProjectileSpeed], GetDamage());
	}

	private void StartMeleeAttack(Node2D target)
	{
		if (_weapon == null)
		{
			return;
		}

		_attacking = true;
		_hitThisSwing.Clear();

		var direction = (target.GlobalPosition - _pivot.GlobalPosition).Normalized();
		var baseAngle = direction.Angle();
		_pivot.Rotation = baseAngle;

		ConfigureMeleeHitbox(_weapon, GetMeleeRange());
		_meleeHitbox.Monitoring = true;

		var tween = CreateTween();
		if (_weapon.MeleeStyle == MeleeStyle.Swing)
		{
			tween.TweenProperty(_pivot, "rotation", baseAngle + Mathf.DegToRad(ParamsConfig.WeaponSwingAnglePeakDeg), ParamsConfig.WeaponSwingTween1)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out);
			tween.TweenProperty(_pivot, "rotation", baseAngle - Mathf.DegToRad(ParamsConfig.WeaponSwingAngleTroughDeg), ParamsConfig.WeaponSwingTween2)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.InOut);
			tween.TweenProperty(_pivot, "rotation", baseAngle, ParamsConfig.WeaponSwingTween3);
		}
		else
		{
			var startPos = Vector2.Zero;
			var thrust = Vector2.FromAngle(baseAngle) * ParamsConfig.WeaponThrustDistance;
			tween.TweenProperty(_pivot, "position", thrust, ParamsConfig.WeaponThrustTweenOut)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.Out);
			tween.TweenProperty(_pivot, "position", startPos, ParamsConfig.WeaponThrustTweenBack)
				.SetTrans(Tween.TransitionType.Quad)
				.SetEase(Tween.EaseType.In);
		}

		tween.Finished += EndMeleeAttack;
	}

	private void EndMeleeAttack()
	{
		_meleeHitbox.Monitoring = false;
		_attacking = false;
		_pivot.Position = Vector2.Zero;
	}

	private void ConfigureMeleeHitbox(WeaponData weapon, float range)
	{
		if (_meleeShape.Shape is not RectangleShape2D rectangle)
		{
			return;
		}

		if (weapon.MeleeStyle == MeleeStyle.Thrust)
		{
			rectangle.Size = new Vector2(range * ParamsConfig.WeaponThrustHitboxWidthRatio, ParamsConfig.WeaponThrustHitboxHeight);
			_meleeShape.Position = new Vector2(range * ParamsConfig.WeaponThrustHitboxPosRatio, 0f);
		}
		else
		{
			rectangle.Size = new Vector2(range * ParamsConfig.WeaponSwingHitboxWidthRatio, ParamsConfig.WeaponSwingHitboxHeight);
			_meleeShape.Position = new Vector2(range * ParamsConfig.WeaponSwingHitboxPosRatio, 0f);
		}
	}

	private void OnMeleeBodyEntered(Node2D body)
	{
		TryDamageEnemy(body);
	}

	private void OnMeleeAreaEntered(Area2D area)
	{
		TryDamageEnemy(area);
	}

	private void TryDamageEnemy(Node node)
	{
		if (!_attacking || node is not Enemy enemy)
		{
			return;
		}

		var id = enemy.GetInstanceId();
		if (!_hitThisSwing.Add(id))
		{
			return;
		}

		enemy.TakeDamage(GetDamage());
	}
}
