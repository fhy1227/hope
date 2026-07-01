using Godot;
using Godot.Collections;

namespace Hope.Steam;

/// <summary>
/// Steam 全局入口（Autoload）：初始化 GodotSteam、驱动回调、对外暴露可用状态。
/// 不实现具体成就/联机等玩法，仅负责 Steamworks 生命周期。
/// </summary>
public partial class SteamManager : Node
{
    public static SteamManager? Instance { get; private set; }

    /// <summary>最近一次初始化返回的 status；未尝试初始化时为 null。</summary>
    public SteamInitResult? LastInitStatus { get; private set; }

    /// <summary>Steamworks 是否已成功初始化。</summary>
    public bool IsEnabled { get; private set; }

    /// <summary>初始化失败时的可读原因；成功时为空字符串。</summary>
    public string InitFailureReason { get; private set; } = string.Empty;

    /// <summary>当前 Steam 用户名；未初始化时为空。</summary>
    public string PersonaName { get; private set; } = string.Empty;

    /// <summary>当前用户 Steam ID；未初始化时为 0。</summary>
    public ulong SteamId { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
    }

    public override void _ExitTree()
    {
        if (IsEnabled)
        {
            SteamBridge.SteamShutdown();
            IsEnabled = false;
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public override void _Ready()
    {
        InitializeSteam();
    }

    public override void _Process(double delta)
    {
        if (IsEnabled)
        {
            SteamBridge.RunCallbacks();
        }
    }

    /// <summary>
    /// 初始化 Steamworks。App ID 读取项目设置 Steam &gt; Initialization；
    /// 失败时禁用 Steam 功能，游戏继续运行。
    /// </summary>
    public void InitializeSteam(uint appId = 0, bool embedCallbacks = false)
    {
        if (!SteamBridge.IsAvailable)
        {
            DisableSteam(SteamInitResult.Failed, "GodotSteam GDExtension not loaded");
            return;
        }

        var resolvedAppId = appId != 0 ? appId : ReadConfiguredAppId();
        var result = SteamBridge.SteamInitEx(resolvedAppId, embedCallbacks);
        ApplyInitResult(result);
    }

    private static uint ReadConfiguredAppId()
    {
        const string settingPath = "steam/initialization/app_data/app_id";
        if (!ProjectSettings.HasSetting(settingPath))
        {
            return 0;
        }

        return (uint)ProjectSettings.GetSetting(settingPath).AsInt32();
    }

    private void ApplyInitResult(Dictionary result)
    {
        if (!result.TryGetValue("status", out var statusVariant))
        {
            DisableSteam(SteamInitResult.Failed, "steamInitEx returned invalid result (missing status)");
            return;
        }

        var status = (SteamInitResult)(int)statusVariant;
        LastInitStatus = status;
        var verbal = result.TryGetValue("verbal", out var verbalVariant)
            ? (string)verbalVariant
            : string.Empty;

        if (status == SteamInitResult.Ok)
        {
            IsEnabled = true;
            InitFailureReason = string.Empty;
            PersonaName = SteamBridge.GetPersonaName();
            SteamId = SteamBridge.GetSteamId();

            GD.Print($"[SteamManager] Initialized. Player={PersonaName}, SteamId={SteamId}");
            return;
        }

        DisableSteam(status, string.IsNullOrEmpty(verbal)
            ? $"Steam init failed (status={(int)status})"
            : verbal);
    }

    private void DisableSteam(SteamInitResult status, string reason)
    {
        IsEnabled = false;
        InitFailureReason = reason;
        PersonaName = string.Empty;
        SteamId = 0;

        if (status == SteamInitResult.NoSteamClient)
        {
            GD.Print($"[SteamManager] Steam disabled: {reason}");
            GD.Print("[SteamManager] 提示：请先启动 Steam 客户端并登录，再运行游戏。未登录 Steam 时游戏仍可正常游玩，只是 Steam 功能不可用。");
            return;
        }

        GD.Print($"[SteamManager] Steam disabled: {reason}");
    }
}
