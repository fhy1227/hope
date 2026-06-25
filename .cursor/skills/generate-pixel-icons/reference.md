# 像素图标参考

## 命名约定

| 类型 | 模式 | 示例 |
|------|------|------|
| 物品图标 | `{item}.png` | `sword.png`, `helmet.png`, `hp_pot.png` |
| 品质变体 | `{item}_{suffix}.png` | `sword_b.png`（蓝）, `sword_y.png`（黄） |
| 装备槽 | `slot_{slot}.png` | `slot_weapon.png`, `slot_helmet.png` |
| 带尺寸后缀（可选） | `{desc}_icon_{size}.png` | `longsword_icon_128x128.png` |

默认输出 **128×128**，文件名可不写尺寸后缀。

## 风格：现代精致系像素风

与 Hope 项目 `longsword_icon_64x64.png` 同类方向（可升级为 128×128）：

- **精致**：金属/皮革等有层次的高光与暗部，非单色平涂
- **像素感**：保留硬边与像素块，但 shading 过渡细腻
- **可读性**：Inventory 格子内一眼可辨，避免过细纹理
- **色板**：每图标约 8–16 色，同类物品共享主色逻辑

### 推荐 Prompt 片段

**武器：**
```
[metal] sword/dagger/axe, sharp edge highlight, wrapped leather grip, golden guard
```

**防具：**
```
[material] helmet/armor/boots, rounded form, metallic sheen, visible straps or plates
```

**消耗品：**
```
glass potion bottle with [color] liquid, cork stopper, subtle glass reflection
```

**装备槽（抽象）：**
```
minimal slot icon, [weapon/helmet/armor/boots/amulet] silhouette, UI glyph style
```

### 避免

- 写实照片风、3D 渲染感过强
- 过大外发光把透明区弄脏
- 文字、数字、复杂背景场景
- 主体贴边（UI 裁切风险）

## 配色与稀有度（参考 item.json）

| 后缀 | 含义 | 色调提示 |
|------|------|----------|
| 无 | 普通 | 中性灰、棕、基础金属色 |
| `_b` | 蓝色品质 | 钢蓝高光、蓝宝石点缀 |
| `_y` | 黄色/金色品质 | 金色镶边、暖色高光 |

## Godot 导入

图标放入 `assets/icons/` 后 Godot 自动导入为 `CompressedTexture2D`。无需手改 `.import` 文件；在编辑器中确认 `fix_alpha_border=true`（项目默认已启用）。
