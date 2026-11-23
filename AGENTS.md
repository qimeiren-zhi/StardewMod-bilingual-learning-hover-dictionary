# Agent 长期规则（星露谷双语学习 Mod）

> 本文件是给 AI/开发者 Agent 的长期规则与项目地图。每次新会话**先读本文件**，再读 `docs/ai/HANDOFF.md`。
> 证据优先：关键结论尽量给 `文件:行号` 或检索关键词；源码与文档冲突以源码为准。

## 项目地图

### 目录结构

```
D:\Project\星露谷双语mod\
├─ src\BilingualLearning\        # mod 源码（业务代码，禁擅自大改）
│  ├─ ModEntry.cs                 # 入口：Harmony 补丁 + 事件订阅 + GMCM
│  ├─ WordOverlay.cs              # 对话查词覆盖层（命中/气泡/星标/桥接）
│  ├─ KnowledgeCardMenu.cs        # 方案A：F1 物品知识卡片菜单
│  ├─ BubbleRenderer.cs           # 气泡布局计算 + 绘制（卡片复用）
│  ├─ TooltipTextCapture.cs       # 重放 tooltip 时捕获 DrawString 文本
│  ├─ TextCapture.cs              # 每帧文本捕获注册表（SpriteText + 4 个 DrawString 重载）
│  ├─ SpriteTextWordLayout.cs     # SpriteText 词布局（对话/信件/日志共用）
│  ├─ ExtendedTextOverlay.cs      # 扩展文本面查词（信件/任务日志，只读气泡 + F10 调试）
│  ├─ DictionaryData.cs           # 词典加载（v2 四列）
│  ├─ Vocabulary.cs               # 生词本（立即写盘、按存档隔离）
│  ├─ VocabData.cs                # 生词本数据模型
│  ├─ Exporter.cs                 # 导出 CSV/Markdown/Anki
│  ├─ ChineseFont.cs              # FontStashSharp 中文字体
│  ├─ ModConfig.cs                # 配置模型
│  ├─ IApi.cs                     # GMCM 最小接口
│  ├─ manifest.json               # mod 清单（版本 0.5.0）
│  ├─ BilingualLearning.csproj    # net6.0 + ModBuildConfig
│  └─ assets\                     # dictionary.tsv / Font.ttf / bubble_*.png
├─ tools\                         # 辅助工具（build-dict 等）
├─ docs\                          # 全部文档（纯文本）
│  ├─ requirements\               # 需求规格说明、资源需求
│  ├─ planning\                   # 开发计划与节点控制
│  ├─ specs\                      # 词典数据格式（离线构建↔运行时数据契约）
│  ├─ guides\                     # 开发环境与构建说明
│  ├─ release\                    # Nexus 发布文案
│  └─ ai\                         # 本交接包
├─ assets\art\                    # 原始气泡设计稿（二进制）
├─ references\decompiled\         # 游戏反编译参考（gitignored，只读参考）
├─ dist\                          # 发布包（gitignore）
├─ README.md / CHANGELOG.md
```

### 入口文件

- **Mod 入口**：`src/BilingualLearning/ModEntry.cs` → `public override void Entry(IModHelper helper)`（L34）
- **清单**：`src/BilingualLearning/manifest.json`
- **游戏内行为入口**：SMAPI 加载 `BilingualLearning.dll` → `ModEntry.Entry`

### mod 加载器 / API 版本

- 游戏：Stardew Valley **1.6.15**（`D:\Program Files (x86)\Steam\steamapps\common\Stardew Valley`）
- SMAPI：**4.5.2**
- `manifest.json`：`MinimumApiVersion 4.0.0`、`MinimumGameVersion 1.6.15`、`UniqueID MarenQi.BilingualLearning`、`Version 0.5.0`、`UpdateKeys ["Nexus:55803"]`
- .NET：**6.0.428 SDK**，装在 `C:\Users\zhiqi\dotnet`（**不在 PATH**）
- 依赖：`Pathoschild.Stardew.ModBuildConfig 4.4.0`、`FontStashSharp.MonoGame 1.6.2`
- GMCM：软依赖（`Dependencies[IsRequired:false]`），运行时 `GetApi` 动态代理

### 构建命令

```powershell
$env:DOTNET_ROOT = 'C:\Users\zhiqi\dotnet'
C:\Users\zhiqi\dotnet\dotnet.exe build "D:\Project\星露谷双语mod\src\BilingualLearning\BilingualLearning.csproj" -c Debug
```

