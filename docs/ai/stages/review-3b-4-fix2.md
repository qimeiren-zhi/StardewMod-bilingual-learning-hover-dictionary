# 阶段 3b-4 修复轮 2 评审（review-3b4fix2，只读）

VERDICT: PASS

BLOCKING: 无。

NON_BLOCKING:
1. 右键仍会误购：Cecil 显示 `ShopMenu.receiveRightClick` 同样含 `forSaleButtons → tryToPurchaseItem` 购买循环（含 1/5/25/999 数量分支），本轮只封 left click，右键点在气泡上仍会买到气泡下方商品；建议同轮补 `receiveRightClick` prefix，或在 §2 RISKS 显式记录（报告现未提）。
2. 吞掉整个 `receiveLeftClick` 会连 `base.receiveLeftClick`（Cecil IL_0003）一起跳过；基类仅处理右上关闭按钮，气泡若压到 X 则该次点不关菜单（概率低，符合「气泡在最上层」语义）。
3. 气泡矩形来自上一帧 `RenderedActiveMenu`（点击在本帧 Update 中先处理）；滚动/翻页同帧点击可能用旧矩形，与 §2 RISKS 一致，需实机看极端时序。
4. prefix 只声明 `(int x,int y)`，依赖 Harmony 按名/按位置绑定原 `x,y,playSound`；若签名重构则匹配失败、退化为不吞（不崩溃）。
5. 无自动化测试，实机 4 条待用户验收。

EVIDENCE:
- `git show c4c2e5c`：3 文件（ModEntry.cs +24、ExtendedTextOverlay.cs +13、stage 文档），源码只新增、无删改；无版本号/manifest/csproj/dist/bin/obj/references；评审前工作树 clean（构建后仍 clean）。
- Cecil：`StardewValley.Menus.ShopMenu::receiveLeftClick(int x,int y,bool playSound)` 存在且为 override（Virtual、NewSlot=false），`AccessTools.Method(...,new[]{int,int,bool})` 命中；失败分支只 Warn（ModEntry.cs:91-106）。左键购买发生在本方法内（`tryToPurchaseItem` 唯一 caller），prefix 返回 false 即整体跳过 → 不购买；`receiveRightClick` 也是 caller（见 NON_BLOCKING 1）。
- 坐标一致：`Game1.updateActiveMenu` 以 `getMouseX()/getMouseY()` 传入（IL_0346/0352），`getMousePosition()` 实现 = `getMouseX(uiMode)/getMouseY(uiMode)`，与 overlay 的 `Game1.getMousePosition()`（ExtendedTextOverlay.cs:180）同一 UI 坐标空间。
- 判定：ExtendedTextOverlay.cs:237-244 = `HasBubble && (BubbleBox ∪ hoveredWordBox ∪ bridgeRect)`；`hasBubble` 仅 :209 置真、`Clear()` :467 置假，`BubbleLayout` 为 readonly struct，无空引用；`instance/extendedOverlay` 为 null 时 `!= true` 放行（ModEntry.cs:461-463）。
- `ExtendLookup=Off` → `IsEnabled` false → :176 `Clear()` → HasBubble 恒 false，零影响；prefix 仅挂 ShopMenu。
- 未回退：对话 `BeforeClick`（ModEntry.cs:455-457）与 F1 问候语过滤（:337-352）、`DrawCursor` 的 `drawMouse(b, ignore_transparency: true)`（ExtendedTextOverlay.cs:219）均保留；未动 TooltipTextCapture/布局/命中/桥接/基类 patch。
- 构建：`dotnet build -c Debug -p:EnableModDeploy=false` → 0 error / 1×既有 CS9057；未部署、未跑游戏。
