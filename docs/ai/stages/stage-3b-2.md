# 阶段 3b-2：小镇任务板 / 日历（Billboard）悬停查词

> 3b 剩余：3b-3 `ShippingMenu` + `ItemListMenu` → 3b-4 `ShopMenu`（tooltip 路径，风险最高）。需求池见 `docs/requirements/需求规格说明.md` §10。

## 0. 元信息

| 字段 | 值 |
|---|---|
| 阶段 ID | `3b-2` |
| 分支 | `stage/3b-2-billboard` |
| 状态 | `MERGED` |
| 开始 / 结束 | 2026-11-23 / 2026-11-24 |
| 执行 Agent | `exec-3b2` |
| 评审 Agent | `review-3b2` |

## 1. Packet（主控填写）

```text
ROLE: Stage Executor
STAGE: 3b-2
BRANCH: stage/3b-2-billboard
REPO: D:\Project\星露谷双语mod
GOAL: 让小镇任务板（Billboard 的每日任务分支）的任务描述可悬停查词；日历分支与其它文本面行为不变。

IN_SCOPE（只允许改这些）:
- src/BilingualLearning/ExtendedTextOverlay.cs：AllowedMenus 增加 typeof(Billboard)（一行；类注释可同步补「3b-2」）。
- docs/ai/stages/stage-3b-2.md：填 §2 执行报告。
- docs/ai/DECISIONS.md / docs/ai/BUG_PATTERNS.md：仅当出现新长期决策/新可复现坑时追加（编号接续 19 / 22）。

OUT_OF_SCOPE（禁止）:
- 其它菜单（ShopMenu / ShippingMenu / ItemListMenu / JunimoNote / GameMenu 各页）
- tooltip 改造、点击与星标、GMCM 文案、气泡外观
- src/BilingualLearning/{ModEntry,TextCapture,SpriteTextWordLayout,WordOverlay,KnowledgeCardMenu,BubbleRenderer}.cs
- manifest.json / csproj / 版本号 / dist / bin / obj / references/decompiled
- 不新增依赖；不重构既有对话/卡片/订单板/木匠路径

ACCEPTANCE（自动可验）:
- dotnet build -p:EnableModDeploy=false → 0 error
- git diff --stat：文件 ⊆ IN_SCOPE；AllowedMenus 只新增 1 个类型且保留 LetterViewerMenu / QuestLog / SpecialOrdersBoard / CarpenterMenu
- ExtendLookup=Off 时闸门/捕获/布局逻辑零改动（IsEnabled、ShouldCapture、BuildSpriteFontHits、TextCapture 不在 diff）
- docs/ai/stages/stage-3b-2.md §2 已填

USER_TESTS（需用户在游戏内确认，5 条）:
1. 找一天小镇任务板有张贴任务（若显示 Nothing Posted，睡一觉再来）→ 打开公告板 → 悬停任务描述句子里的单词 → 出现词性+中文释义气泡；移入气泡不消失。
2. 长描述时把鼠标移到描述靠下的词 → 气泡不长期遮挡底部 Accept Quest 按钮；移到按钮上气泡消失、按钮可点。
3. 接下任务或换一天后重新打开公告板 → 旧气泡不残留；新描述可正常查词。
4. 打开日历（同类型的日历模式）→ 悬停 Spring / Summer 等季节名与 Year 文本 → 正常出气泡（或按 F13 无收录不出气泡），无错位/崩溃。
5. 关闭 Extended lookup → 公告板与日历都不出气泡；对话、F1 卡片、信件、任务日志、订单板、木匠菜单与此前一致。

CONSTRAINTS:
- 构建：$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src/BilingualLearning/BilingualLearning.csproj" -c Debug -p:EnableModDeploy=false
- 游戏运行中只编译不部署，并在报告注明「未部署」；主控会在验收前做带部署构建
- 只用编辑工具改源码；禁止 PowerShell Get-Content/Set-Content（BUG_PATTERNS 17）
- 改游戏内部字段/方法前先用 Mono.Cecil 核对（AGENTS 禁止事项 11）；本阶段证据已给，无需重复调查
- 只在 stage/3b-2-billboard 提交（1 个语义化提交）；不合并 master

EVIDENCE（已用 Cecil 核实，勿重复调查）:
- 类型：StardewValley.Menus.Billboard : IClickableMenu；ctor(Boolean dailyQuestBoard)：base(0,0,0,0,true)，width=(quest?338:301)*4（任务板 1352 / 日历 1204）、height=792，x/y 居中 → 4 字段有效，RebuildWordRects 裁剪可用。
- 打开时确为 Game1.activeClickableMenu（GameLocation.performAction IL_383a、Furniture.checkForAction IL_0295，均紧跟 setter；gameWindowSizeChanged 内重建）。
- draw() 文本：
  - IL_00e9 / IL_012e SpriteBatch.DrawString（dialogueFont）：季节名、UI:Billboard_Year（日历分支）。
  - IL_0510 今日高亮框；日历 1–28 日数字烘在 LooseSprites\Billboard 贴图里，无日号文本。
  - IL_0634 SpriteBatch.DrawString（dialogueFont）：UI:Billboard_NothingPosted（无任务时）。
  - IL_06ab Utility.drawTextWithShadow（dialogueFont）：questOfTheDay.questDescription 经 parseText(...,640) → 多行（\n/^），pos=(x+352, y+256)；被阴影重复绘制，现有 ≤4px 去重保留正文（BUG_PATTERNS 19）。
  - IL_07b9 Utility.drawTextWithShadow（dialogueFont）：UI:AcceptQuest（接受按钮文本）。
  - IL_08b1 SpriteText.drawString（像素字体）：奖励 "x1"，pos=Position+(936,596)；单行。
  - IL_0a50 drawHoverText（hoverText：生日/事件/任务提示）→ 该路径当前不接管，属预期。
- 无滚动、无裁剪、无 scissor；任务描述为多行对话字体，由 BuildSpriteFontHits 处理（\n/^ 分行 + 前缀 MeasureString）。
- 风险（已知）：长描述可能压到底部 Accept Quest 按钮（按钮 y = y+height-128）；文字重叠时气泡与按钮点击不冲突（气泡不拦截点击），但视觉上可能遮挡，属可接受。

START_POINTS:
- src/BilingualLearning/ExtendedTextOverlay.cs → AllowedMenus（L20-25）、IsAllowedMenu（L123）、RebuildWordRects（L270）、BuildSpriteFontHits（L342）
- docs/ai/BUG_PATTERNS.md 18~21；docs/ai/stages/stage-3b-2.md
- 参考同构阶段：docs/ai/stages/stage-3b-1.md

REPORT_FORMAT: SUBAGENT_PROMPTS.md §6（只回该结构）

STOP_CONDITIONS（必须 BLOCKED，不要猜）:
- 加入白名单后任务描述仍无捕获（F10 无矩形）→ BLOCKED，附 [诊断] 证据；不要新增 hook
- 需要改 ModEntry/TextCapture/布局类才能完成 → 先 BLOCKED 并回报方案
- 构建连续 2 次失败 → BLOCKED
```

