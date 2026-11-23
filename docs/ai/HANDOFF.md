# 当前交接（Bilingual Learning / 星露谷双语学习 Mod）

> 更新时间：2026-11-23（晚）。基线：`master` @ `b2fae14`（本轮代码提交 `e818b9f` + 文档提交 `b2fae14`），版本 `0.5.0`。
> `dist/` 未重建；工作区干净。
> 新会话顺序：`AGENTS.md` → 本文件 → `docs/ai/BUG_PATTERNS.md` → `docs/ai/DECISIONS.md` → `docs/ai/CURRENT_TASK.md`。
> 本次会话的完整调试过程与证据见 `docs/ai/HANDOVER_2026-11-23.md`；协作模式见 `docs/ai/DEV_MODE.md`。

## 当前目标

- 在 0.5.0 可发布基线上，把「悬浮查词」从 `DialogueBox` 扩展到更多文本面；按文本面切片交付，不重写既有对话/卡片路径。
- **2026-11-23 已完成并实机验证**：
  - 阶段 0：统一文本捕获基础设施（`SpriteText.drawString` + 全部 4 个 `SpriteBatch.DrawString` 重载）；
  - 阶段 1：**信件 / 秘密纸条 / 日志残页**悬停查词；
  - 阶段 2：**任务日志（QuestLog）**悬停查词 + 滚动/翻页不残留 + 命中裁剪；
  - 对齐问题全部修复（见「本轮修复」），用户确认「结果完美」。
- 目标状态：扩展功能已可用（默认关闭、GMCM 开启），下一步是阶段 3（特殊订单板/菜单长尾）。

## 本轮成果（相对 `b53598d`）

| 类别 | 内容 |
|---|---|
| 新功能 | 信件 / 秘密纸条 / 日志残页 / 任务日志 悬停查词（GMCM `Extended lookup`，默认 `Off`） |
| 基础设施 | 每帧文本捕获注册表（双缓冲、去重、来源标记、零开销闸门）；共享词布局 `SpriteTextWordLayout` |
| 修复 | 矩形逐词右漂（前缀 `MeasureString`）、阴影被优先捕获（保留正文）、`^` 换行、`width=-1` 换行原点 |
| 调试工具 | `F10` 词矩形可视化 + `[诊断]` 日志（不进 GMCM，默认关） |
| 文档 | README（含「测试键 F10 的用意」）、CHANGELOG、需求 §9、计划 §8、AGENTS、DECISIONS 16~18、BUG_PATTERNS 18~21 |

## 调试键 `F10` 的用意（重要）

- **定位**：开发/对位调试键，`config.json` 的 `DebugTextRectsKey`，**不注册进 GMCM**。
- **行为**：开启后在白名单菜单（信件、任务日志）里把每个命中词矩形画成半透明绿色；同时把捕获行与前 30 个词矩形写入 SMAPI 日志（`[诊断]` 前缀，含 `rect/wrap/originX/cp/scale/font/space` 等）。
- **为什么隐藏**：普通玩家不需要矩形叠加；放进 GMCM 易误触。需要时手改 `config.json`。
- **开销与安全**：关闭且 `ExtendLookup=Off` 时 `TextCapture.Enabled=false`，捕获逻辑完全不执行；只读绘制，不改游戏状态、不写存档、不新增文件。
- **日志位置**：SMAPI 控制台，或 `%APPDATA%\StardewValley\ErrorLogs\SMAPI-latest.txt`（由 SMAPI 决定，mod 不额外落盘）。
- **常用流程**：GMCM 开 `Extended lookup` → 打开信件/任务日志 → `F10` 看矩形 → 不对齐时把 `[诊断]` 段发给开发者 → 再按 `F10` 关闭。

> `F11`（查词总开关）、`F9`（导出）、`F1`（物品知识卡片）是正式功能键，不属于测试键。

## 非目标

