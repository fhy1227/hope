---
name: persist-runtime-data
description: >-
  将 Hope 运行时类型（通常为 Autoload Manager）的部分字段接入局外存档。
  扩展 CharacterSaveData、实现 IPersistedDataParticipant 与 [PersistedData]。
  Use when persisting manager state, adding save/load for new game data,
  CharacterSaveData fields, PersistedDataAttribute, PersistenceMgr participants,
  or 持久化 / 存档 / 读档 / MarkDirty.
---

# 接入运行时数据持久化

将 Autoload Manager 持有的**角色级**数据写入 `user://saves/`，由 `PersistenceMgr` 统一调度。

> 细则见 `docs/存档方案.md`；架构契约见 `docs/ARCHITECTURE.md`。

## 适用 / 不适用

| 适用 | 不适用 |
|------|--------|
| 背包、装备、金币（角色级）、材料、法典 | 敌人、弹道、地面 Pickup |
| 跨场景保留的 Autoload 状态 | 仅 `main.tscn` 内 `RunManager.RunStats` 等局内状态 |
| 删档需清空的数据 | 战斗 Buff、冷却、波次计数 |

局内状态进关重置 → `Main.ResetCombatState()`；角色数据 → 本 skill。

## 硬规则

1. DTO 放 `scripts/persistence/`（`Hope.Persistence`），运行时模型留原命名空间（如 `Hope.Core.ItemInstance`）
2. 参与者必须是 **Autoload**，且有 `public static X Instance`
3. 类上 `[PersistedData]` + 实现 `IPersistedDataParticipant`
4. `project.godot` 中 `PersistenceMgr` 排在所有参与者**之后**
5. 数据变更时 `PersistenceMgr.Instance?.MarkDirty()`；离关/退菜单由现有流程 `FlushSave()`
6. **禁止**在进战斗时 `Clear()` 参与者；`ClearPersistedState()` 仅删档/切换角色
7. 复杂运行时对象 → 独立 `*SaveData` DTO + `FromX` / `ToX`（参考 `ItemSaveData`）

## 工作流程

```
任务进度:
- [ ] 1. 确认数据作用域（角色级 vs 局内）
- [ ] 2. 在 CharacterSaveData 增加字段（或新建 *SaveData）
- [ ] 3. Manager 实现 LoadFromSave / ExportToSave（可选公开方法）
- [ ] 4. 实现 IPersistedDataParticipant（显式接口转发）
- [ ] 5. 标注 [PersistedData]；变更处 MarkDirty
- [ ] 6. dotnet build；手动测 读档→改数据→存档→再读档
```

### Step 1: 扩展存档 DTO

在 `scripts/persistence/CharacterSaveData.cs` 增加 JSON 字段：

```csharp
[JsonPropertyName("gold")]
public int Gold { get; set; }
```

复杂结构新建 `scripts/persistence/XxxSaveData.cs`，提供与运行时互转方法。

### Step 2: Manager 实现参与者

以 `WalletManager`（示例）为例：

```csharp
using Hope.Persistence;

namespace Hope.Systems;

[PersistedData]
public partial class WalletManager : Node, IPersistedDataParticipant
{
    public static WalletManager? Instance { get; private set; }

    private int _gold;

    public override void _EnterTree() => Instance = this;

    public override void _ExitTree()
    {
        if (Instance == this) Instance = null;
    }

    public void AddGold(int amount)
    {
        _gold += amount;
        PersistenceMgr.Instance?.MarkDirty();
    }

    void IPersistedDataParticipant.ApplySaveData(CharacterSaveData data) =>
        _gold = data.Gold;

    void IPersistedDataParticipant.CollectSaveData(CharacterSaveData data) =>
        data.Gold = _gold;

    void IPersistedDataParticipant.ClearPersistedState() => _gold = 0;
}
```

已有 `LoadFromSave` / `ExportToSave` 时，显式接口转发即可（见 `InventoryManager`）。

### Step 3: 注册 Autoload

`project.godot`（参与者在前，`PersistenceMgr` 在后）：

```ini
WalletManager="*res://scripts/systems/WalletManager.cs"
PersistenceMgr="*res://scripts/persistence/PersistenceMgr.cs"
```

### Step 4: 验证

```bash
dotnet build hope.csproj
```

- [ ] `Load` 后参与者状态与 JSON 一致
- [ ] 运行时修改 → `Save` → 重进 → 数据保留
- [ ] `DeleteProfile` 后参与者已清空
- [ ] 进战斗未调用 `ClearPersistedState`

## 架构速查

```
PersistenceMgr.Load()
  → PersistenceSerializer 反序列化 character.json
  → PersistedParticipantRegistry 扫描 [PersistedData] 类型
  → 各参与者 Instance.ApplySaveData(ActiveCharacter)

PersistenceMgr.Save()
  → 各参与者 CollectSaveData(ActiveCharacter)
  → 写 character.json.tmp → rename
```

| 类型 | 路径 | 职责 |
|------|------|------|
| `PersistenceMgr` | `scripts/persistence/PersistenceMgr.cs` | Autoload；读写盘、调度 |
| `PersistedDataAttribute` | `scripts/persistence/PersistedDataAttribute.cs` | 标记参与者类 |
| `IPersistedDataParticipant` | `scripts/persistence/IPersistedDataParticipant.cs` | 读档/存档/清空契约 |
| `CharacterSaveData` | `scripts/persistence/CharacterSaveData.cs` | 存档根 DTO |
| `ItemSaveData` | `scripts/persistence/ItemSaveData.cs` | 物品 DTO 范例 |

## 参考实现

- `scripts/systems/InventoryManager.cs` — `List<ItemSaveData>` ↔ 背包
- `scripts/systems/EquipManager.cs` — `Dictionary<int, List<ItemSaveData>>` ↔ 装备栏

## 常见错误

| 错误 | 处理 |
|------|------|
| 参与者未被收集 | 缺 `[PersistedData]`、未实现接口、或 `Instance` 为 null |
| 存档有字段但读档不生效 | 未实现 `ApplySaveData` 或字段名/类型与 DTO 不一致 |
| 战斗结束数据丢失 | 数据在 `RunStats` 等场景节点，未提升到参与者或合并进 `CharacterSaveData` |
| `CharacterSaveData` 与 `EquipManager` 循环引用 | DTO 中用常量槽位 id，勿直接引用 Manager 类型 |

## schema 升级

修改 `CharacterSaveData` 字段时：

1. 递增 `SaveSchema.CurrentVersion`
2. 在 `PersistenceMgr.Load` 或专用迁移器中按 `SchemaVersion` 分支处理旧档
3. 更新 `docs/存档方案.md` 数据模型小节
