---
name: sync-architecture-rule
description: >-
  对照 docs/ARCHITECTURE.md 审计 Hope 项目架构一致性，并同步更新
  .cursor/rules/project-architecture.mdc 硬性契约摘要。
  Use when syncing architecture docs, auditing project structure against ARCHITECTURE.md,
  updating project-architecture.mdc, or 架构契约 / 架构审计 / 规则同步.
---

# 同步架构契约规则

将 `docs/ARCHITECTURE.md`（完整契约）与 `.cursor/rules/project-architecture.mdc`（AI 常驻摘要）保持同步。

> **分工**：`ARCHITECTURE.md` 写细节与场景树；`project-architecture.mdc` 只保留**硬性边界**与速查表，控制在 ~80 行内。

## 工作流程

```
任务进度:
- [ ] 1. 读取 docs/ARCHITECTURE.md 全文
- [ ] 2. 抽样核对代码是否与文档一致
- [ ] 3. 更新 project-architecture.mdc（摘要，不复制全文）
- [ ] 4. 若代码与 ARCHITECTURE.md 漂移，先改代码或先改文档（向用户说明）
- [ ] 5. 汇报差异清单与已同步项
```

## Step 1: 读取权威文档

必读 `docs/ARCHITECTURE.md`。相关专题文档按需查阅：

| 文档 | 何时读 |
|------|--------|
| `docs/存档方案.md` | Autoload 持久化、FlushSave |
| `docs/技能系统-实现总结.md` | 技能树 / 施放边界 |
| `.cursor/skills/player-combat-actions/` | 玩家战斗行为扩展 |

## Step 2: 审计检查点

并行核对以下**可验证**项（文档声称 ↔ 代码实际）：

### 场景与路径

```bash
# ScenePaths 是否覆盖主要场景
rg "public const string" scripts/framework/core/ScenePaths.cs

# 入口与战斗根场景
rg "run/main_scene" project.godot
```

- [ ] `project.godot` → `scenes/meta/main_menu.tscn`
- [ ] 战斗根 → `scenes/gameplay/combat.tscn` + `Combat.cs`
- [ ] 场景切换均经 `ScenePaths`，无散落硬编码 `res://scenes/`

### Autoload 顺序与持久化

```bash
rg -A 20 "\[autoload\]" project.godot
rg "\[PersistedData\]" scripts/
```

- [ ] `PersistenceMgr` 排在所有 `[PersistedData]` 参与者之后
- [ ] 参与者：`InventoryManager`、`EquipManager`、`SkillManager`、`DungeonManager`
- [ ] 进战斗不 `Clear()` 参与者；临时状态仅 `Combat.ResetCombatState()`

### 战斗场景树

打开或 grep `scenes/gameplay/combat.tscn`：

- [ ] 子节点含 `GameWorld`、`SkillSystems`、`UI_Hud/Overlay/Pause/Transition/Debug`
- [ ] `UiLayers` 常量与 `combat.tscn` layer 一致（Hud=1 … Debug=10）
- [ ] `GameWorld` 含 `Entities/`（玩家由 `RunManager.SpawnPlayer` 生成）

### 三层目录

```bash
# 抽样：新文件是否落在正确层
ls scenes/gameplay scenes/meta scripts/gameplay scripts/meta scripts/framework
```

- [ ] `gameplay`：战斗对局、实体、波次、战斗 UI
- [ ] `meta`：主菜单、Hub、结算、角色级 Manager
- [ ] `framework`：Autoload、配置、存档、Steam、`ScenePaths`

### 跨模块访问（违规扫描）

```bash
rg "GetParent\(\)\.GetParent\(\)" scripts/
rg "new (Enemy|Player|Projectile)\(" scripts/ --glob "*.cs"
```

- [ ] 无 `GetParent().GetParent()` 跨模块取引用
- [ ] 实体实例化用 `PackedScene.Instantiate<T>()`

### 配置数据流

- [ ] 配置源 `tools/config/*.xlsx` → `assets/config/*.json` → `scripts/framework/config/*Config.cs`
- [ ] 未手改生成文件 `*Config.cs`

## Step 3: 更新 project-architecture.mdc

**原则**：

1. **保留** frontmatter：`alwaysApply: true`
2. **首段**指向 `docs/ARCHITECTURE.md` 为完整契约
3. **必含区块**（与 ARCHITECTURE.md 章节对应，但压缩）：
   - 三层目录表 + 场景流程一行图
   - 硬性规则编号列表（≤10 条，每条可执行）
   - 战斗 `combat.tscn` 场景树简图
   - Autoload / 持久化边界表
   - 跨模块访问优先级
   - 战斗 UI 层速查（层 + 典型面板 + 快捷键）
   - 新增功能检查清单（checkbox）
4. **不写**：命名空间大全、子系统长篇、配置表枚举、ProcessMode 细节——留在 `ARCHITECTURE.md`
5. 路径用 `` `scenes/...` `` / `` `scripts/...` ``，与文档一致

**硬性规则模板**（按 ARCHITECTURE.md §6–§8、§14 提炼，措辞可微调但语义不变）：

```
1. 入口 main_menu.tscn；战斗根 combat.tscn + Combat.Instance
2. 战斗 UI 挂既定 CanvasLayer（UiLayers）
3. 跨模块：Combat.Instance / Autoload / EventBus；禁止 GetParent().GetParent()
4. 实体 PackedScene.Instantiate<T>()；关卡不拥有玩家
5. 角色级数据 meta/systems + PersistenceMgr；进战斗不清；临时状态 ResetCombatState()
6. 场景路径 ScenePaths；配置改 xlsx
7. 新 Autoload 注意 PersistenceMgr 注册顺序
8. RunManager 不操作 UI；Player 不 ChangeScene
```

## Step 4: 处理漂移

| 情况 | 动作 |
|------|------|
| 代码正确、文档过时 | 更新 `ARCHITECTURE.md`，再同步 mdc |
| 文档正确、代码违规 | 列出违规文件，询问是否修复代码 |
| 仅 mdc 过时 | 只更新 mdc |

**禁止**在未核对代码的情况下，把 ARCHITECTURE.md 全文粘贴进 mdc。

## Step 5: 汇报格式

```markdown
## 架构同步结果

### 已核对（通过 / 需修复）
- ...

### project-architecture.mdc
- 更新项：...

### 建议跟进（可选）
- ARCHITECTURE.md 需修订：...
- 代码违规：...
```

## 与其他规则的关系

| 文件 | 关系 |
|------|------|
| `godot-csharp.mdc` | C# 编码与注释；场景树细节以本规则 + ARCHITECTURE.md 为准 |
| `csharp-feature-docs.mdc` | 新增代码注释规范 |
| `persist-runtime-data` skill | 扩展 `[PersistedData]` 时配合使用 |

修改 `combat.tscn` 结构或新增 Autoload 时：**先读本 skill → 审计 → 再改代码 → 最后同步 mdc**。
