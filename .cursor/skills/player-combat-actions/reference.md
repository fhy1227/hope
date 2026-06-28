# 战斗行为参考手册

## 文件索引

| 文件 | 用途 |
|------|------|
| `scripts/core/PlayerActionId.cs` | 行为枚举 |
| `scripts/components/actions/IPlayerAction.cs` | 行为接口 |
| `scripts/components/actions/PlayerActionContext.cs` | 运行时上下文 |
| `scripts/components/actions/PlayerActionController.cs` | 调度、输入、格挡入口 |
| `scripts/components/actions/RollAction.cs` | 翻滚 |
| `scripts/components/actions/ChargeAction.cs` | 聚气 |
| `scripts/components/actions/StompAction.cs` | 震地 |
| `scripts/components/actions/ParryAction.cs` | 格挡 |
| `scripts/components/actions/CombatPulse.cs` | 范围伤害 + 击退 |
| `scripts/entities/Player.cs` | 移动层、受击、视觉反馈 |
| `scripts/entities/Enemy.cs` | ApplyStun / ApplyKnockback / 接触伤害 |
| `scenes/characters/player.tscn` | PlayerActionController 节点 |
| `tools/config/item.xlsx` | 传奇底材 2001–2036 |

## 输入映射（project.godot）

| Action | 键位 |
|--------|------|
| roll | Space |
| charge | R（按住） |
| stomp | Q |
| parry | 鼠标右键 |

## 行为参数（当前实现常量）

### Roll 翻滚

| 参数 | 值 |
|------|-----|
| Duration | 0.25s |
| Speed | 480 px/s |
| Cooldown | 1s |
| 无敌 | 全程 |
| 穿敌 | 临时去掉 Enemy CollisionMask |
| 方向 | 输入方向 → LastMoveDirection → Down |

### Charge 聚气

| 阶段 | 时长 | 说明 |
|------|------|------|
| WindUp | 0.15s | 锁定移动 |
| Charging | 最长 2s | 移速 ×0.5；按住 R 蓄力 |
| Release | 0.2s | 锁定移动 |
| Cooldown | 2s | |
| 最低释放 | 25% | 低于则取消，无 CD |

**爆发档位**

| Charge % | 半径 | 伤害倍率 | 击退 |
|----------|------|----------|------|
| 25–49% | 60 | ×0.8 | 80 |
| 50–74% | 90 | ×1.2 | 100 |
| 75–100% | 120 | ×2.0 | 140 |

受伤打断：WindUp / Charging 阶段 `OnPlayerHit()` → Cancel。

### Stomp 震地

| 参数 | 值 |
|------|-----|
| WindUp | 0.1s |
| Recovery | 0.05s |
| Radius | 80 |
| Damage | ×0.6 |
| Knockback | 120 |
| Cooldown | 3s |
| 无敌 | 无 |

### Parry 格挡

| 参数 | 值 |
|------|-----|
| Window | 0.25s |
| PerfectWindow | 前 0.08s |
| CounterRadius | 70 |
| CounterDamage | ×1.0 |
| Stun | 0.4s |
| Success CD | 0.8s（完美 = 0） |
| Fail CD | 1.5s |
| 移动 | 窗口内移速 ×0.3 |

## 互斥矩阵

进行中 ↓ / 尝试 → | Roll | Charge | Stomp | Parry |
|-----------------|:----:|:------:|:-----:|:-----:|
| **Roll** | — | ✗ | ✗ | ✗ |
| **Charge** | ✗ | — | ✗ | ✗ |
| **Stomp** | ✗ | ✗ | — | ✗ |
| **Parry** | ✗ | ✗ | ✗ | — |

实现方式：行为进行中 `BlocksOtherActions = true`；`CanStart` 检查 `_phase == Idle && cooldown <= 0`。

**Charge 特例**：Charging 阶段 `BlocksMovement = false`（可慢移），WindUp/Release 锁定。

## Player._PhysicsProcess 时序

