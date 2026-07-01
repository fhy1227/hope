# Godot 4.7 接入 Steam 指南

> 基于 GodotSteam v4.20 + Steamworks SDK 1.64
> 适用于 Godot 4.7 (C# / GDScript)
> 2026年7月

---

## 目录

1. [概述](#1-概述)
2. [安装方式选择](#2-安装方式选择)
3. [方式一：GDExtension 插件安装（推荐）](#3-方式一gdextension-插件安装推荐)
4. [方式二：编译 Module 安装](#4-方式二编译-module-安装)
5. [方式三：预编译二进制（最快）](#5-方式三预编译二进制最快)
6. [Steam App ID 配置](#6-steam-app-id-配置)
7. [初始化 Steam API](#7-初始化-steam-api)
8. [回调处理](#8-回调处理)
9. [错误处理](#9-错误处理)
10. [基本功能使用](#10-基本功能使用)
11. [C# 集成说明](#11-c-集成说明)
12. [发布与导出](#12-发布与导出)
13. [常见问题](#13-常见问题)
14. [附：版本兼容对照表](#14-附版本兼容对照表)

---

## 1. 概述

### 1.1 GodotSteam 是什么

GodotSteam 是一个开源项目，为 Godot 引擎提供 Valve Steamworks SDK 的完整绑定，支持在游戏中集成 Steam 的各项功能：

| 功能 | 说明 |
|------|------|
| 成就系统 | 解锁、存储成就 |
| 云存档 | 自动同步存档到 Steam 云 |
| 多人游戏 | P2P 网络、大厅系统、Steam 联机 |
| 统计追踪 | 玩家统计数据 |
| 覆盖层 | Steam Shift+Tab 界面 |
| 用户信息 | 获取用户名、Steam ID 等 |
| DLC/微交易 | 管理可下载内容和游戏内购买 |
| UGC 工坊 | 创意工坊内容 |

### 1.2 版本信息

| 组件 | 版本 |
|------|------|
| GodotSteam | 4.20 |
| Steamworks SDK | 1.64 |
| 适用 Godot | 4.4+（含 4.7） |
| 支持平台 | Windows (32/64-bit)、Linux (32/64-bit/ARM64)、macOS (Universal)、Android ARM64 |

---

## 2. 安装方式选择

| 方式 | 难度 | 说明 | 推荐场景 |
|:----:|:----:|------|----------|
| **GDExtension** | ⭐ | 从 Asset Library 安装/下载预编译包 | **首选**，最简单 |
| **编译 Module** | ⭐⭐⭐ | 从源码编译 GodotSteam + Godot | 需要修改引擎源码 |
| **预编译二进制** | ⭐⭐ | 下载现成的编译文件 | 不想用 Asset Library 时 |

---

## 3. 方式一：GDExtension 插件安装（推荐）

### 3.1 从 Asset Library 安装

1. 打开 Godot 编辑器
2. 点击 **AssetLib** 标签
3. 搜索 **GodotSteam GDExtension**
4. 选择 **GodotSteam GDExtension 4.4+** 版本 4.20
5. 点击 **Download** → **Install**

> 直接下载地址：https://godotengine.org/asset-library/asset/2445

### 3.2 手动安装（从 Releases 下载）

1. 从发布页下载预编译包：
   ```
   https://codeberg.org/godotsteam/godotsteam/releases
   ```

2. 解压到项目目录的 `addons/godotsteam/` 文件夹下

3. 目录结构应如下：
   ```
   项目根目录/
   └─ addons/
      └─ godotsteam/
         ├─ godotsteam.gdextension
         ├─ godotsteam.gdnlib (旧版)
         └─ 平台相关二进制文件
   ```

### 3.3 启用

GDExtension 版本**不需要手动启用插件**。在 Project Settings 中启用插件**仅用于显示 Steamworks 面板**，不影响功能。

> 可能需要重启编辑器。

---

## 4. 方式二：编译 Module 安装

如果需要最大兼容性或修改引擎源码，可以编译 Module。

### 4.1 准备环境

参照 Godot 官方文档安装编译工具链：

| 平台 | 参考文档 |
|------|----------|
| Windows | [编译 Godot Windows 版](https://docs.godotengine.org/en/latest/engine_details/development/compiling/compiling_for_windows.html) |
| Linux | [编译 Godot Linux 版](https://docs.godotengine.org/en/latest/engine_details/development/compiling/compiling_for_linuxbsd.html) |
| macOS | [编译 Godot macOS 版](https://docs.godotengine.org/en/latest/engine_details/development/compiling/compiling_for_macos.html) |

### 4.2 获取源码

```bash
# 克隆 Godot 4.7 源码
git clone https://github.com/godotengine/godot.git -b 4.7-stable godot

# 进入 modules 目录
cd godot/modules/

# 克隆 GodotSteam (godot4 分支)
git clone -b godot4 https://codeberg.org/godotsteam/godotsteam.git godotsteam
```

### 4.3 放置 Steamworks SDK

需要 Steam 开发者账号才能下载 SDK：

```bash
# 下载 Steamworks SDK 后，将 public 和 redistributable_bin 文件夹放入
godot/modules/godotsteam/sdk/
```

目录结构：
```
godot/
└─ modules/
   └─ godotsteam/
      ├─ doc_classes/
      ├─ icons/
      ├─ sdk/
      │  ├─ public/*
      │  └─ redistributable_bin/*
      ├─ config.py
      ├─ godotsteam.cpp
      ├─ godotsteam.h
      ├─ register_types.cpp
      ├─ SCsub
      └─ ...
```

### 4.4 编译

```bash
# 编译编辑器
scons platform=windows target=editor

# 编译调试模板
scons platform=windows target=template_debug

# 编译发布模板
scons platform=windows target=template_release
```

其他平台将 `platform` 替换为 `linux` 或 `macos`。

### 4.5 复制 Steam API 动态库

编译完成后，将对应平台的 Steam API 库复制到编译输出目录：

```bash
# 从 sdk/redistributable_bin/ 复制
# Windows: steam_api64.dll
# Linux: libsteam_api.so
# macOS: libsteam_api.dylib

# 复制到 godot/bin/ 目录下
```

> **缺少 Steam API .dll/.so/.dylib 会导致编辑器或游戏在 Steam 客户端外运行时崩溃。**

---

## 5. 方式三：预编译二进制（最快）

直接从 GitHub Releases 下载预编译版本：

```bash
# 访问以下链接下载对应平台的预编译包
https://codeberg.org/godotsteam/godotsteam/releases
```

下载后解压到项目 `addons/` 目录即可。

---

## 6. Steam App ID 配置

### 6.1 为什么需要 App ID

开发和测试时必须提供有效的 App ID。如果没有自己的游戏 App ID，可以使用 Valve 示例游戏 **SpaceWar 的 App ID 480**。

### 6.2 配置方法（四种）

| 方法 | 说明 | 推荐度 |
|------|------|:----:|
| **项目设置** | 项目设置 > Steam > 初始化 > App ID | ⭐⭐⭐ 最推荐 |
| `steamInit()` 传参 | 初始化函数第一个参数 | ⭐⭐ |
| 环境变量 | `OS.set_environment()` | ⭐⭐ |
| `steam_appid.txt` | 创建文本文件写入 App ID | ⭐ (仅测试) |

#### 方法 1：项目设置（推荐）

GodotSteam 4.14+ 支持在 **项目设置 > Steam > 初始化** 中直接设置 App ID。

#### 方法 2：初始化传参

```gdscript
Steam.steamInitEx(480)
# 或
Steam.steamInitEx(480, true)  # 第二个参数：嵌入回调
```

#### 方法 3：环境变量

```gdscript
func _init():
    OS.set_environment("SteamAppId", str(480))
    OS.set_environment("SteamGameId", str(480))
```

#### 方法 4：steam_appid.txt

创建 `steam_appid.txt` 文件，内容仅为 App ID 数字，放在以下位置之一：
- Godot 编辑器可执行文件所在目录
- 插件模式：项目根目录

> **⚠️ 发布游戏到 Steam 时，不要包含 `steam_appid.txt` 文件！**

---

## 7. 初始化 Steam API

### 7.1 创建 Steam 自动加载脚本

建议创建一个单例脚本 `steamworks.gd`：

```gdscript
# steamworks.gd
extends Node

var steam_enabled: bool = false

func _ready():
    initialize_steam()

func initialize_steam():
    # steamInitEx() 返回字典，包含 status 和 verbal
    var result: Dictionary = Steam.steamInitEx()
    print("Steam init result: ", result)

    if result["status"] == Steam.STEAM_API_INIT_RESULT_OK:
        steam_enabled = true
        print("Steam initialized successfully!")
        print("Player: ", Steam.getPersonaName())
        print("Steam ID: ", Steam.getSteamID())
    else:
        steam_enabled = false
        print("Steam init failed: ", result["verbal"])
```

### 7.2 设置自动加载

1. 打开 **项目 > 项目设置 > 自动加载**
2. 添加 `steamworks.gd`，命名为 `Steam`（注意不要和 GodotSteam 的 `Steam` 内置单例冲突）

> **注意**：GodotSteam 本身已经在 `Steam` 命名空间下提供了所有 Steamworks API。你的自动加载脚本建议命名为其他名字（如 `Steamworks`、`SteamManager`）。

### 7.3 初始化 API 对照

| 函数 | 返回值 | 说明 |
|------|--------|------|
| `Steam.steamInit()` | `bool` | 简单初始化，成功返回 true |
| `Steam.steamInitEx()` | `Dictionary` | 推荐，返回详细状态信息 |

`steamInitEx()` 返回字典包含：
- `"verbal"` (`String`)：状态描述
- `"status"` (`int`)：状态码

状态码 | 含义
:----:|------
0 | 成功
1 | 其他失败
2 | 无法连接到 Steam（客户端未运行）
3 | Steam 客户端版本过旧

### 7.4 在 C# 中使用

```csharp
using Godot;
using Steam;

public partial class SteamManager : Node
{
    public bool SteamEnabled { get; private set; }

    public override void _Ready()
    {
        var result = Steam.Steam.steamInitEx();
        GD.Print("Steam init result: ", result);

        if ((int)result["status"] == 0)
        {
            SteamEnabled = true;
            GD.Print("Steam initialized!");
            GD.Print("Player: ", Steam.Steam.getPersonaName());
        }
        else
        {
            SteamEnabled = false;
            GD.Print("Steam init failed: ", result["verbal"]);
        }
    }
}
```

---

## 8. 回调处理

### 8.1 为什么需要回调

Steamworks API 使用回调机制异步传递事件（如邀请、成就解锁、大厅更新等），需要每帧刷新。

### 8.2 启用方式

#### 方式 1：在 _process 中手动调用（推荐）

```gdscript
# 在自动加载脚本中
func _process(_delta):
    if steam_enabled:
        Steam.run_callbacks()
```

> **重要**：确保 `run_callbacks()` 所在的脚本/节点**不会被暂停**，否则回调将无法触发。

#### 方式 2：项目设置自动嵌入

在 **项目设置 > Steam > 初始化** 中勾选 **Embed callbacks**，GodotSteam 会在内部处理回调。

> **注意**：GodotSteam 4.16 修复前，同时启用"自动初始化"和"嵌入回调"可能导致编辑器崩溃。4.20 已修复。

#### 方式 3：初始化传参

```gdscript
Steam.steamInitEx(480, true)  # 第二个参数 true 表示嵌入回调
```

### 8.3 监听回调

直接在脚本中定义对应名称的函数即可接收回调：

```gdscript
# 示例：监听大厅创建回调
func _on_lobby_created(connect_result: int, lobby_id: int):
    if connect_result == 1:
        print("Lobby created! ID: ", lobby_id)

# 示例：监听成就存储回调
func _on_user_stats_received(result: int, steam_id: int):
    if result == 1:
        print("Stats received!")
```

---

## 9. 错误处理

### 9.1 检查初始化状态

```gdscript
func initialize_steam():
    var result = Steam.steamInitEx()

    # 方案 A：初始化失败则关闭游戏
    if result["status"] > Steam.STEAM_API_INIT_RESULT_OK:
        print("Steam init failed, shutting down: ", result["verbal"])
        show_error_dialog("Steam not running!")
        get_tree().quit()
        return

    # 方案 B：初始化失败则禁用 Steam 功能，游戏继续运行
    if result["status"] > Steam.STEAM_API_INIT_RESULT_OK:
        print("Steam disabled: ", result["verbal"])
        steam_enabled = false
        return

    steam_enabled = true
```

### 9.2 检查用户是否拥有游戏

```gdscript
if Steam.isSubscribed():
    print("User owns this game")
else:
    print("User does not own this game (family sharing?)")
```

> **注意**：仅用 `isSubscribed()` 关闭游戏可能误伤家庭共享、免费周末等合法用户。

---

## 10. 基本功能使用

### 10.1 获取用户信息

```gdscript
var steam_id: int        = Steam.getSteamID()
var username: String     = Steam.getPersonaName()
var is_online: bool      = Steam.loggedOn()
var language: String     = Steam.getCurrentGameLanguage()
var is_steam_deck: bool  = Steam.isSteamRunningOnSteamDeck()
var build_id: int        = Steam.getAppBuildId()
var launch_cmd: String   = Steam.getLaunchCommandLine()
```

### 10.2 成就系统

```gdscript
# 解锁成就
Steam.setAchievement("ACHIEVEMENT_NAME")
Steam.storeStats()

# 查询成就
var is_unlocked = Steam.getAchievement("ACHIEVEMENT_NAME")

# 清除成就（调试用）
Steam.clearAchievement("ACHIEVEMENT_NAME")
Steam.storeStats()
```

### 10.3 大厅与多人游戏

```gdscript
# 创建大厅
Steam.createLobby(Steam.LOBBY_TYPE_PUBLIC, 4)

# 监听大厅创建结果
func _on_lobby_created(connect_result: int, lobby_id: int):
    if connect_result == 1:
        print("Lobby created: ", lobby_id)
        Steam.setLobbyData(lobby_id, "name", "My Game Lobby")

# 加入大厅
Steam.joinLobby(lobby_id)

# 离开大厅
Steam.leaveLobby(lobby_id)
```

### 10.4 云存档

```gdscript
# 写文件到 Steam 云
Steam.fileWrite("savefile.sav", save_data)

# 读文件
var data = Steam.fileRead("savefile.sav")

# 检查文件是否存在
var exists = Steam.fileExists("savefile.sav")

# 获取云存档配额
var quota = Steam.getQuota()
print("Total bytes: ", quota["total_bytes"])
print("Available bytes: ", quota["available_bytes"])
```

### 10.5 覆盖层

```gdscript
# 打开 Steam 覆盖层到指定页面
Steam.activateGameOverlay("achievements")  # 成就页面
Steam.activateGameOverlay("friends")       # 好友列表
Steam.activateGameOverlay("community")     # 社区
Steam.activateGameOverlay("players")       # 当前玩家
Steam.activateGameOverlay("settings")      # 设置
Steam.activateGameOverlay("chat")          # 聊天
Steam.activateGameOverlayToWebPage("https://example.com")
```

### 10.6 统计系统

```gdscript
# 设置统计值
Steam.setStatInt("kills", 100)
Steam.setStatFloat("accuracy", 85.5)
Steam.storeStats()

# 获取统计值
var kills = Steam.getStatInt("kills")
var accuracy = Steam.getStatFloat("accuracy")
```

### 10.7 创意工坊 (UGC)

```gdscript
# 查询用户已订阅的 Mod
var ugc_result = Steam.queryUserUGCList(
    Steam.UGCSortOrder_CreationOrderDesc,
    Steam.UGCMatchingType_Items
)

# 安装并订阅 Mod
Steam.subscribeItem(published_file_id)

# 获取 Mod 信息
var title = Steam.getItemTitle(published_file_id)
var description = Steam.getItemDescription(published_file_id)
var preview_url = Steam.getItemPreviewImageUrl(published_file_id)
```

---

## 11. C# 集成说明

### 11.1 GDScript vs C# 的 Steam 使用

| 语言 | 支持情况 |
|:----:|----------|
| GDScript | 完全支持，所有 API 可用 |
| C# | 完全支持，需添加命名空间引用 |

### 11.2 C# 使用示例

```csharp
using Godot;
using Steam;  // GodotSteam 命名空间

public partial class SteamManager : Node
{
    public bool SteamEnabled { get; private set; }

    public override void _Ready()
    {
        var result = Steam.Steam.steamInitEx();

        if ((int)result["status"] == (int)Steam.Steam.STEAM_API_INIT_RESULT_OK)
        {
            SteamEnabled = true;
            GD.Print("Steam initialized: " + Steam.Steam.getPersonaName());

            // 成就
            Steam.Steam.setAchievement("FIRST_ACHIEVEMENT");
            Steam.Steam.storeStats();

            // 统计
            Steam.Steam.setStatInt("kills", 50);
            Steam.Steam.storeStats();
        }
        else
        {
            SteamEnabled = false;
            GD.Print("Steam init failed: " + result["verbal"]);
        }
    }

    public override void _Process(double delta)
    {
        if (SteamEnabled)
            Steam.Steam.run_callbacks();
    }
}
```

### 11.3 在 C# 中启用回调

```csharp
// 订阅回调事件
Steam.Steam.SteamworksCallbacks.LobbyCreated += OnLobbyCreated;
Steam.Steam.SteamworksCallbacks.UserStatsReceived += OnStatsReceived;

private void OnLobbyCreated(int connectResult, ulong lobbyId)
{
    GD.Print($"Lobby created: {lobbyId}");
}

private void OnStatsReceived(int result, ulong steamId)
{
    GD.Print("Stats received!");
}
```

---

## 12. 发布与导出

### 12.1 发布前检查清单

- [ ] 移除 `steam_appid.txt`（如果有）
- [ ] 确认导出的可执行文件包含 `steam_api64.dll`（Windows）`libsteam_api.so`（Linux）或 `libsteam_api.dylib`（macOS）
- [ ] 确认 Steam 工具（Steam Pipe）配置正确
- [ ] 测试在 Steam 客户端外运行时能正常提示而非崩溃

### 12.2 Steam 导出的特殊注意事项

| 平台 | 注意事项 |
|:----:|----------|
| Windows | `steam_api64.dll` 必须和 `.exe` 放在同一目录 |
| Linux | 可能需要设置 `LD_PRELOAD` 来启用覆盖层 |
| macOS | `libsteam_api.dylib` 必须在 `Contents/MacOS/` 文件夹内 |

#### Linux 覆盖层修复

如果 Steam 覆盖层在 Linux 上不工作，创建启动脚本：

```bash
#!/bin/bash
export LD_PRELOAD=~/.local/share/Steam/ubuntu12_32/gameoverlayrenderer.so:$LD_PRELOAD
./YourGame.x86_64
```

### 12.3 GDExtension 导出设置

GDExtension 版本会自动包含平台相关的二进制文件。在导出时：

1. 打开 **项目 > 导出**
2. 在导出模板中确认 `addons/godotsteam/` 目录被包含
3. 确保目标平台对应的 `.dll` / `.so` / `.dylib` 文件在导出包内

---

## 13. 常见问题

### 13.1 Steam 覆盖层不工作

- **Forward+ 渲染器**: 从编辑器运行时覆盖层可能不工作，启用项目设置中的自动初始化可解决
- **导出后**: 导出版本在 Steam 客户端内运行应该正常
- **Linux**: 可能需要设置 `LD_PRELOAD` 环境变量

### 13.2 编辑器崩溃

- 检查 GodotSteam 版本是否与 Godot 版本兼容
- 尝试使用 GDExtension 版本而非编译版本
- 禁用自动初始化和嵌入回调的重复配置

### 13.3 初始化失败（status=2）

- Steam 客户端未运行
- 缺少 `steam_appid.txt` 或 App ID 配置
- 在外部测试时没有 `steam_api64.dll`

### 13.4 编译错误

- **Godot master 分支**: 可能存在未在稳定版中出现的兼容问题。建议使用稳定版。
- **MinGW**: 不推荐使用 MinGW 编译 GodotSteam（4.17 前），建议使用 MSVC。

---

## 14. 附：版本兼容对照表

### 14.1 GodotSteam ↔ Steamworks SDK

| GodotSteam 版本 | Steamworks SDK | 备注 |
|:---------------:|:--------------:|------|
| 4.20 | 1.64 | **最新版**，支持 Godot 4.7 |
| 4.17 | 1.63 | 支持 Proton 11 / Steam Deck |
| 4.14 - 4.16 | 1.62 | 部分 API 变更 |
| 4.12 - 4.13 | 1.61 | |
| 4.6 - 4.11 | 1.60 | |
| 4.5.4 及更早 | 1.58a 及更早 | |

### 14.2 GodotSteam 破坏性变更

| 版本 | 变更内容 |
|:----:|----------|
| 4.20 | Godot 4.7 变更 `callable_method_pointer.h` → `callable_mp.h`，**必须使用此版** |
| 4.19 | Voice 函数大量变更 |
| 4.17 | Windows 版需 Steam SDK 1.63 以支持 Proton |
| 4.14 | `steamInit()` 移除第一个参数（统计同步），`steamInit()` 返回正确 bool |
| 4.11 | `setLeaderboardDetailsMax` 移除 |
| 4.9 | `sendMessages` 返回 Array |
| 4.8 | 网络身份系统移除，改用 Steam ID |

### 14.3 Godot 4.x 各版本兼容

| Godot 版本 | 推荐 GodotSteam 安装方式 |
|:----------:|-------------------------|
| 4.7 | GDExtension 4.20 / 编译 godot4 分支 |
| 4.4 - 4.6 | GDExtension 4.20 / 编译 godot4 分支 |
| 4.1 - 4.3 | [GDExtension 旧版](https://godotengine.org/asset-library/asset/3866) |
| 4.0 | [GDExtension 旧版](https://godotengine.org/asset-library/asset/1768) |

---

> **参考来源：**
> - GodotSteam 官方文档：https://godotsteam.com
> - GodotSteam 源码：https://codeberg.org/godotsteam/godotsteam
> - Godot Engine Asset Library：https://godotengine.org/asset-library/asset/2445
> - Steamworks API 文档：https://partner.steamgames.com/doc/
>
> 最后更新：2026年7月
