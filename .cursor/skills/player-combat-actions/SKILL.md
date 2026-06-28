---
name: player-combat-actions
description: Hope 玩家战斗行为系统（翻滚/聚气/震地/格挡）架构契约与扩展指南。Use when adding or modifying player combat actions, IPlayerAction, PlayerActionController, roll/charge/stomp/parry, action affixes, or combat input bindings.
---

# 玩家战斗行为系统

> 修改或新增战斗行为前**必读**。行为层与移动层分离，武器仍自动攻击（`WeaponSlot`），行为是主动技巧层。

## 架构概览

```
Player._PhysicsProcess
  ├── PlayerActionController.UpdateActions()   ← 行为调度（优先）
  └── StateMachine (idle/move)                 ← 仅当 !BlocksMovement

PlayerActionController
  ├── RollAction / ChargeAction / StompAction / ParryAction
  └── 查询：BlocksMovement, GrantsInvincibility, MoveSpeedMultiplier
```

| 层 | 职责 | 位置 |
|----|------|------|
| 移动 | idle / move | `Player.cs` 内嵌 `IGameState` |
| 行为 | 翻滚/聚气/震地/格挡 | `scripts/components/actions/` |
| 范围伤害 | AOE + 击退 | `CombatPulse.cs` |
| 输入 | roll/charge/stomp/parry | `project.godot` [input] |

**硬规则**

1. 新行为实现 `IPlayerAction`，在 `PlayerActionController` 注册，**不要**把行为塞进 `StateMachine`
2. 行为占 `PlayerActionController` 节点（`player.tscn`），禁止在 `Main` / `RunManager` 写行为逻辑
3. 范围伤害复用 `CombatPulse.HitCount`，禁止复制敌人遍历
4. 接触伤害必须带 `Enemy? source` 供格挡判定（`Enemy.cs` → `Player.TakeContactDamage`）
5. 新行为 ID 写入 `PlayerActionId`，EventBus 广播 `PlayerActionStarted/Ended`
6. 行为词条底材在 `tools/config/item.xlsx`（ID 2001–2036），机制接线见 [reference.md](reference.md)

## 核心接口

```csharp
public interface IPlayerAction
{
    PlayerActionId Id { get; }
    bool IsActive { get; }
    float CooldownRemaining { get; }
    bool CanStart(PlayerActionContext ctx);
    void Enter / Update / Exit / TickInactive(...);
    bool BlocksMovement { get; }      // true → Player 跳过 StateMachine
    bool BlocksOtherActions { get; }  // true → 进行中不可开新行为
    bool GrantsInvincibility { get; } // true → Player.IsInvincible
    float MoveSpeedMultiplier { get; } // 蓄力/格挡减速时 < 1
}
```

`PlayerActionContext` 提供：`Player`、`Controller`、`InputDirection`、`LastMoveDirection`、`GetRollDirection()`、`GetDamage(multiplier)`。

## 现有行为速查

| 行为 | 输入 | 定位 | CD |
|------|------|------|-----|
| **Roll** 翻滚 | Space | 定向位移 + 全程无敌 + 穿敌 | 1s |
| **Charge** 聚气 | R 按住 | 站桩蓄力 → 范围爆发；可边蓄边慢移 | 释放后 2s |
| **Stomp** 震地 | Q | 瞬发 AOE + 击退 | 3s |
| **Parry** 格挡 | 右键 | 0.25s 窗口免伤反制；完美帧 0.08s | 成功 0.8s / 失败 1.5s |

详细参数、互斥矩阵、聚气档位见 [reference.md](reference.md)。

## 新增行为工作流

```
任务进度:
- [ ] 1. PlayerActionId 追加枚举值
- [ ] 2. 新建 XxxAction.cs 实现 IPlayerAction
- [ ] 3. PlayerActionController 注册实例 + TryStartFromInput 顺序
- [ ] 4. project.godot 添加 input action
- [ ] 5. 更新互斥逻辑（CanStart / BlocksOtherActions）
- [ ] 6. Player 集成点（如需：TakeContactDamage / 新查询属性）
- [ ] 7. EventBus 信号（若 HUD 需要）
- [ ] 8. dotnet build 验证
```

### Step 2: Action 实现模板