```
1. _invincibilityTimer -= delta
2. SetLastMoveDirection(input)
3. UpdateActions(delta)          ← 行为 Update；可能 MoveAndSlide（翻滚）
4. if BlocksMovement:
     if !GrantsInvincibility → Velocity = 0
     return                     ← 跳过移动状态机
5. StateMachine.Update(delta)    ← idle/move，用 GetEffectiveMoveSpeed()
```

## TakeContactDamage 链

```
Enemy 碰撞 Player
  → player.TakeContactDamage(amount, this)
    → if IsInvincible → return
    → if TryParry(source) → return（免伤 + 反击）
    → OnPlayerHit()（打断聚气）
    → 扣血 + 0.4s 无敌帧 + FlashDamage
```

## EventBus 信号

```csharp
PlayerActionStarted(int actionId)  // (int)PlayerActionId
PlayerActionEnded(int actionId)
```

## 行为词条设计（待代码接线）

### AffixStat 扩展规划

```
Roll:     RollCooldownReduction, RollDistance, RollIFrameBonus
Charge:   ChargeSpeed, ChargeDamage, ChargeRadius, ChargeMovePenaltyReduction
Stomp:    StompCooldownReduction, StompRadius, StompKnockback, StompDamage
Parry:    ParryWindow, ParryCounterDamage, ParryStunDuration, ParryCooldownReduction
```

### 传奇底材 ID 映射（item.xlsx）

| ID 段 | 行为 | 槽位 |
|-------|------|------|
| 2001–2006 | 翻滚 | 靴子 (4) |
| 2011–2016 | 聚气 | 饰品 (5) |
| 2021–2026 | 震地 | 护甲 (3) |
| 2031–2036 | 格挡 | 头盔 (2) + 武器 (1) |

| ID | affix_id | 中文 |
|----|----------|------|
| 2001 | roll_haste | 轻身 |
| 2002 | roll_distance | 远滚 |
| 2003 | roll_phantom | 幻影 |
| 2004 | roll_momentum | 滚势 |
| 2005 | roll_afterimage | 残影 |
| 2006 | shadow_step | 影步 |
| 2011 | charge_focus | 凝气 |
| 2012 | charge_power | 蓄爆 |
| 2013 | charge_wave | 气浪 |
| 2014 | charge_flow | 行气 |
| 2015 | charge_unbroken | 不破 |
| 2016 | primal_surge | 原初 surge |
| 2021 | stomp_haste | 踏频 |
| 2022 | stomp_crater | 裂地 |
| 2023 | stomp_force | 重踏 |
| 2024 | stomp_sunder | 碎甲 |
| 2025 | stomp_quake | 余震 |
| 2026 | titan_stomp | 泰坦踏 |
| 2031 | parry_patience | 从容 |
| 2032 | parry_riposte | 还施 |
| 2033 | parry_lock | 锁敌 |
| 2034 | parry_recovery | 收势 |
| 2035 | parry_perfect_master | 宗师格 |
| 2036 | aegis_counter | 神盾反击 |

`name_key` 格式：`item.legend.{affix_id}`。

### 词条平衡封顶（设计）

| 属性 | 软上限 |
|------|--------|
| RollCooldownReduction | 35% |
| RollIFrameBonus | +0.15s |
| ChargeSpeed | 40% |
| ChargeRadius | +80px |
| StompCooldownReduction | 30% |
| StompRadius | +60px |
| ParryWindow | +0.12s |
| ParryCounterDamage | +80% |

叠加公式（设计）：`effective = cap × (1 - Π(1 - value_i / cap))`

### ActionStatBonus 接线计划（未实现）

```
ItemInstance.ComputeActionBonus()
  → EquipManager.CurrentActionBonus
  → PlayerActionController.SetBonus()
  → 各 *Action 读取修正 BaseCooldown / Radius 等
```

特殊机制词条（残影、不破、余震等）需 `AffixSpecialEffect` 枚举，Action 内查询。

## 后续规划行为（仅设计，未实现）

| 行为 | 键位建议 | 定位 |
|------|----------|------|
| Mark 标记 | E | 单点集火 debuff |
| Barrier 护盾 | F | 持续减伤 |
| Frenzy 狂怒 | Tab | 进攻 buff |

添加时遵循 SKILL.md 工作流，并更新本 reference。
