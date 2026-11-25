# 阶段 5a：扩展面统一点击吞掉（气泡区域）

> 阶段 5 拆两刀：**5a 点击吞掉（本文件）** → 5b 星标收藏。先在 5a 建立统一的「气泡区域吞点击」通道，5b 再在通道里先 toggle 星标。
> 前置：3b-4 已在 `ShopMenu` 上验证了「prefix + IsPointerOnBubble」范式（`BUG_PATTERNS` #24）。

## 0. 元信息

| 字段 | 值 |
|---|---|
| 阶段 ID | `5a` |
| 分支 | `stage/5a-click-swallow` |
| 状态 | `MERGED` |
| 开始 / 结束 | 2026-11-24 / 2026-11-24 |
| 执行 Agent | `exec-5a` |
| 评审 Agent | `review-5a` |

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
STATUS: DONE
STAGE: 5a   BRANCH: stage/5a-click-swallow   COMMITS: 2（主体 + 评审收尾；哈希见提交记录）
SUMMARY: 把 3b-4 的 ShopMenu 专用「气泡区域吞点击」泛化为遍历 `ExtendedTextOverlay.SupportedMenus` 的统一 prefix，共 patch 12 个自身 override（左键 8 个 + 右键 4 个），未命中气泡时一律放行。`AllowedMenus` 内容与顺序未变，仅新增只读视图 `SupportedMenus`；`ExtendLookup=Off` 时无气泡建立，prefix 恒放行。构建 0 error（未部署，待主控带部署重建后实机验收）。
FILES:
- src/BilingualLearning/ExtendedTextOverlay.cs — 新增 `internal static IReadOnlyList<Type> SupportedMenus => Array.AsReadOnly(AllowedMenus);`（白名单内容/顺序不变；真只读包装）；`IsPointerOnBubble` 增加活动菜单守卫 `ReferenceEquals(trackedMenu, Game1.activeClickableMenu)`
- src/BilingualLearning/ModEntry.cs — 删除 ShopMenu 专用注册块，改为遍历 8 类型 × 左/右键注册；只 patch `method.DeclaringType == type` 的自身 override；`BeforeShopClick` 更名 `BeforeExtendedMenuClick`（实现不变）
- docs/ai/stages/stage-5a.md — 本报告（§2）
VERIFY:
- build: 0 error（`-p:EnableModDeploy=false`；仅既有 CS9057 警告；**未部署**）
- checks:
  - `git diff --stat master...stage/5a-click-swallow` ⊆ IN_SCOPE（源码 2 文件 + stage-5a.md；无 manifest/csproj/版本号/dist/bin/obj/references）
  - Mono.Cecil 复核 Stardew Valley 1.6.15：8 个类型均有 `receiveLeftClick(int,int,bool)` 自身 override；`receiveRightClick(int,int,bool)` 自身 override 仅 LetterViewerMenu / SpecialOrdersBoard / Billboard / ShopMenu 4 个 → 实际注册 12 个 patch，无重复、无基类 patch
  - 跳过右键（未声明 override，继承基类空实现）：QuestLog、CarpenterMenu、ShippingMenu、ItemListMenu
  - 左键同名方法找不到或声明类型非自身时 Warn 并跳过；右键缺失/继承时**静默 continue（不写日志）**，为预期行为
  - `IsPointerOnBubble` 增加菜单守卫：`HasBubble && ReferenceEquals(trackedMenu, Game1.activeClickableMenu) && (气泡∪词∪桥接)`，消除「切菜单后一帧旧气泡误吞点击」窗口
  - `ExtendLookup=Off`：`UpdateAndDraw` 走 `Clear()` → `HasBubble=false` → `IsPointerOnBubble` 返回 false → prefix 返回 true，零行为变化
  - 白名单 8 类型不变：LetterViewerMenu / QuestLog / SpecialOrdersBoard / CarpenterMenu / Billboard / ShippingMenu / ItemListMenu / ShopMenu
