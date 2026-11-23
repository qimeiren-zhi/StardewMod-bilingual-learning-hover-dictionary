# 当前任务卡

> 本文件描述「当前正在做/刚完成」的任务；新会话若接手，先确认下面「任务」是否仍是当前任务。

- **任务**：阶段 5b（扩展面星标收藏）**已完成并合并**（`d501217`）。**阶段 5 完成**（5a 点击吞掉 + 5b 星标）。
- **下一任务（三选一，待用户定）**：3c `JunimoNote` / 阶段 4 世界层 / 发布准备（版本号 + `dist` 重建）。
- **本轮交付（5b）**：
  - 扩展气泡显示星标（`showStar:true`），左键点星 toggle 收藏（立即写盘），右键只吞不 toggle；
  - 点击吞掉区域收窄为**仅气泡矩形**（词/桥接放行，进任务面板/翻页/接单/购买）；
  - 复用对话路径 `Vocabulary`/`BubbleRenderer`（DECISIONS #20）；`SpriteWordHit.LineText` 提供 Sentence；
  - 评审 PASS；用户实机通过。
- **状态**：构建 0 error（含部署，Mods DLL 与 bin 一致）；`stage/5b-star` 已 `--no-ff` 合并；`dist/` 未重建；版本仍 `0.5.0`。
- **本次会话累计完成（相对 0.5.0）**：3a 订单板、3b-1 木匠、3b-2 任务板/日历、3b-3 发货箱/丢物、3b-4 商店正文、5a 点击吞掉、5b 星标。
- **已知遗留（非阻塞）**：
  - 需求池 §10：连字符分词、卷轴居中标题可查、同形多义词义项、商店 tooltip 抢焦点（可接受）；
  - `BUG_PATTERNS` #13：白名单菜单内 tooltip 文本可能被通用 DrawString 捕获；
  - 中键/滚轮、`GameMenu` 各页、世界层未覆盖；
  - `dist/` 旧包未重建。
- **非范围**：
  - 不提交发布、不升版本、不动 `dist`（等用户决定版本与打包时机）。
- **验收标准（沿用）**：
  1. 改动文件 ⊆ Packet IN_SCOPE；2. 构建 0 error；3. 评审 PASS；4. 用户实机通过后合并。
- **相关文件**：
  - 5b 记录：`docs/ai/stages/stage-5b.md`、`review-5b.md`
  - 实现：`src/BilingualLearning/{ExtendedTextOverlay,ModEntry,SpriteTextWordLayout,WordOverlay}.cs`
  - 交接：`docs/ai/HANDOFF.md`、`docs/ai/DEV_MODE.md`、`docs/ai/SUBAGENT_PROMPTS.md`
- **验证命令**：
  - 构建：`$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src\BilingualLearning\BilingualLearning.csproj" -c Debug`
  - 仅编译：加 `-p:EnableModDeploy=false`（游戏运行时必须加）
  - 日志：SMAPI 控制台 / `%APPDATA%\StardewValley\ErrorLogs\SMAPI-latest.txt`
- **风险与回滚**：
  - 5b 已合并；回滚 `git revert -m 1 d501217`；
  - 部署需关闭游戏（DLL 锁）。
