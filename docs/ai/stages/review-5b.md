# 阶段 5b 只读评审（master...stage/5b-star）

VERDICT: PASS

BLOCKING:
- 无。越界、构建、点击分层、写盘路径均符合 Packet，未发现必须打回的问题。

NON_BLOCKING:
1. 注释/文档口径不准：`ModEntry.cs:91,507` 与 DECISIONS #20 称「右键只吞不 toggle（与对话一致）」。反编译 `DialogueBox.receiveRightClick`（DialogueBox.cs:267-270）转调 `receiveLeftClick`，对话路径右键点星标实际**会** toggle；扩展面因右键 prefix 先吞而不会。扩展面行为正确且更严格，但「与对话一致」与事实相反，建议改文案（非代码问题）。
2. `Capitalize` 在 `ExtendedTextOverlay.cs:428-436` 复制了 `WordOverlay.cs:239-247` 的实现；与 `CleanSentence` 同理应提升复用，当前语义一致但有漂移风险。
3. `IsPointerOnStar`（:256-262）为 `public` 却只被 `TryToggleStar` 调用，可降为 `private`（可选）。
4. `Sentence` 粒度：SpriteText 路径在 `characterPosition` 截断时仍写整行原文（`SpriteTextWordLayout.cs:97-99`），SpriteFont 路径写整段捕获文本（`ExtendedTextOverlay.cs:501`），多行经 `CleanSentence` 压平；§2 RISKS 已自述，可接受。
5. `showStar` 使气泡 +34px 后，`ComputeLayout` 仍只把 x 钳到 `viewport-width`，width>viewport 时 x<0（既有问题，§2 已列）；另有 `CURRENT_TASK.md:17`「不改 WordOverlay」与本轮授权的 CleanSentence 可见性调整字面冲突（文档层面）。
6. 未实机：点击/写盘仅静态+构建验证，仍需 USER_TESTS 5 条兜底（本次亦未部署）。

EVIDENCE:
- `git diff --stat master...HEAD` = 4 源码 + 2 文档：ExtendedTextOverlay.cs(+81/-5)、ModEntry.cs(+28/-5)、SpriteTextWordLayout.cs(+4)、WordOverlay.cs(+2/-2)、DECISIONS.md(+12)、stage-5b.md(+138)。BubbleRenderer/VocabData/Exporter/ModConfig 零改动，无越界。
- `Init` 签名 `ExtendedTextOverlay.cs:64-72`；唯一调用点 `ModEntry.cs:58`，参数顺序一致；vocabulary 在 Entry:43 构造、Init 前非空（`!` 合理），空引用分支仅为防御。
- 命中/布局：:204-221 记录 `hoveredSentence = hit.LineText` 且 `ComputeLayout(..., showStar:true)`；:246-254 `IsPointerOnBubble` = BubbleBox∪词∪桥接 + `trackedMenu` 守卫（5a 评审建议已落实）。
- StarBox 24×24 落在 BubbleBox 内（BubbleRenderer.cs:39-40,55）→ `IsPointerOnBubble` 自动覆盖星标；左键 prefix（ModEntry.cs:491-505）先 `TryToggleStar`、命中返回 false，再走气泡吞，顺序正确；右键 :508-511 不调用 toggle。
- 右键不会误 toggle：`LetterViewerMenu.receiveRightClick→receiveLeftClick` 的委托在气泡内已被右 prefix 吞掉，到不了左 prefix（5a 文档已确认该委托）；气泡外无星标可命中。
- toggle 语义与对话基线一致：`:283-310` Contains→Remove / dictionary 取 Pos/Translation / `CleanSentence(hoveredSentence)` / FirstSeen 年+季(首字母大写)+日；`Vocabulary.Add/Remove` 内部立即 `Save→WriteJsonFile`（Vocabulary.cs:37-54）。
- Off 零影响：`ModConfig.cs:21` 默认 Off；`IsEnabled` :315-323 false → :181-185 `Clear()` → HasBubble=false → `TryToggleStar`/`IsPointerOnBubble` 恒放行（:270-273）。
- `LineText` 覆盖全部 hit 构造点：SpriteTextWordLayout.cs:95-100 与 ExtendedTextOverlay.cs:498-503；`WordOverlay.BuildWordHits` 复用同一 Build（WordOverlay.cs:391-394）。
- `CleanSentence` 仅 private→internal static（WordOverlay.cs:233-237），调用点 :189 同程序集；`SpriteWordHit` 新字段纯附加 → 对话星标、F1 卡片（KnowledgeCardMenu 未改）零行为变化。
- 构建：`dotnet build -p:EnableModDeploy=false` → 0 error，仅既有 CS9057 警告；未部署（无游戏进程；评审前工作树干净，仅本轮新增本文件）。
