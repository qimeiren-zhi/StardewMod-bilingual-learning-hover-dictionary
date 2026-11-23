# 阶段 3b-4：商店（ShopMenu）正文查词（不含 tooltip）

> 3b 收官。范围仅「正文版」：物品名 / 价格 / 以物易物数量 / 缺货提示；**物品描述与售价 tooltip 不接管**（用户已确认：tooltip 里的描述走 F1 知识卡片间接查词）。
> 之后：3c `JunimoNote` / 精通追踪 → 4 世界层 → 5 交互统一。

## 0. 元信息

| 字段 | 值 |
|---|---|
| 阶段 ID | `3b-4` |
| 分支 | `stage/3b-4-shop` |
| 状态 | `QUEUED` |
| 开始 / 结束 | 2026-11-24 / — |
| 执行 Agent | `<待派发>` |
| 评审 Agent | `<待派发>` |

## 1. Packet（主控填写）

```text
ROLE: Stage Executor
STAGE: 3b-4
BRANCH: stage/3b-4-shop
REPO: D:\Project\星露谷双语mod
GOAL: 让商店菜单（ShopMenu）正文的像素字体文本（物品名、价格、以物易物数量、缺货提示）可悬停查词；不接管 tooltip，其它文本面行为不变。

IN_SCOPE（只允许改这些）:
- src/BilingualLearning/ExtendedTextOverlay.cs：AllowedMenus 增加 typeof(ShopMenu)（一行；类注释同步补「3b-4」）。
- docs/ai/DECISIONS.md：追加决策 19「商店 tooltip 不接管，物品描述用 F1 知识卡片间接查词」（含理由与替代方案）。
- docs/ai/stages/stage-3b-4.md：填 §2 执行报告。
- docs/ai/BUG_PATTERNS.md：仅当出现新可复现坑时追加（编号接续 23）。

OUT_OF_SCOPE（禁止）:
- tooltip 路径（drawHoverText / drawToolTip）、TooltipTextCapture、背包 tooltip
- 点击与星标、GMCM 文案、气泡外观
- src/BilingualLearning/{ModEntry,TextCapture,SpriteTextWordLayout,WordOverlay,KnowledgeCardMenu,BubbleRenderer}.cs
- 其它菜单（JunimoNote / GameMenu 各页 / 世界层）
- manifest.json / csproj / 版本号 / dist / bin / obj / references/decompiled
- 不新增依赖

ACCEPTANCE（自动可验）:
- dotnet build -p:EnableModDeploy=false → 0 error
- git diff --stat：文件 ⊆ IN_SCOPE；AllowedMenus 只新增 1 个类型且保留 LetterViewerMenu / QuestLog / SpecialOrdersBoard / CarpenterMenu / Billboard / ShippingMenu / ItemListMenu
- ExtendLookup=Off 时闸门/捕获/布局逻辑零改动（IsEnabled、ShouldCapture、BuildSpriteFontHits、TextCapture 不在 diff）
- DECISIONS #19 已追加且与用户确认的理由一致（tooltip → F1 卡片）
- docs/ai/stages/stage-3b-4.md §2 已填

USER_TESTS（需用户在游戏内确认，5 条）:
1. 皮埃尔商店（或任意商店）→ 悬停商品名 → 出现词性+中文释义气泡；移入气泡不消失；气泡显示时点击该商品仍能购买。
2. 悬停价格数字/以物易物数量 → 不出气泡（数字不在词典）或只出可查词的气泡，均不遮挡导致无法购买。
3. 切换商店分类页签（或换一家商店）→ 旧气泡不残留，新页物品名可查词。
4. 悬停玩家背包里的物品（tooltip）→ 不出现查词气泡（本阶段不接管 tooltip）；按 F1 仍能打开知识卡片查描述。
5. 关闭 Extended lookup → 商店不出气泡；对话、F1 卡片、信件、任务日志、订单板、木匠、任务板/日历、发货箱结算、丢物列表与此前一致。

CONSTRAINTS:
- 构建：$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src/BilingualLearning/BilingualLearning.csproj" -c Debug -p:EnableModDeploy=false
- 游戏运行中只编译不部署，并在报告注明「未部署」；主控会在验收前做带部署构建
- 只用编辑工具改源码；禁止 PowerShell Get-Content/Set-Content（BUG_PATTERNS 17）
- 改游戏内部字段/方法前先用 Mono.Cecil 核对（AGENTS 禁止事项 11）；本阶段证据已给，无需重复调查
- 只在 stage/3b-4-shop 提交（1 个语义化提交）；不合并 master

EVIDENCE（已用 Cecil 核实，勿重复调查）:
- ShopMenu : IClickableMenu；ctor 7 参版 → Initialize → updatePosition：w=1000+2*borderWidth、h=600+2*borderWidth（有店长头像时用 800 宽一侧）、x/y 居中 → 4 字段有效。
- 打开时确为 Game1.activeClickableMenu（6 个 new 站点全部随后 set_activeClickableMenu）。
- draw() 正文（全部 SpriteText.drawString，像素字体）：
  - IL_04e0 / IL_0538：item.DisplayName（stack>1 时 IL_0224 追加 " xN"），x=row.bounds.X+40（有图标）/ +104（无图标），y=row+28，width=-1。
  - IL_060f：价格数字，x=row.Right-getWidthOfString-60，y=row+28。
  - IL_0838：以物易物 "xN"，x=row.Right-16，y=row+20/28。
  - IL_08d3：本地化缺货语（ShopMenu.cs.11583），居中，y=yPosition+height/2-128。
- tooltip 现状：drawHoverText ×1 + drawToolTip ×2（物品描述/售价/背包格），本阶段**不接管**（F1 知识卡片已覆盖物品描述，用户确认）。
- 无滚动；物品行是 forSaleButtons 可点按钮，像素字体矩形与其重叠——本 mod 只读、不拦截点击，气泡只影响视觉。
- 菜单矩形裁剪可用；像素字体走 SpriteTextWordLayout（width=-1/换行原点已处理，BUG_PATTERNS 21）。

START_POINTS:
- src/BilingualLearning/ExtendedTextOverlay.cs → AllowedMenus（L20-30）、IsAllowedMenu、RebuildWordRects
- docs/ai/DECISIONS.md 末尾（决策 19）；docs/ai/BUG_PATTERNS.md 末尾
- docs/ai/stages/stage-3b-4.md
- 参考同构阶段：docs/ai/stages/stage-3b-3.md

REPORT_FORMAT: SUBAGENT_PROMPTS.md §6（只回该结构）

STOP_CONDITIONS（必须 BLOCKED，不要猜）:
- 加入白名单后仍无捕获（F10 无矩形）→ BLOCKED，附 [诊断] 证据；不要新增 hook
- 需要动 tooltip/捕获闸门才能完成 → 先 BLOCKED（本阶段明确不做）
- 构建连续 2 次失败 → BLOCKED
```

