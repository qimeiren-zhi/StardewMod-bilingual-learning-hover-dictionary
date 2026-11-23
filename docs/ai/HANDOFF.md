# 当前交接（Bilingual Learning / 星露谷双语学习 Mod）

> 生成时间：2026-11-23。基线：`master` @ `0472442`，版本 `0.5.0`，标签 `v0.5.0`。
> 新会话请先读 `AGENTS.md`，再读本文件，再读 `docs/ai/BUG_PATTERNS.md` 与 `docs/ai/DECISIONS.md`。

## 当前目标

- **项目已回滚到 `0472442`**：其他 Agent 的「阶段2」提交（`807c988`）效果不理想，已 `git reset --hard 0472442`，其内容保留在分支 `backup/other-agent-807c988`。
- 当前已实现并实机验证通过的功能：对话查词、生词本、导出、GMCM、气泡双风格、方案 A（F1 物品知识卡片：图标 + 全文字查词 + 星标收藏 + 桥接连通）。
- 目标状态：**可发布基线**（构建 0 error、发布包 `dist/BilingualLearning 0.5.0.zip` 就绪）。

## 非目标

- **不重做**其他 Agent 的「阶段2（扩展 SpriteText 文本面）」，除非用户明确要求并从 `backup/other-agent-807c988` 取回。
- **不擅自修改业务代码**：新一轮任务开始前先只读扫描、先报告缺口与假设，等用户确认。
- 不在本轮处理「选项文字查词」（问题对话框选项，仍待定）。

## 验收标准

