# 阶段 3a：特殊订单板（SpecialOrdersBoard）悬停查词

## 0. 元信息

| 字段 | 值 |
|---|---|
| 阶段 ID | `3a` |
| 分支 | `stage/3a-orders-board` |
| 状态 | `MERGED` |
| 开始 / 结束 | 2026-11-23 / 2026-11-23 |
| 执行 Agent | `exec-3a` |
| 评审 Agent | `review-3a` |

## 1. Packet（主控填写）

```text
ROLE: Stage Executor
STAGE: 3a
BRANCH: stage/3a-orders-board
REPO: D:\Project\星露谷双语mod
GOAL: 让「特殊订单板」的订单标题/描述/剩余天数/奖励文本可悬停查词；其它文本面行为不变。

IN_SCOPE（只允许改这些）:
- src/BilingualLearning/ExtendedTextOverlay.cs：
  - 必做：AllowedMenus 增加 typeof(SpecialOrdersBoard)（一行）。
  - 仅当实机证据不足时：最小限度调整该菜单的裁剪/过滤；不要重写 BuildSpriteFontHits/SpriteTextWordLayout。
- docs/ai/DECISIONS.md / docs/ai/BUG_PATTERNS.md：仅当出现新长期决策/新可复现坑时追加（编号接续 19 / 22）。
- docs/ai/stages/stage-3a.md：填 §2 执行报告。

OUT_OF_SCOPE（禁止）:
- 其它文本面（商店/木匠/日历/Billboard/发货箱/ItemList/JunimoNote/世界层）
- 点击与星标、气泡外观、GMCM 选项
- src/BilingualLearning/{ModEntry,TextCapture,SpriteTextWordLayout,WordOverlay,KnowledgeCardMenu,BubbleRenderer}.cs
- manifest.json / csproj / 版本号 / dist / bin / obj / references/decompiled
- 不新增 NuGet 依赖；不重构既有对话/卡片路径

ACCEPTANCE（自动可验）:
- dotnet build -p:EnableModDeploy=false → 0 error
- git diff --stat：文件 ⊆ IN_SCOPE；AllowedMenus 只新增 1 个类型
- ExtendLookup=Off 时 IsEnabled/ShouldCapture 闸门不变 → 零行为变化（不新增每帧开销）
- LetterViewerMenu / QuestLog 仍能捕获（白名单未被删除）
- docs/ai/stages/stage-3a.md §2 已填

USER_TESTS（需用户在游戏内确认，4 条）:
1. GMCM 开 Extended lookup → 打开刘易斯家门前的特殊订单板 → 鼠标悬停订单描述里的单词 → 出现词性+中文释义气泡；移入气泡不消失。
2. 对一段多行描述逐行悬停（可开 F10 看绿色矩形）→ 矩形贴合当前词，无逐行右漂/串行。
3. 左侧切换选中不同订单 → 旧气泡不残留；新订单描述可正常查词。
4. 关闭 Extended lookup → 订单板不再出气泡；对话、F1 卡片、信件、任务日志行为与之前一致。

CONSTRAINTS:
- 构建：$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src/BilingualLearning/BilingualLearning.csproj" -c Debug -p:EnableModDeploy=false
- 游戏运行中只编译不部署，并在报告注明「未部署」
- 只用编辑工具改源码；禁止 PowerShell Get-Content/Set-Content（BUG_PATTERNS 17）
- 改游戏内部字段/方法前先用 Mono.Cecil 核对（AGENTS 禁止事项 11）；本阶段证据已给，无需重复调查
- 不提交到 master；只在 stage/3a-orders-board 提交（1 个语义化提交）

EVIDENCE（已用 Cecil 核实，勿重复调查）:
- 类型：StardewValley.Menus.SpecialOrdersBoard : IClickableMenu（1.6.15）。
- .ctor：base(0,0,0,0,true) → width=1352 @IL_01bc、height=792 @IL_01c7 → getTopLeftPositionForCenteringOnScreen → xPositionOnScreen/yPositionOnScreen。菜单矩形有效（1352×792），RebuildWordRects 的 area 裁剪可用。
- 打开时确为 Game1.activeClickableMenu（5 个 new 站点均紧跟 set_activeClickableMenu）。
- draw() 文本调用：
  - IL_01fe SpriteText.drawStringWithScrollCenteredAt（顶部 ChooseOne 横幅，scrollType=0）→ 被现有 RegisterSpriteText 的 drawBGScroll != -1 拒绝（TextCapture.cs:75），无需处理。
  - IL_0353 / IL_045f Utility.drawTextWithShadow（AcceptQuest 按钮）→ 9 参 DrawString，已 hook。
  - DrawQuestDetails：标题 IL_025a；描述 IL_0385（Game1.parseText(desc, font, 512) + 缩字循环 scale 0.25~1，pos=(x, y+192)）；剩余天数 IL_0463；奖励 IL_05d7/065d/07dc/0866。全部 Utility.drawTextWithShadow → 9 参 string DrawString。
- 现有管线已覆盖：\n 与 ^ 多行（BUG_PATTERNS 20，BuildSpriteFontHits:352）、scale（line.Scale，:350/358/382-383）、阴影去重（19）、前缀 MeasureString（18）。
- 无 scissor / 无滚动字段 / 无滚动偏移；文本溢出无裁剪（不影响捕获，仅可能超出菜单矩形的部分不可悬停）。
- 同帧还会捕获「Accept Quest」按钮、天数、奖励名（预期，可接受）；描述缩字会用 scale 换算，不需要额外适配。

START_POINTS:
- src/BilingualLearning/ExtendedTextOverlay.cs → AllowedMenus（L20-24）、IsAllowedMenu（L123）、RebuildWordRects（L270）、BuildSpriteFontHits（L342）
- docs/ai/BUG_PATTERNS.md 18~21；docs/ai/stages/stage-3a.md

REPORT_FORMAT: SUBAGENT_PROMPTS.md §6（只回该结构）

STOP_CONDITIONS（必须 BLOCKED，不要猜）:
- 加入白名单后仍无任何捕获（F10 无矩形）→ BLOCKED，附 [诊断] 日志证据，不要新增 hook 猜测
- 需要改 ModEntry/TextCapture 才能完成 → 先 BLOCKED 并回报方案
- 构建连续 2 次失败 → BLOCKED
```