## 1.1 修复轮范围（2026-11-24，用户实机打回第 1 轮）

> 白名单本身通过；以下 2 个问题需要修，允许本轮临时扩大 IN_SCOPE（只加 `ModEntry.cs`、`ExtendedTextOverlay.cs`）。

**问题 A：商店 F1 打开的是店长问候语卡片，不是商品**
- 证据（Cecil + 日志）：`ShopMenu.draw` 顺序为「物品 tooltip（`drawToolTip` → 内部 `drawHoverText`）→ base.draw → 头像/问候框 → `drawHoverText(portraitPersonDialogue)` IL_0C80」；现有 `AfterDrawHoverText` 只缓存**最后一次**调用，故被问候语覆盖。
- 修法（A'，最小）：`ModEntry.AfterDrawHoverText` 中，当 `Game1.activeClickableMenu is ShopMenu` 且 `__args[9]` 不是 `Item`（问候语调用）时**不覆盖**缓存；保留物品 tooltip 的 args。
- 验收：商店悬停商品按 F1 → 卡片标题/描述 = 该商品；Joja（无问候语）仍正常；其它菜单 F1 行为不变。

**问题 B：Joja 商店气泡盖住光标**
- 现象：悬停商品名出气泡后鼠标移入气泡，光标画在气泡下方（截图 3）。
- 已核实：`RenderedActiveMenu` 事件顺序为 menu.draw（含 drawMouse）→ 我方气泡 → `ExtendedTextOverlay.DrawCursor`，静态顺序本应正确，需加固。
- 修法（B'）：`ExtendedTextOverlay.DrawCursor` 改为显式 `drawMouse(b, ignore_transparency: true, Game1.mouseCursor)`（避免 `Game1.mouseCursorTransparency` 影响）。
- 若加固后仍复现：在 `DebugRects`（F10）开启时输出一次 `[诊断] cursor` 日志（菜单类型 / transparency / mouseCursor / hardwareCursor），供定位；不要改气泡布局或命中逻辑。

