# Bilingual Learning（星露谷双语学习 Mod）

> 版本 0.5.0

在《星露谷物语》里边玩边学英语：游戏语言设为英文时，悬停对话中的单词即可弹出**词性 + 中文释义**气泡，点星标收藏生词，随时导出复习。

## 功能

- **对话框内悬浮查词**：对话中的英文单词，鼠标悬停即显示「词性 + 中文释义」气泡。
- **生词本**：点星标把可选词加入生词本，按存档隔离，记录单词/词性/释义/原句/首次游戏时间。
- **星露谷专属词典**：基于 ECDICT 精筛 + 释义收敛，覆盖游戏常用词与变形（`went→go`、`channels→channel`），专有名词按类别显示官方译名（NPC 名不弹释义）。
- **生词本导出**：一键导出 Markdown / CSV / Anki，导出位置可配置。
- **GMCM 配置**：开关/导出快捷键、导出位置可在游戏内配置。

## 环境要求

| 项 | 要求 |
|---|---|
| Stardew Valley | 1.6.x（开发验证于 1.6.15） |
| SMAPI | 4.x（开发验证于 4.5.2） |
| 运行时 | .NET 6 |
| 可选前置 | [Generic Mod Config Menu](https://www.nexusmods.com/stardewvalley/mods/5098)（GMCM，用于游戏内配置） |

## 安装

1. 安装 SMAPI（见 [smapi.io](https://smapi.io)）。
2. 将本 mod 文件夹放入游戏 `Mods/` 目录。
3. 启动游戏（游戏语言设为**英文**时功能才会启用）。

## 使用

- **查词**：对话中把鼠标移到英文单词上，气泡显示释义。
- **收藏**：点击气泡左侧的星标，收藏/取消收藏。
- **开关**：默认 `F11` 一键开/关查词。
- **导出**：默认 `F9` 导出生词本（CSV / Markdown / Anki），默认输出到 `文档\StardewValley\BilingualLearning\`。

## 配置

通过 **GMCM**（游戏内或标题页）或直接编辑 `config.json`：

| 配置项 | 说明 | 默认 |
|---|---|---|
| `ToggleKey` | 查词总开关快捷键 | `F11` |
| `ExportKey` | 导出生词快捷键 | `F9` |
| `ExportLocation` | 导出位置 | `Documents`（可选 `Desktop` / `Downloads` / `Mod folder`） |

生词本数据按存档存放于 `Mods/BilingualLearning/data/<存档id>.json`。

## 词典与数据

- 词典：基于 [ECDICT](https://github.com/skywind3000/ECDICT) 精筛 + 释义收敛，`assets/dictionary.tsv`（约 5.2 万词，含变形）。
- 字体：`assets/Font.ttf` 为按词典用字裁剪的子集（约 2 MB）。
- 词典格式（v2，4 列）：`word \t pos \t translation \t tag`。

## 致谢

- [ECDICT](https://github.com/skywind3000/ECDICT) —— 英汉词典数据。
- [Yozai / 悠哉字体](https://github.com/lxgw/yozai) —— 中文显示字体。
- [SMAPI](https://github.com/Pathoschild/SMAPI) 与 [Generic Mod Config Menu](https://github.com/spacechase0/GenericModConfigMenu)。

## 许可

待定（TBD）。