1. **构建**：`dotnet build` → 0 error（`CS9057` 警告可忽略）。
2. **实机**（游戏英文、关闭后重建部署）：
   - 悬停对话单词 → 出「词性+中文释义」气泡；鼠标移入气泡不消失；
   - 点星标收藏 → 退出重进仍在（`Mods/BilingualLearning/data/<存档id>.json`）；
   - F9 导出 → `文档\StardewValley\BilingualLearning\` 有 CSV/MD/Anki；
   - F1 悬停物品 → 知识卡片（图标 + 类别/增益/价格/描述齐全、可悬停查词、星标可点）；
   - GMCM 配置页可见（Default/Sandy's Notes、快捷键、导出位置）。
3. **日志**：`C:\Users\zhiqi\AppData\Roaming\StardewValley\ErrorLogs\SMAPI-latest.txt` 无异常堆栈。

## 已改文件清单

> 本轮（回滚后的基线）包含整个会话的改动。风险点标注。

| 文件 | 改动原因 | 风险点 |
|---|---|---|
| `src/BilingualLearning/ModEntry.cs` | 入口：Harmony 补丁、SaveLoaded 读生词本、GameLaunched 注册 GMCM、F11/F9/F1 快捷键、知识卡片捕获、UpdateTicked 恢复菜单 | 事件时机（Entry 不能调 ReadSaveData/GetApi）；反射 `_activeClickableMenu` 依赖游戏私有字段 |
| `src/BilingualLearning/WordOverlay.cs` | 对话查词覆盖层：命中、气泡、星标、名称过滤、连通/桥接、`NormalizeWord` 改 internal | `BuildWordHits` 复刻 SpriteText 布局，随游戏字体比例变动 |
| `src/BilingualLearning/KnowledgeCardMenu.cs` | 方案A 菜单：重放原生 tooltip + 图标 + 全文字查词 + 星标 + 桥接 + `isWithinBounds` | 重放 `drawHoverText` 依赖参数索引（0/13/14）；描述坐标靠捕获 |
| `src/BilingualLearning/TooltipTextCapture.cs` | 重放期间捕获 `DrawString` 文本/位置 | 依赖 3 个重载 hook；按位置去重 |
| `src/BilingualLearning/BubbleRenderer.cs` | 气泡布局/绘制抽离（卡片复用） | 气泡风格/文字色与对话一致 |
| `src/BilingualLearning/DictionaryData.cs` | 词典 v2 四列解析（word/pos/translation/tag） | 列顺序变更会静默错位 |
| `src/BilingualLearning/Vocabulary.cs` | 生词本改 `WriteJsonFile/ReadJsonFile`（立即落盘） | 路径 `data/<SaveFolderName>.json` 与 UniqueID 无关 |
| `src/BilingualLearning/VocabData.cs` | 生词本数据模型（Word/Pos/Translation/Sentence/FirstSeen） | JSON 形状变更需迁移 |
| `src/BilingualLearning/Exporter.cs` | 导出 CSV/Markdown/Anki + 位置解析 | 路径重定向（Downloads 仅 best-effort） |
| `src/BilingualLearning/ModConfig.cs` | 配置模型（ToggleKey/ExportKey/ExportLocation/BubbleStyle/LookupKey） | 新字段默认值 |
| `src/BilingualLearning/IApi.cs` | GMCM 最小接口（RegisterModConfig/SetDefaultIngameOptinValue/RegisterSimpleOption/RegisterChoiceOption） | 与 GMCM 1.16 签名需一致 |
| `src/BilingualLearning/ChineseFont.cs` | FontStashSharp 加载中文字体（字号 16*FontPixelZoom=48f） | 子集字体缺字会显示豆腐块 |
| `src/BilingualLearning/manifest.json` | 版本 0.5.0、作者 MarenQi、UniqueID `MarenQi.BilingualLearning`、GMCM 软依赖 | UniqueID 发布后不可改 |
| `src/BilingualLearning/BilingualLearning.csproj` | 版本 0.5.0、BundleExtraAssemblies=ThirdParty、bubble PNG Content | 游戏运行时部署失败（DLL 锁） |
| `src/BilingualLearning/assets/dictionary.tsv` | v2 词典（5.26 万词含变形） | 53% 空 tag（已无等级过滤，不影响） |
| `src/BilingualLearning/assets/Font.ttf` | 子集字体（15.6MB→2.08MB） | 用字表变化需重新子集化 |
| `src/BilingualLearning/assets/bubble_default.png` / `bubble_sandy.png` | 双气泡风格 | 只读属性需清除 |
| `docs/art/*.png` | 原始设计稿归档 | — |
| `README.md` / `CHANGELOG.md` / `docs/release/Nexus发布文案.md` | 发布文档、AI 声明 | — |
| `docs/planning/开发计划与节点控制.md` | 进度 v0.5、§9 版本记录 | — |
| `docs/guides/开发环境与构建说明.md` | UniqueID 更新 | — |
| `.gitignore` | 忽略 dist/ | — |

## 失败尝试/禁止事项

> 详见 `BUG_PATTERNS.md` 与 `AGENTS.md`「禁止事项」。以下为**已试过无效/已回滚**的方案：

1. **等级过滤（P5 曾实现，已移除）**：按 ECDICT `tag` 过滤，导致 **52.8% 空 tag 词**被误杀（`channeled`/`pelican` 无气泡）。结论：悬停是主动行为，无需过滤；且 ECDICT `tag` 是考纲等级 ≠ 常见度。**不要重新加回**（除非用户明确要求并重新设计数据依据）。
2. **`WriteSaveData` 存生词本（已改）**：只写 `Game1.CustomData` 内存，随游戏存档周期，退出即丢。**不要用**。
3. **Harmony postfix on `DialogueBox.draw` 抬升 LayerDepth 修光标（无效）**：光标仍被压。最终方案 = `RenderedActiveMenu` 画气泡后 `box.drawMouse`。
4. **中文 GMCM 标签（已改英文）**：英文游戏字体无中文字形 → 乱码。
5. **只 hook 4 参数字符串 `DrawString`（已补全）**：类别/增益/描述走 9 参数与 StringBuilder 重载，只 hook 一个会漏。
6. **其他 Agent 的「阶段2」扩展（`807c988`，已回滚）**：效果不理想，用户要求回滚。**不要**直接复用，除非用户明确要。

## 下一步 TODO

1. **确认基线**：新会话先只读核对 `git log -1` == `0472442`、`git status` 干净、`dist/` 发布包存在。
2. **发布准备收尾**（若用户要发布）：
   - Nexus Tag 勾选 `AI Assisted`；
   - 填 `UpdateKeys`（GitHub / Nexus）；
   - 上传 `dist/BilingualLearning 0.5.0.zip` + 3 张截图。
3. **若要继续功能**（按优先级）：
   - 方案 A 的「其他 Agent 阶段2」是否采纳：先 diff `0472442..backup/other-agent-807c988`，评估后决定；
   - §8 阶段 2（hook `SpriteText.drawString` → HUD/气泡/电视/事件/信件）；
   - 「选项文字查词」（问题对话框选项，待定）；
   - 卡片内星标状态即时刷新（当前依赖下一帧绘制）。

## 待确认问题

1. 【待确认】其他 Agent 的「阶段2」是否要部分采纳？还是完全弃用？（分支 `backup/other-agent-807c988`）
2. 【待确认】发布版本保持 `0.5.0`，还是要因方案 A 升级为 `0.6.0`？
3. 【待确认】GitHub 仓库地址 / Nexus mod ID 是否已建？`UpdateKeys` 待填。
4. 【待确认】`MinimumGameVersion` 是否要从 `1.6.0` 收紧到实测的 `1.6.15`（用了游戏内部字段）？
5. 【待确认】知识卡片图标是否要「包进面板内」（当前在面板外左侧，需自绘面板 + 透明化原生框，工作量更大）。
