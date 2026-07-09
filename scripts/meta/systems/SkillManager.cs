using Godot;
using System.Collections.Generic;
using System.Linq;
using Hope.Persistence;
using Hope.SkillSystem;

namespace Hope.Systems;

/// <summary>
/// 技能管理 Autoload（meta 层）：持有玩家技能状态，接入局外存档。
/// </summary>
[PersistedData]
public partial class SkillManager : Node, IPersistedDataParticipant
{
    public static SkillManager? Instance { get; private set; }

    public PlayerSkillState State { get; private set; } = new();

    [Signal] public delegate void SkillStateChangedEventHandler();

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

    /// <summary>根据角色等级同步可用技能点。</summary>
    public void SyncPointsFromLevel(int characterLevel, int renownPoints = 0)
    {
        SkillPointCalculator.SyncAvailablePoints(State, characterLevel, renownPoints);
        EmitSignal(SignalName.SkillStateChanged);
    }

    /// <summary>投入技能点。</summary>
    public ESkillCastResult Invest(string skillId)
    {
        var def = SkillDB.GetDefinition(skillId);
        if (def == null)
        {
            return ESkillCastResult.NotLearned;
        }

        var result = State.Invest(skillId, def);
        if (result == ESkillCastResult.Success)
        {
            TryAutoAssignToBar(skillId, def);
            PersistenceMgr.Instance?.MarkDirty();
            EventBus.Instance?.EmitSkillLearned(skillId, State.InvestedRanks[skillId]);
            EmitSignal(SignalName.SkillStateChanged);
        }

        return result;
    }

    /// <summary>将已学主动技能填入空快捷键槽（战斗开始前或读档后调用）。</summary>
    public void AutoFillHotbarFromLearnedSkills()
    {
        var changed = false;
        foreach (var (skillId, rank) in State.InvestedRanks)
        {
            if (rank <= 0 || IsSkillAlreadyOnBar(skillId))
            {
                continue;
            }

            var def = SkillDB.GetDefinition(skillId);
            if (def == null || !CanAssignToBar(def))
            {
                continue;
            }

            var slot = FindFirstEmptySlot();
            if (slot < 0)
            {
                break;
            }

            State.AssignToSlot(skillId, slot);
            changed = true;
        }

        if (changed)
        {
            PersistenceMgr.Instance?.MarkDirty();
            EmitSignal(SignalName.SkillStateChanged);
        }
    }

    private void TryAutoAssignToBar(string skillId, SkillDefinition def)
    {
        if (!CanAssignToBar(def) || IsSkillAlreadyOnBar(skillId))
        {
            return;
        }

        var slot = FindFirstEmptySlot();
        if (slot < 0)
        {
            return;
        }

        State.AssignToSlot(skillId, slot);
    }

    private static bool CanAssignToBar(SkillDefinition def) =>
        !def.IsPassive && def.Tag != ESkillTag.KeyPassive;

    private bool IsSkillAlreadyOnBar(string skillId)
    {
        foreach (var slot in State.SkillSlots)
        {
            if (slot.SkillId == skillId)
            {
                return true;
            }
        }

        return false;
    }