- **不整包复用**其他 Agent 的「阶段2」（`backup/other-agent-807c988`）；只可参考其布局算法/分层思路。
- **不按绘制 API 分阶段**：同一菜单常混用 `SpriteText` 与 `SpriteFont`，按 API 切会重复实现（见计划 §8.2）。
- 扩展文本面**暂不接管点击/星标**：星标收藏留到阶段 5 统一做。
- 不在本轮处理「问题对话框选项文字查词」（仍待定）。

## 验收标准

1. **构建**：`dotnet build` → 0 error（`CS9057` 可忽略）。
2. **实机（游戏英文）**：
   - 对话悬停出气泡、移入气泡不消失；
   - `F1` 物品卡片：图标/类别/增益/价格/描述齐全，可悬停查词、星标可点；
   - `F9` 导出 CSV/MD/Anki；
   - `Extended lookup=On` 时：信件正文与任务日志可悬停查词；滚动/翻页后气泡不残留；`F10` 矩形与文字贴合（误差约 3px 字形左留白）；
   - `Extended lookup=Off` 时：信件/任务日志不查词，对话与卡片不受影响。
3. **日志**：SMAPI 日志无异常堆栈。

## 已改文件清单（本轮）

### 新增

| 文件 | 作用 | 风险/限制 |
|---|---|---|
| `src/BilingualLearning/TextCapture.cs` | 每帧文本注册表：双缓冲、按「文本+字体+位置≤4px」去重、`Enabled/Suppressed` 闸门、记录 `LineOriginX` | 阴影/正文去重依赖「后到者为正文」 | 
| `src/BilingualLearning/SpriteTextWordLayout.cs` | `SpriteText.drawString` 词布局（对话/信件/日志共用），支持 `characterPosition` 与「首行起点/换行原点」 | 与游戏内部 4px 钳制差异已建模 |
| `src/BilingualLearning/ExtendedTextOverlay.cs` | 扩展文本面：白名单、菜单区域裁剪、翻页校验、只读气泡、F10 矩形/诊断 | 仅 `LetterViewerMenu`、`QuestLog`；世界层/订单板未接 |

### 修改

| 文件 | 改动 | 风险点 |
|---|---|---|
| `src/BilingualLearning/ModEntry.cs` | hook `SpriteText.drawString` + 4 个 `DrawString` 重载；`Display.Rendering` 驱动捕获；`F10`；GMCM `Extended lookup`；`OnRenderedActiveMenu` 画扩展气泡+光标 | hook 找不到时只 Warn |
| `src/BilingualLearning/ModConfig.cs` | 新增 `ExtendLookup`（Off/On）、`DebugTextRectsKey`（F10） | GMCM 重置会替换 config 实例（既有问题） |
| `src/BilingualLearning/BubbleRenderer.cs` | 新增 `showStar` 可选参数 | 现有调用行为不变 |
| `src/BilingualLearning/WordOverlay.cs` | 词布局改为调用 `SpriteTextWordLayout`（逻辑等价） | 对话路径未改行为，已实机回归 |
| `src/BilingualLearning/manifest.json` | `MinimumGameVersion` 1.6.15、`UpdateKeys ["Nexus:55803"]` | `dist/` 内 zip 仍是旧 manifest |

### 文档

`README.md`（新增扩展功能 + F10 说明 + 配置表）、`CHANGELOG.md`（Issue 级条目）、`AGENTS.md`、`docs/planning/开发计划与节点控制.md` §8、`docs/requirements/需求规格说明.md` §9、`docs/ai/{HANDOFF,CURRENT_TASK,DECISIONS,BUG_PATTERNS,START_HERE}.md`、`docs/ai/HANDOVER_2026-11-23.md`（新增）。

## 架构要点（新增部分）

