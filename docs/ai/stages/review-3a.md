# 阶段 3a 评审（只读评审，未改仓库源码）

评审对象：`master...stage/3a-orders-board`（1d117fa feat + 5b53215 docs packet）

```text
VERDICT: PASS
BLOCKING: 无。
NON_BLOCKING:
1. §2 REPORT VERIFY「游戏进程未运行；部署目录 DLL 仍为 20:20 旧版本 → 确认未部署」与实测不符：Mods\BilingualLearning\BilingualLearning.dll mtime 22:17:58，SHA256 508CF55D…C9A3 与新构建 bin 完全一致（即 3a 代码已在部署目录）；RISKS「需先部署再测」已过期。建议执行 Agent 更正报告（不阻断，游戏未运行故部署无副作用）。
2. §2 COMMITS 写「本阶段单提交」，实际 master..HEAD 有 2 个提交（5b53215 docs Packet、1d117fa feat），与「1 个语义化提交」措辞有出入；建议改述。
3. 陈旧文案（OUT_OF_SCOPE，未要求本阶段改）：ModEntry.cs:308 注释仍写「阶段0/2：信件、任务日志」；GMCM「Extended lookup」描述仍为 letters/quest log，未含订单板。建议后续阶段一并同步。
4. 顶部 ChooseOne 横幅（SpriteText）不接管、超出菜单矩形的文本不可悬停，均为 Packet 已声明预期，无新增风险。
EVIDENCE:
- git diff --stat master...HEAD：docs/ai/stages/stage-3a.md 133+/0-、src/BilingualLearning/ExtendedTextOverlay.cs 3+/2-；name-status 仅 A(文档)/M(单文件)。无越界文件、无删除、无 manifest/csproj/版本号/dist/bin/obj 变更；工作树干净。
- diff 仅两处：L14 类注释追加「阶段3a」；AllowedMenus L24 新增 typeof(SpecialOrdersBoard)（L22-24 保留 LetterViewerMenu、QuestLog，共 3 项）。
- 命名可见性：ExtendedTextOverlay.cs:8 `using StardewValley.Menus;`，SpecialOrdersBoard 编译解析成功。
- IsInstanceOfType：ExtendedTextOverlay.cs:131-137 逐类型 IsInstanceOfType，子类亦命中（安全，无 sealed/子类误伤）。
- 闸门未变：IsEnabled（:213-221）、UpdateAndDraw（:151-159）、RebuildWordRects（:271-319）、BuildSpriteFontHits（:343-395）diff 零改动；ModEntry.cs:309 走同一 IsAllowedMenu；ModEntry.cs:64-65 每帧 Enabled 逻辑未改 → ExtendLookup=Off 零行为变化。
- 回归：对话/F1 卡片路径（ModEntry.cs:303-306 ShouldCapture 排除 KnowledgeCardMenu、TooltipTextCapture）与 WordOverlay 未触及。
- 构建：dotnet build -c Debug -p:EnableModDeploy=false → 「0 个错误」，1 警告 CS9057（既有分析器版本警告）；产物 bin\Debug\BilingualLearning.dll。
- 未执行：实机 4 条 USER_TESTS（需用户在游戏内验收）。
```
