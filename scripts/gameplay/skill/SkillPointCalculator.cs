using Godot;

namespace Hope.SkillSystem;

/// <summary>技能点成长公式（暗黑4 规则简化版）。</summary>
public static class SkillPointCalculator
{
    /// <summary>计算角色应获得的总技能点（1~50 级每级 1 点 + 声望奖励）。</summary>
    public static int CalculateTotalPoints(int level, int renownPoints = 0)
    {
        var levelPoints = Mathf.Max(0, Mathf.Min(level, 50) - 1);
        return levelPoints + renownPoints;
    }

    /// <summary>升级时获得的新技能点数。</summary>
    public static int GetPointsOnLevelUp(int newLevel)
    {
        return newLevel <= 50 ? 1 : 0;
    }

    /// <summary>根据角色等级同步可用技能点（总点数 - 已花费）。</summary>
    public static void SyncAvailablePoints(PlayerSkillState state, int characterLevel, int renownPoints = 0)
    {
        var total = CalculateTotalPoints(characterLevel, renownPoints);
        state.AvailablePoints = total - state.TotalPointsSpent;
    }
}
