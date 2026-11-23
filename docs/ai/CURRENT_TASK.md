# 当前任务卡

> 本文件描述「当前正在做/刚完成」的任务；新会话若接手，先确认下面「任务」是否仍是当前任务。

- **任务**：阶段 3a（特殊订单板 `SpecialOrdersBoard`）**已完成并合并**（`ceb0a46`）。下一任务：阶段 3b（商店/木匠/日历/Billboard/发货箱/ItemList），按 `DEV_MODE.md` 派发给阶段 SubAgent。
- **本轮交付（3a）**：
  - `src/BilingualLearning/ExtendedTextOverlay.cs`：`AllowedMenus` 增加 `typeof(SpecialOrdersBoard)`（唯一源码改动，另有类注释一行）；
  - 订单标题/描述/剩余天数/奖励复用现有 SpriteFont 管线（9 参 `DrawString`、`\n` 多行、缩字 `scale`），未新增 hook、未改布局；
  - 评审 PASS（`docs/ai/stages/review-3a.md`）；用户实机通过（多行/缩字贴合、切换订单不残留）。
- **状态**：构建 0 error（含部署，Mods DLL 与 bin 一致）；`stage/3a-orders-board` 已 `--no-ff` 合并；`dist/` 未重建；版本仍 `0.5.0`。
- **已定行为**：`Extended lookup=Off` 是**总闸门**，关闭后信件/任务日志/订单板全部不查词（对话、F1 卡片不受影响）；不再按「遗留 bug」排查。
- **非范围**：
  - 不做阶段 3b/3c 的功能；不改 `WordOverlay`/`KnowledgeCardMenu` 既有语义；
  - 不提交、不发布、不升版本、不动 `dist`（等用户决定版本与打包时机）。
- **验收标准（3b 起沿用）**：
  1. 改动文件 ⊆ Packet IN_SCOPE；
  2. `dotnet build -p:EnableModDeploy=false` 0 error；
  3. 评审 PASS；`stage-<id>.md` 报告完整；用户实机 3~5 条通过后合并。
- **相关文件**：
  - 3a 记录：`docs/ai/stages/stage-3a.md`、`docs/ai/stages/review-3a.md`
  - 实现：`src/BilingualLearning/{ExtendedTextOverlay,TextCapture,SpriteTextWordLayout,ModEntry}.cs`
  - 交接：`docs/ai/HANDOFF.md`、`docs/ai/DEV_MODE.md`、`docs/ai/SUBAGENT_PROMPTS.md`
- **3b 开工前检查点**：
  - 白名单：`ExtendedTextOverlay.AllowedMenus`；闸门：`ModEntry.ShouldCapture` / `IsEnabled`；
  - 布局：`SpriteTextWordLayout.Build`（像素字体）、`ExtendedTextOverlay.BuildSpriteFontHits`（SpriteFont 前缀测量 + 多行 + scale）；
  - 裁剪/残留：`RebuildWordRects` 的菜单矩形裁剪 + `IsHoveredWordStillPresent`；
  - 顺带同步 GMCM `Extended lookup` 描述与 `ModEntry.cs:308` 注释（评审 NON_BLOCKING）。
- **验证命令**：
  - 构建：`$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src\BilingualLearning\BilingualLearning.csproj" -c Debug`
  - 仅编译：加 `-p:EnableModDeploy=false`（游戏运行时必须加）
  - 日志：SMAPI 控制台 / `%APPDATA%\StardewValley\ErrorLogs\SMAPI-latest.txt`
- **风险与回滚**：
  - 3a 已合并；如需回滚，`git revert -m 1 ceb0a46`（只影响白名单一行与文档）；
  - 部署需关闭游戏（DLL 锁）。