- **捕获**：`Display.Rendering` 每帧 `TextCapture.BeginFrame()` 并把上一帧结果翻为 `LastFrame`；绘制期间 hook 写入当前帧；命中查询使用上一帧。
- **闸门**：`TextCapture.Enabled = ExtendLookup==On || DebugRects`，且 `ShouldCapture()` 还要求当前菜单在白名单、是英文、非卡片重放。
- **布局**：`SpriteText` 用 `SpriteTextWordLayout`（逐字符复刻）；`SpriteFont` 用「行内前缀 `MeasureString`」定位（不能用词宽+空格宽累加，见 BUG_PATTERNS 18）。
- **命中**：词矩形先裁剪到菜单矩形；鼠标在「词 ∪ 气泡 ∪ 桥接」内保持；每帧校验该位置仍是原词（防翻页/滚动残留）。
- **绘制**：`OnRenderedActiveMenu` 末尾画气泡并重绘菜单光标；扩展面不画星标。

## 失败尝试/禁止事项

1. **等级过滤（已移除）**：空 tag 误杀常见词，**不要加回**。
2. **`WriteSaveData` 存生词本（已改）**：退出即丢。
3. **`DialogueBox.draw` 抬 LayerDepth 修光标（无效）**：用 `RenderedActiveMenu` + `drawMouse`。
4. **中文 GMCM 标签（已改英文）**。
5. **只 hook 1 个 `DrawString` 重载（已补全 4 个）**。
6. **备份分支整包（`807c988`，已回滚）**：不要直接复用。
7. **PowerShell `Get-Content/Set-Content` 改源码**：会按 ANSI 解码损坏 UTF-8（本次已踩坑并修复），只用编辑工具。

## 已知问题/限制

1. **滚动中的任务列表未实测**：测试存档任务太少；代码已裁剪到菜单区域并每帧校验原词，滚动后不应残留，待有长列表时验证。
2. **无翻页长信可测**：翻页残留用「每帧校验原词」通用修复，逻辑上覆盖，未实机遇到。
3. **扩展面不支持星标/点击**（阶段 5）。
4. **单字母词矩形略宽**：宽度用 `MeasureString(word)`（含右留白），约多 4px；点击/悬停仍准确，视觉上盒子比字形宽一点（可接受，若要求像素级可再收紧）。
5. **世界层未接**（头顶气泡、节日/矿洞横幅）。
6. **SpecialOrdersBoard/商店/日历等**未接（阶段 3）。
7. **`dist/` 发布包未重建**：manifest 已改，发布前必须重新打包。

## 下一步 TODO

> 协作方式：2026-11-23 起建议用**开发模式**（主控 Agent + 阶段 SubAgent）推进，见 `docs/ai/DEV_MODE.md`、`docs/ai/MAIN_AGENT_PROMPT.md`、`docs/ai/SUBAGENT_PROMPTS.md`。代码阶段严格串行，只有只读研究/评审可并行。

1. **【已完成】** 本轮已提交：`e818b9f`（代码）+ `b2fae14`（文档）；`git status` 干净。
2. **阶段 3**：`SpecialOrdersBoard` → 商店/木匠/日历/Billboard/发货箱/ItemList → `JunimoNote`；复用 `TextCapture` + 白名单，按面验收。
3. **阶段 4**：世界层（`NPC.drawAboveAlwaysFrontLayer`、`*.drawAboveAlwaysFrontLayer`），需要世界→屏幕坐标换算。
4. **阶段 5**：扩展面星标收藏/点击吞掉（谨慎，避免与菜单交互冲突）。
5. **发布前**：重建 `dist/BilingualLearning 0.5.0.zip`（或升 0.6.0，待定），补 GitHub `UpdateKeys`，Nexus 勾 `AI Assisted`。

## 待确认问题

1. 【待确认】本轮扩展功能是否升版本到 `0.6.0`（当前 manifest/csproj 仍 0.5.0，dist 未重建）。
2. 【待确认】是否把「单字母矩形右留白」收紧到字形边缘（会略减点击容差）。
3. 【待确认】阶段 3 顺序：先订单板还是先商店/日历等短文本菜单。
4. 【待确认】GitHub 仓库地址（`UpdateKeys` 目前只有 `Nexus:55803`）。
5. 【待确认】知识卡片图标是否要包进面板内（旧问题，未做）。
