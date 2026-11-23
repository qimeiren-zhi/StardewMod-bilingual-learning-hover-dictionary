# 当前交接（Bilingual Learning / 星露谷双语学习 Mod）

> 更新时间：2026-11-24（凌晨）。基线：`master` @ `8fda139`（阶段 3a、3b-1~3b-4 全部合并），版本 `0.5.0`。
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
- 目标状态：**阶段 3b（菜单长尾）全部完成**（3b-1 木匠、3b-2 任务板/日历、3b-3 发货箱/丢物、3b-4 商店）；下一步 3c（`JunimoNote` / 精通追踪），随后 4 世界层、5 交互统一。

## 阶段 3b-4（2026-11-24 凌晨，已合并）

- 范围：`ShopMenu` 加入 `AllowedMenus`（一行）；正文像素字体（物品名/价格/以物易物/缺货提示）复用现有管线；**tooltip 不接管**（DECISIONS #19：物品描述走 F1 知识卡片）。
- 3 轮修复（均由用户实机发现）：① F1 卡片错成店长问候语 → `AfterDrawHoverText` 忽略非 `Item` tooltip；② Joja 气泡盖光标 → `DrawCursor` 用 `drawMouse(b, ignore_transparency: true)`；③ 气泡点击穿透误购 → `ShopMenu.receiveLeftClick`/`receiveRightClick` prefix + `IsPointerOnBubble`。
- 结果：构建 0 error（含部署）；三份评审 PASS（`review-3b-4*.md`）；用户**通过**；合并 `8fda139`。
- 记录：`BUG_PATTERNS.md` #23（F1 缓存被问候语覆盖）、#24（气泡点击穿透）；需求 §10-4（商店 tooltip 随光标抢焦点，可接受）。


## 阶段 3b-3（2026-11-24 凌晨，已合并）

- 范围：`ShippingMenu`（发货箱日终结算）与 `ItemListMenu`（丢物列表）加入 `AllowedMenus`（两行）；正文全为像素字体 `SpriteText.drawString`，复用现有布局/裁剪/命中，**无新增 hook**。
- 结果：构建 0 error（含部署，DLL 与 bin 一致）；评审 PASS（`docs/ai/stages/review-3b-3.md`）；用户**全部通过**。
- 合并：`c5901a8`；阶段记录 `docs/ai/stages/stage-3b-3.md`。
- 测试备注：`debug eventScript .../showItemsLost/end` 只回「no output」、未直接弹菜单；用户改用人工触发事件验证成功。后续复测优先 `debug ebi PlayerKilled`（扣钱掉物，需测试存档）。


## 阶段 3b-2（2026-11-24 凌晨，已合并）

- 范围：`Billboard`（任务板 + 日历共用类型）加入 `ExtendedTextOverlay.AllowedMenus`（一行）；任务描述（dialogueFont、`parseText(...,640)` 多行）复用现有 SpriteFont 管线，**无新增 hook**。
- 结果：构建 0 error（含部署，DLL 与 bin 一致）；评审 PASS（`docs/ai/stages/review-3b-2.md`）；用户实机通过（公告板描述可查、长描述遮挡可接受、日历季节名可查、Off 无行为）。
- 合并：`d6b5ac5`；阶段记录 `docs/ai/stages/stage-3b-2.md`。
- 用户反馈（与本次无关、已记录）：日历悬停 `Fall` 显示「落下，爱上」而非「秋天」→ 同形多义词义项问题，见 `BUG_PATTERNS.md` #22 与需求 §10-3。

## 阶段 3b-1（2026-11-23 深夜，已合并）

- 范围：`CarpenterMenu` 加入 `ExtendedTextOverlay.AllowedMenus`（一行）；蓝图描述（dialogueFont、`parseText` 多行）、建造费、配料名/数量复用现有 SpriteFont 管线，**无新增 hook**。
- 结果：构建 0 error（含部署，DLL 与 bin 一致）；评审 PASS（`docs/ai/stages/review-3b-1.md`）；用户实机通过。
- 合并：`5987765`；阶段记录 `docs/ai/stages/stage-3b-1.md`。
- 用户反馈（已转需求，见 `docs/requirements/需求规格说明.md` §10）：① 连字符复合词（`coop-dwelling`）不命中，属既有分词限制；② 蓝图建筑标题（`Coop`/`Barn`）需可查词，走 scroll 居中文本，需专门布局；③ 悬停描述时切换蓝图：**只有手柄 LT/RT**（`receiveGamePadButton` IL_0010/IL_0018），键盘/滚轮无效，鼠标需点左下角箭头（`backButton`/`forwardButton`）；放置模式 `onFarm` 下全部短路。


## 本轮成果（相对 `b53598d`）

| 类别 | 内容 |
|---|---|
| 新功能 | 信件 / 秘密纸条 / 日志残页 / 任务日志 悬停查词（GMCM `Extended lookup`，默认 `Off`） |
| 基础设施 | 每帧文本捕获注册表（双缓冲、去重、来源标记、零开销闸门）；共享词布局 `SpriteTextWordLayout` |
| 修复 | 矩形逐词右漂（前缀 `MeasureString`）、阴影被优先捕获（保留正文）、`^` 换行、`width=-1` 换行原点 |
| 调试工具 | `F10` 词矩形可视化 + `[诊断]` 日志（不进 GMCM，默认关） |
| 文档 | README（含「测试键 F10 的用意」）、CHANGELOG、需求 §9、计划 §8、AGENTS、DECISIONS 16~18、BUG_PATTERNS 18~21 |

