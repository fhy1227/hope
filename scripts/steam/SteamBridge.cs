using Godot;
using Godot.Collections;

namespace Hope.Steam;

/// <summary>
/// GodotSteam GDExtension 单例桥接。
/// 通过 <see cref="Engine.GetSingleton"/> 访问，避免 C# 编译期依赖编辑器生成的绑定。
/// 方法名与 GDScript 侧一致（如 <c>steamInitEx</c>、<c>run_callbacks</c>）。
/// </summary>
public static class SteamBridge
{
    /// <summary>GodotSteam 是否已加载（GDExtension 单例是否存在）。</summary>
    public static bool IsAvailable => Engine.HasSingleton("Steam");

    /// <summary>GodotSteam 单例；未加载时为 null。</summary>
    public static GodotObject? Api => IsAvailable ? Engine.GetSingleton("Steam") : null;

    /// <summary>
    /// 调用 <c>steamInitEx</c>。App ID 与嵌入回调可在项目设置 Steam &gt; Initialization 中配置。
    /// </summary>
    /// <param name="appId">Steam App ID；0 表示使用项目设置。</param>
    /// <param name="embedCallbacks">是否由 GodotSteam 内部驱动 <c>run_callbacks</c>。</param>
    /// <returns>包含 <c>status</c>（int）与 <c>verbal</c>（string）的字典。</returns>
    public static Dictionary SteamInitEx(uint appId = 0, bool embedCallbacks = false)
    {
        var api = Api;
        if (api == null)
        {
            return FailedResult("Steam singleton not found (GodotSteam GDExtension missing?)");
        }

        Variant raw = appId == 0 && !embedCallbacks
            ? Call(api, SteamMethod.SteamInitEx)
            : Call(api, SteamMethod.SteamInitEx, appId, embedCallbacks);

        if (raw.VariantType != Variant.Type.Dictionary)
        {
            return FailedResult($"steamInitEx returned unexpected type: {raw.VariantType}");
        }

        return (Dictionary)raw;
    }

    /// <summary>刷新 Steamworks 回调队列；未初始化时静默忽略。</summary>
    public static void RunCallbacks()
    {
        var api = Api;
        if (api != null)
        {
            Call(api, SteamMethod.RunCallbacks);
        }
    }

    /// <summary>关闭 Steamworks API；未初始化时静默忽略。</summary>
    public static void SteamShutdown()
    {
        var api = Api;
        if (api != null)
        {
            Call(api, SteamMethod.SteamShutdown);
        }
    }

    /// <summary>获取当前 Steam 用户名；失败时返回空字符串。</summary>
    public static string GetPersonaName()
    {
        var api = Api;
        if (api == null)
        {
            return string.Empty;
        }

        Variant raw = Call(api, SteamMethod.GetPersonaName);
        return raw.VariantType == Variant.Type.String ? (string)raw : string.Empty;
    }

    /// <summary>获取当前用户 Steam ID；失败时返回 0。</summary>
    public static ulong GetSteamId()
    {
        var api = Api;
        if (api == null)
        {
            return 0UL;
        }

        Variant raw = Call(api, SteamMethod.GetSteamId);
        return raw.VariantType == Variant.Type.Int ? (ulong)(long)raw : 0UL;
    }

    /// <summary>用户是否已订阅本游戏（含家庭共享等场景，需结合业务判断）。</summary>
    public static bool IsSubscribed()
    {
        var api = Api;
        if (api == null)
        {
            return false;
        }

        Variant raw = Call(api, SteamMethod.IsSubscribed);
        return raw.VariantType == Variant.Type.Bool && (bool)raw;
    }

    private static Dictionary FailedResult(string verbal)
    {
        return new Dictionary
        {
            ["status"] = (int)SteamInitResult.Failed,
            ["verbal"] = verbal,
        };
    }

    private static Variant Call(GodotObject api, StringName method, params Variant[] args)
    {
        return api.Call(method, args);
    }

    /// <summary>GodotSteam GDScript API 方法名（与插件文档一致，勿用 snake_case）。</summary>
    private static class SteamMethod
    {
        public static readonly StringName SteamInitEx = "steamInitEx";
        public static readonly StringName RunCallbacks = "run_callbacks";
        public static readonly StringName SteamShutdown = "steamShutdown";
        public static readonly StringName GetPersonaName = "getPersonaName";
        public static readonly StringName GetSteamId = "getSteamID";
        public static readonly StringName IsSubscribed = "isSubscribed";
    }
}
