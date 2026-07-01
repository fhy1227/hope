# Hope 项目架构

> 面向 AI 协作与人类维护的场景树契约。新增功能前请先对照本文。  
> 装备与背包的局外持久化细则见 **[存档方案](存档方案.md)**；装备玩法见 `装备系统-*.md`。

## 场景流程

**目标架构**（刷宝 ARPG）：

```
MainMenu
    ├── 继续游戏 → SaveManager.Load → Hub（主城，持久化数据已注入 Autoload）
    ├── 新游戏   → SaveManager.CreateProfile → Hub
    └── Hub
            ├── 铁匠 / 背包 / 赌博师（UI_Overlay）
            └── 进入关卡 → Main（战斗场景）
                    ├── 加载区域 Level
                    ├── 战斗结束 / 回城 → SaveManager.Save → Hub 或 MainMenu
                    └── 返回主菜单 → SaveManager.Save → MainMenu
```

**当前 Demo**：`MainMenu` 直接进入 `Main`，`RunManager` 驱动波次↔商店循环；`ResetRunState()` 仍会清空背包/装备。**正式刷宝流程落地后应移除该清空逻辑**，改由 `SaveManager` 接管，见 [存档方案 §6 迁移](存档方案.md#6-从-demo-迁移)。

## 主场景树（`scenes/main.tscn`）

战斗场景根节点；主城 `Hub` 为规划中的独立场景（`scenes/hub.tscn`）。

```
Main (Node, Main.cs, ProcessMode=Always)
├── GameWorld (Node2D, ProcessMode=Pausable)     ← 暂停时停止
│   ├── Background
│   ├── Levels/          ← 关卡实例（含 SpawnPoint），不拥有玩家
│   ├── Entities/        ← 玩家、NPC
│   ├── Enemies/
│   ├── Effects/
│   │   └── Projectiles/
│   ├── Pickups/         ← 未拾取掉落；离关后不存档
│   └── RunManager/      ← 【Demo】波次循环；正式版可改为 AreaDirector / 关卡驱动
├── UI_Hud       (CanvasLayer layer=1)
├── UI_Overlay   (CanvasLayer layer=2)  ← 背包、商店、铁匠
├── UI_Pause     (CanvasLayer layer=3)  ← 暂停菜单
├── UI_Transition(CanvasLayer layer=4)  ← 转场占位
└── UI_Debug     (CanvasLayer layer=10) ← 调试覆盖层占位
```

层级常量见 `scripts/core/UiLayers.cs`。

## 节点职责

| 节点 | 职责 | 禁止 |
|------|------|------|
| **Main** | 持有世界/UI 引用；进关时 `ResetCombatState()`；离关时触发存档 | 不写移动、攻击等玩法逻辑 |
| **GameWorld** | 世界容器路径契约 | 不直接操控玩家 |
| **RunManager** | 【Demo】波次与商店；实例化玩家到 Entities | 不操作 UI 节点；不持久化装备 |
| **BaseLevel** | 关卡几何 + `SpawnPoint` | 不实例化/拥有玩家 |
| **Player** | 移动、受击、武器 | 不 `ChangeScene` |
| **UI 脚本** | 展示与输入；订阅 EventBus / Manager 信号 | 不直接改敌人列表 |

## 跨模块访问（优先级）

1. **Main.Instance** — 获取 `World`、`Run`、各 UI 层（仅战斗场景有效）
2. **Autoload** — `SaveManager`、`GameManager`、`EventBus`、`ConfigManager`（经 ConfigBootstrap）
3. **EventBus 信号** — 模块间广播，避免实体互相 `GetNode`
4. **禁止** — `GetParent().GetParent()`、随意 `GetFirstNodeInGroup`（遗留 group 仅作兜底）

```csharp
// ✅ 获取背包（任意场景）
var inv = InventoryManager.Instance;

// ✅ 战斗场景获取 RunManager
var run = Main.Instance?.Run;

// ❌ 禁止
var run = GetTree().GetFirstNodeInGroup("run_manager");
```

## Autoload 与数据作用域

| Autoload | 作用域 | 存档 | 说明 |
|----------|--------|:----:|------|
| ConfigBootstrap | 全局 | — | 配置表加载 |
| EventBus | 全局 | — | 信号总线 |
| GameManager | 全局 | — | 流程状态、场景切换 |
| AudioManager | 全局 | 偏好 | 音量等设置（可选） |
| **SaveManager** | 全局 | 读写 | **存档入口**（已实现） |
| **InventoryManager** | **角色** | ✓ | 背包；随角色存档持久化 |
| **EquipManager** | **角色** | ✓ | 装备栏；随角色存档持久化 |
| AspectCodexManager | **角色** | ✓ | 威能法典（规划） |
| CraftingManager | **角色** | ✓ | 强化材料仓库（规划） |

### 重置边界

| 时机 | 行为 |
|------|------|
| 进战斗关 | `Main.ResetCombatState()` — 仅清战斗临时状态（波次计数、未拾取 Pickup 等） |
| 离关 / 回城 / 主菜单 | `SaveManager.Save()` — 写盘背包、装备、金币、法典 |
| 新角色 | `SaveManager.CreateProfile()` — 空背包与默认装备 |
| 删档 | `SaveManager.DeleteProfile()` |

**禁止**在进关时调用 `InventoryManager.Clear()` / `EquipManager.Clear()`（Demo 遗留，待移除）。

## 目录约定

| 路径 | 用途 |
|------|------|
| `scripts/autoload/` | 全局单例（含 `SaveManager`） |
| `scripts/components/` | 可复用节点组件 |
| `scripts/components/numeric/` | 数值系统组件（唯一实现位置） |
| `scripts/config/` | 配置表 C#（xlsx 生成，勿手改） |
| `scripts/core/` | 数据、常量、`Main`、协调入口 |
| `scripts/dropSystem/` | 装备掉落随机（`Hope.DropSystem`） |
| `scripts/entities/` | 实体脚本（Enemy、Pickup、Projectile） |
| `scripts/levels/` | 关卡基类与关卡脚本 |
| `scripts/systems/` | 游戏系统（背包、装备、刷怪） |
| `scripts/ui/` | UI 控件脚本 |
| `scenes/characters/` | 玩家角色场景 |
| `scenes/entities/` | 敌人、弹道、拾取物场景 |
| `scenes/levels/` | 关卡场景 |
| `scenes/hub/` | 主城场景（规划） |
| `scenes/systems/` | 世界等系统场景 |
| `scenes/ui/` | UI 场景 |
| `scenes/weapons/` | 武器槽等 |
| `assets/config/` | 运行时 JSON 配置 |
| `tools/config/` | 配置源 xlsx |
| `docs/` | 架构、装备、存档设计文档 |

## ProcessMode 约定

| 节点 | ProcessMode |
|------|-------------|
| Main | Always |
| GameWorld | Pausable |
| UI_Overlay / UI_Pause / UI_Transition / UI_Debug | Always |
| GameManager / EventBus / SaveManager | Always |
| 需在暂停时交互的 UI 控件 | Always |

## 配置数据流

```
tools/config/*.xlsx → export_config → assets/config/*.json + scripts/config/*Config.cs
运行时：ConfigManager.Get<T>(id)
```

## 新增功能检查清单

- [ ] 脚本是否放在上表对应目录？
- [ ] 场景路径是否加入 `ScenePaths`？
- [ ] UI 是否挂在正确的 `CanvasLayer`（`UiLayers`）？
- [ ] 跨模块是否走 `EventBus` 或 `Main.Instance` / Autoload？
- [ ] 角色级数据是否纳入 `SaveManager` / `CharacterSaveData`？
- [ ] 战斗临时状态是否仅在 `ResetCombatState()` 清理（而非清空背包）？
- [ ] 是否使用 `PackedScene.Instantiate<T>()` 而非 `new`？

## 相关文档

| 文档 | 内容 |
|------|------|
| [存档方案](存档方案.md) | 存盘格式、时机、API、迁移 |
| [装备系统-基础](装备系统-基础.md) | 槽位、品质、词缀、威能 |
| [装备系统-强化](装备系统-强化.md) | 淬炼、精工、附魔 |
| [装备系统-来源](装备系统-来源.md) | 掉落、Boss、赌博 |
