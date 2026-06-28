using System.Collections.Generic;
using Godot;

namespace Hope.Core;

/// <summary>
/// 字符串键驱动的有限状态机。同一状态实例重复 Change 时不会重复 Enter。
/// 未知状态名会 PushError 并忽略切换。
/// </summary>
public class StateMachine
{
    private readonly Dictionary<string, IGameState> _states = new();
    private IGameState? _current;

    /// <summary>当前状态名；无状态时 null。</summary>
    public string? CurrentName { get; private set; }

    /// <summary>
    /// 注册命名状态；同名会覆盖旧实例。
    /// </summary>
    /// <param name="name">状态标识，如 "idle"、"move"。</param>
    /// <param name="state">实现 <see cref="IGameState"/> 的状态对象。</param>
    public void Add(string name, IGameState state)
    {
        _states[name] = state;
    }

    /// <summary>
    /// 切换到已注册状态：先 Exit 当前，再 Enter 目标。
    /// </summary>
    /// <param name="name">须已通过 <see cref="Add"/> 注册。</param>
    public void Change(string name)
    {
        if (!_states.TryGetValue(name, out var next))
        {
            GD.PushError($"StateMachine: unknown state '{name}'");
            return;
        }

        if (_current == next)
        {
            return;
        }

        _current?.Exit();
        _current = next;
        CurrentName = name;
        _current.Enter();
    }

    /// <summary>驱动当前状态的 Update；无当前状态时不执行。</summary>
    /// <param name="delta">帧间隔（秒）。</param>
    public void Update(double delta)
    {
        _current?.Update(delta);
    }
}
