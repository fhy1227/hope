---
name: generate-params-config
description: 从 tools/config/params.xlsx 生成 Hope.Config.ParamsConfig 静态参数类（每个 name_key 一个变量，类型由 val_num/val_str/val_color 自动推断）。Use when generating ParamsConfig.cs, editing params.xlsx, or adding global game balance constants.
---

# 生成 ParamsConfig.cs

`params.xlsx` **不走** [generate-config-cs](../generate-config-cs/SKILL.md) 的 `IConfigData` 行模式，而是生成**单个**静态类，把每行 `name_key` 映射为具名变量。

| | 路径 |
|---|------|
| **输入** | `tools/config/params.xlsx` |
| **输出** | `scripts/config/ParamsConfig.cs` |
| **生成脚本** | [scripts/generate_params_config.py](scripts/generate_params_config.py) |

## xlsx 表结构

与常规配置表相同的前 5 行表头；**数据从第 6 行起**。

| 字段 | 类型 | 说明 |
|------|------|------|
| `id` | int | 主键（导出 JSON 用，生成 C# 时不生成 Id 属性） |
| `name_key` | string | **必填**；snake_case，转为 PascalCase 属性名 |
| `desc` | string | 可选；中文说明，写入 XML 注释 |
| `val_num` | float | 数值参数；有值则生成 `float` |
| `val_str` | string | 字符串参数；有值则生成 `string` |
| `val_color` | string | 颜色参数；有值则生成 `Godot.Color` |

Row 5 注释写入 XML `/// <summary>` 第二行（可选）。

工作表名建议为 `params`（与文件名一致），以便 `tools/export_config.py` 导出 `assets/config/params.json`。

## 类型推断

对每一数据行，按**互斥**原则取**唯一**类型（只应填一列）：

1. `val_num` 非空 → `float`
2. 否则 `val_str` 非空 → `string`
3. 否则 `val_color` 非空 → `Color`
4. 三者皆空 → **跳过**该行

若多列同时有值：打印警告，优先级 **`val_num` > `val_str` > `val_color`**。

「非空」：`None`、空字符串、纯空白视为未填；`val_num` 的 `0` 视为有效值。

## 命名规则

- `name_key` `shop_option_count` → 属性 `ShopOptionCount`
- snake_case → PascalCase；非法字符替换为 `_`
- 与 C# 保留字冲突时前缀 `_`（如 `_string`）
- 重复 `name_key` → 脚本报错退出

## 工作流程

```
任务进度:
- [ ] 1. 确认 params.xlsx 表头与数据行
- [ ] 2. 运行生成脚本
- [ ] 3. 检查 scripts/config/ParamsConfig.cs
- [ ] 4. 运行 tools/export_config.py 导出 params.json（可选，供 ConfigManager 行数据）
```

### 运行生成

```bash
python .cursor/skills/generate-params-config/scripts/generate_params_config.py
```

或指定路径：

```bash
python .cursor/skills/generate-params-config/scripts/generate_params_config.py \
  --xlsx tools/config/params.xlsx \
  --out scripts/config/ParamsConfig.cs
```

## 输出模板

```csharp
using Godot;

namespace Hope.Config;

/// <summary>
/// 全局参数 - 对应 tools/config/params.xlsx（自动生成，请勿手改）。
/// </summary>
public static partial class ParamsConfig
{
    /// <summary>
    /// shop_option_count
    /// 商店选项数量
    /// </summary>
    public const float ShopOptionCount = 3f;

    /// <summary>
    /// ui_accent
    /// HUD 强调色
    /// </summary>
    public static readonly Color UiAccent = new(1f, 0.85f, 0.3f);
}
```

- `float` → `public const float Name = {value}f;`
- `string` → `public const string Name = "...";`（转义 `"`、`\`）
- `Color` → `public static readonly Color Name = new(r, g, b);` 或 `new(r, g, b, a);`

### val_color 解析

| 格式 | 示例 |
|------|------|
| `#RRGGBB` | `#FF8800` |
| `#AARRGGBB` | `#80FF8800` |
| 逗号分隔 RGB(A)，0–255 | `255,136,0` 或 `255,136,0,255` |
| 逗号分隔 RGB(A)，0–1 | `1,0.85,0.3` 或 `1,0.85,0.3,1` |

无法解析时脚本报错并指出行号。

## 生成后检查

- [ ] 文件位于 `scripts/config/ParamsConfig.cs`
- [ ] 每个有效 `name_key` 对应一个 `const` 或 `static readonly Color`
- [ ] 未实现 `IConfigData`（与 `*Config.cs` 行表不同）
- [ ] 修改 xlsx 后重新运行脚本，**不要**手改生成文件
- [ ] 需要 JSON 时运行 `python tools/export_config.py`

## 与 generate-config-cs 的关系

批量生成 `*Config.cs` 时 **必须跳过** `params.xlsx`，改由本技能处理。见 [generate-config-cs](../generate-config-cs/SKILL.md)「排除表」。
