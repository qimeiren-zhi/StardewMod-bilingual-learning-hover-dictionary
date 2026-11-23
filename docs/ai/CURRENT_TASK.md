# 当前任务卡

> 本文件描述「当前正在做/刚完成」的任务；新会话若接手，先确认下面「任务」是否仍是当前任务。

- **任务**：阶段 3b-2（小镇任务板/日历 `Billboard`）**已完成并合并**（`d6b5ac5`）。下一任务：阶段 3b-3（`ShippingMenu` + `ItemListMenu`），按 `DEV_MODE.md` 派发阶段 SubAgent。
- **本轮交付（3b-2）**：
  - `src/BilingualLearning/ExtendedTextOverlay.cs`：`AllowedMenus` 增加 `typeof(Billboard)`（唯一源码改动）；
  - 任务描述（dialogueFont、`parseText(...,640)` 多行）复用现有管线；零新 hook、零布局改动；
  - 评审 PASS（`docs/ai/stages/review-3b-2.md`）；用户实机通过。
- **状态**：构建 0 error（含部署，Mods DLL 与 bin 一致）；`stage/3b-2-billboard` 已 `--no-ff` 合并；`dist/` 未重建；版本仍 `0.5.0`。
- **已记录需求（非本轮 bug）**：需求 §10 三条 —— 连字符复合词分词（中）、卷轴居中标题可查词（中）、同形多义词义项选择（低，`Fall` 显示「落下」而非「秋天」，`BUG_PATTERNS` #22）。
- **非范围**：
  - 不做 3b-3/3b-4、3c、4、5 的功能；不改 `WordOverlay`/`KnowledgeCardMenu` 既有语义；
  - 不提交发布、不升版本、不动 `dist`（等用户决定版本与打包时机）。
- **验收标准（沿用）**：
  1. 改动文件 ⊆ Packet IN_SCOPE；
  2. `dotnet build -p:EnableModDeploy=false` 0 error；
  3. 评审 PASS；`stage-<id>.md` 报告完整；用户实机 3~5 条通过后合并。
- **相关文件**：
  - 3b-2 记录：`docs/ai/stages/stage-3b-2.md`、`docs/ai/stages/review-3b-2.md`
  - 实现：`src/BilingualLearning/{ExtendedTextOverlay,TextCapture,SpriteTextWordLayout,ModEntry}.cs`
  - 交接：`docs/ai/HANDOFF.md`、`docs/ai/DEV_MODE.md`、`docs/ai/SUBAGENT_PROMPTS.md`
- **3b-3 开工检查点**：
  - `ShippingMenu`：满屏 1280×920 框，正文全为 `SpriteText.drawString`（物品名+数量+金额、分类名、日期横幅），**无 tooltip**，按 `currentTab` 翻页、无滚动；行不可点；
  - `ItemListMenu`：标题/总金额/逐项名（像素字体，行距 68），无 tooltip，`Event/DefaultCommands.ShowItemsLost` 打开；
  - 两者菜单矩形有效，可直接白名单接入；注意满屏 crop 会纳入同帧其它文本（菜单本身无 tooltip，风险小）。
- **验证命令**：
  - 构建：`$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src\BilingualLearning\BilingualLearning.csproj" -c Debug`
  - 仅编译：加 `-p:EnableModDeploy=false`（游戏运行时必须加）
  - 日志：SMAPI 控制台 / `%APPDATA%\StardewValley\ErrorLogs\SMAPI-latest.txt`
- **风险与回滚**：
  - 3b-2 已合并；回滚 `git revert -m 1 d6b5ac5`（只影响白名单一行与文档）；
  - 部署需关闭游戏（DLL 锁）。
