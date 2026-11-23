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

## 2. 执行报告（执行 Agent 填写，schema 见 §6）

```text
STATUS: DONE
STAGE: 3b-4
SUMMARY: 将 ShopMenu 加入 ExtendedTextOverlay.AllowedMenus，商店正文（物品名/价格/以物易物数量/缺货提示）复用现有像素字体捕获与词布局即可悬停查词；tooltip 路径（drawHoverText/drawToolTip、TooltipTextCapture、背包格）零改动，长期决策记录为 DECISIONS #19（描述改由 F1 知识卡片间接查词）。构建 0 error（未部署），diff 仅两文件。
FILES:
- src/BilingualLearning/ExtendedTextOverlay.cs — AllowedMenus 追加 typeof(ShopMenu)，类注释补 3b-4 与「tooltip 不接管」
- docs/ai/DECISIONS.md — 追加决策 19（不接管商店 tooltip，走 F1 卡片）
- docs/ai/stages/stage-3b-4.md — 本报告（§2）
VERIFY:
- build: 0 error（`-p:EnableModDeploy=false`，仅 1 条既有 CS9057 警告；**未部署**，游戏运行中只编译）
- checks: `git diff --stat` = 2 文件（ExtendedTextOverlay.cs 5 行、DECISIONS.md +8/-0），⊆ IN_SCOPE；旧白名单 7 类型（LetterViewerMenu/QuestLog/SpecialOrdersBoard/CarpenterMenu/Billboard/ShippingMenu/ItemListMenu）全部保留；IsEnabled/ShouldCapture/BuildSpriteFontHits/TextCapture/RebuildWordRects 均无 diff（Off 零行为变化）；未改 manifest/csproj/版本号/dist/bin/obj/references，未动 tooltip/ModEntry/TextCapture
- user_tests:
  1. 任意商店悬停商品名 → 出词性+中文释义气泡；移入气泡不消失；气泡显示时点击商品仍能购买（只读、不吞点击）。
  2. 悬停价格数字 / 以物易物 "xN" 数量 → 不出气泡或仅出可查词气泡，均不遮挡导致无法购买。
  3. 切换商店分类页签（或换一家商店）→ 旧气泡不残留，新页物品名可查词。
  4. 悬停背包物品（tooltip）→ 不出现查词气泡（本阶段不接管 tooltip）；按 F1 仍可打开知识卡片查描述。
  5. GMCM 关闭 Extended lookup → 商店不出气泡；对话/F1 卡片/信件/任务日志/订单板/木匠/任务板/日历/发货箱结算/丢物列表与此前一致。
DECISIONS: 新增 #19「商店 tooltip 不接管，物品描述用 F1 知识卡片间接查词」（背景/决策/原因/影响/替代方案）。
PITFALLS: none（未出现新可复现坑；BUG_PATTERNS 未改）
RISKS:
- 商店 draw 内的 drawHoverText/drawToolTip 仍走通用 SpriteBatch.DrawString Hook，理论上 tooltip 文本可能被捕获成词矩形并落在菜单矩形内（与木匠/任务板同构风险）；未做针对性排除（属 OUT_OF_SCOPE），实机若出现「悬停 tooltip 也出气泡」按设计不改，仅记录。
- 价格数字/数量不可查属预期（词典无数字条目）；物品名带 " xN" 后缀时仅单词部分命中，未实测。
- ShopMenu 无滚动；页签切换残留依赖每帧「原词仍存在」校验，通用逻辑已覆盖但未实机验证。
- 本次构建未部署，游戏内验证需主控带部署重建后再测。
DOCS: docs/ai/DECISIONS.md #19；docs/ai/stages/stage-3b-4.md §2（本报告）。
NEXT_READY: yes — 待评审 + 用户实机验收（5 条）。
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
