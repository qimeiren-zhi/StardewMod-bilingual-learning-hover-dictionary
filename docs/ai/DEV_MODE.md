# 开发模式：主控 Agent + 阶段 SubAgent

> 目的：让**主对话只承担项目管理**（节点控制、分发、验收、状态维护），把每阶段的实现放进独立的 SubAgent 上下文，避免长上下文导致注意力分散与编码质量下降。
> 适用：本仓库（星露谷双语学习 Mod）。所有阶段以 `AGENTS.md` 的硬性约束为前提。

## 1. 角色与职责

| 角色 | 上下文 | 职责 | 明确不做 |
|---|---|---|---|
| **主控 Agent**（主对话） | 长期、跨阶段 | 维护路线与状态；写阶段 Packet；分发；验收；合并；对用户汇报 | 不写业务代码、不读整份源码、不替 SubAgent 调试 |
| **阶段执行 Agent** | 单阶段、一次性 | 按 Packet 实现、构建、自检、写阶段报告；必要时提文档改动 | 不越界改文件、不升版本、不动 `dist/`、不发布 |
| **评审 Agent**（只读） | 单阶段 | 独立审 diff、复跑构建、检查边界与回归风险，给出 PASS/FAIL | 不改任何文件（只写 `docs/ai/stages/review-<id>.md`） |
| **研究 Agent**（只读） | 单问题 | Mono.Cecil/反编译/调用点调查，返回结论与证据 | 不改代码；不推测签名 |
| **发布 Agent** | 发布阶段 | 版本号、打包 `dist`、README/CHANGELOG/Nexus 文案 | 不改业务逻辑 |

**单写者原则**：任意时刻**最多 1 个 Agent 写 `src/`**。并行只允许「只读研究 / 只读评审」。

## 2. 阶段划分与串并行

### 2.1 阶段清单（与计划 §8.4 对齐）

| ID | 范围 | 写 src | 前置 | 并行性 |
|---|---|---|---|---|
| S3a | 特殊订单板 `SpecialOrdersBoard` | 是 | 当前基线 | 串行 |
| S3b | 菜单长尾：商店/木匠/日历/Billboard/发货箱/ItemList | 是 | S3a 验收 | 串行 |
| S3c | `JunimoNote` / 精通追踪 | 是 | S3b 验收 | 串行 |
| S4 | 世界层：NPC 头顶气泡 + `drawAboveAlwaysFrontLayer` 横幅 | 是 | S3c 验收 | 串行（高风险） |
| S5 | 扩展面星标收藏 / 点击吞掉 | 是 | S4 验收 | 串行（最高风险） |
| R1 | 发布：版本号、`dist` 重建、README/Nexus/CHANGELOG | 只改 manifest/文案 | S5 或用户指定冻结 | 串行（写） |
| P* | 只读研究（Cecil 调用点、某菜单绘制入口） | 否 | 任意 | **可并行** |
| V* | 只读评审（每阶段后一次） | 否 | 阶段构建通过 | 与主控验收并行（不写） |
| D* | 只读草稿（发布文案/截图清单） | 否 | 任意 | 可并行，但只能写 `docs/release/` 或 `docs/ai/stages/` |

> **实际执行（2026-11-24 更新）**：S3b 拆成 3b-1 木匠 / 3b-2 任务板·日历 / 3b-3 发货箱·丢物 / 3b-4 商店；S5 拆成 5a 点击吞掉 / 5b 星标；S3c 的 Mastery 因未实机未实装。全部已完成（R1 待用户确认）。阶段记录见 `docs/ai/stages/`。

### 2.2 结论

- **代码阶段严格串行**：同一工作树、同一批文件（`ModEntry.cs`、`ExtendedTextOverlay.cs`、`TextCapture.cs` 等），并行写必然互相覆盖。
- **只有只读任务可并行**：研究、评审、文档草稿；与写阶段并行时不得 `git add` 业务文件。
- **阶段内部也串行**：执行 → 构建 → 评审 → 用户实机验收 → 合并。评审与执行不同时。
- **并行上限**：1 个写 Agent + 1 个只读 Agent。

