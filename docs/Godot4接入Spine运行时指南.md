# Godot 4.6 + C# 接入 Spine 运行时指南

> 基于 spine-runtimes 4.3 / spine-godot 官方文档
> 适用：Godot 4.6.x / 4.7 + C#
> 2026年7月

---

## 目录

1. [概述](#1-概述)
2. [安装方式选择](#2-安装方式选择)
3. [方式一：GDExtension 安装（简单，不支持 C#）](#3-方式一gdextension-安装简单不支持-c)
4. [方式二：自定义引擎模块安装（支持 C#）](#4-方式二自定义引擎模块安装支持-c)
5. [C# 项目设置](#5-c-项目设置)
6. [资源导出与导入](#6-资源导出与导入)
7. [节点类型总览](#7-节点类型总览)
8. [SpineSprite 节点](#8-spinesprite-节点)
9. [SpineBoneNode 节点](#9-spinebonenode-节点)
10. [SpineSlotNode 节点](#10-spineslotnode-节点)
11. [C# 脚本开发](#11-c-脚本开发)
12. [动画控制](#12-动画控制)
13. [信号与回调](#13-信号与回调)
14. [Mix-and-Match 皮肤](#14-mix-and-match-皮肤)
15. [2D 光照](#15-2d-光照)
16. [从磁盘动态加载](#16-从磁盘动态加载)
17. [更新运行时](#17-更新运行时)
18. [常见问题](#18-常见问题)
19. [下载链接汇总](#19-下载链接汇总)

---

## 1. 概述

### 1.1 什么是 spine-godot

spine-godot 是 Esoteric Software 官方提供的 Spine 2D 动画运行时，用于在 Godot 引擎中加载、播放和操作由 Spine 编辑器创建的骨骼动画。

### 1.2 版本信息

| 组件 | 版本 |
|------|------|
| spine-runtimes | 4.3 |
| Godot 支持 | 3.x, 4.x（含 4.6、4.7） |
| 语言支持 | GDScript、C++、C# |
| 您的项目 | Godot 4.6.3 + C# |

### 1.3 两种安装方式对比

| 方式 | 难度 | C# 支持 | AnimationPlayer | 导出主机平台 |
|:----:|:----:|:-------:|:--------------:|:----------:|
| **GDExtension** | ⭐ 简单 | ❌ 不支持 | ❌ 不支持 | 全部平台 |
| **自定义引擎模块** | ⭐⭐⭐ 复杂 | ✅ 支持 | ✅ 支持 | 不支持主机 |

> **⚠️ 重要**：您的项目使用 **C#**，因此**必须**选择"自定义引擎模块"方式。GDExtension 目前不支持 C# 绑定。

---

## 2. 安装方式选择

### 2.1 决策树

```
需要 C# 支持？
├─ 是 → 使用自定义引擎模块（方式二）
│   ├─ 下载预编译 Godot 编辑器 + 导出模板
│   └─ 或自行编译
└─ 否 → 使用 GDExtension（方式一，更简单）
```

### 2.2 推荐方案

由于您的项目是 Godot 4.6.3 + C#：

**最快方案**：下载 Esoteric 官方**预编译的带 C# 支持的 Godot 编辑器**，用其替换现有的 Godot 编辑器。

**更灵活方案**：自行从源码编译 Godot + spine-godot 模块。

---

## 3. 方式一：GDExtension 安装（简单，不支持 C#）

适用于**纯 GDScript** 项目或不需要 C# 绑定的场景。

### 3.1 下载

| Godot 版本 | 下载链接 |
|:----------:|----------|
| 4.6.2 | [spine-godot-extension-4.3-4.6.2-stable.zip](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.6.2-stable/spine-godot-extension-4.3-4.6.2-stable.zip) |
| 4.7 | [spine-godot-extension-4.3-4.7-stable.zip](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.7-stable/spine-godot-extension-4.3-4.7-stable.zip) |

### 3.2 安装步骤

1. 下载对应 Godot 版本的 `.zip` 文件
2. 解压后将 `bin/` 文件夹复制到**项目根目录**
3. 确保目录结构为：
   ```
   项目根目录/
   └─ bin/
      └─ spine_godot_extension.gdextension
      └─ (各平台二进制文件)
   ```
4. 启动 Godot 编辑器即可自动加载

> GDExtension 无需在插件设置中启用，启动即生效。

---

## 4. 方式二：自定义引擎模块安装（支持 C#）

### 4.1 下载预编译版本（推荐）

Esoteric 官方提供了预编译的 Godot 编辑器二进制文件，**直接下载替换现有编辑器即可**。

#### Godot 4.6.2 带 C# 支持

| 平台 | 编辑器下载 | 导出模板下载 |
|:----:|-----------|-------------|
| Windows | [Windows 编辑器 (Mono)](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.6.2-stable/godot-editor-windows-mono.zip) | [导出模板 (Mono)](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.6.2-stable/spine-godot-templates-4.3-4.6.2-stable-mono.tpz) |
| Linux | [Linux 编辑器 (Mono)](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.6.2-stable/godot-editor-linux-mono.zip) | 同上 |
| macOS | [macOS 编辑器 (Mono)](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.6.2-stable/godot-editor-macos-mono.zip) | 同上 |

#### Godot 4.7 带 C# 支持

| 平台 | 编辑器下载 | 导出模板下载 |
|:----:|-----------|-------------|
| Windows | [Windows 编辑器 (Mono)](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.7-stable/godot-editor-windows-mono.zip) | [导出模板 (Mono)](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.7-stable/spine-godot-templates-4.3-4.7-stable-mono.tpz) |
| Linux | [Linux 编辑器 (Mono)](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.7-stable/godot-editor-linux-mono.zip) | 同上 |
| macOS | [macOS 编辑器 (Mono)](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.7-stable/godot-editor-macos-mono.zip) | 同上 |

#### 安装步骤

1. 下载对应操作系统的带 C# 支持的 Godot 编辑器 ZIP
2. 解压到任意目录，运行 `godot.windows.editor.x86_64.exe`（Windows）
3. 下载导出模板 `.tpz` 文件
4. 打开 Godot 编辑器 → **Editor → Manage Export Templates...** → **Install from File** → 选中 `.tpz` 文件
5. 现在你有了支持 Spine 且带 C# 绑定的 Godot 编辑器

### 4.2 自行从源码编译

如果需要自定义编译（如特定 Godot 提交版本），可以自行编译：

#### 前提

按照 [Godot 官方编译指南](https://docs.godotengine.org/en/stable/development/compiling/index.html) 安装编译依赖。

#### 步骤

```bash
# 1. 克隆 spine-runtimes 仓库
git clone https://github.com/esotericsoftware/spine-runtimes.git
cd spine-runtimes/spine-godot

# 2. 运行设置脚本（带 C# 支持）
#   第一个参数：Godot 仓库分支/tag
#   第二个参数：false（非开发版）
#   第三个参数：true（启用 C# 支持）
./build/setup.sh 4.6.3-stable false true

# 3. 编译编辑器（带 C# 支持）
./build/build-v4.sh true

# 4. 编译导出模板
./build/build-templates-v4.sh windows
# 可选：linux, macos, web, android, ios
```

编译完成后：
- 编辑器二进制在 `spine-godot/godot/bin/`
- C# 程序集在 `spine-godot/godot/bin/GodotSharp/`

> Windows 用户使用 **Git Bash** 运行上述命令。

---

## 5. C# 项目设置

使用支持 C# 的 spine-godot Godot 编辑器创建 C# 项目时，需要额外配置 NuGet 包源。

### 5.1 创建 Godot 项目

使用下载的 spine-godot Godot 编辑器（Mono 版）创建新项目，或打开现有项目。

> 如果启动时提示"无法加载 .NET 运行时"，请从 [微软官网](https://dotnet.microsoft.com/zh-cn/download) 安装 .NET SDK 6.0 或更高版本。

### 5.2 创建 godot-nuget 文件夹

1. **关闭 Godot 编辑器**
2. 打开项目文件夹，在根目录下创建 `godot-nuget/` 文件夹

### 5.3 复制 C# 程序集

从 Godot 编辑器 ZIP 文件中找到 C# 程序集：

| 平台 | 路径 |
|:----:|------|
| Windows | `godot-editor-windows-mono.zip\GodotSharp\Tools\` |
| Linux | `godot-editor-linux-mono.zip/GodotSharp/Tools/` |
| macOS | `Godot.app/Contents/Resources/GodotSharp/Tools/`（右键显示包内容） |

复制以下文件到项目的 `godot-nuget/` 目录：

```
GodotSharp.<version>.nupkg
GodotSharp.<version>.snupkg
GodotSharpEditor.<version>.nupkg
GodotSharpEditor.<version>.snupkg
Godot.NET.Sdk.<version>.nupkg
Godot.SourceGenerators.<version>.nupkg
```

> `<version>` 取决于 Godot 版本，例如 `4.1.1`。

### 5.4 创建 nuget.config

在项目根目录创建 `nuget.config` 文件：

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <!-- package source is additive -->
    <add key="godot-nuget" value="./godot-nuget" />
  </packageSources>
</configuration>
```

### 5.5 清空 NuGet 缓存

> **重要**：如果之前使用过官方 Godot C# 编辑器，`$USER_HOME/.nuget` 目录下可能存在旧缓存。在构建前务必清空：
>
> ```
> dotnet nuget locals all --clear
> ```
>
> 否则 NuGet 可能不会使用 `godot-nuget` 目录中带 Spine 绑定的程序集。

### 5.6 完成

现在重新打开 Godot 编辑器，项目已可以使用 Godot + spine-godot 的 C# API。

---

## 6. 资源导出与导入

### 6.1 从 Spine 编辑器导出

在 Spine 编辑器中导出项目，生成以下文件：

| 文件 | 说明 |
|:----:|------|
| `skeleton-name.skel` | **推荐**，skeleton 和动画数据（二进制，更小更快） |
| `skeleton-name.spine-json` | 备选，JSON 格式（若使用 JSON，扩展名必须是 `.spine-json`） |
| `skeleton-name.atlas` | 纹理图集信息 |
| `*.png` | 纹理图集页图片 |

> **注意**：不支持预乘 alpha 导出的 atlas。Godot 默认对非预乘纹理执行出血裁切，足以避免显示瑕疵。

> **建议**：使用二进制 `.skel` 格式而非 JSON，文件更小、加载更快。

### 6.2 导入 Godot

1. 将 `.skel`（或 `.spine-json`）、`.atlas` 和 `.png` 文件拖入 Godot 编辑器文件系统面板
2. 导入器自动创建：
   - `.skel` → `SpineSkeletonFileResource`
   - `.atlas` → `SpineAtlasAssetResource`
   - `.png` → Godot Texture
3. **创建 `SpineSkeletonDataResource`**：
   - 在文件系统面板右键 → **New Resource...**
   - 选择 `SpineSkeletonDataResource`
   - 命名并保存
4. **配置 SkeletonDataResource**：
   - 双击打开，在检视器中：
     - `Skeleton File Res` → 选择导入的 `.skel` 资源
     - `Atlas Res` → 选择导入的 `.atlas` 资源
   - 可在检视器中设置动画的 mix 时间

> **最佳实践**：一个 `SkeletonDataResource` 可以被多个 `SpineSprite` 共享。不要在每个 `SpineSprite` 中内联创建，以免成倍增大加载数据量。

### 6.3 更新资源

直接替换 `.skel`、`.atlas`、`.png` 等源文件，Godot 编辑器自动检测并重导入。如未自动重导入，可在导入设置面板中手动触发。

---

## 7. 节点类型总览

| 节点 | 说明 | 必须作为 SpineSprite 子节点 |
|:----:|------|:------------------------:|
| **SpineSprite** | 显示 Spine skeleton 并播放动画的主节点 | — |
| **SpineBoneNode** | 跟随或驱动 skeleton 中的骨骼 | ✅ 是 |
| **SpineSlotNode** | 在 SpineSprite 绘制顺序中插入任意子节点 | ✅ 是 |
| **SpineAnimationTrack** | 通过 Godot AnimationPlayer 控制 SpineSprite | ✅ 是（不支持 GDExtension） |

---

## 8. SpineSprite 节点

### 8.1 创建

点击场景面板 **+** 按钮 → 搜索 **SpineSprite** → 创建。

在检视器中分配 `SkeletonDataResource`。

### 8.2 检视器属性

| 属性 | 说明 |
|------|------|
| `Skeleton Data Res` | 绑定的 SkeletonDataResource |
| `Update Mode` | `Process`（每帧更新）、`Physics`（固定间隔）、`Manual`（手动触发） |
| `Materials` | 为每个 Spine blend 模式设置自定义材质 |
| `Preview` | 编辑器视口中预览动画 |
| `Debug` | 检视骨骼、槽位、附件信息 |

### 8.3 更新模式

| 模式 | 说明 | 适用场景 |
|:----:|------|----------|
| `Process` | 每帧更新（默认） | 一般游戏逻辑 |
| `Physics` | 固定间隔（默认 60fps） | 需要与 Godot 物理引擎交互 |
| `Manual` | 手动调用 `update_skeleton()` | 完全控制更新时机 |

### 8.4 C# 基本使用

```csharp
using Godot;
using System;

public partial class MySpineCharacter : SpineSprite
{
    private SpineSkeleton _skeleton;
    private SpineAnimationState _animState;

    public override void _Ready()
    {
        _skeleton = GetSkeleton();
        _animState = GetAnimationState();

        // 播放动画
        _animState.SetAnimation("run", true, 0);

        // 翻转骨架
        _skeleton.SetScaleX(-1);
    }
}
```

---

## 9. SpineBoneNode 节点

### 9.1 概述

`SpineBoneNode` 可以**跟随** skeleton 中的某根骨骼，也可以**驱动**其变换。

| 模式 | 说明 | 典型用途 |
|:----:|------|----------|
| **Follow** | 节点位置跟随指定骨骼 | 将 CollisionShape、粒子等附加到骨骼 |
| **Drive** | 控制骨骼的位置/旋转 | 根据鼠标输入驱动骨骼 |

### 9.2 创建与配置

1. 右键 SpineSprite → **Add Child Node...** → 选择 **SpineBoneNode**
2. 在检视器中：
   - `Bone Name`：选择要跟随/驱动的骨骼
   - `Bone Mode`：`Follow` 或 `Drive`
3. 节点名称、变换等可自由设置

> **必须**是 SpineSprite 的直接子节点，否则无法找到骨骼。

### 9.3 C# 示例

```csharp
using Godot;

public partial class MouseAim : SpineBoneNode
{
    public override void _Ready()
    {
        BoneMode = SpineBoneNode.BoneModeEnum.Drive;
    }

    public override void _Process(double delta)
    {
        // 骨骼跟随鼠标位置
        var mousePos = GetGlobalMousePosition();
        GlobalPosition = mousePos;
    }
}
```

---

## 10. SpineSlotNode 节点

### 10.1 概述

`SpineSlotNode` 允许在 `SpineSprite` 的**绘制顺序中**插入任意子节点，用于：

- 在特定槽位上添加粒子系统
- 在槽位上叠加自定义精灵
- 覆盖某个槽位的材质
- 在 Spine 组件之间插入其他 Godot 节点

### 10.2 创建

1. 右键 SpineSprite → **Add Child Node...** → 选择 **SpineSlotNode**
2. 在检视器中设置 `Slot Name`

> **必须**是 SpineSprite 的直接子节点。

### 10.3 自定义材质

在 `SpineSlotNode` 的 `Materials` 属性面板中设置，会覆盖 `SpineSprite` 上的全局材质。

---

## 11. C# 脚本开发

### 11.1 API 命名差异

C# API 使用 PascalCase，与 GDScript 的 snake_case 不同：

| 功能 | GDScript | C# |
|------|----------|:--:|
| 获取动画状态 | `get_animation_state()` | `GetAnimationState()` |
| 设置动画 | `set_animation("run", true, 0)` | `SetAnimation("run", true, 0)` |
| 获取骨架 | `get_skeleton()` | `GetSkeleton()` |
| 设置缩放 | `set_scale_x(-1)` | `SetScaleX(-1)` |

### 11.2 完整的 C# 脚本示例

```csharp
using Godot;
using System;

public partial class PlayerSpine : SpineSprite
{
    private SpineSkeleton _skeleton;
    private SpineAnimationState _animState;

    public override void _Ready()
    {
        // 获取骨架和动画状态引用
        _skeleton = GetSkeleton();
        _animState = GetAnimationState();

        // 初始化动画
        _animState.SetAnimation("idle", true, 0);

        // 翻转方向
        _skeleton.SetScaleX(1);
    }

    // 切换到跑步动画（队列方式）
    public void StartRunning()
    {
        _animState.SetAnimation("run", true, 0);
    }

    // 切换到待机动画（平滑过渡）
    public void StopRunning()
    {
        _animState.SetAnimation("idle", true, 0);
    }

    // 播放一次攻击动画后回到 idle
    public void Attack()
    {
        _animState.SetAnimation("attack", false, 1);  // 轨道 1
        _animState.AddAnimation("idle", 0, true, 1);   // 攻击完后队列 idle
    }

    public override void _Process(double delta)
    {
        // 检测触发事件
        var trackEntry = _animState.GetCurrent(1);
        if (trackEntry != null)
        {
            GD.Print("Attack animation playing on track 1");
        }
    }
}
```

### 11.3 各类型获取方式

```csharp
// 获取动画状态
SpineAnimationState animState = GetAnimationState();

// 获取骨架
SpineSkeleton skeleton = GetSkeleton();

// 获取骨架数据
SpineSkeletonData data = skeleton.GetData();

// 查找皮肤
SpineSkin skin = data.FindSkin("skin-name");

// 获取骨骼
SpineBone bone = skeleton.FindBone("bone-name");

// 获取槽位
SpineSlot slot = skeleton.FindSlot("slot-name");
```

---

## 12. 动画控制

### 12.1 设置与队列动画

```csharp
// 设置动画（立即替换）
_animState.SetAnimation("walk", true, 0);
// 参数：动画名, 是否循环, 轨道编号

// 队列动画（当前动画播放完后切换）
_animState.AddAnimation("walk", 0.5, true, 0);
// 参数：动画名, 延迟秒数, 是否循环, 轨道编号
```

### 12.2 空动画（回到 Setup Pose）

```csharp
// 在当前轨道播放空动画（0.5 秒过渡）
_animState.SetEmptyAnimation(0, 0.5);

// 延迟 0.5 秒后队列空动画
_animState.AddEmptyAnimation(0, 0.5, 0.5);
```

### 12.3 清空动画

```csharp
// 清空单条轨道
_animState.ClearTrack(0);

// 清空所有轨道
_animState.ClearTracks();
```

### 12.4 轨道条目控制

```csharp
var trackEntry = _animState.SetAnimation("walk", true, 0);

// 逆向播放
trackEntry.SetReverse(true);

// 设置播放速度倍率
trackEntry.SetTimeScale(2.0f);

// 设置动画结束事件
// （通过 SpineSprite 的信号处理，见下一节）
```

### 12.5 重置到 Setup Pose

```csharp
// 重置所有骨骼和槽位到 setup pose
GetSkeleton().SetToSetupPose();

// 仅重置槽位
GetSkeleton().SetSlotsToSetupPose();
```

---

## 13. 信号与回调

### 13.1 动画状态信号

在 C# 中连接 SpineSprite 的信号：

```csharp
public override void _Ready()
{
    // 连接动画事件
    AnimationStarted += OnAnimationStarted;
    AnimationCompleted += OnAnimationCompleted;
    AnimationEvent += OnAnimationEvent;
    AnimationInterrupted += OnAnimationInterrupted;
    AnimationEnded += OnAnimationEnded;
}

private void OnAnimationStarted(SpineTrackEntry entry)
{
    GD.Print($"Animation started: {entry.GetAnimation().GetName()}");
}

private void OnAnimationCompleted(SpineTrackEntry entry)
{
    GD.Print($"Animation completed on track: {entry.GetTrackIndex()}");
}

private void OnAnimationEvent(SpineTrackEntry entry, SpineEvent e)
{
    GD.Print($"Event: {e.GetData().GetName()}, Int: {e.GetIntValue()}, Float: {e.GetFloatValue()}");
}

private void OnAnimationInterrupted(SpineTrackEntry entry)
{
    GD.Print("Animation interrupted");
}

private void OnAnimationEnded(SpineTrackEntry entry)
{
    GD.Print("Animation ended");
}
```

### 13.2 完整信号列表

| 信号 | 触发时机 |
|:----:|----------|
| `AnimationStarted` | 动画开始播放 |
| `AnimationInterrupted` | 清空轨道或设置新动画 |
| `AnimationCompleted` | 动画完成一个循环 |
| `AnimationEnded` | 动画不再被应用 |
| `AnimationDisposed` | 轨道条目被销毁 |
| `AnimationEvent` | Spine 编辑器中定义的事件被触发 |

### 13.3 高级生命周期信号

| 信号 | 触发时机 |
|:----:|----------|
| `BeforeAnimationStateUpdate` | 以当前 delta 更新动画状态前 |
| `BeforeAnimationStateApply` | 将动画状态应用到 skeleton 前 |
| `BeforeWorldTransformsChange` | 更新 skeleton 世界变换前 |
| `WorldTransformsChanged` | 更新 skeleton 世界变换后 |

---

## 14. Mix-and-Match 皮肤

### 14.1 C# 实现

```csharp
public void CreateCustomSkin()
{
    var skeleton = GetSkeleton();
    var data = skeleton.GetData();

    // 创建自定义皮肤
    var customSkin = SpineSkin.Create("custom-skin");

    // 组合多个皮肤
    customSkin.AddSkin(data.FindSkin("skin-base"));
    customSkin.AddSkin(data.FindSkin("nose/short"));
    customSkin.AddSkin(data.FindSkin("eyelids/girly"));
    customSkin.AddSkin(data.FindSkin("eyes/violet"));
    customSkin.AddSkin(data.FindSkin("hair/brown"));
    customSkin.AddSkin(data.FindSkin("clothes/hoodie-orange"));

    // 应用皮肤
    skeleton.SetSkin(customSkin);
    skeleton.SetSlotsToSetupPose();
}
```

---

## 15. 2D 光照

### 15.1 法线贴图准备

1. 为每个 atlas 页的 `.png` 准备对应的法线贴图
2. 法线贴图命名格式：`n_原文件名.png`（默认前缀 `n_`）
3. 例如 `raptor.png` → `n_raptor.png`

### 15.2 导入设置

在 atlas 资源的导入设置中，可自定义法线贴图前缀（默认 `n`）。

### 15.3 使用

成功导入法线贴图后，直接添加 Godot 的 2D 光照节点（`PointLight2D` 等）即可生效。

---

## 16. 从磁盘动态加载

需要 Modding 支持时，可以从磁盘加载 Spine 资源：

```csharp
using Godot;

public partial class DynamicLoader : Node
{
    public override void _Ready()
    {
        // 1. 加载 skeleton 文件
        var skeletonFile = new SpineSkeletonFileResource();
        skeletonFile.LoadFromFile("user://mods/character.skel");

        // 2. 加载 atlas 文件
        var atlas = new SpineAtlasResource();
        atlas.LoadFromAtlasFile("user://mods/character.atlas");

        // 3. 创建 SkeletonDataResource
        var skeletonData = new SpineSkeletonDataResource();
        skeletonData.SkeletonFileRes = skeletonFile;
        skeletonData.AtlasRes = atlas;

        // 4. 创建 SpineSprite
        var sprite = new SpineSprite();
        sprite.SkeletonDataRes = skeletonData;
        sprite.Position = new Vector2(200, 200);
        sprite.GetAnimationState().SetAnimation("animation", true, 0);
        AddChild(sprite);
    }
}
```

---

## 17. 更新运行时

### 17.1 GDExtension 版本

下载最新版本，将 `bin/` 文件夹覆盖到项目根目录即可。

### 17.2 引擎模块版本

1. 下载新版预编译 Godot 编辑器 + 导出模板
2. 安装导出模板：**Editor → Manage Export Templates... → Install from File**
3. 如果跨 Spine 主版本，用新版 Spine 编辑器重新导出项目文件
4. 如果使用 C#，将新版编辑器中 `GodotSharp/Tools/` 的程序集复制到项目的 `godot-nuget/` 目录

> 更新前请阅读 [Spine 运行时版本管理指南](https://zh.esotericsoftware.com/spine-runtime-architecture#%E7%89%88%E6%9C%AC%E6%8E%A7%E5%88%B6)。

---

## 18. 常见问题

### 18.1 无法加载 .NET 运行时

启动 Godot 编辑器时提示无法加载 .NET 运行时：
- 从 [微软官网](https://dotnet.microsoft.com/zh-cn/download) 安装 .NET SDK 6.0 或更高版本

### 18.2 NuGet 缓存冲突

如果之前使用过官方 Godot C# 编辑器，NuGet 缓存可能与 spine-godot 的程序集冲突：
```bash
dotnet nuget locals all --clear
```

### 18.3 GDExtension 不支持 C#

GDExtension 版本目前不支持 C# 绑定。如果需要 C#，必须使用自定义引擎模块版本。

### 18.4 预乘 alpha 不支持

spine-godot 不支持使用预乘 alpha 导出的 texture atlas。在 Spine 编辑器中导出时注意不要勾选预乘 alpha。

### 18.5 .json 文件导入失败

必须使用 `.spine-json` 扩展名而非 `.json`。如果现有项目使用 `.json`，可以用 [此 Python 脚本](https://zh.esotericsoftware.com/git/spine-runtimes/spine-godot/convert.py) 批量转换。

### 18.6 SpineSprite 子节点排序不正确

`SpineSprite` 不会自动排序子节点的绘制顺序。需要覆盖绘制顺序时，使用 `SpineSlotNode` 替代直接添加子节点。

### 18.7 编辑器视口不显示 skeleton

确认 `SkeletonDataResource` 已正确配置了 `.skel` 和 `.atlas` 资源。在检视器中双击 `SkeletonDataResource` 检查两个文件是否已分配。

---

## 19. 下载链接汇总

### 19.1 spine-godot 运行时

| 类型 | Godot 4.6.2 | Godot 4.7 |
|:----:|:-----------:|:---------:|
| GDExtension | [下载](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.6.2-stable/spine-godot-extension-4.3-4.6.2-stable.zip) | [下载](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.7-stable/spine-godot-extension-4.3-4.7-stable.zip) |
| 编辑器 (Windows) | [Windows](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.6.2-stable/godot-editor-windows.zip) | [Windows](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.7-stable/godot-editor-windows.zip) |
| 编辑器 (Windows Mono/C#) | [Windows Mono](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.6.2-stable/godot-editor-windows-mono.zip) | [Windows Mono](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.7-stable/godot-editor-windows-mono.zip) |
| 导出模板 | [模板](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.6.2-stable/spine-godot-templates-4.3-4.6.2-stable.tpz) | [模板](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.7-stable/spine-godot-templates-4.3-4.7-stable.tpz) |
| 导出模板 (Mono/C#) | [模板 Mono](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.6.2-stable/spine-godot-templates-4.3-4.6.2-stable-mono.tpz) | [模板 Mono](https://spine-godot.s3.eu-central-1.amazonaws.com/4.3/4.7-stable/spine-godot-templates-4.3-4.7-stable-mono.tpz) |

### 19.2 其他资源

| 资源 | 链接 |
|------|------|
| spine-runtimes 源码 | https://github.com/esotericsoftware/spine-runtimes |
| spine-godot 文档 | https://zh.esotericsoftware.com/spine-godot |
| Spine 运行时许可协议 | https://zh.esotericsoftware.com/spine-runtimes-license |
| Godot 编译文档 | https://docs.godotengine.org/en/stable/development/compiling/index.html |
| .NET SDK 下载 | https://dotnet.microsoft.com/zh-cn/download |

---

> **最后更新**：2026年7月
> 
> **参考来源**：
> - Esoteric Software 官方 spine-godot 文档 (zh.esotericsoftware.com/spine-godot)
> - Esoteric Software 博客：Using C# with spine-godot
> - spine-runtimes GitHub / Codeberg 仓库
