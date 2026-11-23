# 阶段 5a：扩展面统一点击吞掉（气泡区域）

> 阶段 5 拆两刀：**5a 点击吞掉（本文件）** → 5b 星标收藏。先在 5a 建立统一的「气泡区域吞点击」通道，5b 再在通道里先 toggle 星标。
> 前置：3b-4 已在 `ShopMenu` 上验证了「prefix + IsPointerOnBubble」范式（`BUG_PATTERNS` #24）。

## 0. 元信息

| 字段 | 值 |
|---|---|
| 阶段 ID | `5a` |
| 分支 | `stage/5a-click-swallow` |
| 状态 | `QUEUED` |
| 开始 / 结束 | 2026-11-24 / — |
| 执行 Agent | `<待派发>` |
| 评审 Agent | `<待派发>` |

## 1. Packet（主控填写）

```text
ROLE: Stage Executor
STAGE: 5a
BRANCH: stage/5a-click-swallow
REPO: D:\Project\星露谷双语mod
GOAL: 把「气泡区域吞掉左/右键」从 ShopMenu 专用推广到全部 8 个扩展白名单菜单；气泡外点击与关闭 Extended lookup 时行为完全不变。

IN_SCOPE（只允许改这些）:
- src/BilingualLearning/ModEntry.cs：
  - 把现有 ShopMenu 专用的 receiveLeftClick/RightClick patch 注册，泛化为遍历 `ExtendedTextOverlay.SupportedMenus`（新暴露的只读类型表）；
  - 统一 prefix（可复用/改名现有 `BeforeShopClick` → `BeforeExtendedMenuClick`），命中 `extendedOverlay.IsPointerOnBubble(x, y)` 时返回 false；
  - 只 patch「该类型自己声明的 override」（`method.DeclaringType == type`），避免把基类方法重复 patch；右键没有 override 的类型跳过。
  - 找不到方法时只 Warn（沿用现有容错）。
- src/BilingualLearning/ExtendedTextOverlay.cs：
  - 把 `AllowedMenus` 暴露为 `internal static readonly Type[]`（或等价只读属性），供 ModEntry 遍历；不改白名单内容。
- docs/ai/stages/stage-5a.md：填 §2 执行报告。
- docs/ai/BUG_PATTERNS.md：仅当出现新可复现坑时追加（编号接续 25）。

OUT_OF_SCOPE（禁止）:
- 星标/收藏/星标绘制（5b 做）；`BubbleRenderer.cs` 不改
- `WordOverlay.cs`、`KnowledgeCardMenu.cs`、对话路径
- 气泡布局/命中/桥接判定、`IsPointerOnBubble` 语义（气泡 ∪ 词 ∪ 桥接）
- 其它菜单（未在白名单的 GameMenu 各页/世界层）
- manifest/csproj/版本号/dist/bin/obj/references；不新增依赖

ACCEPTANCE（自动可验）:
- dotnet build -p:EnableModDeploy=false → 0 error
- git diff --stat：文件 ⊆ IN_SCOPE；白名单 8 类型不变
- 每个类型只 patch 一次（左键 8 个声明 override；右键仅 LetterViewerMenu / SpecialOrdersBoard / Billboard / ShopMenu 等有 override 的）
- `ExtendLookup=Off` 或鼠标不在气泡上时 prefix 放行（返回 true），零行为变化
- stage-5a.md §2 已填

USER_TESTS（需用户在游戏内确认，5 条）:
1. 信件（多页）悬停词出气泡 → 左键点气泡：不翻页、不关闭信；右键点气泡：同样不翻页/不关闭；点气泡外空白：翻页正常。
2. 任务日志：悬停任务描述词 → 点气泡：不选中/不取消任务；点任务行（气泡外）行为正常。
3. 小镇任务板 + 特殊订单板：悬停描述 → 左键点气泡：不接任务；右键点气泡：不关闭菜单（任务板右键原生是关闭）。
4. 木匠菜单：悬停描述 → 点气泡：不切换蓝图、不触发底部按钮；发货箱结算页（或 debug 触发）气泡上点 OK 不生效。
5. 商店：沿用 3b-4 行为（左/右键气泡内不购买）；关闭 Extended lookup 后上述菜单点击全部原生。

CONSTRAINTS:
- 构建：$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src/BilingualLearning/BilingualLearning.csproj" -c Debug -p:EnableModDeploy=false
- 游戏运行中只编译不部署，并在报告注明「未部署」；主控会在验收前做带部署构建
- 只用编辑工具改源码；禁止 PowerShell Get-Content/Set-Content（BUG_PATTERNS 17）
- 改游戏内部字段/方法前先用 Mono.Cecil 核对（AGENTS 禁止事项 11）；本阶段签名已给
- 只在 stage/5a-click-swallow 提交（1 个语义化提交）；不合并 master

EVIDENCE（已用 Cecil + 源码核实，勿重复调查）:
- 8 个白名单类型与左/右键 override（1.6.15）：
  - LetterViewerMenu：L✓ R✓（R 转调 receiveLeftClick，翻页/关信）
  - QuestLog：L✓；R 无 override
  - SpecialOrdersBoard：L✓ R✓（R 直接 exitThisMenu）
  - CarpenterMenu：L✓
  - Billboard：L✓ R✓（R exitThisMenu）
  - ShippingMenu：L✓
  - ItemListMenu：L✓
  - ShopMenu：L✓ R✓（3b-4 已单独 patch）
  - 签名统一 `void receiveLeftClick(int,int,bool)` / `void receiveRightClick(int,int,bool)`；基类 `IClickableMenu.receiveRightClick` 为空。
- 关键坑：`AccessTools.Method(type,...)` 会命中继承来的基类方法；**必须判断 `method.DeclaringType == type`**，否则基类方法会被重复 patch、prefix 多次执行。
- 现有 ShopMenu patch 在 ModEntry.cs 约 92-110；统一 prefix 现有实现 `BeforeShopClick` 在约 461 行，用 `IsPointerOnBubble`。
- `Game1.updateActiveMenu` 左键派发不查 isWithinBounds（IL_02f7-0352），右键 IL_039e；故 prefix 吞掉即可阻止菜单动作。
- 气泡区域 = 气泡 ∪ 词矩形 ∪ 桥接（`ExtendedTextOverlay.IsPointerOnBubble`，已是 3b-4 验收过的语义）。
- 已知风险（验收时注意）：信件里气泡可见时点「正文词」不再翻页（只吞词/气泡/桥接，空白处仍翻页）；气泡上下翻转时都要验。

START_POINTS:
- src/BilingualLearning/ModEntry.cs（patch 注册 ~92-110；`BeforeShopClick` ~461）
- src/BilingualLearning/ExtendedTextOverlay.cs（`AllowedMenus` L20-30；`IsPointerOnBubble` L237-244）
- docs/ai/stages/stage-5a.md
- 参考：docs/ai/stages/stage-3b-4.md §1.2

REPORT_FORMAT: SUBAGENT_PROMPTS.md §6（只回该结构）

STOP_CONDITIONS（必须 BLOCKED，不要猜）:
- 某个类型的 receiveLeftClick 没有 override（`DeclaringType != type`）→ 跳过并写进报告，不要 patch 基类
- 需要改气泡布局/星标才能完成 → 先 BLOCKED（5b 做）
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
