using Godot;
using Hope.Components.Actions;
using Hope.Core;
using Hope.Entities;
using Hope.Systems;

namespace Hope.SkillSystem;

/// <summary>
/// 技能施放系统：挂载于战斗场景，处理快捷键施放、冷却、怒气消耗与基础伤害。
/// </summary>
public partial class SkillCastingSystem : Node
{
    private static readonly string[] SlotActions =
    [
        "skill_slot_1", "skill_slot_2", "skill_slot_3",
        "skill_slot_4", "skill_slot_5", "skill_slot_6",
    ];

    public static SkillCastingSystem? Instance { get; private set; }

    private Player? _player;

    public override void _EnterTree()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void _Ready()
    {
        CallDeferred(MethodName.BindPlayer);
    }

    /// <summary>绑定当前对局玩家实体。</summary>
    public void BindPlayer(Player? player)
    {
        _player = player;
        SkillManager.Instance?.AutoFillHotbarFromLearnedSkills();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (GetTree().Paused)
        {
            return;
        }

        for (var i = 0; i < SlotActions.Length; i++)
        {
            if (!@event.IsActionPressed(SlotActions[i]))
            {
                continue;
            }

            GetViewport().SetInputAsHandled();
            TryCastFromSlot(i);
            return;
        }
    }

    /// <summary>从快捷键槽位施放技能。</summary>
    public ESkillCastResult TryCastFromSlot(int slotIndex)
    {
        var manager = SkillManager.Instance;
        if (manager == null)
        {
            return ESkillCastResult.NotLearned;
        }

        if (slotIndex < 0 || slotIndex >= PlayerSkillState.SlotCount)
        {
            return ESkillCastResult.NoValidTarget;
        }

        var slot = manager.State.SkillSlots[slotIndex];
        if (slot.IsEmpty)
        {
            return ESkillCastResult.NotLearned;
        }

        var def = SkillDB.GetDefinition(slot.SkillId);
        if (def == null)
        {
            return ESkillCastResult.NotLearned;
        }

        return CastSkill(def, slot, slotIndex);
    }

    /// <summary>施放技能主入口。</summary>
    public ESkillCastResult CastSkill(SkillDefinition def, SkillSlotState slotState, int slotIndex = -1)
    {
        if (def.IsPassive)
        {
            return ESkillCastResult.NoValidTarget;
        }

        var manager = SkillManager.Instance;
        if (manager == null)
        {
            return ESkillCastResult.NotLearned;
        }

        var rank = manager.State.InvestedRanks.GetValueOrDefault(def.SkillId, 0);
        if (rank <= 0)
        {
            return ESkillCastResult.NotLearned;
        }

        if (CooldownManager.Instance?.IsOnCooldown(def.SkillId) == true)
        {
            return ESkillCastResult.OnCooldown;
        }

        var effect = def.EffectResource;
        if (effect == null)
        {
            return ESkillCastResult.NoValidTarget;
        }

        var resourceCost = effect.GetResourceCost(rank);
        if (resourceCost > 0f && FuryResourceSystem.Instance?.CanAfford(resourceCost) != true)
        {
            return ESkillCastResult.InsufficientResource;
        }

        if (_player == null || !GodotObject.IsInstanceValid(_player))
        {
            return ESkillCastResult.NoValidTarget;
        }

        if (resourceCost > 0f)
        {
            FuryResourceSystem.Instance?.Spend(resourceCost);
        }

        var cooldown = effect.GetCooldown(rank);
        if (cooldown > 0f)
        {
            CooldownManager.Instance?.StartCooldown(def.SkillId, cooldown, slotIndex);
        }

        ExecuteEffect(def, rank, slotState.AssignedSlot);

        EventBus.Instance?.EmitSkillCast(def.SkillId, rank, _player.GlobalPosition);
        return ESkillCastResult.Success;
    }

    private void ExecuteEffect(SkillDefinition def, int rank, int slotIndex)
    {
        if (_player == null || def.EffectResource == null)
        {
            return;
        }

        var effect = def.EffectResource;
        var weaponDamage = _player.GetActionDamage(1f);
        var damage = Mathf.Max(1, Mathf.RoundToInt(effect.GetFinalDamage(rank, weaponDamage)));

        if (effect.FuryGenerated > 0f)
        {
            FuryResourceSystem.Instance?.Generate(effect.FuryGenerated);
        }

        var aim = _player.GlobalPosition + _player.FacingDirection * 80f;
        var targets = SkillEffectResolver.ResolveTargets(def, rank, _player, aim);

        if (targets.Count > 0)
        {
            foreach (var enemy in targets)
            {
                enemy.TakeDamage(damage);
                EventBus.Instance?.EmitSkillHit(def.SkillId, enemy, damage);

                if (effect.KnockbackForce > 0f)
                {
                    var dir = enemy.GlobalPosition - _player.GlobalPosition;
                    if (dir.LengthSquared() < 0.01f)
                    {
                        dir = Vector2.Right;
                    }

                    enemy.ApplyKnockback(dir.Normalized(), effect.KnockbackForce);
                }
            }
        }
        else if (effect.HitShape is EHitShape.Circle or EHitShape.Cone)
        {
            CombatPulse.HitCount(_player, effect.Radius, damage, effect.KnockbackForce);
        }

        SpawnHitVfx(effect);
        EventBus.Instance?.EmitSkillEnded(def.SkillId);
    }

    private void SpawnHitVfx(SkillEffectResource effect)
    {
        if (_player == null)
        {
            return;
        }

        if (ResourceLoader.Exists(ScenePaths.CircleFlashEffect))
        {
            var scene = ResourceLoader.Load<PackedScene>(ScenePaths.CircleFlashEffect);
            var fx = scene?.Instantiate<Node2D>();
            if (fx == null)
            {
                return;
            }

            Combat.Instance?.World.GetNodeOrNull("Effects")?.AddChild(fx);
            fx.GlobalPosition = _player.GlobalPosition;
            fx.Scale = Vector2.One * effect.Radius / 80f * effect.HitEffectScale;
        }
    }

    private void BindPlayer()
    {
        _player = Combat.Instance?.Run?.Player;
        if (_player != null)
        {
            return;
        }

        GetTree().CreateTimer(0.5f).Timeout += () => _player = Combat.Instance?.Run?.Player;
    }
}
