# 阶段 3c：JunimoNote 收集包 + 精通追踪

> 阶段 3 收官。研究结论：两者都是独立 `activeClickableMenu`，正文落在现有像素字体/SpriteFont 捕获管线内，**白名单两行**即可；卷轴奖励名与 GameMenu Skills 页不在本阶段范围。

## 0. 元信息

| 字段 | 值 |
|---|---|
| 阶段 ID | `3c` |
| 分支 | `stage/3c-junimo-mastery` |
| 状态 | `MERGED`（Mastery 待实机） |
| 开始 / 结束 | 2026-11-24 / 2026-11-24 |
| 执行 Agent | `exec-3c` |
| 评审 Agent | `review-3c` |

## 1. Packet（主控填写）

```text
ROLE: Stage Executor
STAGE: 3c
BRANCH: stage/3c-junimo-mastery
REPO: D:\Project\星露谷双语mod
GOAL: 让社区中心收集包菜单（JunimoNoteMenu）与精通追踪菜单（MasteryTrackerMenu）的正文可悬停查词/星标；其它文本面行为不变。

IN_SCOPE（只允许改这些）:
- src/BilingualLearning/ExtendedTextOverlay.cs：AllowedMenus 增加 typeof(JunimoNoteMenu)、typeof(MasteryTrackerMenu)（两行；类注释同步补 3c）。
- docs/ai/stages/stage-3c.md：填 §2 执行报告。
- docs/ai/DECISIONS.md / docs/ai/BUG_PATTERNS.md：仅当出现新长期决策/新可复现坑时追加（编号接续 21 / 27）。

OUT_OF_SCOPE（禁止）:
- 卷轴奖励名（`drawStringWithScrollCenteredAt`，drawBGScroll=1 被闸门过滤）——留需求 §10-2「卷轴居中文本」
- GameMenu 各页（不能整体白名单 GameMenu；精通条 x/y 在 Skills 页，另立项）
- 物品名/材料名 tooltip（鼠标锚定，需新布局）
- 其它菜单、点击/星标机制（5a/5b 已通用，新增白名单会自动注册）
- manifest/csproj/版本号/dist/bin/obj/references；不新增依赖

ACCEPTANCE（自动可验）:
- dotnet build -p:EnableModDeploy=false → 0 error
- git diff --stat：文件 ⊆ IN_SCOPE；AllowedMenus 只新增 2 个类型且保留旧 8 个
- 点击吞掉/星标自动覆盖新类型（`SupportedMenus` 驱动，无需改 ModEntry）
- ExtendLookup=Off 零行为变化
- stage-3c.md §2 已填

USER_TESTS（需用户在游戏内确认，5 条）:
1. 社区中心收集包（如工艺室/储藏室）：打开 Junimo 菜单 → 悬停区域名/当前收集包名 → 出气泡；鼠标移入气泡不消失。
2. 未读 Junimo 文（若当前包有）：悬停正文单词 → 出气泡；若文字为乱码/无意义词，允许不出气泡。
3. 点击收集包名/材料词（气泡外或点词本身）→ 原生捐物/翻页；点气泡空白不触发；点星标收藏/取消正常。
4. 精通洞窟牌匾：打开精通菜单 → 悬停奖励名/说明/Claim/下一级 XP 文本 → 出气泡；点 Claim（气泡外）正常领取。
5. 关闭 Extended lookup → 两个菜单不出气泡；其它菜单（信件/任务日志/订单板/木匠/任务板/商店/发货箱）与此前一致。

CONSTRAINTS:
- 构建：$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src/BilingualLearning/BilingualLearning.csproj" -c Debug -p:EnableModDeploy=false
- 游戏运行中只编译不部署，并在报告注明「未部署」；主控会在验收前做带部署构建
- 只用编辑工具改源码；禁止 PowerShell Get-Content/Set-Content（BUG_PATTERNS 17）
- 只在 stage/3c-junimo-mastery 提交（1 个语义化提交）；不合并 master

EVIDENCE（已用 Cecil 核实，勿重复调查）:
- JunimoNoteMenu : IClickableMenu，独立 activeClickableMenu（InventoryPage / SkillsPage / CommunityCenter.checkBundle / Raccoon 打开；`fromGameMenu` 只影响回退目标，不是内嵌页）。1280×720 居中，菜单矩形有效。
  - 区域名：`SpriteText.drawStringHorizontallyCenteredAt` IL_0115（内部 drawString，drawBGScroll=-1）→ 可捕获。
  - 未读 Junimo 正文：`SpriteText.drawString` IL_0190（width=1152，drawBGScroll=-1）→ 可捕获（可能为乱码）。
  - 收集包名：`SpriteBatch.DrawString(dialogueFont)` ×4（3 阴影+正文，IL_058e/060c/068a/06fe，手算居中）→ ≤4px 去重保留正文。
  - 底部区域奖励名：`drawStringWithScrollCenteredAt` IL_0a61（drawBGScroll=+1）→ **被过滤，本阶段不接管**。
  - tooltip：drawToolTip ×1 + drawHoverText ×1（物品/包名，鼠标锚定）→ 理论上会捕获但鼠标不可达，属既有局限。
  - 1.6.15 无「3/5」进度字符串；进度由图标/槽位表达。
- MasteryTrackerMenu : IClickableMenu，独立 activeClickableMenu（精通洞窟牌匾 GameLocation.performAction）。800×320 居中。
  - 标题 `drawStringHorizontallyCenteredAt`；FinalPath 时 `drawBar` → 4 参 DrawString(smallFont) "cur/next"；which≥0 时奖励 name(dialogueFont)+label(smallFont) 经 Utility.drawTextWithColoredShadow→9 参 DrawString；Claim(dialogueFont) 居中。
- 点击吞掉：5a 的 prefix 遍历 `SupportedMenus`（即 AllowedMenus），新增类型自动覆盖；两类型均有 receiveLeftClick/receiveRightClick 自身 override。
- 已知坑：吞点击只吞气泡矩形；词/桥接放行（5b 收窄）；Mastery 右键任意处关闭，气泡内右键被吞。
- GameMenu 不能整体白名单（会放开 inventory/social/collections/options），精通条 x/y 在 Skills 页 → 本阶段不做。

START_POINTS:
- src/BilingualLearning/ExtendedTextOverlay.cs → AllowedMenus（L20-33）
- docs/ai/stages/stage-3c.md
- 参考：docs/ai/stages/stage-3b-4.md（同构白名单）

REPORT_FORMAT: SUBAGENT_PROMPTS.md §6（只回该结构）

STOP_CONDITIONS（必须 BLOCKED，不要猜）:
- 加入白名单后仍无捕获（F10 无矩形）→ BLOCKED 附 [诊断]
- 需要改 ModEntry/TextCapture/布局才能完成 → 先 BLOCKED（卷轴奖励名明确不做）
- 构建连续 2 次失败 → BLOCKED
```

