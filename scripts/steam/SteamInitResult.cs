namespace Hope.Steam;

/// <summary>
/// <c>steamInitEx</c> 返回的 status 字段，对应 GodotSteam <c>STEAM_API_INIT_RESULT_*</c> 常量。
/// </summary>
public enum SteamInitResult
{
    /// <summary>初始化成功。</summary>
    Ok = 0,

    /// <summary>其他失败。</summary>
    Failed = 1,

    /// <summary>无法连接 Steam（客户端未运行等）。</summary>
    NoSteamClient = 2,

    /// <summary>Steam 客户端版本过旧。</summary>
    SteamClientOutdated = 3,
}
