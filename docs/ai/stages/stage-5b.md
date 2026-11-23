# 阶段 5b：扩展面星标收藏

> 5a 已建立「气泡区域吞点击」通道；5b 在该通道里先 toggle 星标，再吞点击。
> 星标视觉/写盘复用对话路径基线：`WordOverlay.ToggleStar` + `BubbleRenderer`（`showStar/starred`）。

## 0. 元信息

| 字段 | 值 |
|---|---|
| 阶段 ID | `5b` |
| 分支 | `stage/5b-star` |
| 状态 | `QUEUED` |
| 开始 / 结束 | 2026-11-24 / — |
| 执行 Agent | `<待派发>` |
| 评审 Agent | `<待派发>` |

## 1. Packet（主控填写）

```text
ROLE: Stage Executor
STAGE: 5b
BRANCH: stage/5b-star
REPO: D:\Project\星露谷双语mod
GOAL: 扩展文本面的气泡显示星标，点击星标可收藏/取消收藏（立即写盘）；其它区域与其它菜单行为不变。

IN_SCOPE（只允许改这些）:
- src/BilingualLearning/ExtendedTextOverlay.cs：
  - `Init` 增加 `Vocabulary vocabulary` 参数并保存字段；
  - 命中词时记录「所在行文本」用于 `VocabEntry.Sentence`；
  - `ComputeLayout(..., showStar: true)`；`BubbleRenderer.Draw(..., starred: vocabulary.Contains(word), ..., showStar: true)`；
  - 新增 `public bool TryToggleStar(int x, int y)`：`HasBubble && bubbleLayout.StarBox.Contains(x,y)` 时 toggle（dictionary 取 Pos/Translation，Sentence=所在行，FirstSeen=年/季/日），返回 true；
  - 新增 `public bool IsPointerOnStar(int x, int y)`（可选，供 prefix 用）。
- src/BilingualLearning/ModEntry.cs：
  - `extendedOverlay.Init(...)` 传入 `this.vocabulary`；
  - 左键 prefix 改为：先 `TryToggleStar(x,y)` → 命中返回 false；否则沿用 `IsPointerOnBubble` 吞；
  - 右键 prefix 保持「只吞不 toggle」（星标只响应左键，与对话一致）。
- src/BilingualLearning/SpriteTextWordLayout.cs：`SpriteWordHit` 增加 `public string LineText = "";`，`Build` 给每个 hit 填 `s`。
- src/BilingualLearning/WordOverlay.cs：把 `CleanSentence` 提升为 `internal static`（供扩展面复用；不改行为）。
- docs/ai/stages/stage-5b.md：填 §2 执行报告。
- docs/ai/DECISIONS.md：追加决策 #20「扩展面星标复用对话路径的 Vocabulary 与气泡布局，不新建收藏体系」。
- docs/ai/BUG_PATTERNS.md：仅当出现新可复现坑时追加（编号接续 26）。

OUT_OF_SCOPE（禁止）:
- 星标视觉重设计、`BubbleRenderer.cs` 逻辑（`showStar/starred` 已够用，不改）
- 生词本数据格式/导出格式（`VocabData`/`Exporter` 不改）
- 其它菜单（3c/4）、GMCM 新选项
- manifest/csproj/版本号/dist/bin/obj/references；不新增依赖

ACCEPTANCE（自动可验）:
- dotnet build -p:EnableModDeploy=false → 0 error
- git diff --stat：文件 ⊆ IN_SCOPE
- `ExtendLookup=Off`：无气泡、无星标、点击零变化
- 左键点星标 → toggle 且吞点击；点气泡其他区域 → 只吞不 toggle；右键 → 只吞
- 写盘：toggle 后 `Mods/BilingualLearning/data/<存档>.json` 立即出现/移除该词（`Vocabulary` 既有 Save）
- stage-5b.md §2 已填；DECISIONS #20 已追加

USER_TESTS（需用户在游戏内确认，5 条）:
1. 信件/任务日志：悬停词 → 气泡左侧出现灰星标；左键点星标 → 变亮（收藏），关闭菜单再打开同一词仍亮。
2. 再点一次星标 → 变灰（取消收藏）；按 F9 导出 CSV，能看到/看不到该词（Sentence 为所在行文本）。
3. 点击气泡内**非星标**区域（释义文字/背景）→ 不 toggle、不触发底层菜单动作；右键点气泡 → 不 toggle、不触发底层。
4. 特殊订单板/木匠/小镇任务板/商店：星标可点、不误触底层按钮（如接单/建造/购买）。
5. 关闭 Extended lookup → 无气泡无星标；商店/信件等点击与原生一致；对话与 F1 卡片的星标行为不变。

CONSTRAINTS:
- 构建：$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src/BilingualLearning/BilingualLearning.csproj" -c Debug -p:EnableModDeploy=false
- 游戏运行中只编译不部署，并在报告注明「未部署」；主控会在验收前做带部署构建
- 只用编辑工具改源码；禁止 PowerShell Get-Content/Set-Content（BUG_PATTERNS 17）
- 只在 stage/5b-star 提交（1 个语义化提交）；不合并 master

EVIDENCE（已核实，勿重复调查）:
- `BubbleLayout` 已含 `BubbleBox/StarBox/TransPos`；`BubbleRenderer.ComputeLayout(..., showStar)` 里 `starBlock = Gap(10)+StarSize(24)`，`StarBox` 24×24 在气泡左内边距；`Draw(..., starred, ..., showStar)` 已实现亮/灰与图层。
- `ExtendedTextOverlay` 现状：`ComputeLayout(..., showStar:false)`（:210）、`Draw(..., starred:false, ..., showStar:false)`（:272-281）；`IsPointerOnBubble`（:241）已含气泡∪词∪桥接；`bubbleLayout` 字段可直接取 `StarBox`。
- `Vocabulary`：`Contains(word)` 忽略大小写；`Add(VocabEntry)` 立即 `WriteJsonFile`；`Remove(word)` 立即写盘；`VocabEntry{Word,Pos,Translation,Sentence,FirstSeen{Year,Season,Day}}`。
- 对话基线 `WordOverlay.ToggleStar`：已收藏→Remove；否则 dictionary 取 Pos/Translation、`Sentence=CleanSentence(box.getCurrentString())`、`FirstSeen=年/当前季/日`；本次复用同语义。
- `ModEntry` 现有：`extendedOverlay.Init(...)` 在 :58；patch 注册遍历 `SupportedMenus`（:93）；统一 prefix `BeforeExtendedMenuClick`（:485）当前左右键共用 → 5b 需拆分左/右键（左键先 toggle）。
- `SpriteWordHit` 只有 `Text/Box`；扩展面的 Sentence 需要建 hit 时带上所在行文本（`SpriteTextWordLayout.Build` 的 `s`；SpriteFont 路径用 `line.Text`）。
- 已知坑：星标让气泡宽 34px，靠近右边缘时 `ComputeLayout` 把 x 钳到视口内，但 width>视口 的极端情况不处理（可接受）；每次 toggle 立即写盘。

START_POINTS:
- src/BilingualLearning/ExtendedTextOverlay.cs（Init :62、UpdateAndDraw :190-215、DrawBubble :261、TryBuildTranslation :364）
- src/BilingualLearning/ModEntry.cs（Init :58、prefix :485、patch 注册 :93）
- src/BilingualLearning/BubbleRenderer.cs（只读参考）、`WordOverlay.cs`（ToggleStar :168、CleanSentence）
- docs/ai/stages/stage-5b.md

REPORT_FORMAT: SUBAGENT_PROMPTS.md §6（只回该结构）

STOP_CONDITIONS（必须 BLOCKED，不要猜）:
- 需要改 `BubbleRenderer` 才能画星标 → 先 BLOCKED（理论上不需要）
- `Vocabulary` 在菜单打开前未初始化/为 null → 报告并说明，不要静默吞异常
- 构建连续 2 次失败 → BLOCKED
```