## 2. 执行报告（执行 Agent 填写，schema 见 §6）

```text
STATUS: DONE_WITH_RISK
STAGE: 3a   BRANCH: stage/3a-orders-board   COMMITS: 本阶段执行提交 1d117fa（另有 5b53215 Packet、37f052a 评审）
SUMMARY: 白名单新增 SpecialOrdersBoard 一行（含类注释同步）；捕获闸门与命中逻辑零改动，构建 0 error。主控已用默认构建（带部署）更新 Mods\BilingualLearning，部署 DLL 与 bin 内容一致，可直接实机验收。无新增决策/新坑。
FILES:
- src/BilingualLearning/ExtendedTextOverlay.cs — AllowedMenus 增加 typeof(SpecialOrdersBoard)（1 行）+ 类注释补「阶段3a」（0 行为）
VERIFY:
- build: 0 error（仅既有 CS9057 分析器版本警告）；命令 = dotnet build -c Debug -p:EnableModDeploy=false
- checks:
  - git diff --stat：仅 ExtendedTextOverlay.cs（3 insertions / 2 deletions），无 bin/obj/dist/版本号
  - AllowedMenus 仅新增 1 个类型；LetterViewerMenu、QuestLog 保留
  - 闸门唯一入口：ModEntry.cs:309 与 ExtendedTextOverlay.cs:220 均走 IsAllowedMenu，故一行即覆盖「捕获」与「命中」两侧；ExtendLookup=Off 时 Enabled/ShouldCapture 逻辑未改（diff 不涉及）
  - 未重写 BuildSpriteFontHits / SpriteTextWordLayout（diff 无相关改动）
  - 执行 Agent 构建时为「未部署」（游戏进程未运行，但按 Packet 用 EnableModDeploy=false）；主控在评审前已用默认构建（带部署）更新 Mods\BilingualLearning：部署 DLL mtime 22:17:58、SHA256 508CF55D…C9A3，与 bin\Debug\BilingualLearning.dll 完全一致
- user_tests:
  1. GMCM 开 Extended lookup → 打开刘易斯家门前的特殊订单板 → 悬停订单描述里的单词 → 出现词性+中文释义气泡；移入气泡不消失
  2. 对一段多行描述逐行悬停（可开 F10 看绿色矩形）→ 矩形贴合当前词，无逐行右漂/串行
  3. 左侧切换选中不同订单 → 旧气泡不残留；新订单描述可正常查词
  4. 关闭 Extended lookup → 订单板不再出气泡；对话、F1 卡片、信件、任务日志行为与之前一致
DECISIONS: none
PITFALLS: none
RISKS:
- 顶部 ChooseOne 横幅（SpriteText scrollType=0）按 EVIDENCE 被 drawBGScroll != -1 闸门跳过，本阶段不接管（预期）
- 文本若溢出菜单矩形，超出部分被 area 裁剪不可悬停（EVIDENCE 已注明，无裁剪修正）
- 实机未验证（主控已完成部署，Mods DLL 与 bin 一致）
DOCS: docs/ai/stages/stage-3a.md（§2 本报告）；未追加 DECISIONS/BUG_PATTERNS（无新增）
NEXT_READY: yes — 部署已完成，待用户实机 4 条验收
```

