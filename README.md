# Bilingual Learning（星露谷双语学习 Mod）

> 版本 0.5.0

在《星露谷物语》里边玩边学英语：游戏语言设为英文时，悬停对话中的单词即可弹出**词性 + 中文释义**气泡，点星标收藏生词，随时导出复习。

## 功能

- **对话框内悬浮查词**：对话中的英文单词，鼠标悬停即显示「词性 + 中文释义」气泡。
- **生词本**：点星标把可选词加入生词本，按存档隔离，记录单词/词性/释义/原句/首次游戏时间。
- **星露谷专属词典**：基于 ECDICT 精筛 + 释义收敛，覆盖游戏常用词与变形（`went→go`、`channels→channel`），专有名词按类别显示官方译名（NPC 名不弹释义）。
- **生词本导出**：一键导出 Markdown / CSV / Anki，导出位置可配置。
- **GMCM 配置**：开关/导出快捷键、导出位置可在游戏内配置。
- **物品知识卡片**（`F1`）：悬停物品后按 `F1` 打开原生风格卡片，卡片内任意文字都可继续悬停查词、点星标收藏。
- **扩展文本面查词（实验性，默认关闭）**：开启 `Extended lookup` 后，**信件 / 秘密纸条 / 日志残页 / 任务日志**里的文字也可悬停查词（只读气泡，暂不支持星标收藏）。


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
- **物品知识卡片**：悬停物品后按 `F1` 打开/关闭卡片（打开时截图式复用原生 tooltip，可继续悬停查词）。
- **扩展文本面**：在 GMCM 把 `Extended lookup` 设为 `On`，然后在信件 / 任务日志里直接悬停单词即可。

## 测试键 `F10` 的用意

`F10`（`config.json` 里的 `DebugTextRectsKey`）是**开发/对位用的调试键**，不是给日常游玩准备的：

- **作用**：在 GMCM 里开启 `Extended lookup` 后，按 `F10` 会把「当前这一帧捕获到的每个单词矩形」用半透明绿色画出来，用来检查气泡的命中区域是否与屏幕文字贴合；同时把捕获的文本行与词矩形写进 SMAPI 日志（`[诊断]` 开头），便于排查对齐问题。
- **为什么不在 GMCM 里**：它只服务于开发者/测试，普通玩家用不到；放在 GMCM 里容易被误开，反而遮挡画面。需要时直接改 `config.json` 的 `DebugTextRectsKey` 即可。
- **开销与安全性**：默认关闭；关闭且 `Extended lookup` 也为 `Off` 时，扩展文本捕获完全不运行（零开销）。它只读游戏绘制结果，不修改游戏状态、不写存档。
- **在哪里看输出**：SMAPI 控制台窗口，或 SMAPI 自己的日志文件 `%APPDATA%\StardewValley\ErrorLogs\SMAPI-latest.txt`（由 SMAPI 决定位置，本 mod 不额外写文件）。
- **实测完成后**：再按一次 `F10` 关闭矩形显示即可，不需要改回任何配置。

> 其余快捷键 `F11`（查词总开关）、`F9`（导出）、`F1`（物品卡片）都是**正式功能键**，可在 GMCM 或 `config.json` 中修改。

## 配置

通过 **GMCM**（游戏内或标题页）或直接编辑 `config.json`：

| 配置项 | 说明 | 默认 |
|---|---|---|
| `ToggleKey` | 查词总开关快捷键 | `F11` |
| `ExportKey` | 导出生词快捷键 | `F9` |
| `ExportLocation` | 导出位置 | `Documents`（可选 `Desktop` / `Downloads` / `Mod folder`） |
| `BubbleStyle` | 气泡风格 | `Default`（可选 `Sandy's Notes`） |
| `ExtendLookup` | 扩展文本面查词（信件/任务日志），`Off` / `On` | `Off` |
| `LookupKey` | 物品知识卡片键（仅 `config.json`，未进 GMCM） | `F1` |
| `DebugTextRectsKey` | 矩形调试键（仅 `config.json`，见上文「测试键」） | `F10` |

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
