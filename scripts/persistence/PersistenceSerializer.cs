using System;
using System.Text.Json;
using Hope.Persistence;

namespace Hope.Persistence;

/// <summary>
/// 角色存档 JSON 序列化/反序列化；与磁盘读写解耦，供 <see cref="PersistenceMgr"/> 使用。
/// </summary>
public static class PersistenceSerializer
{
    /// <summary>与 <c>character.json</c> 读写共用的 JSON 选项。</summary>
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = null,
    };

    /// <summary>将 JSON 文本反序列化为 <see cref="CharacterSaveData"/>。</summary>
    /// <returns>解析成功返回对象；失败返回 null 并通过 <paramref name="error"/> 说明原因。</returns>
    public static CharacterSaveData? DeserializeCharacter(string json, out string? error)
    {
        error = null;
        try
        {
            return JsonSerializer.Deserialize<CharacterSaveData>(json, JsonOptions);
        }
        catch (Exception ex)
        {
            error = ex.Message;
            return null;
        }
    }

    /// <summary>将角色存档序列化为 JSON 文本。</summary>
    public static string SerializeCharacter(CharacterSaveData data) =>
        JsonSerializer.Serialize(data, JsonOptions);
}