## 2. 执行报告（执行 Agent 填写，schema 见 §6）

```text
STATUS: DONE
STAGE: 3c   BRANCH: stage/3c-junimo-mastery   COMMITS: 本提交（代码 + 本报告同一提交）
SUMMARY: ExtendedTextOverlay.AllowedMenus 新增 JunimoNoteMenu、MasteryTrackerMenu 两行并同步类注释；旧 8 个白名单保持原样，无新增 hook。点击吞掉/星标经 SupportedMenus（ModEntry.cs:94）自动覆盖，无需改 TextCapture/布局。用户实机反馈后补修复轮：未读 Junimo 文（junimo 乱码字体，`__args[9]`）在 `AfterSpriteTextDrawString` 直接不注册捕获。构建 0 error（EnableModDeploy=false，未部署）。发布前按用户要求移除 `MasteryTrackerMenu` 白名单（未实机测试，不随本次发布实装）；代码/研究结论保留，待补测通过后单独恢复。
FILES:
- src/BilingualLearning/ExtendedTextOverlay.cs — AllowedMenus 加两行 + 类注释补 3c
- src/BilingualLearning/ModEntry.cs — 修复轮：`SpriteText.drawString` 的 junimoText 为 true 时不注册捕获
- docs/ai/stages/stage-3c.md — 填 §2 执行报告
VERIFY:
- build: 0 error（1 warning CS9057 无害）；-p:EnableModDeploy=false，未部署
- checks: git diff --stat 仅上述文件；AllowedMenus 数量 8→10 且旧 8 项逐项保留（mastery-defer 收口后为 9 项，Junimo 保留）；ModEntry.cs:94 遍历 SupportedMenus 自动注册点击吞掉；junimo 判断用 `is bool` 且位于长度检查（<12 已 return）之后
- user_tests:
  1. JunimoNoteMenu：悬停区域名/当前收集包名出气泡，移入气泡不消失
  2. 未读 Junimo 文（junimo 乱码字体）：悬停**不出**气泡（修复轮行为）
  3. 点收集包名/材料词走原生（捐物/翻页），点气泡空白被吞，点星标收藏/取消正常
  4. MasteryTrackerMenu：悬停奖励名/说明/Claim/下一级 XP 出气泡；点 Claim（气泡外）正常领取
  5. Extended lookup=Off：两菜单不出气泡，其余 8 个白名单菜单行为与 5b 一致
DECISIONS: none（沿用 DECISIONS 17/19/20，无新长期决策）
PITFALLS: none（沿用 BUG_PATTERNS #25/#26；junimo 字体问题属菜单特例，已就地修复，未升级为通用坑）
RISKS: ① 底部区域奖励名走 drawStringWithScrollCenteredAt（drawBGScroll=+1）被闸门过滤，本阶段明确不接管（需求 §10-2）；② GameMenu Skills 页精通条不在白名单（不能整体白名单 GameMenu），仍不可查；③ 材料名/物品名 tooltip 鼠标锚定，不接管（DECISIONS 19）；④ **MasteryTrackerMenu 代码已接入但未实机测试（当前存档条件不足），发布前必须补测**；⑤ 未读 Junimo 文按用户要求整体不查词（含其乱码正文）；⑥ 悬停位置/裁剪仅 JunimoNoteMenu 部分经用户确认。
DOCS: docs/ai/stages/stage-3c.md §2；DECISIONS/BUG_PATTERNS 无追加
NEXT_READY: yes — 待评审 + 用户实机验收；未合并 master
```

