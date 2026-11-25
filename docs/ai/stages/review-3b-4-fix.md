# 阶段 3b-4 修复轮评审（review-3b4fix，只读）

VERDICT: PASS

BLOCKING: 无。修复 A 最小且语义正确；修复 B 安全但根因未证实（见 NON_BLOCKING 1/2），需实机复测。

NON_BLOCKING:
1. 修复 B 根因证据不足：Cecil 扫描 `ShopMenu.draw` 无 `set_mouseCursorTransparency` 调用；全 DLL 仅 Game1.cctor/set/Reset 三处写 backing field，`IClickableMenu.ctor` 与各扩展菜单 draw 末尾（Billboard/QuestLog/ShippingMenu/SpecialOrdersBoard/ItemListMenu/CraftingPage）均设为 1 → 商店内 transparency 通常已是 1，`ignore_transparency:true` 对商店多半是 no-op。若 Joja 仍复现，按 F10 `[诊断] cursor` 行定位（z 序/硬件光标/他处覆盖），不要盲改布局或命中。
2. `Game1.mouseCursor` 与游戏默认不等价：`IClickableMenu.drawMouse` 在 `cursor<0` 时选 0（snappy+gamepad 时 44，IClickableMenu.cs:173-176）；改传全局值后，手柄或残留非零值会把别的光标图标画到菜单里。建议只传 `ignore_transparency:true`（cursor 省略）或显式 0。
3. `[诊断] cursor` 仅在气泡存在时可输出（DrawCursor 由 `HasBubble` 闸门调用，ModEntry.cs:456-459）；ExtendLookup=Off 时无气泡故永不打——"每气泡 1 条"成立，但横扫多词会每词 1 条（F10 调试可接受）。
4. 修复 A 的既有语义副作用（报告 §2 RISKS 已记）：商店内未悬停商品时 F1 沿用上次缓存；非本轮回归。
5. `cursorDiagLogged` 只在 `Clear()`（失泡/切菜单/禁用）重置（ExtendedTextOverlay.cs:457），重置时机正确；F10 关闭时短路，零额外开销。

EVIDENCE:
- `git show 868e299`：src 仅 ModEntry.cs +18/-4、ExtendedTextOverlay.cs +19/-1；`git diff --name-status 868e299~1..868e299` = 3 文件（2 src + stage 文档）。
- 修复 A（ModEntry.cs:317-335）：`Length<=9` early-return 原语义等价；仅 `is ShopMenu && __args[9] is not Item` 跳过；body/title/args 三行赋值未变 → 非商店菜单零变化；`KnowledgeCardMenu` 重放仍拿完整 args 克隆。空引用安全（`is not Item` 判 null）、数组越界由长度闸门保证。
- Cecil：ShopMenu.draw IL_0aa7/0xaed 两次 `drawToolTip`（params[3]=Item；其 IL_0167 再调 drawHoverText string 重载）→ 商品/背包 tooltip 的 args[9]=Item 仍缓存；IL_0c80 问候语 `drawHoverText` 的 hoveredItem 为 IL_0c39 `ldnull` → 被忽略。参数名确认 index9=hoveredItem:Item、index1=text、index6=boldTitleText。
- 修复 B（ExtendedTextOverlay.cs:216-232）：三参 `drawMouse(b,bool,int)` 与游戏签名一致（IClickableMenu.cs:164）；日志三重闸门（DebugRects/!cursorDiagLogged/monitor），关闭时无插值开销；未动气泡布局、命中、桥接、DrawBubble。
- 无回退/越界：`git diff 1ca7e6b..868e299 -- TooltipTextCapture.cs TextCapture.cs WordOverlay.cs BubbleRenderer.cs KnowledgeCardMenu.cs SpriteTextWordLayout.cs manifest.json csproj DECISIONS.md BUG_PATTERNS.md` 为空；首轮 AllowedMenus 8 类型与类注释保留（ExtendedTextOverlay.cs:20-30）；对话光标路径（ModEntry.cs:444-449）未变；无版本号/dist/bin/obj/references 改动。
- 构建：`dotnet build -c Debug -p:EnableModDeploy=false` → 0 error（1×既有 CS9057 警告）；工作树 clean @868e299。
