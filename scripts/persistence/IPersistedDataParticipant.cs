using Hope.Persistence;

namespace Hope.Persistence;

/// <summary>
/// 可持久化数据的运行时持有者契约：由 <see cref="PersistenceMgr"/> 在读档/存档时调用。
/// 实现类须同时标注 <see cref="PersistedDataAttribute"/>，并通过静态 <c>Instance</c> 暴露单例。
/// </summary>
public interface IPersistedDataParticipant
{
    /// <summary>将 <paramref name="data"/> 中的相关字段反序列化到运行时状态。</summary>
    void ApplySaveData(CharacterSaveData data);

    /// <summary>将运行时状态写回 <paramref name="data"/> 的对应字段。</summary>
    void CollectSaveData(CharacterSaveData data);

    /// <summary>删档或切换角色时清空本模块的持久化运行时状态。</summary>
    void ClearPersistedState();
}