## 2. 执行报告（执行 Agent 填写，schema 见 §6）

```text
STATUS: DONE
STAGE: 5b   BRANCH: stage/5b-star   COMMITS: 1 个（本提交，hash 见 git log）
SUMMARY: 扩展文本面气泡复用对话路径的 Vocabulary 与 BubbleRenderer，显示星标并支持左键收藏/取消（立即写盘）；左键先 toggle 星标再吞，右键只吞不 toggle；ExtendLookup=Off 零行为变化。
FILES:
- src/BilingualLearning/ExtendedTextOverlay.cs — Init 接入 Vocabulary；记录命中行文本；showStar:true；新增 IsPointerOnStar/TryToggleStar（toggle 后立即写盘）。
- src/BilingualLearning/ModEntry.cs — Init 传 vocabulary；左右键 prefix 拆分（左键先 TryToggleStar，右键只吞）。
- src/BilingualLearning/SpriteTextWordLayout.cs — SpriteWordHit 增加 LineText，Build 时填入整行文本。
- src/BilingualLearning/WordOverlay.cs — CleanSentence 提升为 internal static（仅可见性）。
- docs/ai/stages/stage-5b.md — 本报告。
- docs/ai/DECISIONS.md — 追加决策 #20。
VERIFY:
- build: 0 error（-p:EnableModDeploy=false；未部署，游戏运行中只编译）
- checks: git diff --stat 仅 4 个源码文件 + 2 个文档，⊆ IN_SCOPE；左键 prefix 先 TryToggleStar 后 IsPointerOnBubble；右键 prefix 不调用 TryToggleStar；Off 时 IsEnabled=false → 无气泡/wordRects 清空，两个 prefix 均放行；toggle 走 Vocabulary.Add/Remove（内部 Save→WriteJsonFile 立即写盘）。
- user_tests: 1) 信件/任务日志悬停词出现星标，左键点击收藏（变亮），重开菜单仍亮；2) 再点取消（变灰），F9 导出能看到/看不到该词且 Sentence 为所在行；3) 点气泡释义/背景不 toggle 也不触发底层，右键点气泡不 toggle；4) 订单板/木匠/任务板/商店星标可点且不误触接单/建造/购买；5) ExtendLookup=Off 无气泡无星标、点击与原生一致，对话与 F1 卡片星标不变。
DECISIONS: #20 扩展面星标复用对话路径的 Vocabulary 与气泡布局（已追加）。
PITFALLS: none（未出现新可复现坑；`SpriteWordHit` 新增字段纯附加，不影响对话路径）。
RISKS: 星标使气泡宽 34px，极窄视口/超长释义时 ComputeLayout 只把 x 钳回视口，width 超视口未处理（既有问题，可接受）；Sentence 取捕获行整行（多行段落含 ^，经 CleanSentence 变空格），与对话「当前整句」粒度略有差异；未实机验证，需用户按 USER_TESTS 验收。
DOCS: docs/ai/DECISIONS.md #20；本文件 §2。
NEXT_READY: yes — 构建 0 error，diff ⊆ IN_SCOPE，等待主控评审/用户实机验收。
```

## 3. 评审结论（评审 Agent 填写）

```text
VERDICT: PASS | FAIL
BLOCKING:
NON_BLOCKING:
EVIDENCE:
```

## 4. 验收结论（主控填写）

- 越界检查：`<git diff --stat 结果摘要>`
- 构建：`<0 error>`
- 用户实机：`<通过 / 打回原因>`
- 合并提交：`<merge commit hash>`
- 后续动作：`<下一阶段 / 修复项>`

## 5. 经验沉淀（可选）

- 新增决策：`DECISIONS #__`
- 新增坑：`BUG_PATTERNS #__`
- 未覆盖场景：