### 2.3 依赖图

```
S3a ──► S3b ──► S3c ──► S4 ──► S5 ──► R1
  │        │        │       │       │
  └─V──┘   └─V──┘   └─V──┘  └─V──┘  └─V──┘        (V=只读评审，紧随其阶段)
P（研究）可挂在任意阶段旁边；D（草稿）仅限 docs/release、docs/ai/stages
```

## 3. 阶段状态机

```
QUEUED → IN_PROGRESS → BUILT → REVIEWED → AWAITING_USER → ACCEPTED → MERGED
                ↑            │        │
                └─ FIXING ◄──┴────────┘   (失败/评审 FAIL/用户打回)
```

- `AWAITING_USER` 是**用户实机验收门**：主控不得自行通过（UX/可玩性只有用户能确认）。
- 连续 2 次 `FIXING` 未解决 → 主控升级给用户（给选项：缩小范围/回滚/换方案）。

## 4. Git 与分支

- 每阶段一个分支：`stage/3a-orders-board`、`stage/3b-menu-tail`…
- 执行 Agent 在分支上提交（小步、语义化）；**不合并**。
- 主控验收通过后 `git checkout master && git merge --no-ff stage/<id>`。
- 失败/打回：`git checkout master`，分支保留取证，必要时 `git branch -D`。
- 合并前检查：`git diff --stat master...stage/<id>` 只含允许文件；无 `bin/obj/dist`；无版本号变更（除非 R1）。

## 5. 文档所有权（防冲突）

| 文件 | 写者 | 说明 |
|---|---|---|
| `AGENTS.md` | 主控 | 长期规则，阶段不改 |
| `docs/ai/HANDOFF.md` | 主控 | 每阶段验收后更新「当前状态/下一步」 |
| `docs/ai/CURRENT_TASK.md` | 主控 | 每阶段开始/结束各更新一次 |
| `docs/ai/DECISIONS.md` | 执行（追加）+ 主控（复核） | 仅记录**长期决策**，一个决策一条 |
| `docs/ai/BUG_PATTERNS.md` | 执行（追加）+ 主控（复核） | 仅记录**可复现的坑**，含症状/根因/修法/关键词 |
| `docs/ai/stages/stage-<id>.md` | 执行（写报告）+ 主控（写验收结论） | 阶段唯一事实来源 |
| `docs/ai/stages/review-<id>.md` | 评审 | 只读评审结论 |
| `README/CHANGELOG/docs/release` | 发布 Agent | 代码阶段不动 |
| `docs/planning/开发计划与节点控制.md` | 主控 | 阶段状态标记 |

**规则**：执行 Agent 只在「引入长期决策/新坑」时追加 DECISIONS/BUG_PATTERNS，并在报告里列出；主控验收时只读这几段，不读全文。

## 6. 输入契约：阶段 Packet

主控给执行 Agent 的 Prompt 必须**自包含**（SubAgent 不共享上下文），使用固定模板（见 `SUBAGENT_PROMPTS.md` §1）：

1. `STAGE` / `ROLE` / `BRANCH`
2. `GOAL`（1~2 句）
3. `IN_SCOPE`：允许修改的文件白名单 + 行为范围
4. `OUT_OF_SCOPE`：明确禁止（其它文本面、点击/星标、版本号、dist、发布）
5. `ACCEPTANCE`：可自动验证的条目（构建、日志、边界）
6. `USER_TESTS`：需要用户在游戏里点的 3~5 条
7. `CONSTRAINTS`：构建命令、游戏关闭才能部署、只用编辑工具、不改 `references/`
8. `EVIDENCE`：已核实的事实（Cecil 结果、行号、已知坑），避免 SubAgent 重复调查
9. `START_POINTS`：从哪个文件/方法开始（如 `AllowedMenus`）
10. `REPORT_FORMAT`：输出 schema（§7）
11. `STOP_CONDITIONS`：何时必须 BLOCKED 而不是猜

