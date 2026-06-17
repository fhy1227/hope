using System.Collections.Generic;
using Godot;

namespace Hope.Core;

public class StateMachine
{
    private readonly Dictionary<string, IGameState> _states = new();
    private IGameState? _current;

    public string? CurrentName { get; private set; }

    public void Add(string name, IGameState state)
    {
        _states[name] = state;
    }

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

    public void Update(double delta)
    {
        _current?.Update(delta);
    }
}