## 阶段 3a（2026-11-23 深夜，已合并）

- 范围：`SpecialOrdersBoard` 加入 `ExtendedTextOverlay.AllowedMenus`（一行）；订单正文为 `Utility.drawTextWithShadow` 的 9 参 `DrawString`，多行 `\n`/缩字 `scale` 复用现有 SpriteFont 管线，**无新增 hook**。
- 结果：构建 0 error（含部署）；评审 PASS（`docs/ai/stages/review-3a.md`）；用户实机通过（订单板可查词、多行/缩字贴合、切换订单不残留、Off 全局不查词为设计行为）。
- 合并：`ceb0a46`；阶段记录 `docs/ai/stages/stage-3a.md`。
- 遗留（非阻断）：GMCM `Extended lookup` 描述与 `ModEntry.cs:308` 注释仍只写 letters/quest log，未含订单板，**3b 时同步**。

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
6. **剩余文本面未接**：`JunimoNote` / 精通追踪（3c）、世界层（阶段 4）；订单板、木匠、任务板/日历、发货箱结算/丢物列表、商店正文均已接入（3b 收官）。
7. **`dist/` 发布包未重建**：manifest 已改，发布前必须重新打包。
8. **连字符复合词不命中**（`coop-dwelling`）：既有分词限制，非 3b-1 回归；见 `docs/requirements/需求规格说明.md` §10-1。
9. **卷轴居中标题不可查**：木匠蓝图标题（`Coop`/`Barn`）、订单板 `ChooseOne` 横幅等走 `drawStringWithScrollCenteredAt`，被 `drawBGScroll != -1` 闸门跳过；见 §10-2。
10. **木匠菜单换蓝图无键盘/滚轮**（游戏原生限制）：目录视图下只有手柄 LT/RT（`receiveGamePadButton` IL_0010/IL_0018）或鼠标点左下角箭头（`backButton`/`forwardButton`，`receiveLeftClick` IL_00A9/IL_00FD）；`receiveKeyPress` 不含任何换蓝图键、`receiveScrollWheelAction` 为空。放置模式（`onFarm`）下全部短路。若要让键盘也可换，需 mod 侧反射 `SetNewActiveBlueprint`，未做。
11. **同形多义词显示错误义项**：日历 `Fall` 显示「落下，爱上」而非「秋天」（`saw` 同类）。既有词典按拼写精确匹配、无 POS/语境消歧；见 `BUG_PATTERNS.md` #22、需求 §10-3。
12. **扩展面点击吞掉只做了商店**（`ShopMenu` 左/右键）：信件/任务日志/订单板/木匠/任务板的同类穿透仍在（阶段 5 统一，见 `BUG_PATTERNS.md` #24）。
13. **白名单菜单内的 tooltip 文本会被通用 `DrawString` 捕获**（商店/木匠/任务板/任务日志同源）：正常位置一般不触发，靠近屏幕边缘 tooltip 翻转到光标处时可能误出气泡；属架构层遗留，后续统一处理（可在 `drawHoverText` 处抑制捕获）。

## 下一步 TODO

> 协作方式：2026-11-23 起建议用**开发模式**（主控 Agent + 阶段 SubAgent）推进，见 `docs/ai/DEV_MODE.md`、`docs/ai/MAIN_AGENT_PROMPT.md`、`docs/ai/SUBAGENT_PROMPTS.md`。代码阶段严格串行，只有只读研究/评审可并行。

1. **【已完成】** 本轮已提交：`e818b9f`（代码）+ `b2fae14`（文档）；`git status` 干净。
2. **阶段 3c**：`JunimoNote`（收集包）与精通追踪——混合 SpriteText + `DrawString`，需先只读核实现有捕获/裁剪是否够用。随后阶段 4 世界层、阶段 5 交互统一（其它扩展面点击吞掉 + 星标）。
3. **需求池（`docs/requirements/需求规格说明.md` §10）**：连字符复合词分词（中）、卷轴居中标题可查词（中，影响木匠标题/订单板横幅）、同形多义词义项选择（低，`Fall`/`saw`）。
4. **阶段 4**：世界层（`NPC.drawAboveAlwaysFrontLayer`、`*.drawAboveAlwaysFrontLayer`），需要世界→屏幕坐标换算。
5. **阶段 5**：扩展面星标收藏/点击吞掉（谨慎，避免与菜单交互冲突）。
6. **发布前**：重建 `dist/BilingualLearning 0.5.0.zip`（或升 0.6.0，待定），补 GitHub `UpdateKeys`，Nexus 勾 `AI Assisted`。

## 待确认问题

1. 【待确认】本轮扩展功能是否升版本到 `0.6.0`（当前 manifest/csproj 仍 0.5.0，dist 未重建）。
2. 【待确认】是否把「单字母矩形右留白」收紧到字形边缘（会略减点击容差）。
3. 【已定】阶段 3 顺序：订单板（3a 已完成）→ 菜单长尾（3b）→ `JunimoNote`（3c）。
4. 【待确认】GitHub 仓库地址（`UpdateKeys` 目前只有 `Nexus:55803`）。
5. 【待确认】知识卡片图标是否要包进面板内（旧问题，未做）。
