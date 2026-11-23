# 阶段 3b-3：发货箱日终结算 + 丢物列表（ShippingMenu / ItemListMenu）

> 3b 剩余：3b-4 `ShopMenu`（tooltip 路径，风险最高）。需求池见 `docs/requirements/需求规格说明.md` §10。

## 0. 元信息

| 字段 | 值 |
|---|---|
| 阶段 ID | `3b-3` |
| 分支 | `stage/3b-3-shipping-itemlist` |
| 状态 | `MERGED` |
| 开始 / 结束 | 2026-11-24 / 2026-11-24 |
| 执行 Agent | `exec-3b3` |
| 评审 Agent | `review-3b3` |

## 1. Packet（主控填写）

```text
ROLE: Stage Executor
STAGE: 3b-3
BRANCH: stage/3b-3-shipping-itemlist
REPO: D:\Project\星露谷双语mod
GOAL: 让发货箱日终结算（ShippingMenu）与丢物列表（ItemListMenu）的正文文本可悬停查词；其它文本面行为不变。

IN_SCOPE（只允许改这些）:
- src/BilingualLearning/ExtendedTextOverlay.cs：AllowedMenus 增加 typeof(ShippingMenu) 与 typeof(ItemListMenu)（两行；类注释可同步补「3b-3」）。
- docs/ai/stages/stage-3b-3.md：填 §2 执行报告。
- docs/ai/DECISIONS.md / docs/ai/BUG_PATTERNS.md：仅当出现新长期决策/新可复现坑时追加（编号接续 20 / 23）。

OUT_OF_SCOPE（禁止）:
- 其它菜单（ShopMenu / JunimoNote / GameMenu 各页 / 世界层）
- tooltip 改造、点击与星标、GMCM 文案、气泡外观
- src/BilingualLearning/{ModEntry,TextCapture,SpriteTextWordLayout,WordOverlay,KnowledgeCardMenu,BubbleRenderer}.cs
- manifest.json / csproj / 版本号 / dist / bin / obj / references/decompiled
- 不新增依赖；不重构既有对话/卡片/订单板/木匠/任务板路径

ACCEPTANCE（自动可验）:
- dotnet build -p:EnableModDeploy=false → 0 error
- git diff --stat：文件 ⊆ IN_SCOPE；AllowedMenus 只新增 2 个类型且保留 LetterViewerMenu / QuestLog / SpecialOrdersBoard / CarpenterMenu / Billboard
- ExtendLookup=Off 时闸门/捕获/布局逻辑零改动（IsEnabled、ShouldCapture、BuildSpriteFontHits、TextCapture 不在 diff）
- docs/ai/stages/stage-3b-3.md §2 已填

USER_TESTS（需用户在游戏内确认，5 条）:
1. 往发货箱放几样东西 → 睡觉进入日终结算页 → 悬停物品名 / 分类名（Farming、Foraging 等）/ 总金额文本 → 出现词性+中文释义气泡；移入气泡不消失。
2. 在结算页切换分类/翻页（左右页签）→ 旧气泡不残留；新页文本可正常查词。
3. 点 OK 关闭结算 → 正常进入次日，无异常、无残留气泡。
4. SMAPI 控制台执行 `debug eventScript Farm "none/-100 -100/farmer 5 5 2/showItemsLost/end"` → 丢物列表出现 → 悬停标题/物品名/总价值 → 出气泡；若 `debug` 命令不存在则跳过本条并告知。
5. 关闭 Extended lookup → 上述两个菜单都不出气泡；对话、F1 卡片、信件、任务日志、订单板、木匠、任务板/日历与此前一致。

CONSTRAINTS:
- 构建：$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src/BilingualLearning/BilingualLearning.csproj" -c Debug -p:EnableModDeploy=false
- 游戏运行中只编译不部署，并在报告注明「未部署」；主控会在验收前做带部署构建
- 只用编辑工具改源码；禁止 PowerShell Get-Content/Set-Content（BUG_PATTERNS 17）
- 改游戏内部字段/方法前先用 Mono.Cecil 核对（AGENTS 禁止事项 11）；本阶段证据已给，无需重复调查
- 只在 stage/3b-3-shipping-itemlist 提交（1 个语义化提交）；不合并 master

EVIDENCE（已用 Cecil 核实，勿重复调查）:
- ShippingMenu : IClickableMenu；ctor(IList<Item>) → base(0,0,viewport.Width,viewport.Height,false)（满屏，字段有效），实际绘制框 min(1280)×min(920) 居中；日终由 Game1.endOfNightMenus 弹出后 set_activeClickableMenu（Game1._update IL_1033-103d）。
- ShippingMenu.draw 文本（全部 SpriteText.drawString，像素字体）：
  - IL_0b4b drawStringWithScrollCenteredAt（日期横幅，scroll）→ 现有闸门跳过，不接管。
  - IL_0d1b 分类名（getCategoryName）。
  - IL_107a "DisplayName x单价"；IL_10b6 "$合计"（右对齐）。
  - IL_11b7 drawStringWithScrollCenteredAt（日期牌）→ 同样跳过。
  - 无 drawHoverText / drawToolTip；行不可点；无滚动（按 currentTab 翻页）；唯一按钮是底部 OK。
- ItemListMenu : IClickableMenu；ctor(String,List<Item>)：w=min(800,viewport.W-128)、h=min(720,viewport.H-128) 居中（字段有效）；打开方式唯一：事件命令 `showItemsLost` → Event/DefaultCommands.ShowItemsLost → set_activeClickableMenu。
- ItemListMenu.draw 文本（SpriteText，像素字体）：IL_0069 drawStringHorizontallyCenteredAt（标题）；IL_0121 drawString（总价值）；IL_019d drawString（逐项 DisplayName，行距 68）；无 tooltip；按 currentTab 翻页。
- 两者菜单矩形均有效，RebuildWordRects 裁剪可用；像素字体布局走 SpriteTextWordLayout（width=-1/换行原点已处理，BUG_PATTERNS 21）。
- 已知：ShippingMenu 为满屏矩形，crop 为全屏；该菜单本身无 tooltip，风险小。

START_POINTS:
- src/BilingualLearning/ExtendedTextOverlay.cs → AllowedMenus（L20-26）、IsAllowedMenu（L123）、RebuildWordRects（L270）
- docs/ai/BUG_PATTERNS.md 18~21；docs/ai/stages/stage-3b-3.md
- 参考同构阶段：docs/ai/stages/stage-3b-2.md

REPORT_FORMAT: SUBAGENT_PROMPTS.md §6（只回该结构）

STOP_CONDITIONS（必须 BLOCKED，不要猜）:
- 加入白名单后仍无捕获（F10 无矩形）→ BLOCKED，附 [诊断] 证据；不要新增 hook
- 需要改 ModEntry/TextCapture/布局类才能完成 → 先 BLOCKED 并回报方案
- 构建连续 2 次失败 → BLOCKED
```