## 2. 执行报告（执行 Agent 填写，schema 见 §6）

```text
STATUS: DONE
STAGE: 3b-2

SUMMARY: Billboard 加入 ExtendedTextOverlay.AllowedMenus（一行白名单 + 类注释同步 3b-2），复用既有 SpriteFont 捕获/多行布局/≤4px 去重/菜单矩形裁剪，无新增 hook、未改闸门。构建 0 error（EnableModDeploy=false，未部署）。日历分支与其它文本面行为不变，ExtendLookup=Off 时零行为变化。

FILES:
- src/BilingualLearning/ExtendedTextOverlay.cs — AllowedMenus 增 typeof(Billboard)；类注释补「阶段3b-2：小镇任务板/日历（Billboard）」
- docs/ai/stages/stage-3b-2.md — 本报告（§2）

VERIFY:
- build: 0 error（1 warning CS9057 已知无害）；命令 dotnet build -c Debug -p:EnableModDeploy=false；游戏运行中 → 只编译未部署
- checks: 源码 diff = 1 文件（3+/2-，语义改动仅 1 个类型条目 + 1 行注释），分支 diff = 2 文件（源码 + 本报告）；白名单保留 LetterViewerMenu/QuestLog/SpecialOrdersBoard/CarpenterMenu；diff 不含 IsAllowedMenu/ShouldCapture/IsEnabled/RebuildWordRects/BuildSpriteFontHits/TextCapture；未动 manifest/csproj/版本号/bin/obj/dist/references
- user_tests: ① 任务板有张贴任务时打开公告板 → 悬停任务描述里的单词出词性+中文释义气泡，移入气泡不消失 ② 长描述靠下的词 → 气泡不长期遮挡底部 Accept Quest；移到按钮上气泡消失、按钮可点 ③ 接下任务或换一天重开公告板 → 旧气泡不残留、新描述可查词 ④ 打开日历模式 → 悬停季节名/Year 文本正常出气泡（未收录词按 F13 不出），无错位/崩溃 ⑤ 关闭 Extended lookup → 公告板与日历都不出气泡；对话/F1 卡片/信件/任务日志/订单板/木匠菜单与之前一致

DECISIONS: none（复用 决策 18「白名单 + 只读气泡」，无新长期决策）

PITFALLS: none（本阶段未踩新坑；任务描述被 Utility.drawTextWithShadow 阴影重复绘制，由 19 号坑的 ≤4px 去重既定处理）

RISKS: 未实机验证（只编译未部署）。已知且可接受：长任务描述文字可能压到底部 Accept Quest 按钮，气泡为只读、不拦截点击（按钮仍可点），视觉遮挡属预期。日历分支与任务板共用同一类型，故白名单同时启用两者（日历正文少，仅季节名/Year/日号贴图；日号烘在贴图里无文本可查）。drawHoverText（生日/事件提示）本阶段按设计不接管；奖励 "x1" 走既有 SpriteText 路径被正常捕获，单词无查词价值，未专门验证。

DOCS: docs/ai/stages/stage-3b-2.md §2；未追加 DECISIONS/BUG_PATTERNS
NEXT_READY: yes — 可进评审 + 带部署构建 + 用户实机 5 条
```

