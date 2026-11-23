# 当前任务卡

> 本文件描述「当前正在做/刚完成」的任务；新会话若接手，先确认下面「任务」是否仍是当前任务。

- **任务**：归档 2026-11-23 会话成果 + 文档同步（**已完成，改动未提交**）。下一任务：阶段 3（特殊订单板 / 菜单长尾）。
- **本轮交付**：
  - 阶段 0/1/2 全部实机验证通过：信件、秘密纸条、日志残页、任务日志可悬停查词；
  - 修复四类对齐问题（前缀 `MeasureString`、阴影去重、`^` 换行、换行原点）；
  - `F10` 调试键（矩形 + `[诊断]` 日志）落地并写入 README/HANDOFF 说明；
  - 文档同步：README、CHANGELOG、需求 §9、计划 §8、AGENTS、DECISIONS 16~18、BUG_PATTERNS 18~21、HANDOFF、`HANDOVER_2026-11-23.md`。
- **状态**：构建 0 error；已部署到游戏（2026-11-23 20:20:27）；`git status` 为 12 改 + 3 新增，**未提交**；`dist/` 未重建。
- **非范围**：
  - 不提交、不发布（等用户决定版本与打包时机）；
  - 不做阶段 3/4/5 的功能；不改 `WordOverlay`/`KnowledgeCardMenu` 既有语义；
  - 不合并 `backup/other-agent-807c988`。
- **验收标准**：
  1. 文档与源码一致（验收命令、快捷键、配置项、白名单、已知限制）；
  2. `dotnet build` 0 error；
  3. 新会话按 HANDOFF 能直接进入阶段 3，无需重新踩本轮四个对齐坑。
- **相关文件**：
  - 新增实现：`src/BilingualLearning/{TextCapture,SpriteTextWordLayout,ExtendedTextOverlay}.cs`
  - 交接：`docs/ai/HANDOFF.md`、`docs/ai/HANDOVER_2026-11-23.md`、`docs/ai/DECISIONS.md`、`docs/ai/BUG_PATTERNS.md`
  - 用户文档：`README.md`、`CHANGELOG.md`
- **相似结构检查点**（阶段 3 开工前先看）：
  - 捕获：`TextCapture.RegisterSpriteText/RegisterSpriteFont`、`ModEntry.ShouldCapture`；
  - 白名单：`ExtendedTextOverlay.AllowedMenus`（新增文本面只改这里 + 验收）；
  - 布局：`SpriteTextWordLayout.Build`（像素字体）、`ExtendedTextOverlay.BuildSpriteFontHits`（SpriteFont，前缀测量）；
  - 裁剪/残留：`RebuildWordRects` 的菜单矩形裁剪 + `IsHoveredWordStillPresent`。
- **验证命令**：
  - 构建：`$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src\BilingualLearning\BilingualLearning.csproj" -c Debug`
  - 仅编译：加 `-p:EnableModDeploy=false`（游戏运行时必须加）
  - 日志：SMAPI 控制台 / `%APPDATA%\StardewValley\ErrorLogs\SMAPI-latest.txt`
- **风险与回滚**：
  - 未提交改动可整体 `git checkout -- .` + 删除三个新增文件回滚；
  - 扩展功能默认关闭，回滚风险仅限开启后的体验；
  - 部署需关闭游戏（DLL 锁）。
