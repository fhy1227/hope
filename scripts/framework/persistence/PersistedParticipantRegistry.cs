using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

namespace Hope.Persistence;

/// <summary>
/// 反射收集带 <see cref="PersistedDataAttribute"/> 的 <see cref="IPersistedDataParticipant"/> 类型，
/// 并通过各类型静态 <c>Instance</c> 属性解析运行时单例。
/// </summary>
public static class PersistedParticipantRegistry
{
    private static IReadOnlyList<Type>? _participantTypes;

    /// <summary>返回当前已就绪的参与者实例（<c>Instance</c> 为 null 的会被跳过）。</summary>
    public static IEnumerable<IPersistedDataParticipant> GetActiveParticipants()
    {
        foreach (var type in GetParticipantTypes())
        {
            if (TryResolveInstance(type, out var participant))
            {
                yield return participant;
            }
        }
    }

    private static IReadOnlyList<Type> GetParticipantTypes()
    {
        if (_participantTypes != null)
        {
            return _participantTypes;
        }

        var assembly = typeof(PersistedDataAttribute).Assembly;
        _participantTypes = assembly
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .Where(t => t.IsDefined(typeof(PersistedDataAttribute), inherit: false))
            .Where(t => typeof(IPersistedDataParticipant).IsAssignableFrom(t))
            .ToList();

        GD.Print($"[PersistedParticipantRegistry] 发现 {_participantTypes.Count} 个持久化参与者类型");
        return _participantTypes;
    }

    private static bool TryResolveInstance(Type type, out IPersistedDataParticipant participant)
    {
        participant = null!;

        var instanceProp = type.GetProperty(
            "Instance",
            BindingFlags.Public | BindingFlags.Static);

        if (instanceProp == null)
        {
            GD.PrintErr($"[PersistedParticipantRegistry] {type.Name} 缺少 public static Instance 属性");
            return false;
        }

        if (instanceProp.GetValue(null) is not IPersistedDataParticipant resolved)
        {
            return false;
        }

        participant = resolved;
        return true;
    }
}