    private int FindFirstEmptySlot()
    {
        for (var i = 0; i < State.SkillSlots.Length; i++)
        {
            if (string.IsNullOrEmpty(State.SkillSlots[i].SkillId))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>退还技能全部点数。</summary>
    public bool Refund(string skillId)
    {
        if (!State.Refund(skillId))
        {
            return false;
        }

        PersistenceMgr.Instance?.MarkDirty();
        EventBus.Instance?.EmitSkillRefunded(skillId);
        EmitSignal(SignalName.SkillStateChanged);
        return true;
    }

    /// <summary>退还一级技能点。</summary>
    public bool RefundOneRank(string skillId)
    {
        if (!State.RefundOneRank(skillId))
        {
            return false;
        }

        PersistenceMgr.Instance?.MarkDirty();
        EventBus.Instance?.EmitSkillRefunded(skillId);
        EmitSignal(SignalName.SkillStateChanged);
        return true;
    }

    /// <summary>重置所有技能点。</summary>
    public void RefundAll()
    {
        State.RefundAll();
        PersistenceMgr.Instance?.MarkDirty();
        EventBus.Instance?.EmitSkillTreeReset();
        EmitSignal(SignalName.SkillStateChanged);
    }

    /// <summary>选择强化分支。</summary>
    public bool ChooseEnhancement(string skillId, string enhancementId)
    {
        if (!State.ChooseEnhancement(skillId, enhancementId))
        {
            return false;
        }

        PersistenceMgr.Instance?.MarkDirty();
        EventBus.Instance?.EmitSkillEnhancementChosen(skillId, enhancementId);
        EmitSignal(SignalName.SkillStateChanged);
        return true;
    }

    /// <summary>绑定技能到快捷键槽位；skillId 为空则清空该槽。</summary>
    public bool AssignToSlot(string skillId, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= PlayerSkillState.SlotCount)
        {
            return false;
        }

        if (string.IsNullOrEmpty(skillId))
        {
            State.SkillSlots[slotIndex].SkillId = "";
            State.SkillSlots[slotIndex].IsEnhanced = false;
            PersistenceMgr.Instance?.MarkDirty();
            EmitSignal(SignalName.SkillStateChanged);
            return true;
        }

        if (State.InvestedRanks.GetValueOrDefault(skillId, 0) <= 0)
        {
            return false;
        }

        for (var i = 0; i < State.SkillSlots.Length; i++)
        {
            if (State.SkillSlots[i].SkillId == skillId)
            {
                State.SkillSlots[i].SkillId = "";
            }
        }

        State.SkillSlots[slotIndex].SkillId = skillId;
        PersistenceMgr.Instance?.MarkDirty();
        EmitSignal(SignalName.SkillStateChanged);
        return true;
    }

    public void LoadFromSave(SkillSaveData? data, int characterLevel)
    {
        if (data == null)
        {
            State = new PlayerSkillState();
            SyncPointsFromLevel(characterLevel);
            return;
        }

        State = new PlayerSkillState
        {
            AvailablePoints = data.AvailablePoints,
            TotalPointsSpent = data.TotalPointsSpent,
            InvestedRanks = new Dictionary<string, int>(data.InvestedRanks),
            ChosenEnhancements = new Dictionary<string, string>(data.ChosenEnhancements),
            ChosenKeyPassiveId = data.ChosenKeyPassiveId,
        };

        foreach (var slotData in data.SkillSlots)
        {
            if (slotData.Slot >= 0 && slotData.Slot < PlayerSkillState.SlotCount)
            {
                State.SkillSlots[slotData.Slot].SkillId = slotData.SkillId;
                State.SkillSlots[slotData.Slot].AssignedSlot = slotData.Slot;
            }
        }

        SkillPointCalculator.SyncAvailablePoints(State, characterLevel);
        AutoFillHotbarFromLearnedSkills();
        EmitSignal(SignalName.SkillStateChanged);
    }

    public SkillSaveData ExportToSave()
    {
        return new SkillSaveData
        {
            AvailablePoints = State.AvailablePoints,
            TotalPointsSpent = State.TotalPointsSpent,
            InvestedRanks = new Dictionary<string, int>(State.InvestedRanks),
            ChosenEnhancements = new Dictionary<string, string>(State.ChosenEnhancements),
            ChosenKeyPassiveId = State.ChosenKeyPassiveId,
            SkillSlots = State.SkillSlots.Select(s => new SkillSlotSaveData
            {
                Slot = s.AssignedSlot,
                SkillId = s.SkillId,
            }).ToList(),
        };
    }

    void IPersistedDataParticipant.ApplySaveData(CharacterSaveData data)
    {
        LoadFromSave(data.SkillState, data.Level);

        // 新角色无技能存档时赠送少量点数，便于体验技能树
        if (data.SkillState == null && State.TotalPointsSpent == 0)
        {
            State.AvailablePoints = Mathf.Max(State.AvailablePoints, 5);
        }
    }

    void IPersistedDataParticipant.CollectSaveData(CharacterSaveData data)
    {
        data.SkillState = ExportToSave();
    }

    void IPersistedDataParticipant.ClearPersistedState()
    {
        State = new PlayerSkillState();
        SyncPointsFromLevel(1);
        EmitSignal(SignalName.SkillStateChanged);
    }
}