**问题 C（不修，记录）**：气泡内移动时游戏 tooltip 按光标重算下一行 —— 用户认为可接受，已记入 `docs/requirements/需求规格说明.md` §10-4。

## 1.2 修复轮 2 范围（2026-11-24，点击吞掉）

**问题 D：点击释义气泡会误触下层控件**
- 现象：商店里悬停商品名出气泡，光标移入气泡后点击 → 游戏把点击传给气泡下方的商品行，**误购买下一行商品**（截图 4）；用户指出后续星标功能也会点气泡，误触不可接受。
- 范围（仅本阶段）：`ShopMenu` 的 `receiveLeftClick`，当点击点落在「气泡 ∪ 词矩形 ∪ 桥接矩形」内时吞掉点击（prefix 返回 false），不改变其它菜单点击行为（通用吞掉留阶段 5 统一做）。
- 修法：
  - `ExtendedTextOverlay` 增加 `public bool IsPointerOnBubble(int x, int y)`：`HasBubble && (bubbleLayout.BubbleBox.Contains || hoveredWordBox.Contains || bridgeRect.Contains)`。
  - `ModEntry` 新增 Harmony prefix：`ShopMenu.receiveLeftClick(int x, int y, bool playSound)` → 命中则 return false；在 `Entry` 里注册（与既有 patch 同处），找不到方法时只 Warn。
- 验收：气泡上点击不购买；气泡外点击正常；其它菜单行为不变；`ExtendLookup=Off` 时零影响。
- 不修：其它菜单（信件/任务日志/订单板/木匠/任务板）的同类误触，统一留到阶段 5「扩展面点击吞掉」；本轮只封商店（购买不可逆，风险最高）。

## 2. 执行报告（执行 Agent 填写，schema 见 §6）