```csharp
public sealed class XxxAction : IPlayerAction
{
    private enum Phase { Idle, Active, Cooldown }
    // 常量放类内 private const；后续改读 ActionStatBonus

    public bool CanStart(PlayerActionContext ctx) =>
        _phase == Phase.Idle && _cooldown <= 0f;

    public void Enter(PlayerActionContext ctx)
    {
        ctx.Controller.NotifyActionStarted(Id);
        // 视觉：ctx.Player.SetActionVisual / FlashActionRelease
    }

    public void Update(PlayerActionContext ctx, double delta) { /* 阶段机 */ }

    public void Exit(PlayerActionContext ctx)
    {
        ctx.Player.ResetActionVisual();
        ctx.Controller.NotifyActionEnded(Id);
    }

    public void TickInactive(double delta) { /* Cooldown 倒计时 */ }
}
```

### Step 3: Controller 注册

```csharp
// PlayerActionController.cs
private readonly XxxAction _xxx = new();

// UpdateActions: _xxx.TickInactive(delta);
// TryStartFromInput: 按优先级插入（当前：parry → roll → stomp → charge）
// 按住类输入：在 UpdateActions 的 _active 分支处理 JustReleased
```

### 输入类型

| 类型 | 示例 | 处理位置 |
|------|------|----------|
| 瞬发 | roll, stomp, parry | `TryStartFromInput` + `IsActionJustPressed` |
| 按住 | charge | `Enter` on press；`Update` 检测 hold；`OnInputReleased` on release |
| 被动触发 | parry 判定 | `Player.TakeContactDamage` → `TryParry(source)` |

### Player 集成点

| 场景 | 做法 |
|------|------|
| 无敌 | `GrantsInvincibility` 或 `_invincibilityTimer` |
| 锁定移动 | `BlocksMovement = true`；翻滚自行 `MoveAndSlide` |
| 减速移动 | `BlocksMovement = false` + `MoveSpeedMultiplier < 1` |
| 受伤打断 | `OnPlayerHit()` 中判断 `_active?.Id` |
| 穿敌 | 临时 `CollisionMask &= ~CollisionLayers.Enemy`，Exit 恢复 |
| 反制敌人 | `enemy.ApplyStun()` / `ApplyKnockback()` |

## 修改现有行为

1. **读对应 `*Action.cs`**，确认内部 `Phase` 状态机
2. **改常量**前先查是否已有 `ActionStatBonus` 接线（当前未接线，常量即真值）
3. **互斥变更**需同步 `CanStart` 与 `BlocksOtherActions`
4. **格挡/受击**改动必须跑 `Player.TakeContactDamage` + `Enemy` 接触路径
5. 改输入键位只动 `project.godot`，Action 内用 action 名不硬编码键码

## 与武器/数值边界

| 模块 | 关系 |
|------|------|
| `WeaponSlot` | 自动攻击，**不**改 CD；行为伤害走 `GetActionDamage()` |
| `PlayerStatsComponent` | 行为伤害读 `NumericType.Damage` |
| `EquipStatBonus` | 通用属性；行为词条计划用 `ActionStatBonus`（待实现） |
| `StateMachine` | 仅 idle/move，**禁止**加 roll/charge 等状态 |

## 行为词条（已设计 / 部分配置）

- 24 件传奇底材：`item.xlsx` ID **2001–2036**，`name_key = item.legend.{affix_id}`
- 词条模板、`AffixStat` 扩展、`ActionStatBonus` 聚合：**设计完成，代码未接线**
- 添加词条装备 → 改 `item.xlsx` + 导出；添加随机词条 → 改 `AffixPool.cs`

完整词条表与传奇 ID 映射见 [reference.md](reference.md)。

## 检查清单

- [ ] 新 Action 在 `scripts/components/actions/`，命名 `{Name}Action.cs`
- [ ] `PlayerActionId` 已扩展
- [ ] `player.tscn` 已有 `PlayerActionController`（新行为不需新节点）
- [ ] 未违反 ARCHITECTURE.md（Player 不写 ChangeScene；跨模块走 EventBus）
- [ ] `dotnet build` 通过
- [ ] 互斥与输入优先级已考虑

## 禁止

- ❌ 在 `StateMachine` 添加战斗行为状态
- ❌ `new Enemy()` 或行为内 `GetParent().GetParent()`
- ❌ 复制 `CombatPulse` 敌人遍历逻辑
- ❌ 行为 CD/参数硬编码到 `Player.cs`
- ❌ 跳过 `TakeContactDamage` 直接扣血（破坏格挡/无敌）