## 3. 评审结论（评审 Agent 填写）

```text
VERDICT: PASS | FAIL
BLOCKING:
NON_BLOCKING:
EVIDENCE:
```

## 4. 验收结论（主控填写）

- 越界检查：`git diff --stat master...stage/3c-junimo-mastery` = `ExtendedTextOverlay.cs`（+4/−2）+ `ModEntry.cs`（+6）+ 阶段文档；无版本号/dist/bin/obj/references。
- 构建：`-p:EnableModDeploy=false` 与带部署构建均 0 error（部署时游戏运行被锁，关闭后已重部署，DLL 与 bin 一致）。
- 用户实机：**JunimoNote 通过**（可读英文后区域名/包名/正文正常；点词走原生、星标正常）。**MasteryTrackerMenu 未实机**（存档未达条件）——代码已接入，标记「发布前必补测」。
- 修复轮：用户要求「未读 Junimo 文不出气泡」→ `AfterSpriteTextDrawString` 对 `junimoText=true` 直接跳过捕获（`7582b3d`）。
- 合并提交：`fbf1108`（--no-ff，父 `7582b3d`）。
- 后续动作：**阶段 3 全部完成**。发布前补测 Mastery；之后阶段 4 世界层 / 发布准备。

## 5. 经验沉淀（可选）

- 新增决策：`DECISIONS #__`（本阶段无）
- 新增坑：`BUG_PATTERNS #__`（本阶段无）
- 未覆盖场景：
  1. **MasteryTrackerMenu 未实机测试**（当前存档条件不足：未达精通/洞窟牌匾不可用）——代码已接入，发布前必须用满足条件的存档补测（悬停奖励名/说明/Claim/XP、点 Claim 领取、右键关闭）。
  2. 未读 Junimo 文（`junimoText: true` 乱码字体）按用户要求整体不参与查词；若将来要支持，需要单独建模其字号/缩放与乱码文本。
