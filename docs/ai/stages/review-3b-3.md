# 阶段 3b-3 评审结论（review-3b3，只读）

VERDICT: PASS

BLOCKING: 无。src 净 +4/-2（仅注释 + AllowedMenus 两行）；无删除、无越界；构建 0 error。

NON_BLOCKING:
1. stage-3b-3.md §2 VERIFY 写「git diff --stat → 仅 ExtendedTextOverlay.cs（+4/-2）」，实际 `master...HEAD` 还新增本阶段文档（+137）。建议改为「src 净 +4/-2；另加阶段文档」。
2. Packet 的 START_POINTS 行号已过期：改后 AllowedMenus L20-29、IsAllowedMenu L128（原写 L123）、RebuildWordRects L275（原写 L270）；§0 的「评审 Agent 〈待派发〉」待主控回填。
3. ShippingMenu 满屏矩形（viewport 宽高）使 crop 不裁到实际绘制框（约 1280×920 居中）；本阶段只读气泡、该菜单无框外正文，风险低，已列入 RISKS，3b-4 前可考虑改为与绘制框取交。
4. ItemListMenu 仅能由事件命令打开（未实机验证），用户测试 4 若 `debug` 不可用会跳过，形成覆盖缺口；建议主控确认 debug 命令可用。
5. ShippingMenu 金额右对齐文本依赖 TextCapture 对 drawString x/width 的复刻；用户测试 1 已点名悬停总金额，实机需重点确认该行命中框。

EVIDENCE:
- `git diff --stat master...HEAD`：docs/ai/stages/stage-3b-3.md +137、src/BilingualLearning/ExtendedTextOverlay.cs +4/-2；`--name-status` 仅 A 文档 / M 单文件。
- 白名单 ExtendedTextOverlay.cs:20-29：原 5 类保留（LetterViewerMenu/QuestLog/SpecialOrdersBoard/CarpenterMenu/Billboard）+ ShippingMenu + ItemListMenu。
- 未改敏感面：`git diff master...HEAD --quiet -- ModEntry.cs TextCapture.cs SpriteTextWordLayout.cs manifest.json *.csproj` → exit 0；`git ls-files` 无 bin/obj/dist。
- `using StardewValley.Menus` 已在 ExtendedTextOverlay.cs:8；Cecil 核对 Stardew Valley.dll：ShippingMenu/ItemListMenu 均 `: StardewValley.Menus.IClickableMenu`，命名空间 StardewValley.Menus。
- Cecil ctor：ShippingMenu `base(0,0,uiViewport.Width,uiViewport.Height,false)`；ItemListMenu 设 width=min(800,W-128)、height=min(720,H-128)、x/y 居中 → 两菜单矩形字段有效。
- Cecil draw：两菜单无 drawHoverText/drawToolTip 调用，正文仅 SpriteText.drawString（含 drawStringWithScrollCenteredAt/…HorizontallyCenteredAt）；scroll 调用被 TextCapture RegisterSpriteText 的 drawBGScroll!=-1 跳过。
- 构建：`dotnet build ... -c Debug -p:EnableModDeploy=false` → 0 错误，1 警告 CS9057（已知无害）；bin\Debug\BilingualLearning.dll 与 Mods\BilingualLearning\BilingualLearning.dll SHA256 均为 A1DF50D7…392E80E（内容一致，部署版与本次构建同源）。
- ExtendLookup=Off 零变化：IsEnabled/ShouldCapture/RebuildWordRects/BuildSpriteFontHits 未在 diff；ShouldCapture(ModEntry.cs:300) 仍仅在白名单菜单捕获，F1 卡片/Capturing 旁路不变。
- 前 5 菜单与对话：仅扩白名单集合，逻辑分支未改；光标重绘仅 HasBubble 时执行（ModEntry.cs:446），不干扰 OK 按钮等原生交互。
- 评审前工作树 clean（HEAD 3c0d349）；评审后仅新增本文件（未提交），未改动任何仓库源码。
