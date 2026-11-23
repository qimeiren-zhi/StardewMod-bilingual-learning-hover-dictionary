# 阶段 3b-1：木匠/建造菜单（CarpenterMenu）悬停查词

> 3b 拆分：3b-1 木匠/建造（本文件）→ 3b-2 小镇任务板 `Billboard`（日历为同类型另一分支）→ 3b-3 `ShippingMenu` + `ItemListMenu` → 3b-4 `ShopMenu`（含 tooltip 路径，风险最高，最后做）。`CollectionsPage` 走 `GameMenu` 页、无正文，不单列。

## 0. 元信息

| 字段 | 值 |
|---|---|
| 阶段 ID | `3b-1` |
| 分支 | `stage/3b-1-carpenter` |
| 状态 | `QUEUED` |
| 开始 / 结束 | 2026-11-23 / — |
| 执行 Agent | `<待派发>` |
| 评审 Agent | `<待派发>` |

## 1. Packet（主控填写）

```text
ROLE: Stage Executor
STAGE: 3b-1
BRANCH: stage/3b-1-carpenter
REPO: D:\Project\星露谷双语mod
GOAL: 让「木匠/建造菜单」（CarpenterMenu）的蓝图描述、建造费、配料名/数量文本可悬停查词；其它文本面行为不变。

IN_SCOPE（只允许改这些）:
- src/BilingualLearning/ExtendedTextOverlay.cs：AllowedMenus 增加 typeof(CarpenterMenu)（一行；类注释可同步补「3b-1」）。
- docs/ai/stages/stage-3b-1.md：填 §2 执行报告。
- docs/ai/DECISIONS.md / docs/ai/BUG_PATTERNS.md：仅当出现新长期决策/新可复现坑时追加（编号接续 19 / 22）。

OUT_OF_SCOPE（禁止）:
- 其它菜单（ShopMenu / Billboard / ShippingMenu / ItemListMenu / CollectionsPage / GameMenu）
- tooltip 改造、点击与星标、GMCM 选项文案、气泡外观
- src/BilingualLearning/{ModEntry,TextCapture,SpriteTextWordLayout,WordOverlay,KnowledgeCardMenu,BubbleRenderer}.cs
- manifest.json / csproj / 版本号 / dist / bin / obj / references/decompiled
- 不新增依赖；不重构既有对话/卡片/订单板路径

ACCEPTANCE（自动可验）:
- dotnet build -p:EnableModDeploy=false → 0 error
- git diff --stat：文件 ⊆ IN_SCOPE；AllowedMenus 只新增 1 个类型且保留 LetterViewerMenu / QuestLog / SpecialOrdersBoard
- ExtendLookup=Off 时闸门/捕获逻辑零改动（IsEnabled、ShouldCapture 不在 diff）
- 布局类与去重逻辑未改（BuildSpriteFontHits、TextCapture.Add 不在 diff）
- docs/ai/stages/stage-3b-1.md §2 已填

USER_TESTS（需用户在游戏内确认，4 条）:
1. GMCM 开 Extended lookup → 去木匠罗宾处「建造农场建筑」→ 悬停右侧蓝图描述里的单词 → 出现词性+中文释义气泡；移入气泡不消失。
2. 悬停配料名（Wood / Stone / Clay / Hardwood 等）→ 正确出气泡，且气泡不长期遮挡数量/造价（移开即消失）。
3. 切换不同建筑蓝图（上下切换）→ 旧气泡不残留，新蓝图描述可正常查词。
4. 关闭 Extended lookup → 木匠菜单不出气泡；对话、F1 卡片、信件、任务日志、特殊订单板行为与之前一致。

CONSTRAINTS:
- 构建：$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src/BilingualLearning/BilingualLearning.csproj" -c Debug -p:EnableModDeploy=false
- 游戏运行中只编译不部署，并在报告注明「未部署」；主控会在验收前做带部署构建
- 只用编辑工具改源码；禁止 PowerShell Get-Content/Set-Content（BUG_PATTERNS 17）
- 改游戏内部字段/方法前先用 Mono.Cecil 核对（AGENTS 禁止事项 11）；本阶段证据已给，无需重复调查
- 只在 stage/3b-1-carpenter 提交（1 个语义化提交）；不合并 master

EVIDENCE（已用 Cecil 核实，勿重复调查）:
- 类型：StardewValley.Menus.CarpenterMenu : IClickableMenu；ctor(String, GameLocation) 内 resetBounds() 写入有效 xPositionOnScreen / yPositionOnScreen / width / height（描述区在右侧）。
- 打开时确为 Game1.activeClickableMenu（2 个 new 站点均紧跟 set_activeClickableMenu）。
- draw() 文本：
  - IL_026d SpriteText.drawStringWithScrollCenteredAt（蓝图 DisplayName，像素字体+卷轴）→ 被现有 drawBGScroll != -1 闸门跳过，本阶段不接管。
  - IL_043b / IL_04a0（两层阴影）+ IL_0515（正文）Utility.drawTextWithShadow（dialogueFont）：BlueprintEntry.Description 经 Game1.parseText(width-32) → 多行（\n/^），pos=(x+maxWidthOfBuildingViewer, y+96/100)。三层同文本、位置差 ≤4px/轴 → TextCapture.Add 的 ≤4px 去重保留后到正文（TextCapture.cs:171）。
  - IL_0647/06af/073e 建造费 "$%s"（dialogueFont，钱不够变红）。
  - IL_0892 配料「已有/需要」数量（smallFont，单行）。
  - IL_096e/09cc/0a43 配料 item.DisplayName（dialogueFont）。
  - IL_0bc4 SpriteText.drawStringWithScrollBackground（底部 Choose/Move/Paint/Demolish）→ 同样被闸门跳过。
- 无滚动、无裁剪、无 scissor；正文不与底部按钮重叠。
- 菜单矩形 crop（RebuildWordRects 的 area）对 4 字段有效 → 命中裁剪可用。
- 布局复用：dialogueFont 多行由 BuildSpriteFontHits 处理（\n/^ 分行 + 前缀 MeasureString + scale=1）；无需新几何逻辑。

START_POINTS:
- src/BilingualLearning/ExtendedTextOverlay.cs → AllowedMenus（L20-24）、IsAllowedMenu（L123）、RebuildWordRects（L270）、BuildSpriteFontHits（L342）
- docs/ai/BUG_PATTERNS.md 18~21；docs/ai/stages/stage-3b-1.md
- 上一阶段可参考：docs/ai/stages/stage-3a.md（同构：只加白名单）

REPORT_FORMAT: SUBAGENT_PROMPTS.md §6（只回该结构）

STOP_CONDITIONS（必须 BLOCKED，不要猜）:
- 加入白名单后仍无任何捕获（F10 无矩形）→ BLOCKED，附 [诊断] 证据；不要新增 hook
- 需要改 ModEntry/TextCapture/布局类才能完成 → 先 BLOCKED 并回报方案
- 构建连续 2 次失败 → BLOCKED
```

## 2. 执行报告（执行 Agent 填写，schema 见 §6）

```text
STATUS:
STAGE:
SUMMARY:
FILES:
VERIFY:
DECISIONS:
PITFALLS:
RISKS:
DOCS:
NEXT_READY:
```

## 3. 评审结论（评审 Agent 填写）

```text
VERDICT: PASS | FAIL
BLOCKING:
NON_BLOCKING:
EVIDENCE:
```

## 4. 验收结论（主控填写）

- 越界检查：`<git diff --stat 结果摘要>`
- 构建：`<0 error>`
- 用户实机：`<通过 / 打回原因>`
- 合并提交：`<merge commit hash>`
- 后续动作：`<下一阶段 / 修复项>`

## 5. 经验沉淀（可选）

- 新增决策：`DECISIONS #__`
- 新增坑：`BUG_PATTERNS #__`
- 未覆盖场景：