## 3. 评审结论（评审 Agent 填写）

```text
VERDICT: PASS
BLOCKING: 无。
NON_BLOCKING:
1. 报告 VERIFY「部署目录 DLL 仍为 20:20 旧版本 → 确认未部署」与实测不符：Mods\BilingualLearning\BilingualLearning.dll mtime 22:17:58、SHA256 508CF55D…C9A3 与 bin 一致；RISKS「需先部署再测」已过期 → 本修订已更正（部署状态）。
2. 报告 COMMITS 写「本阶段单提交」，实际 master..HEAD 有 5b53215 Packet + 1d117fa feat → 本修订已改写为「本阶段执行提交 1d117fa（另有 Packet/评审）」。
3. 陈旧文案（OUT_OF_SCOPE，未要求本阶段改）：ModEntry.cs:308 注释仍写「阶段0/2：信件、任务日志」；GMCM「Extended lookup」描述仍为 letters/quest log，未含订单板。建议后续阶段一并同步。
4. 顶部 ChooseOne 横幅（SpriteText）不接管、超出菜单矩形的文本不可悬停，均为 Packet 已声明预期，无新增风险。
EVIDENCE: 详见 docs/ai/stages/review-3a.md（评审提交 37f052a）；构建 0 error、diff 仅 2 处、闸门零改动。
```

## 4. 验收结论（主控填写）

- 越界检查：`git diff --stat master...stage/3a-orders-board` = `review-3a.md`(新增 22) + `stage-3a.md`(新增 137) + `ExtendedTextOverlay.cs`(5+/2-)；无 bin/obj/dist、无 manifest/csproj/版本号。
- 构建：`dotnet build -c Debug`（含部署）与 `-p:EnableModDeploy=false` 均 0 error（仅既有 CS9057 警告）；主控已部署到 `Mods\BilingualLearning`。
- 用户实机：**通过**。订单板描述可悬停查词；多行/缩字对齐、切换订单不残留、Off 时全局不查词均符合预期（用户确认 `Extended lookup=Off` 关闭信件/任务日志为设计行为）。
- 合并提交：`93b0790`（--no-ff，父 `f85eb41`）。
- 后续动作：进入阶段 3b（商店/木匠/日历/Billboard/发货箱/ItemList）；顺手同步 GMCM 文案与 `ModEntry.cs:308` 注释（评审 NON_BLOCKING 3）。

## 5. 经验沉淀（可选）

- 新增决策：`DECISIONS #__`
- 新增坑：`BUG_PATTERNS #__`
- 未覆盖场景：