## 2. 执行报告（执行 Agent 填写，schema 见 §6）

```text
STATUS: DONE
STAGE: 3b-3
SUMMARY: 在 `ExtendedTextOverlay.AllowedMenus` 追加 `typeof(ShippingMenu)` 与 `typeof(ItemListMenu)`（并同步类注释补「3b-3」），复用既有 SpriteText 捕获/裁剪/命中管线，无新增 hook。构建 0 error（本次 `-p:EnableModDeploy=false`，**未部署**；构建时游戏未运行，主控需做一次带部署构建再实机）。闸门/布局/TextCapture/ModEntry 零改动，ExtendLookup=Off 行为不变。
FILES:
- src/BilingualLearning/ExtendedTextOverlay.cs — 白名单新增 2 个类型 + 类注释补阶段名（净 +4/-2 行）
- docs/ai/stages/stage-3b-3.md — 本执行报告 + §0 元信息（状态 BUILT / Agent）
VERIFY:
- build: 0 error（1 warning CS9057 已知无害），命令 `dotnet build src/BilingualLearning/BilingualLearning.csproj -c Debug -p:EnableModDeploy=false`
- checks:
  - `git diff --stat` → 仅 `src/BilingualLearning/ExtendedTextOverlay.cs`（+4/-2）；无 bin/obj/dist/manifest/版本号
  - 白名单旧 5 类保留（LetterViewerMenu / QuestLog / SpecialOrdersBoard / CarpenterMenu / Billboard），新增 ShippingMenu / ItemListMenu
  - diff 仅注释 + AllowedMenus；`IsAllowedMenu`/`ShouldCapture`/`RebuildWordRects`/`BuildSpriteFontHits`/`TextCapture`/`ModEntry` 均未出现在 diff → Off 零行为变化
  - `Get-Process "Stardew Valley"` → 未运行；虽可部署但按 Packet 只编译不部署
- user_tests（5 条，需用户游戏内确认）:
  1. 发货箱放几样东西 → 睡觉进入日终结算页 → 悬停物品名 / 分类名（Farming、Foraging 等）/ 总金额文本 → 出词性+中文气泡；移入气泡不消失
  2. 结算页切换分类/翻页（左右页签）→ 旧气泡不残留，新页文本仍可查
  3. 点底部 OK 关闭结算 → 正常进入次日，无异常、无残留气泡
  4. SMAPI 控制台 `debug eventScript Farm "none/-100 -100/farmer 5 5 2/showItemsLost/end"` → 丢物列表出现 → 悬停标题/物品名/总价值出气泡（若 `debug` 命令不存在则跳过并告知）
  5. 关闭 Extended lookup → 两菜单均不出气泡；对话/F1 卡片/信件/任务日志/订单板/木匠/任务板/日历与此前一致
DECISIONS: none（沿用决策 18「白名单 + 只读气泡」，无新长期决策）
PITFALLS: none（与 3a/3b-1/3b-2 同构；未见新可复现坑）
RISKS:
- ShippingMenu 为满屏矩形（ctor 用 viewport 宽高），crop 按全屏 → 命中不被裁剪到可见绘制框；由于该菜单正文都在框内、无滚动，预期无影响，但若屏幕上有框外文字面（HUD）被捕获，可能多出不可见命中（只读气泡、低风险）
- ItemListMenu 仅能由事件命令 `showItemsLost` 打开，属低频场景；测试需用 debug eventScript
- 两个菜单都按 `currentTab` 翻页、无滚动；翻页后残留依赖既有「每帧校验原词」通用机制，未在本阶段实机验证
- 未部署：本轮仅编译（Packet 要求），实机验收前需带部署构建
DOCS: docs/ai/stages/stage-3b-3.md §2（DECISIONS/BUG_PATTERNS 无追加）
NEXT_READY: yes — 待评审 PASS + 用户实机 5 条后合并；随后 3b-4 ShopMenu（tooltip 路径，风险最高）
```

