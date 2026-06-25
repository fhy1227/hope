---
name: generate-pixel-icons
description: 为 Hope 项目生成 128×128 现代精致系像素风图标，透明背景 PNG，输出到 assets/icons/。Use when the user asks to generate art assets, pixel icons, item icons, slot icons, game sprites, or mentions 128x128 / 像素风 / 透明背景 / 图标.
---

# 生成像素风图标

为 Godot 项目生成统一风格的库存/装备/UI 图标。

## 规格（必须遵守）

| 项 | 要求 |
|----|------|
| 尺寸 | **128×128 px**（最终输出） |
| 格式 | PNG，**透明背景**（alpha 通道） |
| 风格 | **现代精致系像素风**：轮廓清晰、有限色板、细腻明暗过渡，非 chunky 8-bit 复古块面 |
| 构图 | 主体居中，四周留约 10–15% 安全边距，便于 UI 缩放 |
| 输出目录 | `assets/icons/` |
| Godot 路径 | `res://assets/icons/{filename}.png` |

## 工作流程

```
任务进度:
- [ ] 1. 确认主题、文件名、是否需要与现有图标统一
- [ ] 2. 用 GenerateImage 生成初稿
- [ ] 3. 后处理为 128×128 透明 PNG
- [ ] 4. 保存到 assets/icons/ 并告知 res:// 路径
- [ ] 5. （可选）更新 item.json / equip_slot.json 中的 icon 字段
```

### Step 1: 确认需求

向用户确认或从上下文推断：

- **主体**：物品/槽位/技能等具体对象
- **文件名**：snake_case，如 `sword.png`、`slot_weapon.png`、`hp_pot.png`
- **配色/稀有度**：普通 / 蓝色 / 黄色等变体可用 `_b`、`_y` 后缀（参考现有配置）
- **参考图**：若需风格统一，读取 `assets/icons/` 下已有 PNG 作为 reference

命名约定见 [reference.md](reference.md)。

### Step 2: 生成初稿

使用 **GenerateImage** 工具。`description` 按下方模板组装；`filename` 用 `{name}_draft`（不含路径）。

**Prompt 模板：**

```
128x128 pixel art game inventory icon, modern refined pixel art style,
[SUBJECT DESCRIPTION],
clean silhouette, limited color palette, smooth pixel shading with subtle highlights,
centered composition with 10-15% padding, readable at small size,
fully transparent background, alpha channel, no background color, no drop shadow on canvas edge,
no text, no watermark, no frame border unless requested
```

**现代精致系** 关键词（按需组合）：

- `polished pixel art`, `refined shading`, `controlled palette`
- 避免：`retro 8-bit`, `chunky pixels`, `low resolution blur`

若用户提供参考图，传入 `reference_image_paths`。

### Step 3: 后处理

生成结果通常**不是** 128×128，且背景常为**灰白棋盘格假透明**或纯黑底，**必须**后处理：

```bash
pip install pillow
python .cursor/skills/generate-pixel-icons/scripts/postprocess_icon.py INPUT.png assets/icons/OUTPUT.png --size 128
```

脚本会：

1. 从边缘泛洪抠除棋盘格灰底 / 纯黑底（勿用 `--bg black` 硬指定，除非初稿确为纯黑）
2. 裁剪主体后**等比缩放**至 128×128 画布内居中（避免 3:2 初稿被拉成方形导致变形）
3. 输出 RGBA PNG

参数：

| 参数 | 说明 |
|------|------|
| `--size 128` | 目标边长（默认 128） |
| `--bg black` | 强制单色抠图（仅当初稿背景确为该色时使用） |
| `--tolerance 30` | 背景匹配容差（默认 30） |
| `--padding 8` | 画布内边距（默认 8 px） |
| `--no-resize` | 仅抠透明，不缩放 |

无 Python 环境时：在 prompt 中强调 transparent background，生成后告知用户需手动抠图或安装 Pillow 再跑脚本。

### Step 4: 交付

告知用户：

- 磁盘路径：`assets/icons/{name}.png`
- Godot 引用：`res://assets/icons/{name}.png`

### Step 5: 更新配置（仅当用户要求）

| 配置 | 路径 | icon 字段示例 |
|------|------|---------------|
| 物品 | `assets/config/item.json` | `"icon": "res://assets/icons/sword.png"` |
| 装备槽 | `assets/config/equip_slot.json` | `"icon": "res://assets/icons/slot_weapon.png"` |

改 JSON 后如需同步 C# 类型，使用 `generate-config-cs` skill。

## 质量检查

- [ ] 128×128，PNG 带 alpha
- [ ] 透明区域无杂色边（halos）
- [ ] 小尺寸下轮廓可辨认
- [ ] 与同目录已有图标风格一致
- [ ] 文件名与配置引用一致

## 示例

**输入：** 生成蓝色品质长剑图标，文件名 `sword_b.png`

**Prompt 主体：**

```
blue rarity longsword with silver blade and blue gem on golden crossguard
```

**输出：** `assets/icons/sword_b.png` → `res://assets/icons/sword_b.png`

更多 prompt 与风格说明见 [reference.md](reference.md)。
