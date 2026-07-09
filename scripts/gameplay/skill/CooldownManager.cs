using Godot;
using System.Collections.Generic;

namespace Hope.SkillSystem;

/// <summary>单个技能的冷却条目。</summary>
public class CooldownEntry
{
    public string SkillId { get; set; } = "";
    public float TotalDuration { get; set; }
    public float Remaining { get; set; }
    public int SlotIndex { get; set; } = -1;
}

/// <summary>全局技能冷却管理器（战斗内临时状态，不持久化）。</summary>
public partial class CooldownManager : Node
{
    public static CooldownManager? Instance { get; private set; }

    private readonly Dictionary<string, CooldownEntry> _cooldowns = new();
    private readonly List<string> _toRemove = [];

    public float GlobalCooldownReduction { get; set; }

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

    public override void _Process(double delta)
    {
        var dt = (float)delta;
        _toRemove.Clear();

        foreach (var (skillId, entry) in _cooldowns)
        {
            entry.Remaining -= dt;
            if (entry.Remaining <= 0f)
            {
                _toRemove.Add(skillId);
                EventBus.Instance?.EmitSkillCooldownEnded(skillId);
            }
        }

        foreach (var skillId in _toRemove)
        {
            _cooldowns.Remove(skillId);
        }
    }

    /// <summary>启动技能冷却。</summary>
    public void StartCooldown(string skillId, float baseDuration, int slotIndex = -1)
    {
        var finalDuration = Mathf.Max(0.1f, baseDuration * (1f - GlobalCooldownReduction));
        _cooldowns[skillId] = new CooldownEntry
        {
            SkillId = skillId,
            TotalDuration = finalDuration,
            Remaining = finalDuration,
            SlotIndex = slotIndex,
        };

        EventBus.Instance?.EmitSkillCooldownStarted(skillId, finalDuration);
    }

    public float GetRemaining(string skillId)
    {
        return _cooldowns.TryGetValue(skillId, out var entry)
            ? Mathf.Max(0f, entry.Remaining)
            : 0f;
    }

    public bool IsOnCooldown(string skillId)
    {
        return _cooldowns.ContainsKey(skillId) && _cooldowns[skillId].Remaining > 0f;
    }

    public float GetProgress(string skillId)
    {
        if (!_cooldowns.TryGetValue(skillId, out var entry) || entry.TotalDuration <= 0f)
        {
            return 1f;
        }

        return 1f - entry.Remaining / entry.TotalDuration;
    }

    public void EndCooldown(string skillId)
    {
        _cooldowns.Remove(skillId);
        EventBus.Instance?.EmitSkillCooldownEnded(skillId);
    }

    public void ClearAll()
    {
        _cooldowns.Clear();
    }
}
