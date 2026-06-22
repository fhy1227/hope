---
name: generate-config-cs
description: 从 tools/config/ 下的 xlsx 配置表生成 Hope.Config 命名空间的 C# 配置类（参考 ItemConfig.cs）。Use when the user asks to generate config C# types, create *Config.cs from xlsx, or work with config table schema (table/json/comma tags).
---

# 生成配置 C# 类型

从 xlsx 配置表生成 C# 配置类，实现 `IConfigData` 接口，供 `ConfigManager.GetAll<T>()` / `Get<T>()` 使用。

## 输入 / 输出目录

| | 路径 |
|---|------|
| **输入** | `tools/config/{name}.xlsx` |
| **输出** | `scripts/config/{Name}Config.cs` |

生成的 `.cs` 文件**必须**写入项目 `scripts/` 下的 `config/` 目录，不要放到其他位置。

示例：`tools/config/item.xlsx` → `scripts/config/ItemConfig.cs`

## 工作流程

```
任务进度:
- [ ] 1. 读取 xlsx 表头（Row 1-5）
- [ ] 2. 确定类名与输出路径
- [ ] 3. 生成属性与 XML 注释
- [ ] 4. 生成 FromDict 反序列化逻辑
- [ ] 5. 写入 `scripts/config/{Name}Config.cs`（输出目录固定为 `scripts/config/`）
```

### Step 1: 读取 xlsx 表头

xlsx 位于 `tools/config/`。用 openpyxl 读取**第一个非 `!` 开头**的工作表（或用户指定的工作表名）：

| 行 | 内容 |
|----|------|
| Row 1 | 字段名 `field_name`（snake_case） |
| Row 2 | 类型：`int` \| `string` \| `float` \| `bool` |
| Row 3 | 标签（可选）：`ignore` \| `text` \| `comma` \| `table` \| `json` |
| Row 4 | 模式（可选）：`client` 等 |
| Row 5 | 注释说明 |

跳过 `tag == ignore` 的列。可用以下命令快速查看 schema：

```bash
python -c "
import openpyxl, sys
from pathlib import Path
p = Path('tools/config') / sys.argv[1]
wb = openpyxl.load_workbook(p, data_only=True)
ws = next(w for n in wb.sheetnames if not n.startswith('!') for w in [wb[n]])
for c in range(1, ws.max_column + 1):
    n,t,g,m,d = [ws.cell(r,c).value for r in range(1,6)]
    if n: print(f'{n}\t{t}\t{g or \"\"}\t{m or \"\"}\t{d or \"\"}')
" item.xlsx
```

也可参考 [tools/export_config.py](tools/export_config.py) 的 `parse_sheet` 逻辑。

### Step 2: 命名规则

| xlsx | C# |
|------|-----|
| `item.xlsx` | `ItemConfig` |
| `hero_skill.xlsx` | `HeroSkillConfig` |

- 文件名 snake_case → PascalCase + `Config` 后缀
- 输出目录：`scripts/config/`（即 `scripts/` 下的 `config` 子目录）
- 输出文件：`{Name}Config.cs`
- 命名空间：`Hope.Config`
- JSON 表名 = 类名去掉 `Config` 后转 snake_case（与 `ConfigManager` 一致）

### Step 3: 文件模板

以 [scripts/config/ItemConfig.cs](scripts/config/ItemConfig.cs) 为基准：

```csharp
using Godot;
using GodotArray = Godot.Collections.Array;
using GodotDictionary = Godot.Collections.Dictionary;

namespace Hope.Config;

/// <summary>
/// 自动生成的配置类 - 对应 {sheet}.xlsx
/// </summary>
public partial class {Name}Config : IConfigData
{
    // 属性...

    public void FromDict(GodotDictionary dict)
    {
        // 反序列化...
    }
}
```

## 类型与标签映射

### 基础类型（无标签）

| xlsx 类型 | C# 属性类型 | FromDict |
|-----------|-------------|----------|
| int | `int` | `X = (int)dict["field"];` |
| string | `string` | `X = (string)dict["field"];` |
| float | `float` | `X = (float)dict["field"];` |
| bool | `bool` | `X = (bool)dict["field"];` |

### snake_case → PascalCase

`main_type` → `MainType`，`value_rewards` → `ValueRewards`。`dict` 键保持 snake_case。

### comma 标签

逗号分隔列表，JSON 导出为数组。

| xlsx 类型 | C# 属性类型 |
|-----------|-------------|
| int | `int[]` |
| float | `float[]` |
| string | `string[]` |

FromDict 模板（以 `int[]` 为例）：

```csharp
if (dict["tags"].VariantType == Variant.Type.Array)
{
    var arr = dict["tags"].AsGodotArray();
    Tags = new int[arr.Count];
    for (int i = 0; i < arr.Count; i++)
        Tags[i] = (int)arr[i];
}
```

### table 标签（多维数组）

Excel 单元格格式：`{{k1,v1},{k2,v2}}`（花括号包裹的键值对列表）。

导出 JSON 后为 `[[k1,v1],[k2,v2]]`——**二维数组**（`GodotArray`，元素为 `GodotArray`）。

- C# 属性类型：`GodotArray`
- FromDict 需兼容已解析数组与遗留字符串：

```csharp
if (dict["getway"].VariantType == Variant.Type.Array)
{
    Getway = dict["getway"].AsGodotArray();
}
else if (dict["getway"].VariantType == Variant.Type.String
         && !string.IsNullOrEmpty((string)dict["getway"]))
{
    var json = Godot.Json.ParseString((string)dict["getway"]);
    Getway = json.AsGodotArray();
}
```

### json 标签（JSON 字符串）

Excel 单元格为**原始 JSON 文本**（如 `[1,2,3]`、`{"a":1}`）。

- JSON 数组 → `GodotArray`
- JSON 对象 → `GodotDictionary`
- 若类型为 `string` 且仅需原样保留 → `string`（不解析）

FromDict 模板（数组）：

```csharp
if (dict["extra"].VariantType == Variant.Type.Array)
{
    Extra = dict["extra"].AsGodotArray();
}
else if (dict["extra"].VariantType == Variant.Type.String
         && !string.IsNullOrEmpty((string)dict["extra"]))
{
    var json = Godot.Json.ParseString((string)dict["extra"]);
    Extra = json.AsGodotArray();
}
```

对象版本将 `AsGodotArray()` 换为 `AsGodotDictionary()`。

### text 标签

与 `string` 相同，注释中标注 `// @text`。

## XML 注释格式

```csharp
/// <summary>
/// field_name  // @tag [mode]
/// </summary>
```

- 无标签：只写字段名，如 `/// id`
- 有标签：追加 `// @comma`、`// @table [client]` 等（与 ItemConfig 一致）
- Row 5 注释可写入 summary 第二行（可选）

## 生成后检查

- [ ] 类名与 xlsx 对应，`partial class`，实现 `IConfigData`
- [ ] 所有非 ignore 字段均有属性 + `FromDict` 赋值
- [ ] `table` / `json` 字段使用 Godot 集合并处理 Array/String 双分支
- [ ] `comma` 字段使用 C# 数组并遍历 `GodotArray`
- [ ] 文件已写入 `scripts/config/`，未放到其他目录
- [ ] 不修改 `ConfigManager.cs`（除非用户明确要求）

## 参考

- 导出逻辑：[tools/export_config.py](tools/export_config.py)
- 详细标签说明：[reference.md](reference.md)
