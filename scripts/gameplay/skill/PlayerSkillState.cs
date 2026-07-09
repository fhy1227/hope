using System.Collections.Generic;
using System.Linq;

namespace Hope.SkillSystem;

/// <summary>快捷键槽位状态。</summary>
public class SkillSlotState
{
    public string SkillId { get; set; } = "";
    public int AssignedSlot { get; set; }
    public bool IsEnhanced { get; set; }

    public bool IsEmpty => string.IsNullOrEmpty(SkillId);
}

/// <summary>玩家技能运行时状态：加点、强化选择、快捷键绑定。</summary>
public class PlayerSkillState
{
    public const int SlotCount = 6;

    public Dictionary<string, int> InvestedRanks { get; set; } = new();
    public Dictionary<string, string> ChosenEnhancements { get; set; } = new();
    public string ChosenKeyPassiveId { get; set; } = "";
    public int AvailablePoints { get; set; }
    public int TotalPointsSpent { get; set; }
    public SkillSlotState[] SkillSlots { get; set; } = new SkillSlotState[SlotCount];

    public PlayerSkillState()
    {
        for (var i = 0; i < SkillSlots.Length; i++)
        {
            SkillSlots[i] = new SkillSlotState { AssignedSlot = i };
        }
    }

    /// <summary>是否满足加点条件（不含点数检查时可单独用于 UI 预览）。</summary>
    public bool CanInvest(string skillId, SkillDefinition def)
    {
        if (AvailablePoints <= 0)
        {
            return false;
        }

        var currentRank = InvestedRanks.GetValueOrDefault(skillId, 0);
        if (currentRank >= def.MaxRank)
        {
            return false;
        }

        foreach (var prereqId in def.PrerequisiteIds)
        {
            if (InvestedRanks.GetValueOrDefault(prereqId, 0) <= 0)
            {
                return false;
            }
        }

        if (TotalPointsSpent < def.PointsRequired)
        {
            return false;
        }

        if (def.Tag == ESkillTag.KeyPassive)
        {
            if (currentRank > 0)
            {
                return false;
            }

            if (!string.IsNullOrEmpty(ChosenKeyPassiveId) && ChosenKeyPassiveId != skillId)
            {
                return false;
            }
        }

        if (def.IsUltimate)
        {
            if (currentRank > 0)
            {
                return false;
            }

            if (HasUltimateInvested(skillId))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>投入 1 点技能点。</summary>
    public ESkillCastResult Invest(string skillId, SkillDefinition def)
    {
        if (!CanInvest(skillId, def))
        {
            return ESkillCastResult.InsufficientResource;
        }

        var currentRank = InvestedRanks.GetValueOrDefault(skillId, 0);
        InvestedRanks[skillId] = currentRank + 1;
        AvailablePoints--;
        TotalPointsSpent++;

        if (def.Tag == ESkillTag.KeyPassive)
        {
            ChosenKeyPassiveId = skillId;
        }

        return ESkillCastResult.Success;
    }

    /// <summary>是否可退还该技能的所有点数。</summary>
    public bool CanRefund(string skillId)
    {
        if (!InvestedRanks.TryGetValue(skillId, out var currentRank) || currentRank <= 0)
        {
            return false;
        }

        foreach (var kv in InvestedRanks)
        {
            if (kv.Key == skillId || kv.Value <= 0)
            {
                continue;
            }

            var def = SkillDB.GetDefinition(kv.Key);
            if (def == null)
            {
                continue;
            }

            if (def.PrerequisiteIds.Contains(skillId))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>退还指定技能的全部点数。</summary>
    public bool Refund(string skillId)
    {
        if (!CanRefund(skillId))
        {
            return false;
        }

        if (!InvestedRanks.TryGetValue(skillId, out var currentRank))
        {
            return false;
        }

        AvailablePoints += currentRank;
        TotalPointsSpent -= currentRank;
        InvestedRanks.Remove(skillId);
        ChosenEnhancements.Remove(skillId);

        if (ChosenKeyPassiveId == skillId)
        {
            ChosenKeyPassiveId = "";
        }

        for (var i = 0; i < SkillSlots.Length; i++)
        {
            if (SkillSlots[i].SkillId == skillId)
            {
                SkillSlots[i].SkillId = "";
                SkillSlots[i].IsEnhanced = false;
            }
        }

        return true;
    }

    /// <summary>退还一级技能点。</summary>
    public bool RefundOneRank(string skillId)
    {
        if (!InvestedRanks.TryGetValue(skillId, out var currentRank) || currentRank <= 0)
        {
            return false;
        }

        if (currentRank == 1 && !CanRefund(skillId))
        {
            return false;
        }

        if (currentRank == 1)
        {
            return Refund(skillId);
        }

        InvestedRanks[skillId] = currentRank - 1;
        AvailablePoints++;
        TotalPointsSpent--;
        return true;
    }

    /// <summary>重置所有技能点与快捷键绑定。</summary>
    public void RefundAll()
    {
        AvailablePoints += TotalPointsSpent;
        TotalPointsSpent = 0;
        InvestedRanks.Clear();
        ChosenEnhancements.Clear();
        ChosenKeyPassiveId = "";

        for (var i = 0; i < SkillSlots.Length; i++)
        {
            SkillSlots[i].SkillId = "";
            SkillSlots[i].IsEnhanced = false;
        }
    }

    /// <summary>将技能绑定到快捷键槽位。</summary>
    public bool AssignToSlot(string skillId, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SkillSlots.Length)
        {
            return false;
        }

        if (InvestedRanks.GetValueOrDefault(skillId, 0) <= 0)
        {
            return false;
        }

        for (var i = 0; i < SkillSlots.Length; i++)
        {
            if (SkillSlots[i].SkillId == skillId)
            {
                SkillSlots[i].SkillId = "";
            }
        }

        SkillSlots[slotIndex].SkillId = skillId;
        return true;
    }

    /// <summary>选择强化分支。</summary>
    public bool ChooseEnhancement(string skillId, string enhancementId)
    {
        if (InvestedRanks.GetValueOrDefault(skillId, 0) <= 0)
        {
            return false;
        }

        ChosenEnhancements[skillId] = enhancementId;
        return true;
    }

    private bool HasUltimateInvested(string excludeSkillId)
    {
        foreach (var kv in InvestedRanks)
        {
            if (kv.Key == excludeSkillId || kv.Value <= 0)
            {
                continue;
            }

            var def = SkillDB.GetDefinition(kv.Key);
            if (def is { IsUltimate: true })
            {
                return true;
            }
        }

        return false;
    }
}
