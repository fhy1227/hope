using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using Hope.Persistence;

namespace Hope.Persistence;

/// <summary>
/// 局外存档 Autoload：本地缓存读写、JSON 序列化，以及通过 <see cref="PersistedDataAttribute"/>
/// 收集参与者并分发读档/存档。
/// </summary>
public partial class PersistenceMgr : Node
{
    public static PersistenceMgr? Instance { get; private set; }

    /// <summary>开发开关：为 true 时跳过读写（仅本地调试）。</summary>
    [Export]
    public bool DevSkipPersistence { get; set; }

    /// <summary>当前活动角色栏位；-1 表示未加载。</summary>
    public int ActiveSlotIndex { get; private set; } = -1;

    /// <summary>内存中的角色存档根对象。</summary>
    public CharacterSaveData? ActiveCharacter { get; private set; }

    private bool _dirty;

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

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            FlushSave();
        }
    }

    /// <summary>是否存在指定栏位存档。</summary>
    public bool HasProfile(int slotIndex)
    {
        if (DevSkipPersistence)
        {
            return false;
        }

        return Godot.FileAccess.FileExists(SaveSchema.GetCharacterPath(slotIndex));
    }

    /// <summary>创建新角色并写盘。</summary>
    public bool CreateProfile(int slotIndex, string characterName)
    {
        if (slotIndex < 0 || slotIndex >= SaveSchema.MaxProfileSlots)
        {
            GD.PrintErr($"[PersistenceMgr] 无效栏位: {slotIndex}");
            return false;
        }

        ActiveSlotIndex = slotIndex;
        ActiveCharacter = CharacterSaveData.CreateDefault(characterName);

        if (DevSkipPersistence)
        {
            ApplyToParticipants(ActiveCharacter);
            return true;
        }

        if (!WriteCharacterFile(slotIndex, ActiveCharacter))
        {
            return false;
        }

        WriteMetaLastSlot(slotIndex);
        ApplyToParticipants(ActiveCharacter);
        GD.Print($"[PersistenceMgr] 新角色已创建: slot={slotIndex} name={characterName}");
        return true;
    }

    /// <summary>从本地缓存读盘并注入各持久化参与者。</summary>
    public bool Load(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SaveSchema.MaxProfileSlots)
        {
            GD.PrintErr($"[PersistenceMgr] 无效栏位: {slotIndex}");
            return false;
        }

        if (DevSkipPersistence)
        {
            ActiveSlotIndex = slotIndex;
            ActiveCharacter = CharacterSaveData.CreateDefault("Dev");
            ApplyToParticipants(ActiveCharacter);
            return true;
        }

        var path = SaveSchema.GetCharacterPath(slotIndex);
        if (!Godot.FileAccess.FileExists(path))
        {
            GD.PrintErr($"[PersistenceMgr] 存档不存在: {path}");
            return false;
        }

        using var file = Godot.FileAccess.Open(path, Godot.FileAccess.ModeFlags.Read);
        if (file == null)
        {
            GD.PrintErr($"[PersistenceMgr] 无法读取: {path}");
            return false;
        }

        ActiveCharacter = PersistenceSerializer.DeserializeCharacter(file.GetAsText(), out var error);
        if (ActiveCharacter == null)
        {
            GD.PrintErr($"[PersistenceMgr] 解析失败: {error ?? "存档内容为空"}");
            return false;
        }

        if (ActiveCharacter.SchemaVersion > SaveSchema.CurrentVersion)
        {
            GD.PrintErr("[PersistenceMgr] 存档版本高于当前客户端，无法加载");
            ActiveCharacter = null;
            return false;
        }

        ActiveSlotIndex = slotIndex;
        WriteMetaLastSlot(slotIndex);
        ApplyToParticipants(ActiveCharacter);
        _dirty = false;
        GD.Print($"[PersistenceMgr] 读档成功: slot={slotIndex} name={ActiveCharacter.CharacterName}");
        return true;
    }

    /// <summary>从各参与者收集数据、序列化并写入本地缓存。</summary>
    public bool Save()
    {
        if (DevSkipPersistence || ActiveSlotIndex < 0)
        {
            return false;
        }

        if (ActiveCharacter == null)
        {
            return false;
        }

        CollectFromParticipants(ActiveCharacter);
        ActiveCharacter.LastSavedUnixMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        ActiveCharacter.SchemaVersion = SaveSchema.CurrentVersion;

        var ok = WriteCharacterFile(ActiveSlotIndex, ActiveCharacter);
        if (ok)
        {
            _dirty = false;
            GD.Print($"[PersistenceMgr] 存档已写入: slot={ActiveSlotIndex}");
        }

        return ok;
    }

    /// <summary>标记脏数据；在退关等节点调用 <see cref="FlushSave"/> 合并写盘。</summary>
    public void MarkDirty() => _dirty = true;

    /// <summary>若有未保存变更则写盘。</summary>
    public void FlushSave()
    {
        if (_dirty || ActiveSlotIndex >= 0)
        {
            Save();
        }
    }

    /// <summary>删除指定栏位存档。</summary>
    public bool DeleteProfile(int slotIndex)
    {
        if (DevSkipPersistence)
        {
            return false;
        }

        var path = SaveSchema.GetCharacterPath(slotIndex);
        if (Godot.FileAccess.FileExists(path))
        {
            DirAccess.RemoveAbsolute(path);
        }

        if (ActiveSlotIndex == slotIndex)
        {
            ActiveSlotIndex = -1;
            ActiveCharacter = null;
            ClearAllParticipants();
        }

        GD.Print($"[PersistenceMgr] 已删除栏位: {slotIndex}");
        return true;
    }

    /// <summary>读取 meta.json 中的上次游玩栏位；-1 表示无记录。</summary>
    public int GetLastPlayedSlotIndex()
    {
        if (DevSkipPersistence)
        {
            return -1;
        }

        var metaPath = SaveSchema.SavesRoot + SaveSchema.MetaFileName;
        if (!Godot.FileAccess.FileExists(metaPath))
        {
            return -1;
        }

        using var file = Godot.FileAccess.Open(metaPath, Godot.FileAccess.ModeFlags.Read);
        if (file == null)
        {
            return -1;
        }

        try
        {
            var meta = JsonSerializer.Deserialize<MetaSaveData>(file.GetAsText(), PersistenceSerializer.JsonOptions);
            return meta?.LastSlotIndex ?? -1;
        }
        catch
        {
            return -1;
        }
    }

    /// <summary>将内存存档分发给所有 <see cref="PersistedDataAttribute"/> 参与者。</summary>
    public void ApplyToParticipants(CharacterSaveData data)
    {
        foreach (var participant in PersistedParticipantRegistry.GetActiveParticipants())
        {
            participant.ApplySaveData(data);
        }
    }

    /// <summary>从所有参与者汇总数据到 <paramref name="data"/>。</summary>
    public void CollectFromParticipants(CharacterSaveData data)
    {
        foreach (var participant in PersistedParticipantRegistry.GetActiveParticipants())
        {
            participant.CollectSaveData(data);
        }
    }

    private static void ClearAllParticipants()
    {
        foreach (var participant in PersistedParticipantRegistry.GetActiveParticipants())
        {
            participant.ClearPersistedState();
        }
    }

    private bool WriteCharacterFile(int slotIndex, CharacterSaveData data)
    {
        EnsureSlotDirectory(slotIndex);

        var tempPath = SaveSchema.GetCharacterTempPath(slotIndex);
        var finalPath = SaveSchema.GetCharacterPath(slotIndex);
        var json = PersistenceSerializer.SerializeCharacter(data);

        using (var temp = Godot.FileAccess.Open(tempPath, Godot.FileAccess.ModeFlags.Write))
        {
            if (temp == null)
            {
                GD.PrintErr($"[PersistenceMgr] 无法写入临时文件: {tempPath}");
                return false;
            }

            temp.StoreString(json);
        }

        if (Godot.FileAccess.FileExists(finalPath))
        {
            DirAccess.RemoveAbsolute(finalPath);
        }

        var renameError = DirAccess.RenameAbsolute(tempPath, finalPath);
        if (renameError != Error.Ok)
        {
            GD.PrintErr($"[PersistenceMgr] 重命名失败: {renameError}");
            return false;
        }

        return true;
    }

    private static void EnsureSlotDirectory(int slotIndex)
    {
        var root = SaveSchema.SavesRoot;
        if (!DirAccess.DirExistsAbsolute(root))
        {
            DirAccess.MakeDirRecursiveAbsolute(root);
        }

        var slotDir = SaveSchema.GetSlotDirectory(slotIndex);
        if (!DirAccess.DirExistsAbsolute(slotDir))
        {
            DirAccess.MakeDirRecursiveAbsolute(slotDir);
        }
    }

    private void WriteMetaLastSlot(int slotIndex)
    {
        if (DevSkipPersistence)
        {
            return;
        }

        EnsureSlotDirectory(0);

        var meta = new MetaSaveData
        {
            SchemaVersion = SaveSchema.CurrentVersion,
            LastSlotIndex = slotIndex,
        };

        var metaPath = SaveSchema.SavesRoot + SaveSchema.MetaFileName;
        using var file = Godot.FileAccess.Open(metaPath, Godot.FileAccess.ModeFlags.Write);
        file?.StoreString(JsonSerializer.Serialize(meta, PersistenceSerializer.JsonOptions));
    }

    private sealed class MetaSaveData
    {
        [JsonPropertyName("schema_version")]
        public int SchemaVersion { get; set; }

        [JsonPropertyName("last_slot_index")]
        public int LastSlotIndex { get; set; } = -1;
    }
}
