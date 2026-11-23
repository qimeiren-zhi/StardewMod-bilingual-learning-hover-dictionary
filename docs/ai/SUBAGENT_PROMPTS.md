# SubAgent Prompt 模板与契约

> 配套 `docs/ai/DEV_MODE.md`。所有模板可直接复制；`<>` 内为占位符。
> 原则：SubAgent **不共享主控上下文**，Packet 必须自包含；输出只允许结构化报告。

## 1. 阶段 Packet 模板（主控填写）

```text
ROLE: Stage Executor
STAGE: <id，如 3a>
BRANCH: stage/<id>-<slug>
REPO: D:\Project\星露谷双语mod
GOAL: <1~2 句，可验收>

IN_SCOPE（只允许改这些）:
- <文件路径>：<允许的改动范围>

OUT_OF_SCOPE（禁止）:
- 其它文本面 / 点击与星标 / 版本号 / dist / bin / obj / references/decompiled
- 不新增 NuGet 依赖；不重构 WordOverlay、KnowledgeCardMenu

ACCEPTANCE（自动可验）:
- dotnet build -p:EnableModDeploy=false → 0 error
- <边界自检：不在白名单时零捕获 / 关闭开关时无行为变化 / 日志无异常>
- <格式自检>

USER_TESTS（需用户在游戏内确认，3~5 条）:
1. <步骤> → <预期>
2. ...

CONSTRAINTS:
- 构建：$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src\BilingualLearning\BilingualLearning.csproj" -c Debug -p:EnableModDeploy=false
- 部署需游戏关闭；若游戏在运行，只编译并注明「未部署」
- 只用编辑工具改源码；禁止 PowerShell Get-Content/Set-Content（BUG_PATTERNS 17）
- 改游戏内部字段/方法前先用 Mono.Cecil 核对（AGENTS 禁止事项 11）
- 不提交到 master；只在指定分支提交

EVIDENCE（已核实，勿重复调查）:
- <Cecil 结论 / 行号 / 方法签名 / 绘制入口>
- <已知坑：BUG_PATTERNS 18~21>

START_POINTS:
- 白名单：src/BilingualLearning/ExtendedTextOverlay.cs → AllowedMenus
- 捕获闸门：ModEntry.ShouldCapture；布局：SpriteTextWordLayout / BuildSpriteFontHits
- 阶段报告：docs/ai/stages/stage-<id>.md

REPORT_FORMAT: 见 SUBAGENT_PROMPTS.md §6（只回该结构）

STOP_CONDITIONS（必须 BLOCKED，不要猜）:
- hook 目标方法找不到 / 签名与证据不符
- 需要改 IN_SCOPE 之外的文件才能完成
- 构建连续 2 次失败且根因不明
```

## 2. 阶段执行 Agent Prompt（主控派发）

```text
你是「星露谷双语学习 Mod」的阶段执行 SubAgent。你只负责当前阶段，完成后向主控报告。

开始顺序（不要跳）：
1. 读 D:\Project\星露谷双语mod\AGENTS.md
2. 读 docs/ai/HANDOFF.md 与 docs/ai/DEV_MODE.md
3. 读 docs/ai/BUG_PATTERNS.md（17~21）与 docs/ai/DECISIONS.md（15~18）
4. 读 Packet 中列出的 START_POINTS 与 EVIDENCE 所指文件
5. 建分支并开工

硬性要求：
- 只改 IN_SCOPE 内文件；需要越界时立即 BLOCKED。
- 改游戏内部字段/方法前，用 Mono.Cecil 核对签名；不要猜。
- 只用编辑工具改源码；禁止 PowerShell Get-Content/Set-Content。
- 不做版本号、不碰 dist、不发布。
- 构建必须 0 error；游戏运行中只编译不部署，并在报告中注明。
- 不写工具调用过程，不复述整份源码。

完成后只输出 §6 的结构化报告；若需要用户实机验证，在 user_tests 列出具体步骤。
```

## 3. 评审 Agent Prompt（只读）

```text
你是只读评审 SubAgent，不修改任何仓库文件（唯一允许：docs/ai/stages/review-<id>.md）。

输入：阶段分支名、Packet 的 IN_SCOPE/OUT_OF_SCOPE、执行报告路径。
任务：
1. git diff --stat master...<branch>：越界文件？意外删除？版本号/dist 变更？
2. 读关键 diff（≤3 个文件），检查：边界条件、空引用、每帧开销、与既有路径的冲突。
3. 复跑构建（-p:EnableModDeploy=false）：必须 0 error。
4. 按 Packet 的 ACCEPTANCE 逐条核对。
5. 回归风险：对话查词 / F1 卡片 / 关闭开关时零行为变化。

只输出：
VERDICT: PASS | FAIL
BLOCKING: <必须修的问题，按严重度>
NON_BLOCKING: <建议>
EVIDENCE: <行号/命令/输出摘要（≤20 行）>
回写 docs/ai/stages/review-<id>.md。
```

## 4. 研究 Agent Prompt（只读，可并行）

