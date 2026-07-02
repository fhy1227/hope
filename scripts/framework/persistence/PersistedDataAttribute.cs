using System;

namespace Hope.Persistence;

/// <summary>
/// 标记需要参与局外存档读写的类型（通常为 Autoload Manager）。
/// <see cref="PersistenceMgr"/> 启动时扫描程序集，收集带此标签且实现
/// <see cref="IPersistedDataParticipant"/> 的类型，在读档/存档时统一调度。
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class PersistedDataAttribute : Attribute
{
}
