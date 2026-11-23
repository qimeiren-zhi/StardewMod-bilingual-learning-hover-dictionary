# 阶段 3b-4 评审结论（review-3b4，只读）

VERDICT: PASS

BLOCKING: 无。src 仅 +3/-2（类注释 + AllowedMenus 追加 1 行 typeof(ShopMenu)）；旧 7 类型全保留；无删除/越界/版本号/dist/bin/obj；构建 0 error。

NON_BLOCKING:
1. tooltip 文本确实会被「通用捕获」收进词矩形：ShopMenu.draw 内 2×drawToolTip + 1×drawHoverText（Cecil：IL_0aa7 / IL_0aed / IL_0c80）内部走 SpriteBatch.DrawString → AfterDrawStringCapture（ModEntry.cs:257-262）→ ShouldCapture 因 activeClickableMenu=ShopMenu 放行（ModEntry.cs:300-315）→ ExtendedTextOverlay 逐行建词矩形并裁到菜单矩形（ExtendedTextOverlay.cs:296-322）。所以 tooltip 文字可被悬停命中，只是未新增 tooltip 路径。
   - USER_TESTS #4 的影响：常态下 tooltip 画在 oldMouse+32（IClickableMenu.cs:1127），光标在框左上方 → 不出气泡；但靠近屏幕底/右的背包格会触发翻转（IClickableMenu.cs:1137-1150），框覆盖光标时命中 title/category 行即可能冒气泡。建议 #4 改写为「正常位置不出气泡；tooltip 翻转到光标处可能误触发（已知副作用，后续阶段排除）」，实机重点测背包最底行/最右列。
   - 与 F1 卡片不冲突：ShouldCapture 在 TooltipTextCapture.Capturing 或 activeClickableMenu is KnowledgeCardMenu 时直接 false（ModEntry.cs:303-306），卡片重放不会双记；ExtendLookup=Off 时 TextCapture.Enabled=false（ModEntry.cs:64-65）、IsEnabled 亦 false。
   - 风险低：气泡在 tooltip 之后绘制且只读，不吞点击；每帧仅悬停时多算一个 tooltip 的行（前缀 MeasureString，量级小）。建议后续阶段在 IClickableMenu.drawHoverText/drawToolTip 加 prefix/postfix 抑制 TextCapture（或过滤 tooltip 行）；同源问题已存在于 CarpenterMenu/Billboard/QuestLog（Cecil 各自 draw 内均有 drawHoverText）。
2. stage-3b-4.md §2 写「diff 仅两文件 / git diff = 2 文件」，实际 master...HEAD 含本阶段文档共 3 文件（DECISIONS +8、stage 文档 +138、ExtendedTextOverlay.cs +3/-2）；建议照 review-3b-3 改为「src 净 +3/-2；另加阶段文档」。
3. §0 元信息仍为 状态 QUEUED / 执行·评审 Agent <待派发>，待主控在 §4 前回填。
4. §2 RISKS 已自认 tooltip 捕获风险（stage-3b-4.md:109），与第 1 条一致，非漏报，仅 #4 的绝对表述过强。

EVIDENCE:
- `git diff --stat master...HEAD`：DECISIONS.md +8、stage-3b-4.md +138、ExtendedTextOverlay.cs +3/-2；`--name-status`：M DECISIONS / A stage doc / M ExtendedTextOverlay。
- 白名单 ExtendedTextOverlay.cs:20-30：LetterViewerMenu/QuestLog/SpecialOrdersBoard/CarpenterMenu/Billboard/ShippingMenu/ItemListMenu 全保留 + ShopMenu；IsAllowedMenu/IsEnabled/RebuildWordRects 无 diff。
- 敏感面零改动：`git diff master...HEAD --quiet -- ModEntry.cs TextCapture.cs SpriteTextWordLayout.cs KnowledgeCardMenu.cs TooltipTextCapture.cs manifest.json BilingualLearning.csproj` → exit 0；无 tracked bin/obj/dist/references。
- 构建：`dotnet build ... -c Debug -p:EnableModDeploy=false` → 0 error / 1×CS9057（既有）；bin\Debug\BilingualLearning.dll 与 Mods\BilingualLearning\BilingualLearning.dll SHA256 均 F6F22D84…08D1BA（部署 DLL 与本分支同源）。
- 闸门链路：AfterSpriteTextDrawString/AfterDrawStringCapture（ModEntry.cs:237-262）仅 ShouldCapture 通过才记；ShouldCapture 只认白名单（:308-312）+ 英文（:314）；Off 时 Enabled=false（:64）。
- tooltip 几何：drawHoverText 基址 getOldMouseX/Y+32（IClickableMenu.cs:1127-1128），右/下越界翻转（:1137-1150）；Cecil 实测 ShopMenu.draw 调用 2×drawToolTip + 1×drawHoverText。
- 回归面：对话/前 7 菜单逻辑未改；光标重绘仅 HasBubble 时（ModEntry.cs:446）；气泡绘制用 TextCapture.Suppressed 自抑制（ExtendedTextOverlay.cs:236-253）。
- 评审期间未改任何仓库文件（只新增本文件），未 checkout/commit/merge；游戏进程未运行，实机 5 条待用户验收。
