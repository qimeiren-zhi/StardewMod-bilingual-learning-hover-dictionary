# 阶段 5a 只读评审（master...stage/5a-click-swallow）

VERDICT: PASS

BLOCKING:
- 无。

NON_BLOCKING:
1. `IsPointerOnBubble`（ExtendedTextOverlay.cs:240-247）只依据 `HasBubble`，未绑定当前菜单；切菜单后到下一绘制帧前存在一帧旧气泡坐标窗口（作者已在 §2 RISKS 自述、未复现）。建议加 `ReferenceEquals(trackedMenu, Game1.activeClickableMenu)` 或菜单类型守卫，成本极低。
2. §2 写「右键缺失/继承时仅 Debug」，实际代码（ModEntry.cs:113-123）右键是静默 `continue`，无任何 Debug 日志；§0 元信息状态仍为 QUEUED。属文档与实现/流程的小偏差，建议改文案或补 LogLevel.Debug。
3. `SupportedMenus => AllowedMenus`（ExtendedTextOverlay.cs:33）把可变 `Type[]` 以 IReadOnlyList 暴露，内部可强转回数组改写白名单；可改 `Array.AsReadOnly(AllowedMenus)`。
4. 方法名用字符串字面量 `"receiveLeftClick"`/`"receiveRightClick"`（ModEntry.cs:95），同文件其它 patch 用 `nameof`；仅风格问题。
5. 本次仅编译未部署，12 个 patch 的实际注册数以静态分析为准，待用户 5 条实机用例兜底。

EVIDENCE:
- `git diff --stat master...HEAD` 仅 3 文件：ModEntry.cs(+54/-17)、ExtendedTextOverlay.cs(+3)、stage-5a.md；与 commit 8169c7e 相比 stage 文档只改 §2。
- 白名单 8 类型及顺序未变（ExtendedTextOverlay.cs:20-30）；仅新增只读视图 :33。
- ShopMenu 旧专用行为保留：同一 prefix 实现改名后仍同时挂左/右键（ModEntry.cs:125-128, 485-487）；`DialogueBox` 的 `BeforeClick`/hover hook 未动（:71-88, :479）。
- `AccessTools.Method` 后判 `clickMethod.DeclaringType != menuType` 跳过继承方法（:113-123），不会重复 patch 基类；右键缺失静默跳过合理（基类 `receiveRightClick` 为空）。
- Harmony 注册仍在 `Entry`、沿用既有 `harmony.Patch(original:, prefix:)` 风格；失败仅 Warn，且仅左键 Warn。
- 构建：`-p:EnableModDeploy=false` → 0 error，仅既有 CS9057；产物含 `BeforeExtendedMenuClick` 与 `SupportedMenus`。
- Cecil（1.6.15）复核：8 类型均声明 `receiveLeftClick(int,int,bool)`；`receiveRightClick` 自身 override 仅 LetterViewerMenu/SpecialOrdersBoard/Billboard/ShopMenu → 恰好 12 个，无重复；全部参数名 `x,y,playSound`，与 prefix `(int x,int y)` 匹配。
- 返回语义：`BeforeExtendedMenuClick` 返回 `IsPointerOnBubble != true`，false 吞点击；`ExtendLookup=Off` 时 `UpdateAndDraw` → `Clear()`（:177-181, :466-473）→ HasBubble=false → 恒放行。
- 右键原生语义仅在气泡（∪词∪桥接）内被阻断；气泡外 LetterViewerMenu 翻页/关信、SpecialOrdersBoard/Billboard `exitThisMenu` 不受影响。词矩形也属吞掉区域，Packet 已列为已知预期。
