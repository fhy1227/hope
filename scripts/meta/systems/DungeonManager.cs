using System.Collections.Generic;
using System.Linq;
using Godot;
using Hope.Config;
using Hope.Core;
using Hope.Persistence;

namespace Hope.Systems;

/// <summary>
/// 副本选择与进入管理；跨场景保持当前选中副本，并参与局外存档。
/// </summary>
[PersistedData]
public partial class DungeonManager : Node, IPersistedDataParticipant
{
    public static DungeonManager? Instance { get; private set; }

    /// <summary>当前选中的副本配置；未选择时为 null。</summary>
    public DungeonConfig? CurrentDungeon { get; private set; }

    private List<DungeonConfig> _allDungeons = [];

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

    public override void _Ready()
    {
        _allDungeons = ConfigManager.GetAll<DungeonConfig>();
    }

    /// <summary>获取全部副本配置（已排序）。</summary>
    public IReadOnlyList<DungeonConfig> GetAllDungeons() => _allDungeons;

    /// <summary>按 Id 查找副本。</summary>
    public DungeonConfig? GetDungeon(int dungeonId) =>
        _allDungeons.FirstOrDefault(d => d.Id == dungeonId);

    /// <summary>选中副本；无效 Id 时返回 false。</summary>
    public bool SelectDungeon(int dungeonId)
    {
        var dungeon = GetDungeon(dungeonId);
        if (dungeon == null)
        {
            return false;
        }

        CurrentDungeon = dungeon;
        return true;
    }

    /// <summary>副本是否已解锁（等级 + 前置通关）。</summary>
    public bool IsDungeonUnlocked(DungeonConfig dungeon)
    {
        var save = PersistenceMgr.Instance?.ActiveCharacter;
        if (save == null)
        {
            return false;
        }

        if (save.Level < dungeon.MinPlayerLevel)
        {
            return false;
        }

        if (dungeon.RequiredClearedDungeonId <= 0)
        {
            return true;
        }

        return save.ClearedDungeons.Contains(dungeon.RequiredClearedDungeonId);
    }

    /// <summary>进入当前选中副本（切换至战斗场景）。</summary>
    public void EnterDungeon()
    {
        if (CurrentDungeon == null)
        {
            GD.PushWarning("[DungeonManager] 未选择副本");
            return;
        }

        var save = PersistenceMgr.Instance?.ActiveCharacter;
        if (save != null)
        {
            save.CurrentDungeonId = CurrentDungeon.Id;
        }

        PersistenceMgr.Instance?.MarkDirty();
        PersistenceMgr.Instance?.FlushSave();
        GameManager.Instance?.ChangeScene(ScenePaths.Combat);
    }

    void IPersistedDataParticipant.ApplySaveData(CharacterSaveData data)
    {
        CurrentDungeon = data.CurrentDungeonId > 0
            ? GetDungeon(data.CurrentDungeonId)
            : null;
    }

    void IPersistedDataParticipant.CollectSaveData(CharacterSaveData data)
    {
        data.CurrentDungeonId = CurrentDungeon?.Id ?? 0;
    }

    void IPersistedDataParticipant.ClearPersistedState()
    {
        CurrentDungeon = null;
    }
}
