# 阶段 3c 只读评审（master...stage/3c-junimo-mastery）

VERDICT: PASS

BLOCKING:
- 无。

NON_BLOCKING:
1. 文档状态未同步：`stage-3c.md` §0 仍 `QUEUED`、执行/评审 Agent 为占位符，§3 仍是模板；`HANDOFF.md:16/160/174` 仍把 3c 列为待做/下一步。合并时更新（不在 IN_SCOPE，但别漏）。
2. 未收到 `canReadJunimoText` 邮件时 `scrambledText=true`（Cecil：setUpMenu IL_0091-00a4），JunimoNoteMenu 标题/正文按 Junimo 字形绘制（draw IL_0101/017f 传 `junimoText=true`）；SpriteText 对 `junimoText` 强制 `fontPixelZoom=3`（SpriteText.cs:653-656），而 `SpriteTextWordLayout` 用当前 FontPixelZoom 建框（SpriteTextWordLayout.cs:45-47）→ 命中框会偏移；且捕获到的是英文原串，悬停符文可能显示英文词。属「未读状态」边界，建议 USER_TEST 在拿到该邮件前后各跑一次。
3. 未实机、未部署（报告已如实写明），气泡位置/点击吞掉仍靠 5 条 USER_TESTS 兜底。
4. §2 RISKS ①②③ 与 Packet 已知缺口一致；`getRewardNameForArea` 的底部卷轴奖励名确被 `drawBGScroll=+1` 过滤（Cecil draw IL_0a61），记录属实。

EVIDENCE:
- `git diff --stat master...HEAD`：仅 `docs/ai/stages/stage-3c.md`(+130) 与 `src/BilingualLearning/ExtendedTextOverlay.cs`(+4/-2)；工作树干净；无 ModEntry/TextCapture/布局改动。
- 白名单 `ExtendedTextOverlay.cs:20-32`：10 项，旧 8 项原样保留，新增 `typeof(JunimoNoteMenu)`、`typeof(MasteryTrackerMenu)`；`using StardewValley.Menus` 在 :8，二者均属该命名空间（Cecil 确认）；`IsInstanceOfType` 遍历在 :148-155。
- 自动覆盖：`ModEntry.cs:94-133` 在 Entry 遍历 `SupportedMenus`（即 AllowedMenus）注册左右键 prefix；Cecil 确认两类型各自声明 `receiveLeftClick/receiveRightClick(int,int,bool)`（DeclaringType==自身）→ prefix 生效，且不会重复 patch 基类。
- 星标/吞点击链：`TryToggleStar`/`IsPointerOnStar`/`IsPointerOnBubble` :248-313；左 prefix ModEntry.cs:491-505 先 toggle 再吞，右键 :508-511 只吞；`ExtendLookup=Off` 默认（ModConfig.cs:21）→ IsEnabled false（:315-323）→ Clear（:183-186），HasBubble=false → 两个 prefix 恒放行。
- 回归：旧 8 菜单类型/顺序未变，新增仅 2 项；对话/F1 卡片未触及；`ShouldCapture()`（ModEntry.cs:345-360）使新菜单在 ExtendLookup=On 时才开始捕获。
- 构建：`dotnet build ... -p:EnableModDeploy=false` → 0 error，仅既有 CS9057 警告；未部署（工作树仍干净，唯一新增为本评审文件）。
