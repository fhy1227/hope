# Hope 项目架构

> 面向 AI 协作与人类维护的**场景树与模块边界契约**。新增功能前请先对照本文。  
> 装备与背包持久化见 **[存档方案](存档方案.md)**；技能实现现状见 **[技能系统-实现总结](技能系统-实现总结.md)**。

---

## 目录

1. [顶层分类](#1-顶层分类)
2. [场景流程](#2-场景流程)
3. [状态枚举](#3-状态枚举)
4. [战斗对局场景树](#4-战斗对局场景树)
5. [世界与玩家结构](#5-世界与玩家结构)
6. [节点职责](#6-节点职责)
7. [跨模块访问](#7-跨模块访问)
8. [Autoload 与数据作用域](#8-autoload-与数据作用域)
9. [目录约定](#9-目录约定)
10. [子系统速查](#10-子系统速查)
11. [战斗 UI 层](#11-战斗-ui-层)
12. [配置数据流](#12-配置数据流)
13. [ProcessMode 约定](#13-processmode-约定)
14. [新增功能检查清单](#14-新增功能检查清单)
15. [相关文档](#15-相关文档)

---

## 1. 顶层分类

项目资源按职责分为三层，**目录即边界**：

| 层 | 场景 `scenes/` | 脚本 `scripts/` | 职责 |
|----|----------------|-----------------|------|
| **gameplay** | `gameplay/` | `gameplay/` | 战斗对局：关卡、实体、波次、战斗 UI、技能施放 |
| **meta** | `meta/` | `meta/` | 局外流程：主菜单、Hub、结算、角色级 Manager |
| **framework** | — | `framework/` | 基础设施：Autoload、配置、存档、Steam、路径常量 |

```
project.godot  run/main_scene
    └── scenes/meta/main_menu.tscn          ← 游戏入口（meta）
            ├── 继续 / 新游戏 → scenes/meta/hub.tscn
            │       └── 选副本 → scenes/gameplay/combat.tscn
            │               └── 通关/死亡 → scenes/meta/settlement.tscn → Hub
            └── 设置 / 退出
```

**命名空间**（与目录大致对应，允许不完全一致）：

| 命名空间 | 典型位置 |
|----------|----------|
| `Hope.Core` | `framework/core`、部分 gameplay 根类型 |
| `Hope` | `framework/autoload` |
| `Hope.Systems` | `meta/systems`、部分 gameplay 系统 |
| `Hope.SkillSystem` | `gameplay/skill` |
| `Hope.UI` | `gameplay/ui`、`meta/ui` |
| `Hope.Entities` | `gameplay/entities` |
| `Hope.Components.*` | `gameplay/components` |
| `Hope.Persistence` | `framework/persistence` |
| `Hope.Config` | `framework/config`（生成文件勿手改） |

---

## 2. 场景流程

### 2.1 当前已实现流程

```
MainMenu (meta)
    ├── 继续游戏 → PersistenceMgr.Load → Hub
    ├── 新游戏   → PersistenceMgr.CreateProfile → Hub
    └── 退出     → FlushSave → Quit

Hub (meta)
    ├── 展示等级 / 经验 / 金币
    ├── 选副本   → DungeonManager.SelectDungeon → EnterDungeon → Combat
    └── 返回主菜单 → FlushSave → MainMenu

Combat (gameplay)
    ├── RunManager 驱动波次循环
    ├── 副本模式：有限波次 + Boss → Victory → Settlement
    ├── 死亡     → GameOver（局内）→ Settlement
    └── 离场景   → Combat._ExitTree → FlushSave

Settlement (meta)
    ├── 结算经验 / 金币 / 通关记录
    └── 回 Hub 或 重试副本
```

### 2.2 单局内波次循环（`RunManager`）

```
Combat ──波次结束──► FateCard（命运织机三选一）
                          │
                          ▼
                      Shop（波间商店）
                          │
                          ▼
                      Combat（下一波）…
```

副本模式下波次数量有限；通关或死亡后写入 `RunSessionData` 并跳转结算场景。

### 2.3 场景路径常量

全部集中在 `scripts/framework/core/ScenePaths.cs`，切换场景或 `Instantiate` 时**禁止**硬编码路径字符串。

---

## 3. 状态枚举

三套状态各司其职，勿混用：

| 枚举 | 位置 | 维护者 | 含义 |
|------|------|--------|------|
| `GameState` | `framework/core/GameState.cs` | `GameManager` | 应用级：Menu / Hub / Combat / Settlement |
| `RunPhase` | `gameplay/types/RunPhase.cs` | `RunManager` | 单局阶段：Combat / FateCard / Shop / GameOver / Victory |
| `CombatState` | `gameplay/types/CombatState.cs` | `RunManager` | 战斗进行中 / 暂停 / 结束 |

广播：`EventBus.GameStateChanged`、`EventBus.RunPhaseChanged`、`EventBus.CombatStateChanged`。

---

## 4. 战斗对局场景树

`scenes/gameplay/combat.tscn`（`Combat.cs`，`ProcessMode=Always`）：

```
Combat
├── GameWorld                    ← game_world.tscn，ProcessMode=Pausable
├── SkillSystems                 ← 战斗临时，不存档
│   ├── CooldownManager
│   ├── FuryResourceSystem
│   └── SkillCastingSystem
├── UI_Hud          (layer=1)    ← GameHud + SkillBar
├── UI_Overlay      (layer=2)    ← Shop / Inventory / FateCard / SkillTree
├── UI_Pause        (layer=3)    ← PauseMenu / DeathDialog
├── UI_Transition   (layer=4)
└── UI_Debug        (layer=10)
```

层级常量：`scripts/gameplay/types/UiLayers.cs`（`Hud=1, Overlay=2, Pause=3, Transition=4, Debug=10`）。

### GameWorld 子树（`game_world.tscn`）

```
GameWorld (GameWorld.cs)
├── LevelManager
├── Levels/              ← 关卡实例（含 SpawnPoint）
├── Entities/            ← RunManager 生成 Player
├── Enemies/
├── Effects/
│   ├── Projectiles/
│   └── DamageNumbers/   ← DamageNumberSpawner
├── Pickups/
└── RunManager/
    ├── WaveManager
    ├── EnemySpawner
    └── FateCardManager
```

---

## 5. 世界与玩家结构

### 5.1 玩家场景（`characters/player.tscn`）

由 `RunManager.SpawnPlayer()` 实例化到 `Entities`，**关卡不拥有玩家**。

```
Player (CharacterBody2D, Player.cs)
├── Visual (PlayerVisualController)
├── HealthComponent
├── NumericComponent
├── DataModifierComponent      ← 装备词缀加成
├── NumericHealthSyncComponent
├── PlayerStatsComponent
├── PlayerActionController     ← 翻滚/聚气/震地/格挡
├── UnitHealthBar
├── WeaponSlots (PlayerWeaponController → Slot0/Slot1)
└── Camera2D
```

### 5.2 数值管线

```
CharacterSaveData（局外基础属性）
    → RunStats（局内基础，波间可成长）
        → PlayerStatsComponent → NumericComponent
            → DataModifierComponent（装备）
                → NumericHealthSyncComponent → HealthComponent
```

`NumericType` 定义见 `scripts/gameplay/components/numeric/NumericType.cs`。

### 5.3 并行战斗能力

| 系统 | 入口 | 说明 |
|------|------|------|
| 武器自动攻击 | `PlayerWeaponController` / `WeaponSlot` | 双持，同步 `EquipManager` |
| 主动行为 | `PlayerActionController` + `IPlayerAction` | Roll / Charge / Stomp / Parry |
| 技能快捷键 | `SkillCastingSystem` | Z/X/C/V/1/2，消耗怒气，走 `CombatPulse` |
| 命运卡牌 | `FateCardManager` | 波间修改 `RunStats` |

三套主动能力**独立调度**；新增技能若需锁移动/无敌，须与 `IPlayerAction` 或 `SkillCastingSystem` 协调。

---

## 6. 节点职责

| 节点 / 类 | 职责 | 禁止 |
|-----------|------|------|
| **Combat** | 持有 World/UI 引用；`ResetCombatState()` 清战斗临时状态；离关 `FlushSave` | 不写移动、攻击等玩法细节 |
| **GameWorld** | 世界容器路径契约 | 不直接操控玩家 |
| **RunManager** | 波次、商店、命运卡、玩家生成、副本结算跳转 | 不操作 UI 节点；不持久化装备 |
| **BaseLevel** | 关卡几何 + `SpawnPoint` | 不实例化/拥有玩家 |
| **Player** | 移动、受击、武器、行为组件 | 不 `ChangeScene` |
| **SkillManager** | 技能加点、快捷键绑定（局外存档） | 不写伤害公式 |
| **SkillCastingSystem** | 战斗内施放、冷却、怒气、伤害执行 | 不修改加点数据 |
| **UI 脚本** | 展示与输入；订阅 EventBus / Manager | 不直接改敌人列表 |

---

## 7. 跨模块访问

优先级从高到低：

1. **Combat.Instance** — `World`、`Run`、各 UI 层（仅战斗场景有效）
2. **Autoload 单例** — `InventoryManager`、`SkillManager`、`PersistenceMgr` 等
3. **EventBus 信号** — 模块间广播（战斗事件、技能、波次等）
4. **禁止** — `GetParent().GetParent()`、随意 `GetFirstNodeInGroup`（`run_manager` 组除外）

```csharp
// ✅ 任意场景：背包
var inv = InventoryManager.Instance;

// ✅ 战斗场景：波次与玩家
var run = Combat.Instance?.Run;

// ✅ 技能定义
var def = SkillDB.GetDefinition("barb_bash");

// ✅ 角色技能状态（局外持久）
var state = SkillManager.Instance?.State;
```

---

## 8. Autoload 与数据作用域

`project.godot` 注册顺序（**PersistenceMgr 必须在所有 `[PersistedData]` 参与者之后**）：

| Autoload | 层 | 存档 | 说明 |
|----------|-----|:----:|------|
| ConfigBootstrap | framework | — | 启动时加载配置表 |
| EventBus | framework | — | 全局信号总线 |
| GameManager | framework | — | `GameState`、场景切换、Esc 暂停 |
| AudioManager | framework | 偏好 | 音量等 |
| InventoryManager | meta | ✓ | 背包 |
| EquipManager | meta | ✓ | 装备栏 |
| SkillManager | meta | ✓ | 技能树加点、快捷键绑定 |
| SkillDB | gameplay* | — | 技能定义索引（全局只读） |
| PersistenceMgr | framework | 读写 | 存档调度 |
| DungeonManager | meta | ✓ | 副本选择、解锁 |
| SteamManager | framework | — | Steam 桥接 |

\* `SkillDB` 脚本在 `gameplay/skill/`，但作为 Autoload 全局可用；技能**加点数据**在 `SkillManager`（meta）。

### 持久化参与者

标注 `[PersistedData]` 并实现 `IPersistedDataParticipant`：

- `InventoryManager`
- `EquipManager`
- `SkillManager`
- `DungeonManager`

数据写入 `CharacterSaveData`（`user://saves/slot_N/character`）。DTO 在 `scripts/framework/persistence/`。

### 重置边界

| 时机 | 行为 |
|------|------|
| 进入 `combat.tscn` | `Combat.ResetCombatState()` — 冷却、怒气等战斗临时状态 |
| 离关 / Hub / 主菜单 | `PersistenceMgr.FlushSave()` |
| 新角色 | `PersistenceMgr.CreateProfile()` → 各参与者 `ClearPersistedState()` |

**禁止**在进战斗时调用 `InventoryManager.Clear()` / `EquipManager.Clear()` / `SkillManager` 清空加点。

| 数据 | 作用域 | 存档 |
|------|--------|:----:|
| 背包、装备、技能树、副本进度 | 角色级 meta | ✓ |
| RunStats、波次、地面 Pickup、技能冷却/怒气 | 战斗 gameplay | ✗ |

---

## 9. 目录约定

### 9.1 `scenes/`

| 路径 | 层 | 用途 |
|------|-----|------|
| `gameplay/combat.tscn` | gameplay | 战斗根场景 |
| `gameplay/game_world.tscn` | gameplay | 世界子场景 |
| `gameplay/characters/` | gameplay | 玩家 |
| `gameplay/entities/` | gameplay | 敌人、弹道、拾取物、伤害数字 |
| `gameplay/levels/` | gameplay | 关卡（如 `arena.tscn`） |
| `gameplay/effects/` | gameplay | 战斗特效 |
| `gameplay/weapons/` | gameplay | 武器槽场景 |
| `gameplay/*_ui*.tscn`、`skill_*.tscn` 等 | gameplay | 战斗 HUD / 叠加 UI |
| `meta/main_menu.tscn` | meta | 入口 |
| `meta/hub.tscn` | meta | 主城 |
| `meta/settlement.tscn` | meta | 副本结算 |
| `meta/settings_panel.tscn` | meta | 设置 |

### 9.2 `scripts/`

| 路径 | 层 | 用途 |
|------|-----|------|
| `gameplay/Combat.cs` | gameplay | 战斗根协调者 |
| `gameplay/RunManager.cs` | gameplay | 单局流程主控 |
| `gameplay/types/` | gameplay | `RunPhase`、`RunStats`、`UiLayers`、`WeaponData` 等 |
| `gameplay/world/` | gameplay | `GameWorld`、`WeaponSlot`、`PlayerVisualController` 等 |
| `gameplay/systems/` | gameplay | 波次、刷怪、关卡、命运卡、伤害飘字 |
| `gameplay/skill/` | gameplay | 技能定义、施放、冷却、怒气 |
| `gameplay/components/` | gameplay | 数值组件、战斗行为、AOE 工具 |
| `gameplay/dropSystem/` | gameplay | 装备掉落随机 |
| `gameplay/entities/` | gameplay | Player、Enemy、Projectile、Pickup |
| `gameplay/levels/` | gameplay | `BaseLevel` |
| `gameplay/ui/` | gameplay | 战斗 UI（含 `ui/skill/`） |
| `meta/systems/` | meta | `InventoryManager`、`EquipManager`、`SkillManager`、`DungeonManager`、`ExpSystem` |
| `meta/ui/` | meta | `MainMenu`、`HubUI`、`SettlementUI`、`SettingsPanel` |
| `framework/autoload/` | framework | `EventBus`、`GameManager`、`ConfigBootstrap` |
| `framework/config/` | framework | xlsx 生成的 `*Config.cs` |
| `framework/core/` | framework | `ScenePaths`、`GameState`、`ItemInstance`、`NumericModifierMap` |
| `framework/persistence/` | framework | 存档读写、`CharacterSaveData` |
| `framework/steam/` | framework | Steam SDK |

### 9.3 其他资源

| 路径 | 用途 |
|------|------|
| `tools/config/*.xlsx` | 配置表源文件 |
| `assets/config/*.json` | 导出后的配置 JSON |
| `data/skills/` | 可选 `SkillDefinition` `.tres`（当前由 `SkillCatalog.cs` 代码注册） |
| `assets/ui/theme/` | 统一 UI 主题 |

---

## 10. 子系统速查

### 10.1 装备与背包

- Manager：`InventoryManager`、`EquipManager`（meta Autoload）
- 战斗内 UI：`InventoryUI`（`UI_Overlay`，`I` 键）
- 掉落：`dropSystem/EquipDropGenerator` → 地面 `Pickup`
- 文档：[装备系统-基础](装备系统-基础.md)、[装备系统-来源](装备系统-来源.md)、[装备系统-强化](装备系统-强化.md)

### 10.2 技能系统（阶段一）

- 局外：`SkillManager` + `SkillTreePanel`（`K` 键）
- 战斗：`SkillCastingSystem` + `SkillBar`（`Z/X/C/V/1/2`）
- 数据：`SkillDB` + `SkillCatalog`（野蛮人 21 技能）
- 文档：[技能系统-实现总结](技能系统-实现总结.md)、设计规格 [暗黑4技能系统设计方案](暗黑4技能系统设计方案.md)

### 10.3 命运织机

- `FateCardManager` + `FateCardPanel`（`UI_Overlay`）
- 配置：`fate_card.xlsx`、`fate_chain.xlsx`
- 文档：[玩法设计-命运织机](玩法设计-命运织机.md)

### 10.4 副本与成长

- `DungeonManager`：选本、解锁、进战斗
- `ExpSystem`：等级与经验曲线（`exp_level.xlsx`）
- `SettlementUI`：局末结算写回存档
- 文档：[成长刷装副本流实现方案](成长刷装副本流实现方案.md)

### 10.5 玩家战斗行为

- `PlayerActionController`：翻滚 `Space`、聚气 `R`、震地 `Q`、格挡 `鼠标右键`
- 扩展契约：`.cursor/skills/player-combat-actions/`
- 范围伤害复用：`CombatPulse.HitCount`

---

## 11. 战斗 UI 层

| CanvasLayer | 场景 / 脚本 | 输入 | 说明 |
|-------------|-------------|------|------|
| UI_Hud | `game_hud.tscn` | — | 生命、波次、金币、属性面板、技能栏 |
| UI_Overlay | `shop_panel` | — | 波间商店 |
| UI_Overlay | `inventory_ui` | `I` | 背包 + 装备 |
| UI_Overlay | `fate_card_panel` | — | 命运织机 |
| UI_Overlay | `skill_tree_panel` | `K` | 技能树（打开时暂停） |
| UI_Pause | `pause_menu` | `Esc` | 暂停菜单 |
| UI_Pause | `death_dialog` | — | 死亡确认 |

新战斗 UI **必须**挂到上表对应层，勿在 `combat.tscn` 随意新增无名 `CanvasLayer`。

---

## 12. 配置数据流

```
tools/config/*.xlsx
    → 导出工具
    → assets/config/*.json
    → scripts/framework/config/*Config.cs（自动生成，勿手改）

运行时：ConfigManager.Get<T>(id) / GetAll<T>()
全局平衡常量：params.xlsx → ParamsConfig.cs
```

当前配置表：`item`、`affix`、`quality`、`equip_slot`、`drop_table`、`dungeon`、`exp_level`、`fate_card`、`fate_chain`、`params`。

---

## 13. ProcessMode 约定

| 节点 / Autoload | ProcessMode |
|-----------------|-------------|
| Combat | Always |
| SkillSystems（冷却/怒气/施放） | Inherit → Always |
| GameWorld | Pausable |
| UI_Overlay / UI_Pause / UI_Transition / UI_Debug | Always（或 Inherit自 Always 父级） |
| GameManager / EventBus / PersistenceMgr / SkillManager | Always |

暂停游戏（`GetTree().Paused = true`）时，仅 `Always` / `WhenPaused` 节点继续 `_Process`；技能树、背包等 overlay 打开时会暂停，此时快捷键施放无效。

---

## 14. 新增功能检查清单

- [ ] 新代码是否放在 `gameplay` / `meta` / `framework` 对应目录？
- [ ] 场景路径是否加入 `ScenePaths`？
- [ ] 战斗 UI 是否挂在 `combat.tscn` 既定 `CanvasLayer`（`UiLayers`）？
- [ ] 跨模块是否走 `EventBus` 或 `Combat.Instance` / Autoload？
- [ ] 角色级数据是否通过 `[PersistedData]` + `PersistenceMgr`？
- [ ] 战斗临时状态是否只在 `Combat.ResetCombatState()` 清理？
- [ ] 实体是否 `PackedScene.Instantiate<T>()`，而非 `new Enemy()` 等？
- [ ] 配置是否改 xlsx 重新导出，而非手改 `*Config.cs`？

---

## 15. 相关文档

| 文档 | 内容 |
|------|------|
| [存档方案](存档方案.md) | 存盘格式、时机、API |
| [技能系统-实现总结](技能系统-实现总结.md) | 当前技能代码架构与操作说明 |
| [装备系统-基础](装备系统-基础.md) | 槽位、品质、词缀 |
| [装备系统-来源](装备系统-来源.md) | 掉落与物品来源 |
| [装备系统-强化](装备系统-强化.md) | 强化玩法 |
| [成长刷装副本流实现方案](成长刷装副本流实现方案.md) | Hub / 副本 / 结算 |
| [玩法设计-命运织机](玩法设计-命运织机.md) | 波间卡牌 |
| [暗黑4技能系统设计方案](暗黑4技能系统设计方案.md) | 技能完整设计规格（目标态） |