```text
STATUS: DONE
STAGE: 3b-4（修复轮 1：A 问候语覆盖 tooltip / B 光标层级；修复轮 2：气泡区域吞左键）
SUMMARY: 白名单接入本身通过。修复轮 1：A) `ShopMenu.draw` 在物品 tooltip 之后又为店长问候语调用 `drawHoverText`，覆盖了 `AfterDrawHoverText` 的单一缓存，导致 F1 打开问候语卡片，现改为「商店内非 Item tooltip 不覆盖缓存」；B) Joja 商店气泡盖光标，`DrawCursor` 改为 `drawMouse(b, ignore_transparency: true)`（走默认 cursor 选择）。修复轮 2：气泡盖住商品行时点击会穿透误购，新增 `ShopMenu.receiveLeftClick` prefix，命中 `IsPointerOnBubble`（气泡 ∪ 词 ∪ 桥接）即吞掉。构建 0 error（未部署）。
FILES:
- src/BilingualLearning/ExtendedTextOverlay.cs — AllowedMenus 追加 typeof(ShopMenu)（首轮）+ 类注释；修复轮 1：`DrawCursor` 显式 ignore_transparency + F10 下每气泡一次 `[诊断] cursor`；修复轮 2：新增 `public bool IsPointerOnBubble(int x, int y)`
- src/BilingualLearning/ModEntry.cs — 修复轮 1：`AfterDrawHoverText` 在 `ShopMenu` 且 `__args[9] is not Item` 时 return；修复轮 2：`Entry` 注册 `ShopMenu.receiveLeftClick` prefix（找不到只 Warn）+ prefix `BeforeShopLeftClick`
- docs/ai/DECISIONS.md — 追加决策 19（不接管商店 tooltip，走 F1 卡片）
- docs/ai/stages/stage-3b-4.md — 本报告（§2/§5）
VERIFY:
- build: 0 error（`-p:EnableModDeploy=false`，仅 1 条既有 CS9057 警告；**未部署**，游戏运行中只编译）
- checks（首轮）: `git diff --stat` = 2 文件，⊆ IN_SCOPE；旧白名单 7 类型（LetterViewerMenu/QuestLog/SpecialOrdersBoard/CarpenterMenu/Billboard/ShippingMenu/ItemListMenu）全部保留；IsEnabled/ShouldCapture/BuildSpriteFontHits/TextCapture/RebuildWordRects 均无 diff（Off 零行为变化）
- checks（修复轮）: diff 仅 `ModEntry.AfterDrawHoverText`（+18/−4，含注释）与 `ExtendedTextOverlay.DrawCursor`/`cursorDiagLogged`（+19/−1）；未改 `TooltipTextCapture`、卡片重放、气泡布局/命中/桥接、捕获闸门、GMCM、manifest/csproj/版本号/dist/bin/obj/references；修复 A 仅对 `ShopMenu` 生效，其它菜单 `AfterDrawHoverText` 行为不变；`__args.Length > 9` 的判断改写为 early-return，语义等价
  - 修复 A 验证方式：阅读 diff 确认忽略条件是 `Game1.activeClickableMenu is ShopMenu && __args[9] is not Item`；商店内物品 tooltip（`__args[9]` 为 `Item`）仍正常缓存 → F1 标题/描述应为商品。
  - 修复 B 验证方式：`DrawCursor` 已改为两参重载 `drawMouse(b, ignore_transparency: true)`（采纳评审建议：第三参不传 `Game1.mouseCursor`，走默认 `cursor=-1` 的 snappy/手柄光标选择逻辑，避免 snappy/手柄模式画错图标）；`Game1.mouseCursorTransparency`/`Game1.options.hardwareCursor` 均 public（构建通过即签名正确）；诊断日志受 `DebugRects` 与 `cursorDiagLogged` 双重闸门，每气泡最多 1 条。
- checks（修复轮 2）: Mono.Cecil 核实 `StardewValley.Menus.ShopMenu::receiveLeftClick(System.Int32,System.Int32,System.Boolean)` 与 `receiveRightClick(System.Int32,System.Int32,System.Boolean)` 均存在且签名与预期一致（右键同样有 `forSaleButtons → tryToPurchaseItem` 购买分支，故两个 prefix 共用 `BeforeShopClick`）；patch 只挂 `ShopMenu` 的 override（不动 `IClickableMenu` 基类，虚拟分派会命中 override）；prefix 返回 `instance?.extendedOverlay?.IsPointerOnBubble(x, y) != true`（无 overlay/无气泡 → true，即放行）；`IsPointerOnBubble` 仅在 `HasBubble` 为真时判区域，`ExtendLookup=Off` 时气泡不建立 → 零影响；未改其它菜单点击路径。
- user_tests:
  1. 商店悬停商品名出气泡 → 光标移入气泡并左键点击：不购买、不触发气泡下方商品行（重点复测上一行/下一行）；气泡仍显示。
  2. 气泡外（商品名/商品图标/空白）左键：与原生一致，能正常购买/切换页签；移出气泡后点击仍正常。
  3. 皮埃尔商店悬停商品按 F1：卡片 = 该商品（A 复测）；Joja 气泡上光标完整可见（B 复测）。
  4. GMCM 关闭 Extended lookup → 商店无气泡、点击完全走原生；其它菜单（信件/任务日志/任务板/木匠/对话）点击行为不变。
DECISIONS: 新增 #19「商店 tooltip 不接管，物品描述用 F1 知识卡片间接查词」（背景/决策/原因/影响/替代方案）。
PITFALLS: 新坑（暂记本文件，未改 BUG_PATTERNS——本轮 IN_SCOPE 未含）：商店「最后一次 tooltip 缓存」被店长问候语覆盖 → F1 打开问候语卡片；根因是 `ShopMenu.draw` 在物品 tooltip 之后又调用 `drawHoverText(portraitPersonDialogue)`（IL_0C80），而 `AfterDrawHoverText` 只保留最后一次参数；修法是在 `ShopMenu` 下忽略 `__args[9] is not Item` 的调用。关键词：`drawHoverText` `portraitPersonDialogue` `__args[9]` `lastTooltipArgs` `问候语`。
RISKS:
- 修复轮 2 只在 `ShopMenu` 吞点击（用户明确本轮只封商店，其它扩展面留阶段 5 统一做）；信件/任务日志/订单板/木匠/任务板的同类穿透仍存在，属已知范围外。左键与右键（1/5/25/999 数量购买分支）均已覆盖，中键/滚轮未涉及。
- 吞点击只看「当前气泡区域」，若气泡在点击前一刻因移出词/翻页被清空，点击会落回原生路径（正常语义，但极端时序下可能漏吞一次）。
- 修复 A 的副作用：在商店内若尚未悬停任何物品（无物品 tooltip、仅有问候语被忽略），F1 会沿用「上一处缓存的 tooltip」（可能是先前菜单/背包物品）。这是「F1 用上一帧 tooltip」既有语义，非本轮回归；用户测试按「先悬停商品再 F1」覆盖。
- 商店 draw 内的 drawHoverText/drawToolTip 仍走通用 SpriteBatch.DrawString Hook，理论上 tooltip 文本可能被捕获成词矩形并落在菜单矩形内（与木匠/任务板同构风险）；未做针对性排除（属 OUT_OF_SCOPE），实机若出现「悬停 tooltip 也出气泡」按设计不改，仅记录。
- 修复 B 为「显式 ignore_transparency」的加固；若 Joja 仍复现，需按 F10 `[诊断] cursor` 行（menu/transparency/cursor/hardwareCursor）再定位。
- 价格数字/数量不可查属预期（词典无数字条目）；物品名带 " xN" 后缀时仅单词部分命中，未实测。
- ShopMenu 无滚动；页签切换残留依赖每帧「原词仍存在」校验，通用逻辑已覆盖但未实机验证。
- 本次构建未部署，游戏内验证需主控带部署重建后再测。
DOCS: docs/ai/DECISIONS.md #19；docs/ai/stages/stage-3b-4.md §2/§5（本报告）。
NEXT_READY: yes — 修复轮 2 完成，待评审 + 用户复测（4 条）。
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

- 新增决策：无（本轮修复不引入新决策；DECISIONS #19 仍适用）
- 新增坑：**商店店长问候语覆盖 tooltip 缓存**（详见 §2 PITFALLS；待主控复核后决定是否升级为 BUG_PATTERNS #23）
  - 症状：皮埃尔商店悬停商品按 F1，卡片标题/描述是店长问候语而非商品。
  - 根因：`ShopMenu.draw` 顺序 = 物品 tooltip（drawToolTip→drawHoverText，`__args[9]` 是 Item）→ base.draw → 头像/问候框 → `drawHoverText(portraitPersonDialogue)` IL_0C80；`AfterDrawHoverText` 只存最后一次调用。
  - 修法：`ShopMenu` 内 `__args[9] is not Item` 时 return，不覆盖缓存（只影响商店；Joja 无问候语不受影响）。
- 新增现象（已修，修复轮 1/2）：
  - Joja 气泡盖光标 → `DrawCursor` 改为 `drawMouse(b, ignore_transparency: true)`（默认 cursor 选择）；F10 下每气泡输出一次 `[诊断] cursor`（menu/transparency/cursor/hardwareCursor）。
  - **气泡盖住商品行时点击穿透 → 误购下一行**（修复轮 2）：扩展面此前只读、不拦截点击；商店行是 `forSaleButtons`，气泡区域没有点击归属。修法：`ShopMenu.receiveLeftClick` 加 Harmony prefix，`IsPointerOnBubble`（气泡 ∪ 词 ∪ 桥接）命中则 return false 吞掉；`ExtendLookup=Off` 无气泡 → 零影响；其它菜单点击不动（阶段 5 统一）。
    - 关键词：`receiveLeftClick` `IsPointerOnBubble` `BeforeShopLeftClick` `吞点击` `误购` `forSaleButtons`。
    - Cecil 签名：`StardewValley.Menus.ShopMenu::receiveLeftClick(System.Int32,System.Int32,System.Boolean)`（须挂 override；`IClickableMenu` 基类 patch 会被虚拟分派绕过）。
- 未覆盖场景：tooltip 文本是否会被通用 DrawString Hook 捕获成可查词矩形（未实测）；商店内无物品 hover 时 F1 会沿用旧缓存；其它扩展面的点击穿透留阶段 5。