- user_tests:
  1. 信件（多页）悬停词出气泡 → 左键点气泡：不翻页/不关信；右键点气泡：同样不翻页/不关信；点气泡外空白：翻页/关信正常（含气泡上下翻转两种位置）。
  2. 任务日志：悬停任务描述词出气泡 → 左键点气泡：不选中/不取消任务；点任务行（气泡外）行为正常。
  3. 小镇任务板 + 特殊订单板：悬停描述出气泡 → 左键点气泡：不接任务；右键点气泡：不关闭菜单；气泡外右键仍按原生关闭。
  4. 木匠菜单：悬停描述 → 点气泡：不切换蓝图、不触发底部按钮；发货箱结算页（或 debug 触发）气泡上点 OK 不生效。
  5. 商店沿用 3b-4 行为（左/右键气泡内不购买）；GMCM 关闭 Extended lookup 后上述 8 类菜单点击全部原生。
DECISIONS: none（沿用 DECISIONS #18「白名单 + 只读气泡」与 #19 商店 tooltip 不接管）
PITFALLS: 新坑（已写在 Packet EVIDENCE，本阶段未改 BUG_PATTERNS 以守住 IN_SCOPE）：泛化 patch 时 `AccessTools.Method(type, ...)` 会沿继承链返回基类方法；若直接 patch，`IClickableMenu.receiveLeftClick/RightClick` 会被每个类型重复 patch、prefix 执行多次。修法：先取方法，再判 `method.DeclaringType == type`，只 patch 自身 override；右键无 override 的类型跳过（基类方法为空，无功能影响）。关键词：`AccessTools.Method` `DeclaringType` `getMethod` `继承` `重复 patch` `receiveRightClick`。
RISKS:
- 已知风险（Packet 已列）：信件气泡可见时点击正文**词**不再翻页（词矩形属于吞掉区域）；空白处仍翻页。属预期，验收时按此判断。
- prefix 只看上一帧建立的 `HasBubble`；若点击前一刻因移出词/翻页清空气泡，该次点击会落回原生路径（正常语义，极端时序可能漏吞一次）。
- 菜单守卫要求 `trackedMenu` 与当前活动菜单为同一实例；切菜单后到首次绘制前的窗口内气泡判定为 false（放行原生点击），属安全侧行为。
- 右键吞掉对 LetterViewerMenu 会同时阻断其「右键转调左键」的翻页/关信路径（仅在气泡上）；气泡外不受影响。
- 本次构建未部署；实机测试前需主控执行带部署构建（游戏当前未运行，已确认）。
- 未覆盖：中键/滚轮；非白名单菜单（GameMenu 各页、世界层）的同类穿透仍按 DECISIONS #18 留待后续。
DOCS: docs/ai/stages/stage-5a.md §2；未新增 DECISIONS / BUG_PATTERNS 条目（IN_SCOPE 仅三项文件，坑已记录于本报告 PITFALLS）
NEXT_READY: yes — 待只读评审 + 用户 5 条实机验收；主控验收前需带部署重建 DLL。
```

## 3. 评审结论（评审 Agent 填写）

```text
VERDICT: PASS | FAIL
BLOCKING:
NON_BLOCKING:
EVIDENCE:
```

## 4. 验收结论（主控填写）

- 越界检查：`git diff --stat master...stage/5a-click-swallow` = `ModEntry.cs`（+54/−17）+ `ExtendedTextOverlay.cs`（+5）+ 阶段文档；无版本号/dist/bin/obj/references。
- 构建：`dotnet build -c Debug`（含部署）与 `-p:EnableModDeploy=false` 均 0 error；主控已部署，DLL 与 bin 一致。
- 用户实机：**通过**（信件/任务日志/任务板/订单板/木匠/发货箱/商店的气泡内左右键均被吞，气泡外与 Off 全部原生）。
- 合并提交：`5d2f6da`（--no-ff，父 `58dc477`）。
- 后续动作：阶段 5b（星标收藏）——在 5a 的吞点击通道里先 toggle 星标；随后阶段 4 世界层。

## 5. 经验沉淀（可选）

- 新增决策：`DECISIONS #__`
- 新增坑：`BUG_PATTERNS #__`
- 未覆盖场景：
