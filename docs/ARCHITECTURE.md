# Hope 项目架构

> 面向 AI 协作与人类维护的场景树契约。新增功能前请先对照本文。

## 场景流程

```
MainMenu (主场景入口)
    └── ChangeScene → Main (对局根场景)
            └── 返回主菜单 → MainMenu
```

## 主场景树（`scenes/main.tscn`）

```
Main (Node, Main.cs, ProcessMode=Always)
├── GameWorld (Node2D, ProcessMode=Pausable)     ← 暂停时停止
│   ├── Background
│   ├── Levels/          ← 关卡实例（含 SpawnPoint），不拥有玩家
│   ├── Entities/        ← 玩家、NPC
│   ├── Enemies/
│   ├── Effects/
│   │   └── Projectiles/
│   ├── Pickups/
│   └── RunManager/      ← 单局循环（波次、商店、掉落）
├── UI_Hud       (CanvasLayer layer=1)
├── UI_Overlay   (CanvasLayer layer=2)  ← 商店、背包
├── UI_Pause     (CanvasLayer layer=3)  ← 暂停菜单
├── UI_Transition(CanvasLayer layer=4)  ← 转场占位
└── UI_Debug     (CanvasLayer layer=10) ← 调试覆盖层占位
```

层级常量见 `scripts/core/UiLayers.cs`。

## 节点职责

| 节点 | 职责 | 禁止 |
|------|------|------|
| **Main** | 持有世界/UI 引用；进入对局时 `ResetRunState()` | 不写移动、攻击等玩法逻辑 |
| **GameWorld** | 世界容器路径契约 | 不直接操控玩家 |
| **RunManager** | 波次↔商店循环；实例化玩家到 Entities | 不操作 UI 节点 |
| **BaseLevel** | 关卡几何 + `SpawnPoint` | 不实例化/拥有玩家 |
| **Player** | 移动、受击、武器 | 不 `ChangeScene` |
| **UI 脚本** | 展示与输入；订阅 EventBus / Manager 信号 | 不直接改敌人列表 |

## 跨模块访问（优先级）

1. **Main.Instance** — 获取 `World`、`Run`、各 UI 层
2. **Autoload** — `GameManager`、`EventBus`、`ConfigManager`（经 ConfigBootstrap）
3. **EventBus 信号** — 模块间广播，避免实体互相 `GetNode`
4. **禁止** — `GetParent().GetParent()`、随意 `GetFirstNodeInGroup`（遗留 group 仅作兜底）

```csharp
// ✅ 获取 RunManager
var run = Main.Instance?.Run;

// ✅ 获取弹道容器
var projectiles = Main.Instance?.World.Projectiles;

// ❌ 禁止
var run = GetTree().GetFirstNodeInGroup("run_manager");
```

## Autoload 与局内状态

| Autoload | 作用域 | 重置时机 |
|----------|--------|----------|
| ConfigBootstrap | 全局 | 不重置 |
| EventBus | 全局 | 不重置 |
| GameManager | 全局 | 不重置 |
| AudioManager | 全局 | 不重置 |
| InventoryManager | **单局** | `Main.ResetRunState()` |
| EquipManager | **单局** | `Main.ResetRunState()` |

新对局进入 `Main` 时自动清空背包与装备。若增加局内 Autoload，必须在 `Main.ResetRunState()` 中注册重置。

## 目录约定

| 路径 | 用途 |
|------|------|
| `scripts/autoload/` | 全局单例 |
| `scripts/components/` | 可复用节点组件 |
| `scripts/components/numeric/` | 数值系统组件（唯一实现位置） |
| `scripts/config/` | 配置表 C#（xlsx 生成，勿手改） |
| `scripts/core/` | 数据、常量、`Main`、协调入口 |
| `scripts/dropSystem/` | 装备掉落随机（`Hope.DropSystem`） |
| `scripts/entities/` | 实体脚本（Enemy、Pickup、Projectile） |
| `scripts/levels/` | 关卡基类与关卡脚本 |
| `scripts/systems/` | 游戏系统（波次、刷怪、背包逻辑） |
| `scripts/ui/` | UI 控件脚本 |
| `scenes/characters/` | 玩家角色场景 |
| `scenes/entities/` | 敌人、弹道、拾取物场景 |
| `scenes/levels/` | 关卡场景 |
| `scenes/systems/` | 世界等系统场景 |
| `scenes/ui/` | UI 场景 |
| `scenes/weapons/` | 武器槽等 |
| `assets/config/` | 运行时 JSON 配置 |
| `tools/config/` | 配置源 xlsx |

## ProcessMode 约定

| 节点 | ProcessMode |
|------|-------------|
| Main | Always |
| GameWorld | Pausable |
| UI_Overlay / UI_Pause / UI_Transition / UI_Debug | Always |
| GameManager / EventBus | Always |
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
- [ ] 跨模块是否走 `EventBus` 或 `Main.Instance`？
- [ ] 局内状态是否会在 `Main.ResetRunState()` 重置？
- [ ] 是否使用 `PackedScene.Instantiate<T>()` 而非 `new`？
