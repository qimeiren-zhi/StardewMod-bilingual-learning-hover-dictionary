# 当前任务卡

> 本文件描述「当前正在做/刚完成」的任务；新会话若接手，先确认下面「任务」是否仍是当前任务。

- **任务**：阶段 5a（扩展面统一点击吞掉）**已完成并合并**（`63003be`）。下一任务：阶段 5b（扩展面星标收藏），在 5a 的吞点击通道里先 toggle 星标。
- **本轮交付（5a）**：
  - `ModEntry.cs`：遍历 `ExtendedTextOverlay.SupportedMenus`，只 patch 类型自身 override（8 左键 + 4 右键），统一 prefix 吞掉气泡∪词∪桥接内的点击；
  - `ExtendedTextOverlay.cs`：暴露只读 `SupportedMenus`；`IsPointerOnBubble` 加 `trackedMenu` 守卫；
  - 评审 PASS；用户实机通过（信件/任务日志/任务板/订单板/木匠/发货箱/商店）。
- **状态**：构建 0 error（含部署，Mods DLL 与 bin 一致）；`stage/5a-click-swallow` 已 `--no-ff` 合并；`dist/` 未重建；版本仍 `0.5.0`。
- **阶段 5 剩余（5b）**：
  - 扩展面气泡显示星标（`BubbleLayout.StarBox` + `BubbleRenderer.Draw(showStar:true)`），点击星标 toggle `Vocabulary`；
  - `ExtendedTextOverlay.Init` 需接 `Vocabulary`（`ModEntry.cs:58` 同步）；
  - 吞点击 prefix 改为：先 `TryToggleStar(x,y)`（命中星标才 toggle），再按 `IsPointerOnBubble` 吞；
  - 记录失败/风险：星标使气泡变宽 34px，右边缘可能超视口；`Sentence` 取当前行文本（无则空）；每次 toggle 立即写盘。
- **非范围**：
  - 不做 3c/4；不改 `WordOverlay`（对话星标已存在，作为基线参考）；
  - 不提交发布、不升版本、不动 `dist`。
- **验收标准（沿用）**：
  1. 改动文件 ⊆ Packet IN_SCOPE；
  2. `dotnet build -p:EnableModDeploy=false` 0 error；
  3. 评审 PASS；`stage-<id>.md` 报告完整；用户实机通过后合并。
- **相关文件**：
  - 5a 记录：`docs/ai/stages/stage-5a.md`、`review-5a.md`
  - 星标基线：`WordOverlay.cs`（`ComputeBubbleLayout`/`OnClick`/`ToggleStar`/`Draw`）、`BubbleRenderer.cs`（`ComputeLayout(showStar)`/`Draw(starred, showStar)`）、`Vocabulary.cs`
  - 交接：`docs/ai/HANDOFF.md`、`docs/ai/DEV_MODE.md`、`docs/ai/SUBAGENT_PROMPTS.md`
- **验证命令**：
  - 构建：`$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src\BilingualLearning\BilingualLearning.csproj" -c Debug`
  - 仅编译：加 `-p:EnableModDeploy=false`（游戏运行时必须加）
  - 日志：SMAPI 控制台 / `%APPDATA%\StardewValley\ErrorLogs\SMAPI-latest.txt`
- **风险与回滚**：
  - 5a 已合并；回滚 `git revert -m 1 63003be`（会回退 8 菜单吞点击）；
  - 部署需关闭游戏（DLL 锁）。
