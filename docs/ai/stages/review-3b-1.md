# 阶段 3b-1 评审（CarpenterMenu 接入扩展查词）

VERDICT: PASS

BLOCKING: none

NON_BLOCKING:
1. `stage-3b-1.md` §2 VERIFY 写「git diff --stat = 1 文件」，但分支 diff 实为 2 文件（源码 1 + 本报告）；建议改为「源码 1 文件 / 分支 2 文件」。
2. §2 的「未部署」是执行时事实（符合 Packet），但主控已补带部署构建：`bin\Debug\BilingualLearning.dll` 与 `Mods\BilingualLearning\BilingualLearning.dll` SHA256 均为 `AE3B…E357`（65024 B）；建议在 §4 记录，避免 §2 被误读为当前未部署。
3. `CarpenterMenu.draw` 的 `drawHoverText`（IL_10e0，hoverText IL_108e）也被新白名单捕获，物品 tooltip 文本可能同样出气泡；非回归，列入实机观察项（用户测试 2）。
4. §0 状态仍为 `QUEUED`/Agent 占位，属主控 §4 收尾项。

EVIDENCE:
- `git diff --stat master...HEAD`：`docs/ai/stages/stage-3b-1.md`(+132) / `src/BilingualLearning/ExtendedTextOverlay.cs`(5+,2-)；`--name-status` = A/M，无 D；manifest/csproj/bin/obj/dist/references 零变更（manifest Version 0.5.0）。
- 源码 diff 仅两处：L14 注释补「阶段3b-1：木匠/建造菜单」；L25 `typeof(CarpenterMenu)`；L22-24 LetterViewerMenu/QuestLog/SpecialOrdersBoard 保留。`IsAllowedMenu`/`IsEnabled`/`ShouldCapture`/`RebuildWordRects`/`BuildSpriteFontHits`/`TextCapture.Add` 不在 diff。
- Cecil：`StardewValley.Menus.CarpenterMenu` Public、base `StardewValley.Menus.IClickableMenu`、`.ctor(String,GameLocation)` → `Type.IsInstanceOfType` 成立；`using StardewValley.Menus`（L8）可见，构建通过。
- Cecil：2 个 new 站点后紧跟 `Game1::set_activeClickableMenu`（`GameLocation::answerDialogueAction` IL_368b、`<ShowConstructOptions>b__448_0` IL_0011）→ 打开即 activeClickableMenu。
- Cecil `draw`：IL_043b/04a0/0515（描述三层）、0647/06af/073e（造价）、0892（数量）、096e/09cc/0a43（配料名）走 `Utility.drawTextWithShadow`；026d/0bc4 SpriteText 卷轴由 `drawBgScroll != -1` 闸门跳过，与 §2 一致。
- 构建：`dotnet build -c Debug -p:EnableModDeploy=false` → 1 warning CS9057、**0 error**（1.96 s）。
- 回归：ExtendLookup=Off 时 `TextCapture.Enabled=false`（ModEntry L64）、`UpdateAndDraw` 不重建矩形（L152-160），新增类型仅使 `ShouldCapture` 多一个白名单判断；信件/任务日志/订单板数组顺序未变，对话/F1 卡片路径未改。
- 每帧捕获量：木匠菜单约 10 条正文行（三层阴影经 ≤4px 去重），`BuildSpriteFontHits` 按词前缀 MeasureString，属既有量级，可接受。