## 3. 评审结论（评审 Agent 填写）

```text
VERDICT: PASS | FAIL
BLOCKING:
NON_BLOCKING:
EVIDENCE:
```

## 4. 验收结论（主控填写）

- 越界检查：`git diff --stat master...stage/3b-3-shipping-itemlist` = 源码 1 文件（+4/−2）+ 阶段报告 1 文件；无 bin/obj/dist、无 manifest/csproj/版本号。
- 构建：`dotnet build -c Debug`（含部署）与 `-p:EnableModDeploy=false` 均 0 error；主控已部署，`bin\Debug` 与 `Mods\BilingualLearning` DLL SHA256 一致。
- 用户实机：**全部通过**（结算页物品名/分类名/金额可查、翻页不残留、OK 正常、丢物列表可查、Off 无行为）。
- 测试备注：`debug eventScript Farm "none/-100 -100/farmer 5 5 2/showItemsLost/end"` 只回「Sent debug command to the game, but there was no output.」，未直接弹出菜单；用户改用**人工触发事件**验证丢物列表成功。后续如需自动化复测，优先用真实死亡事件（`debug ebi PlayerKilled`，会扣钱/掉物，需测试存档）。
- 合并提交：`c5901a8`（--no-ff，父 `a652b30`）。
- 后续动作：阶段 3b-4 `ShopMenu`（tooltip 路径，唯一需要动 tooltip/背包交互的一刀）；随后 3c `JunimoNote`。

## 5. 经验沉淀（可选）

- 新增决策：`DECISIONS #__`
- 新增坑：`BUG_PATTERNS #__`
- 未覆盖场景：
