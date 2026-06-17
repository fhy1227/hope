using Godot;

namespace Hope.UI.Framework;

/// <summary>
/// C# 桥接 GodotUIFramework 的 UIGroupComponent（GDScript）。
/// </summary>
public static class UiGroupBridge
{
    public static Control? ShowScene(Node group, StringName viewId, Godot.Collections.Dictionary? data = null)
    {
        data ??= new Godot.Collections.Dictionary();
        var result = group.Call("show_scene", viewId, data);
        return result.AsGodotObject() as Control;
    }

    public static void CloseCurrentScene(Node group)
    {
        group.Call("close_current_scene");
    }
}
