# 阶段记录模板

> 用法：主控在新阶段开始时复制本文件为 `stage-<id>.md`，填入 §1 Packet；执行 Agent 完成后填 §2 报告；评审填 §3；主控合并后填 §4。
> 阶段文件是**唯一事实来源**；HANDOFF 只保留一行状态指针。

## 0. 元信息

| 字段 | 值 |
|---|---|
| 阶段 ID | `<3a>` |
| 分支 | `stage/<id>-<slug>` |
| 状态 | `QUEUED / IN_PROGRESS / BUILT / REVIEWED / AWAITING_USER / ACCEPTED / MERGED / BLOCKED` |
| 开始 / 结束 | `<YYYY-MM-DD>` / `<YYYY-MM-DD>` |
| 执行 Agent | `<name/id>` |
| 评审 Agent | `<name/id>` |

## 1. Packet（主控填写）

```text
<从 SUBAGENT_PROMPTS.md §1 复制并填写>
```

## 2. 执行报告（执行 Agent 填写，schema 见 §6）

```text
STATUS:
STAGE:
SUMMARY:
FILES:
VERIFY:
DECISIONS:
PITFALLS:
RISKS:
DOCS:
NEXT_READY:
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
