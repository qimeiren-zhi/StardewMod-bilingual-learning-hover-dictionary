# 当前任务卡

> 本文件描述「当前正在做/刚完成」的任务；新会话若接手，先确认下面「任务」是否仍是当前任务。

- **任务**：阶段 3c（Junimo 收集包 + 精通追踪）**已合并**（`fbf1108`），**阶段 3 全部完成**。
- **发布前硬门槛（未完成）**：
  1. 补测 `MasteryTrackerMenu`（需满足精通条件的存档）——代码已接入、未实机，**未补测前不发布**；测试点见 `docs/ai/stages/stage-3c.md` §5。
  2. 重建 `dist/`（当前仍是 0.5.0 旧包）；版本号 0.5.0 → 0.6.0 待定。
- **本轮交付（3c）**：
  - `AllowedMenus` 新增 `JunimoNoteMenu`、`MasteryTrackerMenu`；点击吞掉/星标自动覆盖；
  - 用户要求：未读 Junimo 文（`junimoText=true`）不参与查词 → `AfterSpriteTextDrawString` 直接跳过；
  - 评审 PASS；JunimoNote 实机通过。
- **状态**：构建 0 error（含部署，Mods DLL 与 bin 一致）；`stage/3c-junimo-mastery` 已 `--no-ff` 合并。
- **本次会话累计（相对 0.5.0）**：3a 订单板、3b-1 木匠、3b-2 任务板/日历、3b-3 发货箱/丢物、3b-4 商店正文、5a 点击吞掉、5b 星标、3c 收集包/精通。
- **下一步候选**：补测 Mastery → 发布准备（版本号/`dist`/README/CHANGELOG/Nexus）→ 阶段 4 世界层（可选）。
- **已知遗留（非阻塞）**：需求池 §10 四条（连字符分词、卷轴居中文本、同形多义词、商店 tooltip 抢焦点）；tooltip 文本可能被捕获（`BUG_PATTERNS` #13 类）；中键/滚轮、GameMenu 各页、世界层未覆盖。
- **非范围**：不提交发布、不升版本、不动 `dist`，除非用户明确指示。
- **验收标准（沿用）**：改动 ⊆ Packet IN_SCOPE；构建 0 error；评审 PASS；用户实机通过后合并。
- **相关文件**：
  - 3c 记录：`docs/ai/stages/stage-3c.md`、`review-3c.md`
  - 交接：`docs/ai/HANDOFF.md`、`docs/ai/DEV_MODE.md`、`docs/ai/SUBAGENT_PROMPTS.md`
- **验证命令**：
  - 构建：`$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src\BilingualLearning\BilingualLearning.csproj" -c Debug`
  - 仅编译：加 `-p:EnableModDeploy=false`（游戏运行时必须加；部署报 `IOException` 即游戏未关）
  - 日志：SMAPI 控制台 / `%APPDATA%\StardewValley\ErrorLogs\SMAPI-latest.txt`
- **风险与回滚**：
  - 3c 已合并；回滚 `git revert -m 1 fbf1108`；
  - 部署需关闭游戏（DLL 锁）。
