# 当前任务卡

> 本文件描述「当前正在做/刚完成」的任务；新会话若接手，先确认下面「任务」是否仍是当前任务。

- **任务**：**阶段 3b 全部完成并合并**（3b-4 商店，`8fda139`）。下一任务：阶段 3c（`JunimoNote` / 精通追踪），先只读研究捕获/裁剪是否够用，再出 Packet。
- **本轮交付（3b-4）**：
  - `ShopMenu` 加入 `AllowedMenus`（正文：物品名/价格/以物易物/缺货）；
  - 3 轮修复：F1 卡片取商品（忽略店长问候语 tooltip）、气泡光标重绘忽略透明度、气泡区域吞左键+右键（避免误购）；
  - 评审 3 份 PASS；用户实机通过。
- **状态**：构建 0 error（含部署，Mods DLL 与 bin 一致）；`stage/3b-4-shop` 已 `--no-ff` 合并；`dist/` 未重建；版本仍 `0.5.0`。
- **3b 收官清单**：3a 订单板、3b-1 木匠、3b-2 任务板/日历、3b-3 发货箱/丢物列表、3b-4 商店（正文）。
- **已记录限制/需求（非本轮 bug）**：
  1. 连字符复合词分词（需求 §10-1）；
  2. 卷轴居中标题可查词（§10-2）；
  3. 同形多义词义项（§10-3，`Fall`）；
  4. 商店 tooltip 随光标抢焦点（§10-4，用户认为可接受）；
  5. 扩展面点击吞掉目前只做商店（`BUG_PATTERNS` #24），其它菜单留阶段 5；
  6. 白名单菜单内 tooltip 文本可能被通用 DrawString 捕获（HANDOFF 已知问题 13）。
- **非范围**：
  - 不做 3c/4/5 的功能；不改 `WordOverlay`/`KnowledgeCardMenu` 既有语义；
  - 不提交发布、不升版本、不动 `dist`（等用户决定版本与打包时机）。
- **验收标准（沿用）**：
  1. 改动文件 ⊆ Packet IN_SCOPE；
  2. `dotnet build -p:EnableModDeploy=false` 0 error；
  3. 评审 PASS；`stage-<id>.md` 报告完整；用户实机通过后合并。
- **相关文件**：
  - 3b-4 记录：`docs/ai/stages/stage-3b-4.md`、`review-3b-4*.md`
  - 实现：`src/BilingualLearning/{ExtendedTextOverlay,ModEntry,TextCapture,SpriteTextWordLayout}.cs`
  - 交接：`docs/ai/HANDOFF.md`、`docs/ai/DEV_MODE.md`、`docs/ai/SUBAGENT_PROMPTS.md`
- **3c 开工前先研究**：
  - `JunimoNoteMenu`（收集包）绘制入口：混合 SpriteText + `DrawString`，注意卷轴/翻页/裁剪与点击（收集包有物品格可点）；
  - 精通追踪（`MasteryTrackerMenu`？）入口与文本；
  - 是否需要新增捕获/布局逻辑，还是仅白名单。
- **验证命令**：
  - 构建：`$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; C:\Users\zhiqi\dotnet\dotnet.exe build "src\BilingualLearning\BilingualLearning.csproj" -c Debug`
  - 仅编译：加 `-p:EnableModDeploy=false`（游戏运行时必须加）
  - 日志：SMAPI 控制台 / `%APPDATA%\StardewValley\ErrorLogs\SMAPI-latest.txt`
- **风险与回滚**：
  - 3b-4 已合并；回滚 `git revert -m 1 8fda139`（会同时回退白名单 + F1/光标/点击修复）；
  - 部署需关闭游戏（DLL 锁）。
