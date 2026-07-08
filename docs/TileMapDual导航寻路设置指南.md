# TileMapDual + Navigation / 可行走区域 设置指南

> 适用于 Godot 4.x (C#) + TileMapDual v5.0.2
> 2026年7月

---

## 核心原理

**TileMapDual 不处理寻路**，它是一个"双网格（Dual Grid）自动地形渲染工具"。但它的设计让导航变得很简单——它的 `DisplayLayer`（实际渲染的子 TileMapLayer）会**自动继承**父节点 `TileMapDual` 的导航和碰撞属性。

所以你需要做的只是在 TileSet 中为每个瓦片配置好**导航多边形**和**碰撞多边形**，然后启用属性即可。

---

## 快速上手（两步）

### 第一步：在 TileSet 中配置导航 / 碰撞

1. 打开你的 TileSet 资源
2. 选择任意一个瓦片
3. 在 **TileData** 面板中：

```
TileData
├─ 地形 (Terrain)           ← TileMapDual 自动管理的
├─ 碰撞 (Collision)         ← 手动配置
│  ├─ 添加多边形 (Add Polygon)
│  └─ 绘制可行走/阻挡区域
├─ 导航 (Navigation)        ← 手动配置
│  ├─ 添加多边形 (Add Polygon)
│  └─ 绘制可行走区域
└─ 其他属性...
```

**关键操作：**

| 操作 | 位置 | 说明 |
|------|------|------|
| 添加碰撞多边形 | TileData → Collision → 添加多边形 | 用矩形/多边形框出碰撞区域 |
| 添加导航多边形 | TileData → Navigation → 添加多边形 | 绘制可行走区域（通常是整个格子的区域） |
| 设置碰撞层 | TileData → Collision → Layer | 例如 layer 1 = 玩家碰撞 |
| 设置导航层 | TileData → Navigation → Layers | 例如 layer 1 = 地面导航 |

### 第二步：在 TileMapDual 节点上启用

选中场景中的 `TileMapDual` 节点，在检视器中：

```
TileMapDual 节点
├─ Collision Enabled        → ☑ 开启
├─ Use Kinematic Bodies     → ☑ 建议开启
├─ Collision Visibility Mode → Debug（调试时可看到碰撞框）
├─ Navigation Enabled       → ☑ 开启
└─ Navigation Visibility Mode → Debug（调试时可看到导航区域）
```

**就是这么简单**——TileMapDual 会自动把这些属性同步到内部的 `DisplayLayer` 上。

> 对应的代码透传（`display_layer.gd` 第 47-53 行）：
```gdscript
# 碰撞
self.collision_enabled = parent.collision_enabled
self.collision_visibility_mode = parent.collision_visibility_mode
# 导航
self.navigation_enabled = parent.navigation_enabled
self.navigation_visibility_mode = parent.navigation_visibility_mode
```

---

## C# 代码控制

### 启用导航 / 碰撞

```csharp
using Godot;

public partial class GameManager : Node
{
    [Export] public TileMapDual TileMap { get; set; }

    public override void _Ready()
    {
        // 启用碰撞
        TileMap.CollisionEnabled = true;

        // 启用导航
        TileMap.NavigationEnabled = true;

        // 设置导航可见性（调试用）
        TileMap.NavigationVisibilityMode = TileMapLayer.VisibilityModeMode.ForceHide;
    }
}
```

### 设置具体瓦片的导航 / 碰撞

导航和碰撞是在 **TileSet 级别的 TileData 中配置**的，而不是在代码中逐格设置。但你可以在代码中通过 `TileData` 修改：

```csharp
using Godot;

public static class TileMapHelper
{
    /// <summary>
    /// 获取瓦片上的导航多边形，用于运行时修改
    /// </summary>
    public static NavigationPolygon GetNavigationPolygon(TileMapLayer layer, Vector2I cell)
    {
        var data = layer.GetCellTileData(cell);
        if (data == null) return null;
        return data.GetNavigationPolygon(0); // layer 0
    }

    /// <summary>
    /// 获取瓦片上的碰撞多边形
    /// </summary>
    public static CollisionPolygon2D GetCollisionPolygon(TileMapLayer layer, Vector2I cell)
    {
        var data = layer.GetCellTileData(cell);
        if (data == null) return null;
        return null; // TileData 不直接暴露 CollisionPolygon2D
    }
}
```

---

## NavigationAgent2D 寻路

配置好 Tileset 导航多边形 + 启用 `NavigationEnabled` 后，导航区域自动生成。然后使用 Godot 内置的寻路系统：

### 场景结构

```
World (Node2D)
├─ TileMapDual              ← 配置了导航和碰撞的瓦片地图
├─ NavigationRegion2D       ← 可选，通常 TileMapLayer 自带导航
├─ Player (CharacterBody2D)
│  ├─ Sprite2D
│  ├─ CollisionShape2D
│  └─ NavigationAgent2D     ← 寻路代理
└─ Enemy (CharacterBody2D)
   ├─ Sprite2D
   ├─ CollisionShape2D
   └─ NavigationAgent2D
```

> **注意**：`TileMapDual` 继承自 `TileMapLayer`，启用 `NavigationEnabled` 后它本身就是一个导航区域，**不需要额外添加 NavigationRegion2D**。

### C# 寻路示例

```csharp
using Godot;

public partial class Player : CharacterBody2D
{
    [Export] public NavigationAgent2D NavAgent { get; set; }
    [Export] public float Speed = 200.0f;

    private Vector2 _targetPosition;

    public override void _Ready()
    {
        // 连接寻路完成信号
        NavAgent.TargetReached += OnTargetReached;
        NavAgent.VelocityComputed += OnVelocityComputed;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        // 鼠标右键点击目标位置
        if (@event is InputEventMouseButton mouseBtn && mouseBtn.ButtonIndex == MouseButton.Right && mouseBtn.Pressed)
        {
            _targetPosition = mouseBtn.GlobalPosition;
            NavAgent.TargetPosition = _targetPosition;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (NavAgent.IsNavigationFinished())
            return;

        // 获取下一段路径方向
        var nextPos = NavAgent.GetNextPathPosition();
        var direction = (nextPos - GlobalPosition).Normalized();

        // 移动
        Velocity = direction * Speed;
        MoveAndSlide();
    }

    private void OnTargetReached()
    {
        GD.Print("到达目标位置");
        Velocity = Vector2.Zero;
    }

    private void OnVelocityComputed(Vector2 safeVelocity)
    {
        Velocity = safeVelocity;
        MoveAndSlide();
    }
}
```

### 避障模式

NavigationAgent2D 支持多种避障模式，在检视器中设置：

```
NavigationAgent2D
├─ Pathfinding Algorithm   → AStar（默认）
├─ Path Post-Processing    → 裁边/曲线优化
├─ Simplification          → 路径简化
└─ Avoidance Enabled      → ☑ 启用避障
```

---

## 动态修改瓦片的可行走状态

### 方案一：运行时切换地形

使用 `TileMapDual.draw_cell()` 切换地形类型，地形变化后导航区域会自动更新：

```csharp
// 在 (5, 3) 位置放置地形 0（通常是空白/不可行走）
tileMap.DrawCell(new Vector2I(5, 3), 0);

// 在 (5, 3) 位置放置地形 1（可行走）
tileMap.DrawCell(new Vector2I(5, 3), 1);
```

### 方案二：锁定 TileMapLayer 的导航（批量操作）

```csharp
// 强制 TileMapDual 的显示层更新导航
tileMap.NavigationEnabled = true;
tileMap.ForceUpdate(); // TileMapDual 没有 ForceUpdate，这里触发 changed 信号
```

更彻底的方案是操作 TileData：

```csharp
/// <summary>
/// 设置某个格子的导航多边形（启用或禁用导航）
/// </summary>
public static void SetCellNavigable(TileMapLayer layer, Vector2I cell, bool navigable)
{
    var data = layer.GetCellTileData(cell);
    if (data == null) return;

    if (navigable)
    {
        // 添加一个覆盖整个格子的导航多边形
        var navPoly = new NavigationPolygon();
        navPoly.AddOutline(new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(16, 0),
            new Vector2(16, 16),
            new Vector2(0, 16),
        });
        navPoly.MakePolygonsFromOutlines();
        data.SetNavigationPolygon(0, navPoly); // layer 0
    }
    else
    {
        // 清除导航多边形=不可行走
        data.SetNavigationPolygon(0, null);
    }
}
```

---

## 完整 TileSet 配置示例

以下是一个典型的地面瓦片的 TileData 配置：

```
TileData (草地瓦片)
├─ Terrain Set: 0
├─ Terrain: 1 (草地)
├─ Collision:
│  ├─ Layer 0: 物理层 1 (地面)
│  └─ Polygon: 覆盖整个 16x16 格子
├─ Navigation:
│  ├─ Layer 0: 导航层 1 (地面导航)
│  └─ Polygon: 覆盖整个 16x16 格子
└─ Custom Data (可选):
   └─ speed_modifier: 1.0 (行走速度倍率)
```

阻挡瓦片（如墙壁）的配置：

```
TileData (墙壁瓦片)
├─ Terrain Set: 0
├─ Terrain: 2 (墙壁)
├─ Collision:
│  ├─ Layer 0: 物理层 1
│  └─ Polygon: 覆盖整个格子
├─ Navigation:
│  └─ Polygon: (不添加 = 不可行走)
└─ Custom Data:
   └─ is_wall: true
```

---

## 调试与可视化

### 调试模式下查看导航/碰撞

```csharp
// 在 TileMapDual 节点上
tileMap.CollisionVisibilityMode = TileMapLayer.VisibilityModeMode.ForceShow;
tileMap.NavigationVisibilityMode = TileMapLayer.VisibilityModeMode.ForceShow;
```

或者在 Godot 编辑器中：

```
调试 (Debug)
├─ 可见的碰撞 (Visible Collision Shapes)     → ☑
├─ 可见的导航 (Visible Navigation)             → ☑
└─ ...

场景运行后，碰撞框和导航区域会以半透明覆盖层显示。
```

### 显示寻路路径

```csharp
// 在场景中绘制寻路路径用于调试
public override void _Draw()
{
    var path = NavAgent.GetCurrentNavigationPath();
    if (path.Length < 2) return;

    for (int i = 0; i < path.Length - 1; i++)
    {
        DrawLine(path[i], path[i + 1], Colors.Yellow, 2.0f);
        DrawCircle(path[i], 3.0f, Colors.Red);
    }
    DrawCircle(path[^1], 5.0f, Colors.Green);
}
```

---

## 常见问题

### Q: 启用了 Navigation Enabled 但看不到导航区域

确保：
1. TileSet 中每个瓦片的 TileData 都配置了**导航多边形**
2. 导航层位掩码设置正确（默认是 layer 1）
3. `Navigation Visibility Mode` 设置为 `Force Show` 或 `Force Hide If Visible`
4. 在编辑器菜单 **Debug → Visible Navigation** 已勾选

### Q: 瓦片行走区域和碰撞区域不匹配

这是因为碰撞多边形和导航多边形是分开配置的。在 TileData 中要**分别设置**：
- 碰撞多边形 = 实体的物理阻挡
- 导航多边形 = 寻路算法的可行走区域

对于大多数情况，两者应该覆盖相同的区域。

### Q: 修改地形后导航没有自动更新

TileMapDual 更新显示层后，Godot 的 TileMapLayer **不会自动重建导航区域**。可以尝试强制刷新：

```csharp
// 重新启用导航来触发导航区域重建
tileMap.NavigationEnabled = false;
await ToSignal(GetTree(), "process_frame");
tileMap.NavigationEnabled = true;
```

或者在 TileSet 瓦片的 TileData 中配置导航时，确保所有地形变体都有导航多边形。

### Q: 多个显示层（Dual Grid）的导航重复

Dual Grid 系统有 1-2 个 `DisplayLayer`。碰撞和导航属性会复制到所有显示层上，所以导航/碰撞区域可能会出现**重叠**。这是 Dual Grid 的正常现象——显示层重叠的区域碰撞和导航也会重叠。

对于碰撞，Godot 的 TileMapLayer 会自动处理重叠。对于导航，建议在 TileSet 中只为**实际显示的瓦片**设置导航多边形，或者接受导航区域略微重叠。

---

> **参考**：
> - TileMapDual 文档：`addons/TileMapDual/docs/`（注意：不存在，但插件自带示例）
> - Godot 官方文档：[Using TileMap Navigation](https://docs.godotengine.org/en/stable/tutorials/navigation/navigation_using_navigationpolygons.html)
> - Godot 官方文档：[NavigationAgent2D](https://docs.godotengine.org/en/stable/classes/class_navigationagent2d.html)