## 3. 评审结论（评审 Agent 填写）

```text
VERDICT: PASS | FAIL
BLOCKING:
NON_BLOCKING:
EVIDENCE:
```

## 4. 验收结论（主控填写）

- 越界检查：`git diff --stat master...stage/3b-2-billboard` = 源码 1 文件（`ExtendedTextOverlay.cs` 5+/2-）+ 阶段报告 1 文件；无 bin/obj/dist、无 manifest/csproj/版本号。
- 构建：`dotnet build -c Debug`（含部署）与 `-p:EnableModDeploy=false` 均 0 error；主控已部署，`bin\Debug` 与 `Mods\BilingualLearning` DLL SHA256 一致。
- 用户实机：**通过**（公告板任务描述可查词、长描述遮挡可接受、日历季节名可查、Off 无行为）。
- 用户反馈（与本次无关、已记录）：日历里悬停 `Fall` 显示「落下，爱上」而非「秋天」——同形多义词义项选择问题，见 `docs/ai/BUG_PATTERNS.md` #22 与 `docs/requirements/需求规格说明.md` §10-3。
- 合并提交：`73db53f`（--no-ff，父 `ee36afd`）。
- 后续动作：阶段 3b-3（`ShippingMenu` + `ItemListMenu`）；随后 3b-4 `ShopMenu`（tooltip 路径）。

## 5. 经验沉淀（可选）

- 新增决策：`DECISIONS #__`
- 新增坑：`BUG_PATTERNS #__`
- 未覆盖场景：
