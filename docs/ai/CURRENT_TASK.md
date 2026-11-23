# 当前任务卡

> 本文件描述「当前正在做/刚完成」的任务；新会话若接手，先确认下面「任务」是否仍是当前任务。

- **任务**：阶段 3b-1（木匠/建造菜单 `CarpenterMenu`）**已完成并合并**（`5987765`）。下一任务：阶段 3b-2（小镇任务板 `Billboard`），按 `DEV_MODE.md` 派发阶段 SubAgent。
- **本轮交付（3b-1）**：
  - `src/BilingualLearning/ExtendedTextOverlay.cs`：`AllowedMenus` 增加 `typeof(CarpenterMenu)`（唯一源码改动，另有类注释一行）；
  - 蓝图描述（dialogueFont、`parseText` 多行）、建造费、配料名/数量复用现有管线；零新 hook、零布局改动；
  - 评审 PASS（`docs/ai/stages/review-3b-1.md`）；用户实机通过。
- **状态**：构建 0 error（含部署，Mods DLL 与 bin 一致）；`stage/3b-1-carpenter` 已 `--no-ff` 合并；`dist/` 未重建；版本仍 `0.5.0`。
- **已记录需求（非本轮 bug）**：
  1. 连字符复合词不命中（`coop-dwelling`），既有分词限制 → `docs/requirements/需求规格说明.md` §10-1；
  2. 卷轴居中标题可查词（木匠 `Coop`/`Barn`、订单板 `ChooseOne` 等）→ §10-2；
  3. 木匠菜单换蓝图只有手柄 LT/RT 或鼠标点箭头（键盘/滚轮为游戏原生不支持），见 `docs/ai/HANDOFF.md` 已知问题 10。
- **非范围**：
  - 不做 3b-2/3/4、3c、4、5 的功能；不改 `WordOverlay`/`KnowledgeCardMenu` 既有语义；
  - 不提交发布、不升版本、不动 `dist`（等用户决定版本与打包时机）。
- **验收标准（沿用）**：
  1. 改动文件 ⊆ Packet IN_SCOPE；
  2. `dotnet build -p:EnableModDeploy=false` 0 error；
  3. 评审 PASS；`stage-<id>.md` 报告完整；用户实机 3~5 条通过后合并。
- **相关文件**：
  - 3b-1 记录：`docs/ai/stages/stage-3b-1.md`、`docs/ai/stages/review-3b-1.md`
  - 实现：`src/BilingualLearning/{ExtendedTextOverlay,TextCapture,SpriteTextWordLayout,ModEntry}.cs`
  - 交接：`docs/ai/HANDOFF.md`、`docs/ai/DEV_MODE.md`、`docs/ai/SUBAGENT_PROMPTS.md`
- **3b-2 开工检查点**：
  - 白名单：`ExtendedTextOverlay.AllowedMenus`；闸门：`ModEntry.ShouldCapture` / `IsEnabled`；
  - `Billboard` 是任务板 + 日历共用类型（`ctor(bool dailyQuestBoard)`）；任务描述为 `Utility.drawTextWithShadow` + `parseText(...,640)` 多行（dialogueFont），正文在菜单矩形内；
  - 注意 `Accept Quest` 按钮位置与长描述可能重叠；日历模式正文只有 Season/Year（价值低）。
- **验证命令**：
  - 构建：`$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src\BilingualLearning\BilingualLearning.csproj" -c Debug`
  - 仅编译：加 `-p:EnableModDeploy=false`（游戏运行时必须加）
  - 日志：SMAPI 控制台 / `%APPDATA%\StardewValley\ErrorLogs\SMAPI-latest.txt`
- **风险与回滚**：
  - 3b-1 已合并；回滚 `git revert -m 1 5987765`（只影响白名单一行与文档）；
  - 部署需关闭游戏（DLL 锁）。