> Packet 长度上限约 150 行；用路径引用文档，不粘贴全文。

## 7. 输出契约：阶段报告

执行 Agent 的**最终回复**只允许以下结构（≤120 行，禁止贴工具调用流水、禁止贴整段源码）：

```
STATUS: DONE | DONE_WITH_RISK | BLOCKED
STAGE: <id>   BRANCH: <branch>   COMMITS: <hash...>
SUMMARY: <不超过 3 句>
FILES:
- <path> — <一句话原因>
VERIFY:
- build: 0 error | N error（附首条错误）
- checks: <跑过的命令/自检>
- user_tests: <3~5 条待用户在游戏内确认>
DECISIONS: <新增/变更的长期决策，无则 none>
PITFALLS: <新坑（症状/根因/修法），无则 none>
RISKS: <已知风险/未覆盖场景，无则 none>
DOCS: <已追加到 DECISIONS/BUG_PATTERNS 的条目；stage 文件路径>
NEXT_READY: yes | no — <原因>
BLOCKED_REASON: <仅 BLOCKED 时>
```

主控**只消费这份报告**；需要细节时按报告里的路径定点阅读。

## 8. 主控上下文预算

- 每阶段主控最多读：1 份阶段报告 + 1 次 `git diff --stat` + ≤1 个文件的 ≤200 行抽查 + 构建最后 10 行。
- 不读：SubAgent 的命令历史、完整构建日志、整份源码、整份历史 diff。
- 记忆外置：阶段结论写进 `docs/ai/stages/stage-<id>.md`，HANDOFF 只留一行指针 + 状态。
- 用户汇报：固定 5 行状态板（阶段/状态/改动/待验收/风险），不转述报告全文。

## 9. 验收清单（主控每阶段必做）

1. `git diff --stat master...stage/<id>`：文件是否都在 `IN_SCOPE` 内（越界=FAIL）。
2. 构建：`dotnet build -p:EnableModDeploy=false` → 0 error（执行 Agent 已跑，主控复跑一次）。
3. 评审 Agent 结论 = PASS（或主控给出等价人工结论）。
4. 报告 schema 完整；`STATUS` 非 `BLOCKED`。
5. 文档：`stage-<id>.md` 已写；HANDOFF/CURRENT_TASK 已更新；有长期决策/新坑则已追加。
6. 无 `dist/`、`bin/`、`obj/`、`tools/ecdict.csv`；无版本号变更（R1 除外）。
7. 给用户 3~5 条实机测试脚本；`AWAITING_USER` 等待确认。
8. 用户确认 → 合并 → `stage-<id>.md` 写验收结论 → 下一阶段。

## 10. 失败处理

| 情况 | 主控动作 |
|---|---|
| 构建失败 | 打回同一 Agent（保持上下文）修复，最多 2 轮 |
| 越界改动 | 直接回滚该分支，重发 Packet（强调边界） |
| 缺少 hook/签名不符 | 必须 BLOCKED；主控派研究 Agent 用 Mono.Cecil 核实后再决定 |
| 用户实机打回 | `send_message` 继续同一 Agent（保留上下文），最多 2 轮；仍不行则拆分范围 |
| 连续 2 次 BLOCKED | 升级用户：缩小范围 / 换方案 / 暂停 |

## 11. 起步方式

1. 主控每会话先读：`AGENTS.md` → `docs/ai/HANDOFF.md` → 本文件 → `docs/ai/CURRENT_TASK.md`。
2. 建 `stage/<id>` 分支与 `docs/ai/stages/stage-<id>.md`（复制 `TEMPLATE.md`）。
3. 用 `SUBAGENT_PROMPTS.md` §1 生成 Packet → 派发执行 Agent。
4. 按 §9 验收。

> 提示：阶段 3a 的可用起点与已知坑见 `docs/ai/HANDOVER_2026-11-23.md` 与 `BUG_PATTERNS.md` 18~21。
