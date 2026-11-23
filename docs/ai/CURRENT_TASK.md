# 当前任务卡

> 本文件描述「当前正在做/刚完成」的任务；新会话若接手，先确认下面「任务」是否仍是当前任务。

- **任务**：阶段 3b-3（`ShippingMenu` + `ItemListMenu`）**已完成并合并**（`c5901a8`）。下一任务：阶段 3b-4（`ShopMenu`，需先只读研究 tooltip 路径），按 `DEV_MODE.md` 派发。
- **本轮交付（3b-3）**：
  - `src/BilingualLearning/ExtendedTextOverlay.cs`：`AllowedMenus` 增加 `typeof(ShippingMenu)`、`typeof(ItemListMenu)`（唯一源码改动）；
  - 正文全为像素字体 `SpriteText.drawString`，复用现有布局/裁剪/命中；零新 hook；
  - 评审 PASS（`docs/ai/stages/review-3b-3.md`）；用户实机**全部通过**。
- **状态**：构建 0 error（含部署，Mods DLL 与 bin 一致）；`stage/3b-3-shipping-itemlist` 已 `--no-ff` 合并；`dist/` 未重建；版本仍 `0.5.0`。
- **测试备注**：`debug eventScript .../showItemsLost/end` 只回「no output」、未直接弹菜单；如实测丢物列表，优先真实死亡事件（`debug ebi PlayerKilled`，会扣钱掉物，需测试存档）。
- **非范围**：
  - 不做 3b-4、3c、4、5 的功能；不改 `WordOverlay`/`KnowledgeCardMenu` 既有语义；
  - 不提交发布、不升版本、不动 `dist`（等用户决定版本与打包时机）。
- **验收标准（沿用）**：
  1. 改动文件 ⊆ Packet IN_SCOPE；
  2. `dotnet build -p:EnableModDeploy=false` 0 error；
  3. 评审 PASS；`stage-<id>.md` 报告完整；用户实机 3~5 条通过后合并。
- **相关文件**：
  - 3b-3 记录：`docs/ai/stages/stage-3b-3.md`、`docs/ai/stages/review-3b-3.md`
  - 实现：`src/BilingualLearning/{ExtendedTextOverlay,TextCapture,SpriteTextWordLayout,ModEntry}.cs`
  - 交接：`docs/ai/HANDOFF.md`、`docs/ai/DEV_MODE.md`、`docs/ai/SUBAGENT_PROMPTS.md`
- **3b-4 开工前必须先研究（不直接派执行）**：
  - `ShopMenu.draw` 的像素字体正文（物品名 `row.bounds.X+40`、价格右对齐、以物易物 xN）可走现有管线；
  - 但物品**描述/售价只在 tooltip**（`drawHoverText` / `drawToolTip`），且同屏有玩家背包 36 格 tooltip；需要决定：是否接入 tooltip 文本（复用 `TooltipTextCapture`？）还是只做正文物品名；
  - `drawHoverText` 文本绘制发生在 `IClickableMenu.drawHoverText` 内，现有 hook（`AfterDrawHoverText`）只缓存参数给 F1 卡片，不注册到 `TextCapture`；接入需新 hook 或改捕获闸门，属架构级改动，须单独立项评估。
- **验证命令**：
  - 构建：`$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src\BilingualLearning\BilingualLearning.csproj" -c Debug`
  - 仅编译：加 `-p:EnableModDeploy=false`（游戏运行时必须加）
  - 日志：SMAPI 控制台 / `%APPDATA%\StardewValley\ErrorLogs\SMAPI-latest.txt`
- **风险与回滚**：
  - 3b-3 已合并；回滚 `git revert -m 1 c5901a8`（只影响白名单两行与文档）；
  - 部署需关闭游戏（DLL 锁）。