- 构建会自动：① 编译 DLL；② **部署到游戏 `Mods\BilingualLearning`**（游戏运行时 DLL 被锁 → 失败）；③ 在 `bin\Debug\` 生成 `BilingualLearning <版本>.zip`。
- 仅编译不部署：加 `-p:EnableModDeploy=false`。
- `CS9057` 警告无害。

### 运行命令

- Steam 启动选项设为：
  `"D:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\StardewModdingAPI.exe" %command%`
- 必须**游戏语言 = 英文**（F14：非英文自动停用查词）。

### 测试命令

- **无自动化测试**；验证均为**实机手动**（悬停查词、F9 导出、F11 开关、GMCM 配置）。
- 构建通过（0 error）是硬性前置。
- **`F10` 是调试键**（`DebugTextRectsKey`，仅 `config.json`，不进 GMCM）：在信件/任务日志等白名单菜单里画词矩形，并把捕获数据写进 SMAPI 日志（`[诊断]`）。默认关闭；关闭且 `ExtendLookup=Off` 时零捕获开销；只读，不写存档。说明见 `README.md`「测试键 F10 的用意」。

### 日志位置

- SMAPI 日志：`C:\Users\zhiqi\AppData\Roaming\StardewValley\ErrorLogs\SMAPI-latest.txt`
- 游戏内控制台：SMAPI 控制台窗口（mod 用 `Monitor.Log` 输出的中文日志，如「生词本加载完成」「知识卡片已打开」）。

## 架构约束

### 事件顺序

- `Entry`（mod 加载）→ 此时**存档未加载**，不能用 `ReadSaveData`。
- `GameLoop.SaveLoaded` → 存档加载完成，可 `ReadSaveData`/读生词本（`ModEntry.cs:45`）。
- `GameLoop.GameLaunched` → 所有 mod 初始化完成，可 `GetApi`（GMCM）（`ModEntry.cs:124`）。
- 每帧绘制：`Display.Rendering` → `TextCapture.BeginFrame()`（翻帧并设置捕获闸门）→ `Display.RenderedActiveMenu`（`ModEntry.cs:78`）→ 画对话/扩展气泡后**重绘光标**；`GameLoop.UpdateTicked`（`ModEntry.cs:121`）→ 恢复菜单。

### 生命周期

- 生词本：`SaveLoaded` 时 `vocabulary.Load()`；星标时立即 `WriteJsonFile`。
- 知识卡片：F1 打开 → 反射设 `_activeClickableMenu`（绕过 setter 的 Dispose）→ 关闭后 `UpdateTicked` 恢复 `pendingRestore`。
- 对话气泡：`OnHover` 维护 `hoveredWord`；`transitioning`/`isQuestion` 门槛；打字机完成才出气泡。

### hook 时机

- Harmony 补丁均在 `Entry` 注册（`ModEntry.cs:58/68/85/110`）：
  - `DialogueBox.performHoverAction`（postfix）→ `AfterHover`
  - `DialogueBox.receiveLeftClick`（prefix）→ `BeforeClick`
  - `IClickableMenu.drawHoverText`（postfix）→ 捕获 tooltip 参数
  - `SpriteBatch.DrawString`（3 个重载 postfix）→ 捕获文本位置
  - `SpriteBatch.DrawString`（4 个重载 postfix）→ 扩展文本面捕获（`TextCapture`）
  - `SpriteText.drawString`（postfix）→ 扩展文本面捕获（信件/任务日志）
- 扩展文本面白名单在 `ExtendedTextOverlay.AllowedMenus`；关闭 `ExtendLookup` 且未开 F10 调试时零捕获开销。
- **禁止**在 `Entry` 调 `ReadSaveData` 或 `GetApi`（见 BUG_PATTERNS）。

### 存档兼容

- 生词本：`Mods/BilingualLearning/data/<SaveFolderName>.json`（**独立文件，不随游戏存档周期**）。
- 词典：`assets/dictionary.tsv`（v2 四列，UTF-8 无 BOM）。
- 配置：`config.json`（`ToggleKey`/`ExportKey`/`ExportLocation`/`BubbleStyle`/`LookupKey`）。

### 配置格式

- `ModConfig`：`ToggleKey=F11`、`ExportKey=F9`、`ExportLocation=Documents|Desktop|Downloads|Mod folder`、`BubbleStyle=Default|Sandy's Notes`、`LookupKey=F1`、`ExtendLookup=Off|On`（扩展文本面）、`DebugTextRectsKey=F10`。
- 词典 v2：`word \t pos \t translation \t tag`（`DictionaryData.cs:62-64`）。

### 性能限制

- 词典 ~5.26 万词，启动一次性加载（`File.ReadLines`）。
- 字体子集化 2.08MB（FontStashSharp 动态加载）。
- 气泡布局每帧计算（轻量）；知识卡片重放 tooltip 每帧一次（原生绘制，开销可接受）。
- 扩展文本捕获：白名单菜单内每帧重建词矩形（信件/任务日志量级，轻量）；`ExtendLookup=Off` 且未开 F10 时零捕获。

## 禁止事项

1. **不要**在 `Entry` 调用 `ReadSaveData`、`GetApi`（会崩溃/报错）。
2. **不要**用 `WriteSaveData` 存生词本（只写内存，退出即丢）。
3. **不要**修改 `references/decompiled/` 下的反编译参考源码（gitignored，仅只读参考）。
4. **不要**提交 `bin/`、`obj/`、`dist/`、`tools/ecdict.csv`（见 `.gitignore`）。
5. **不要**用系统 `C:\Program Files\dotnet`（无 SDK）构建；必须用 `C:\Users\zhiqi\dotnet\dotnet.exe`。
6. **不要**在游戏运行时构建部署（DLL 被锁）；先关闭游戏。
7. **不要**假设 `Item` 有嵌套 `StackDrawType`（它在 `StardewValley.StackDrawType`）。
8. **不要**用中文做 GMCM 标签（英文游戏字体无中文字形 → 乱码）。
9. **不要**给气泡命中只认词矩形（必须含气泡+桥接，否则鼠标进不去气泡）。
10. **不要**给自定义菜单忘画光标（`drawMouse`）或忘 override `isWithinBounds`（否则气泡点不到）。
11. 修改游戏内部字段/方法前，**先用 Mono.Cecil 核对**（`smapi-internal\Mono.Cecil.dll`），不要凭空猜签名。
12. 源码与文档冲突时**以源码为准**；本文件记录的是当前 `b53598d` / v0.5.0 基线（代码内容等同 `0472442`，其后提交均为文档）。
13. **不要**用 PowerShell 的 `Get-Content`/`Set-Content` 批量改源码（PS 5.1 按 ANSI 解码 UTF-8，会损坏中文与字符串字面量）；一律用编辑工具 `edit`/`write`。
