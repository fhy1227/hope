# Hope 项目架构

> 面向 AI 协作与人类维护的场景树契约。新增功能前请先对照本文。  
> 装备与背包的局外持久化细则见 **[存档方案](存档方案.md)**；装备玩法见 `装备系统-*.md`。

## 顶层分类

项目资源按职责分为三层，**目录即边界**：

| 层 | 目录 | 职责 |
|----|------|------|
| **gameplay** | `scenes/gameplay/`、`scripts/gameplay/` | 战斗对局内玩法：关卡、实体、波次、战斗 UI |
| **meta** | `scenes/meta/`、`scripts/meta/` | 局外流程：主菜单、设置、角色背包/装备（跨场景持久化） |
| **framework** | `scripts/framework/` | 基础设施：Autoload、配置、存档、Steam、路径常量 |

```
project.godot run/main_scene
    └── scenes/meta/main_menu.tscn             ← 游戏入口（meta）
            └── ScenePaths.Combat
                    └── scenes/gameplay/combat.tscn          ← 战斗对局（gameplay）
```

## 场景流程

**目标架构**（刷宝 ARPG）：

```
MainMenu (meta)
    ├── 继续游戏 → SaveManager.Load → Hub（主城，meta）
    ├── 新游戏   → SaveManager.CreateProfile → Hub
    └── Hub (meta)
            ├── 铁匠 / 背包 / 赌博师
            └── 进入关卡 → Combat（gameplay）
                    ├── 加载区域 Level
                    ├── 战斗结束 / 回城 → SaveManager.Save → Hub 或 MainMenu
                    └── 返回主菜单 → SaveManager.Save → MainMenu
```

**当前 Demo**：`MainMenu` 直接进入 `Combat`，`RunManager` 驱动波次↔商店循环。

## 战斗对局场景树（`scenes/gameplay/combat.tscn`）

```
Combat (Node, Combat.cs, ProcessMode=Always)
├── GameWorld (Node2D, ProcessMode=Pausable)
│   ├── Levels/          ← 关卡实例（含 SpawnPoint）
│   ├── Entities/        ← 玩家
│   ├── Enemies/
│   ├── Effects/Projectiles/
│   ├── Pickups/
│   └── RunManager/
├── UI_Hud       (CanvasLayer layer=1)
├── UI_Overlay   (CanvasLayer layer=2)  ← 商店、背包
├── UI_Pause     (CanvasLayer layer=3)
├── UI_Transition(CanvasLayer layer=4)
└── UI_Debug     (CanvasLayer layer=10)
```

层级常量见 `scripts/gameplay/types/UiLayers.cs`。

## 节点职责

| 节点 | 职责 | 禁止 |
|------|------|------|
| **Combat** | 持有世界/UI 引用；进关时 `ResetCombatState()`；离关时触发存档 | 不写移动、攻击等玩法逻辑 |
| **GameWorld** | 世界容器路径契约 | 不直接操控玩家 |
| **RunManager** | 波次与商店；实例化玩家到 Entities | 不操作 UI 节点；不持久化装备 |
| **BaseLevel** | 关卡几何 + `SpawnPoint` | 不实例化/拥有玩家 |
| **Player** | 移动、受击、武器 | 不 `ChangeScene` |
| **UI 脚本** | 展示与输入；订阅 EventBus / Manager 信号 | 不直接改敌人列表 |

## 跨模块访问（优先级）

1. **Combat.Instance** — 获取 `World`、`Run`、各 UI 层（仅战斗对局场景有效）
2. **Autoload** — `GameManager`、`EventBus`、`InventoryManager`、`PersistenceMgr` 等
3. **EventBus 信号** — 模块间广播
4. **禁止** — `GetParent().GetParent()`、随意 `GetFirstNodeInGroup`

```csharp
// ✅ 获取背包（任意场景，meta 层 Autoload）
var inv = InventoryManager.Instance;

// ✅ 战斗场景获取 RunManager
var run = Combat.Instance?.Run;
```

## Autoload 与数据作用域

| Autoload | 层 | 存档 | 说明 |
|----------|-----|:----:|------|
| ConfigBootstrap | framework | — | 配置表加载 |
| EventBus | framework | — | 信号总线 |
| GameManager | framework | — | 流程状态、场景切换 |
| AudioManager | framework | 偏好 | 音量等 |
| PersistenceMgr | framework | 读写 | 存档入口 |
| **InventoryManager** | **meta** | ✓ | 背包 |
| **EquipManager** | **meta** | ✓ | 装备栏 |
| SteamManager | framework | — | Steam 桥接 |

