---
name: batch-commit-by-module
description: 按功能模块分批提交 Git，保证每次提交后代码可编译、项目可运行。Use when the user asks to batch commit, split commits by module/feature, incremental commits, or ensure each commit is runnable/buildable.
---

# 按功能模块分批提交

将大量改动拆成多个小提交，**每个提交后 checkout 该版本应能正常编译运行**。

## 硬规则

- **先规划再提交**：列出分批计划，用户确认后再执行（除非用户已明确「直接按模块提交」）。
- **禁止丢弃工作**：拆分前创建可恢复快照；不用 `reset --hard`、`clean -fdx`、强推等破坏性命令（除非用户明确要求）。
- **禁止 `git add .` / `git add -A`**：只暂存本批计划内的文件；`.godot/` 编辑器缓存、`.workbuddy/`、`__pycache__/` 等不入库。
- **每批提交后必须验证**；失败则在本批内修复或调整拆分，**不得带着坏提交继续下一批**。
- **仅在用户明确要求时 push**；默认只本地分批 commit。

## 工作流程

```
任务进度:
- [ ] 1. 盘点改动与依赖
- [ ] 2. 按模块分组并排序
- [ ] 3. 输出分批计划（待确认）
- [ ] 4. 创建快照备份
- [ ] 5. 逐批：暂存 → 验证 → 提交
- [ ] 6. 汇报每批 commit 与剩余未提交内容
```

### Step 1: 盘点改动

并行执行：

```bash
git status
git diff
git diff --cached
git log -5 --oneline
```

结合对话上下文，区分：新功能、增强、修复、配置、场景/UI 资源。

### Step 2: 模块分组原则

同一功能模块的文件放在同一批，典型边界：

| 模块类型 | 通常包含 | 示例 |
|----------|----------|------|
| 配置表 | `tools/config/*.xlsx` + `assets/config/*.json` + `scripts/framework/config/*Config.cs` + `ConfigBootstrap` 注册 | quality、drop_table |
| 核心数据 | `scripts/framework/core/*` | ItemInstance、ScenePaths |
| 系统/管理器 | `scripts/gameplay/systems/*`、`scripts/meta/systems/*` | RunManager、InventoryManager |
| 实体/组件 | `scripts/gameplay/entities/*`、`scripts/gameplay/components/*` | Enemy、Pickup |
| UI | `scripts/gameplay/ui/*` + `scenes/gameplay/combat/*`（战斗）/ `scripts/meta/ui/*` + `scenes/meta/ui/*`（局外） | GameHud、MainMenu |
| 场景集成 | `scenes/gameplay/combat/combat.tscn`、`project.godot` | 挂接战斗系统 |
| 工具/技能 | `.cursor/skills/*`、纯工具脚本 | 与运行时无关的可单独一批 |

**依赖顺序（先提交被依赖方）**：

```
配置表 → 核心类型 → 系统逻辑 → 实体/场景 → UI → 主场景/项目设置集成
```

**强耦合必须同批**：

- C# 脚本与其 `.tscn` 场景（节点路径、Export 字段）
- 新 Config 类与其 JSON 数据、xlsx 源表、`ConfigBootstrap` 注册
- 新 Autoload 与 `project.godot` 中的注册项
- 调用方与被调用方若拆开会编译失败，则合并到同一批或按依赖分两批（先被依赖）

### Step 3: 分批计划模板

向用户展示（可用 Mermaid 表示依赖）：

```markdown
## 分批提交计划

| # | 模块 | 文件 | 验证 |
|---|------|------|------|
| 1 | feat(config): quality 品质配置 | tools/config/quality.xlsx, assets/config/quality.json, scripts/framework/config/QualityConfig.cs | build |
| 2 | feat(core): 物品实例 | scripts/framework/core/ItemInstance.cs | build |
| ... | | | |

预计 N 批；每批通过后 `dotnet build`。
```

每批 commit message 遵循仓库近期风格（`git log`），聚焦「为什么」：

```
feat(inventory): add inventory manager and slot model

Introduce InventoryManager as run-scoped storage before UI wiring.
```

### Step 4: 快照备份

在首次 `git add` 前：

```bash
SHA=$(git stash create "pre-batch-commit")
if [ -n "$SHA" ]; then
  git update-ref "refs/backup/pre-batch-commit-$(date +%s)" "$SHA"
fi
```

PowerShell 等价：

```powershell
$sha = git stash create "pre-batch-commit"
if ($sha) { git update-ref "refs/backup/pre-batch-commit-$(Get-Date -Format 'yyyyMMddHHmmss')" $sha }
```

### Step 5: 逐批执行

对每一批：

1. **暂存**：仅本批文件
   ```bash
   git add path/to/file1 path/to/file2
   ```
2. **验证**（本批全部暂存后、commit 前）：
   ```bash
   dotnet build hope.csproj
   ```
   - 若本批含 xlsx 且 JSON 未更新：先 `python tools/export_config.py`（或 `export_config.bat`），再 build
   - build 失败：修复或回退暂存，不 commit
3. **提交**：
   ```bash
   git commit -m "$(cat <<'EOF'
   commit message

   EOF
   )"
   ```
4. **记录**：`git log -1 --oneline` 写入进度汇报

可选：用户要求时，每批后在 Godot 编辑器中冒烟测试（启动主场景、点关键路径）。`dotnet build` 是**最低必做**门槛。

### Step 6: 汇报

```
已完成:
- abc1234 feat(config): quality 品质配置
- def5678 feat(core): 物品实例

剩余未提交:
- scripts/gameplay/ui/InventoryUI.cs
- ...

备份 ref: refs/backup/pre-batch-commit-...
```

## 验证清单（每批 commit 前）

- [ ] `dotnet build hope.csproj` 退出码 0（警告可接受，错误不可）
- [ ] 本批配置：xlsx / json / Config.cs / Bootstrap 一致
- [ ] 本批场景：`.tscn` 引用的脚本路径、节点名与 C# 一致
- [ ] 本批 Autoload：`project.godot` 与脚本类名匹配
- [ ] 未混入无关文件或编辑器缓存

## 常见问题

**跨模块引用导致中间批无法编译**

→ 将共享最小接口/types 提前一批；或把强耦合调用方与被调用方合并为一批。

**单文件含多个模块的改动**

→ `git add -p` 按 hunk 拆分；无法拆分时归入主要功能批，并在计划中说明。

**大量新功能一次开发完成**

→ 仍按「可独立运行」切片：每批至少是「编译通过 + 不破坏已有玩法」，新功能可未接入主场景直到集成批。

## 附加资源

- 本仓库模块拆分示例，见 [examples.md](examples.md)