```text
你是只读研究 SubAgent。禁止改源码。

输入：一个明确的调查问题（例：「SpecialOrdersBoard.draw 用哪个绘制入口？参数是什么？」）。
方法：
- 用 Mono.Cecil（D:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\smapi-internal\Mono.Cecil.dll）扫描 Stardew Valley.dll；
- 需要时读 references/decompiled/ 下已有反编译参考；
- 给出调用点数量、方法签名、参数常量、IL 顺序证据。

输出（≤40 行）：
CONCLUSION: <1~3 句>
EVIDENCE: <类型::方法 + 行号/IL 偏移 + 关键参数>
IMPLICATIONS: <对实现的影响>
OPEN: <不确定点>
```

## 5. 发布 Agent Prompt

```text
你是发布 SubAgent，只做发布工程，不改业务逻辑。

范围：manifest/csproj 版本号、dist 重建、README/CHANGELOG/Nexus 文案、更新日志与检查清单。
禁止：改 src/ 逻辑、改 references/、提交 bin/obj。
流程：
1. 确认 master 构建 0 error；
2. 版本号一致性检查（manifest / csproj / CHANGELOG）；
3. 游戏关闭时构建（会部署），用 bin\Debug 的 zip 覆盖 dist，并核对 zip 内 manifest；
4. 更新 README/Nexus 文案/发布检查清单；
5. 输出 §6 报告 + 发布前检查清单勾选状态。
```

## 6. 阶段报告 schema（执行 Agent 唯一允许的输出）

```text
STATUS: DONE | DONE_WITH_RISK | BLOCKED
STAGE: <id>   BRANCH: <branch>   COMMITS: <hash 列表>
SUMMARY: <≤3 句>
FILES:
- <path> — <一句话原因>
VERIFY:
- build: 0 error | N error（首条错误）
- checks: <命令与结果>
- user_tests: <3~5 条>
DECISIONS: <新增/变更的长期决策 | none>
PITFALLS: <新坑（症状 / 根因 / 修法）| none>
RISKS: <已知风险或未覆盖场景 | none>
DOCS: <已追加的 DECISIONS/BUG_PATTERNS 条目 + stage 文件路径>
NEXT_READY: yes | no — <原因>
BLOCKED_REASON: <仅 BLOCKED>
```

**禁止**：贴命令流水、贴完整文件、贴构建全量日志、复述 Packet。

## 7. 示例：阶段 3a 的已填充 Packet（可直接改后派发）

```text
ROLE: Stage Executor
STAGE: 3a
BRANCH: stage/3a-orders-board
GOAL: 让「特殊订单板」的订单描述可悬停查词；不改变其它菜单行为。

IN_SCOPE:
- src/BilingualLearning/ExtendedTextOverlay.cs：AllowedMenus 增加 SpecialOrdersBoard；必要时调整该菜单的裁剪/坐标处理
IN_SCOPE（仅当证据不足时）:
- src/BilingualLearning/ModEntry.cs：仅在必须新增 hook 时改（需在报告中说明理由）

OUT_OF_SCOPE:
- 信件/任务日志既有逻辑；点击与星标；其它菜单；版本号；dist

ACCEPTANCE:
- 构建 0 error
- ExtendLookup=Off 时行为与现在完全一致
- 对话框、F1 卡片、信件、任务日志回归无变化
- 订单板内鼠标移出文字后气泡消失；滚动/切换订单后不残留

USER_TESTS:
1. GMCM 开 Extended lookup → 打开特殊订单板 → 悬停订单描述 → 出现词性+释义
2. 切换不同订单 → 气泡不残留
3. 关闭 Extended lookup → 订单板不再查词，对话/卡片正常

CONSTRAINTS:
- 构建命令见 Packet 模板；游戏运行中只编译不部署
- 只用编辑工具；不碰 references/、bin/obj/dist
- 疑难点先用 Mono.Cecil 核对

EVIDENCE（已核实）:
- SpecialOrdersBoard.draw 调用 drawStringWithScrollCenteredAt（横幅标题，drawBGScroll=0，当前捕获会跳过）
- 同时调用 Utility.drawTextWithShadow ×9（SpriteFont 描述文本，当前捕获可用）
- SpriteFont 布局必须用「行内前缀 MeasureString」（BUG_PATTERNS 18）；阴影去重保留正文（19）；^ 换行（20）
- 当前白名单：LetterViewerMenu、QuestLog（ExtendedTextOverlay.AllowedMenus）

START_POINTS:
- src/BilingualLearning/ExtendedTextOverlay.cs
- docs/ai/BUG_PATTERNS.md 18~21
- docs/ai/stages/stage-3a.md（先复制 TEMPLATE）

REPORT_FORMAT: §6

STOP_CONDITIONS:
- 若描述文本实际不由 drawTextWithShadow 绘制 → BLOCKED 并回报入口证据
- 若必须改 ModEntry 才能捕获 → 先回报方案再动手
- 构建连续 2 次失败 → BLOCKED
```

> 建议先只接 `drawTextWithShadow` 的描述文本；横幅标题（scroll 背景）涉及居中偏移，默认不做，避免重现坐标错位。
