# 配置表 Schema 参考

## xlsx 模板结构

```
Row 1: id          | name    | getway              | tags
Row 2: int         | string  | string              | int
Row 3:             | text    | table               | comma
Row 4:             | client  | client              |
Row 5: 唯一ID      | 名称    | 获取途径            | 标签列表
Row 6+: 1001       | ...     | {{1,9},{1,3}}       | 1,2,3
```

## 标签详解

### ignore

不导出、不生成 C# 字段。

### text

本地化 key 字段，运行时通过文本系统解析。C# 类型仍为 `string`。

### comma

单元格：`1,2,3` → JSON：`[1, 2, 3]`

解析规则见 `export_config.py` 的 `parse_value`（comma 分支）：按逗号分割，优先转 int，失败转 float，再失败保留 string。

### table

单元格格式（Lua 风格表）：

```
{{1,9},{1,3}}
{{4,"ItemGetway_2"},{3,1}}
```

解析步骤（与 export_config.py 一致）：

1. 去掉外层 `{{` `}}`
2. 按 `},{` 分割为多个 pair
3. 每个 pair 去掉 `{` `}`，按第一个逗号拆成两个元素
4. 元素尝试转 int，失败保留 string
5. 输出 `[[e1,e2], ...]`

C# 侧统一用 `GodotArray` 表示，访问子数组时再 `.AsGodotArray()`。

### json

单元格为合法 JSON 字符串，**不做** table 的 `{{}}` 解析。

示例：

| 单元格内容 | C# 类型 |
|-----------|---------|
| `[1,2,3]` | `GodotArray` |
| `{"hp":100,"atk":50}` | `GodotDictionary` |
| `"raw"` | `string`（type=string 时） |

`FromDict` 中：若 JSON 已是 Array/Dictionary 则直接 `AsGodotArray()` / `AsGodotDictionary()`；若是 String 则 `Godot.Json.ParseString` 后转换。

## ConfigManager 表名映射

`ItemConfig` → JSON 文件 `item.json`

规则：去掉 `Config` 后缀 → PascalCase 转 snake_case。

```csharp
ConfigManager.GetAll<ItemConfig>();
ConfigManager.Get<ItemConfig>("1001");
```

## ItemConfig 字段对照

| 字段 | 类型 | 标签 | C# 属性 |
|------|------|------|---------|
| id | int | - | `int Id` |
| name | string | text | `string Name` |
| getway | string | table | `GodotArray Getway` |
| tags | int | comma | `int[] Tags` |
| maxlv_to_money | int | comma | `int[] MaxlvToMoney` |
