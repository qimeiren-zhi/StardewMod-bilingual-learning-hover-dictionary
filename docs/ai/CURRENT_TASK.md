# 当前任务卡

> 本文件描述「当前正在做/刚完成」的任务；新会话若接手，先确认下面「任务」是否仍是当前任务。

- **任务**：生成交接包（AGENTS.md + docs/ai/*），并确认 Git 已回滚到 `0472442` 的基线状态。
- **范围**：
  - 只读扫描项目结构、源码、文档；
  - 创建/更新 `AGENTS.md`、`docs/ai/HANDOFF.md`、`docs/ai/BUG_PATTERNS.md`、`docs/ai/DECISIONS.md`、`docs/ai/CURRENT_TASK.md`；
  - 确认 git HEAD == `0472442`、工作区干净、发布包存在。
- **非范围**：
  - **不修改业务代码**（`src/` 下任何 .cs / .csproj / manifest.json / assets）；
  - 不重做其他 Agent 的「阶段2」（`backup/other-agent-807c988`）；
  - 不执行发布（Nexus/GitHub 上传）。
- **验收标准**：
  1. 5 个交接文件存在且内容完整（八项内容覆盖）；
  2. `git log -1` == `0472442`，`git status --short` 为空；
  3. `dist\BilingualLearning 0.5.0.zip` 存在；
  4. 构建可复现（`dotnet build` 0 error，若需要）。
- **相关文件**：
  - `AGENTS.md`、`docs/ai/HANDOFF.md`、`docs/ai/BUG_PATTERNS.md`、`docs/ai/DECISIONS.md`、`docs/ai/CURRENT_TASK.md`
  - `src/BilingualLearning/ModEntry.cs`（入口 L34）
  - `src/BilingualLearning/WordOverlay.cs`（OnHover L55 / Draw L132）
  - `src/BilingualLearning/KnowledgeCardMenu.cs`（draw L72 / UpdateHover L163）
  - `src/BilingualLearning/manifest.json`（版本 0.5.0）
  - `开发计划与节点控制.md`（§9 版本记录）
- **相似结构检查点**（改代码前先看这些同类实现，避免重复造轮子/漏 hook）：
  - 气泡渲染：`BubbleRenderer.Draw`（卡片复用）与 `WordOverlay.Draw`（对话自带）两处；
  - 文本捕获：`TooltipTextCapture`（卡片专用，只在 `Capturing` 时记录）；
  - 连通判定：`WordOverlay` 的 `bridgeRect` 与 `KnowledgeCardMenu.ComputeBridge` 是同一套思路；
  - Harmony 补丁集中注册：`ModEntry.cs:58/68/85/110`。
- **验证命令/日志**：
  - 构建：`$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "D:\Project\星露谷双语mod\src\BilingualLearning\BilingualLearning.csproj" -c Debug`
  - 日志：`C:\Users\zhiqi\AppData\Roaming\StardewValley\ErrorLogs\SMAPI-latest.txt`
  - Git：`git -C "D:\Project\星露谷双语mod" log --oneline -1` / `status --short`
- **风险与回滚**：
  - 本次只写文档，不改业务代码 → 无构建风险。
  - 若误改代码：`git -C "D:\Project\星露谷双语mod" checkout -- .` 恢复。
  - 其他 Agent 的提交在 `backup/other-agent-807c988`，可用 `git diff 0472442..backup/other-agent-807c988` 查看。