### 重置边界

| 时机 | 行为 |
|------|------|
| 进战斗关 | `Combat.ResetCombatState()` — 仅清战斗临时状态 |
| 离关 / 回城 / 主菜单 | `PersistenceMgr.FlushSave()` |
| 新角色 | `PersistenceMgr.CreateProfile()` |

**禁止**在进关时调用 `InventoryManager.Clear()` / `EquipManager.Clear()`。

## 目录约定

### scenes/

| 路径 | 层 | 用途 |
|------|-----|------|
| `scenes/gameplay/combat.tscn` + `scenes/gameplay/game_world.tscn` | gameplay | 战斗根场景与世界子场景 |
| `scenes/gameplay/characters/` | gameplay | 玩家角色 |
| `scenes/gameplay/entities/` | gameplay | 敌人、弹道、拾取物 |
| `scenes/gameplay/levels/` | gameplay | 关卡场景 |
| `scenes/gameplay/effects/` | gameplay | 战斗特效 |
| `scenes/gameplay/weapons/` | gameplay | 武器槽等 |
| `scenes/meta/` | meta | 主菜单、设置 |
| `scenes/meta/hub/` | meta | 主城（规划） |

### scripts/

| 路径 | 层 | 用途 |
|------|-----|------|
| `scripts/gameplay/Combat.cs` | gameplay | 战斗根场景协调者 |
| `scripts/gameplay/RunManager.cs` | gameplay | 单局流程主控（波次/商店/战斗状态） |
| `scripts/gameplay/types/` | gameplay | 战斗枚举与数据类型（`RunPhase`、`CombatState`、`UiLayers`、`RunStats` 等） |
| `scripts/gameplay/world/` | gameplay | 世界节点/可视组件（`GameWorld`、`WeaponSlot`、`DamageNumber` 等） |
| `scripts/gameplay/systems/` | gameplay | 波次、刷怪、关卡加载、掉落等系统 |
| `scripts/gameplay/components/` | gameplay | 可复用战斗组件 |
| `scripts/gameplay/dropSystem/` | gameplay | 装备掉落随机 |
| `scripts/gameplay/entities/` | gameplay | 实体脚本 |
| `scripts/gameplay/levels/` | gameplay | 关卡基类 |
| `scripts/gameplay/ui/` | gameplay | 战斗 UI 脚本 |
| `scripts/meta/systems/` | meta | InventoryManager、EquipManager |
| `scripts/meta/ui/` | meta | MainMenu、SettingsPanel |
| `scripts/framework/autoload/` | framework | EventBus、GameManager 等 |
| `scripts/framework/config/` | framework | 配置表 C#（xlsx 生成） |
| `scripts/framework/core/` | framework | ScenePaths、GameState、ItemInstance |
| `scripts/framework/persistence/` | framework | 存档读写 |
| `scripts/framework/steam/` | framework | Steam SDK |

## ProcessMode 约定

| 节点 | ProcessMode |
|------|-------------|
| Combat | Always |
| GameWorld | Pausable |
| UI_Overlay / UI_Pause / UI_Transition / UI_Debug | Always |
| GameManager / EventBus / PersistenceMgr | Always |

## 配置数据流

```
tools/config/*.xlsx → export_config → assets/config/*.json + scripts/framework/config/*Config.cs
运行时：ConfigManager.Get<T>(id)
```

## 新增功能检查清单

- [ ] 新代码是否放在 gameplay / meta / framework 对应目录？
- [ ] 场景路径是否加入 `ScenePaths`？
- [ ] 战斗 UI 是否挂在 `combat.tscn` 既定 `CanvasLayer`？
- [ ] 跨模块是否走 `EventBus` 或 `Combat.Instance` / Autoload？
- [ ] 角色级数据是否纳入 `PersistenceMgr`？
- [ ] 是否使用 `PackedScene.Instantiate<T>()` 而非 `new`？

## 相关文档

| 文档 | 内容 |
|------|------|
| [存档方案](存档方案.md) | 存盘格式、时机、API |
| [装备系统-基础](装备系统-基础.md) | 槽位、品质、词缀 |
