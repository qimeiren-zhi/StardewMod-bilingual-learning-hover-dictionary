# 当前任务卡

> 本文件描述「当前正在做/刚完成」的任务；新会话若接手，先确认下面「任务」是否仍是当前任务。

- **任务**：**0.6.0 发布准备已完成，等待用户实机确认**（`release/0.6.0` 分支未合并）。
- **立即要做（第一优先）**：
  1. 收用户 0.6.0 实机 4 条结果（SMAPI 显示 0.6.0 / 扩展面查词+星标 / 商店点击与 F9 / Off 行为）；
  2. 通过后：`git checkout master && git merge --no-ff release/0.6.0`，打 `v0.6.0` tag（可选），把 `stage-r1.md` §0 置 `MERGED`、§4 写验收结论，更新 HANDOFF/CURRENT_TASK/计划 §9；
  3. 合入后即可发布 Nexus（上传 `dist/BilingualLearning 0.6.0.zip`，勾 `AI Assisted`）。
- **下一开发任务（用户已提出，未排期）**：**需求 §10-5 取消 `Extended lookup` 开关、扩展面默认开启**。要点：
  - 移除 `ModConfig.ExtendLookup` 与 GMCM 选项（`ModEntry.RegisterConfigMenu`）；
  - `TextCapture.Enabled` 改为「白名单菜单（或 F10 调试）」；`ExtendedTextOverlay.IsEnabled` 去掉配置判断；
  - 同步 README/CHANGELOG/Nexus、AGENTS、需求 §10-5；需实机回归对话/卡片/9 个扩展面。
- **状态**：master `eca1969`（功能全齐、版本号仍 0.5.0）；`release/0.6.0` `835613f`（版本 0.6.0 + 文案 + 报告）；游戏 Mods 已部署 0.6.0；`dist` 有 0.6.0 zip（旧 0.5.0 仍留）。
- **已知遗留（非阻塞）**：
  - `MasteryTrackerMenu` 未实装（未实机，白名单已移除，代码保留）；
  - 需求池 §10：连字符分词、卷轴居中标题、同形多义词、商店 tooltip 抢焦点；
  - 白名单菜单内 tooltip 文本可能被捕获、中键/滚轮未覆盖；
  - 阶段 4 世界层未做（可选）。
- **非范围**：不擅自发布/上传；不升版本号（0.6.0 已定）；不改业务逻辑（发布阶段）。
- **验收标准（沿用）**：改动 ⊆ Packet IN_SCOPE；构建 0 error；评审 PASS；用户实机通过后合并。
- **相关文件**：
  - 发布记录：`docs/ai/stages/stage-r1.md`；改动清单/会话总结：`docs/ai/HANDOVER_2026-11-24.md`
  - 下一会话 Prompt：`docs/ai/NEXT_SESSION_PROMPT.md`
  - 交接：`docs/ai/HANDOFF.md`、`AGENTS.md`、`docs/ai/DEV_MODE.md`、`docs/ai/SUBAGENT_PROMPTS.md`
- **验证命令**：
  - 构建：`$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src\BilingualLearning\BilingualLearning.csproj" -c Debug`
  - 仅编译（游戏运行中必须）：加 `-p:EnableModDeploy=false`（仍会生成 zip）
  - 日志：SMAPI 控制台 / `%APPDATA%\StardewValley\ErrorLogs\SMAPI-latest.txt`
- **风险与回滚**：
  - 0.6.0 已部署；回滚可重新部署 0.5.0（`git checkout v0.5.0` 构建）或删除/替换 `Mods\BilingualLearning`；
  - 部署需关闭游戏（DLL 锁）。
