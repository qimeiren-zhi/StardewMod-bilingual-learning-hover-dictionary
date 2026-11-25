# 架构决策记录（ADR）

> 记录已做出的关键决策及原因，避免新会话重复踩坑或推翻已验证方案。

## 决策 1：气泡绘制放在 `Display.RenderedActiveMenu` + 手动重绘光标

- **背景**：气泡需盖在游戏对话框上，但不能盖住鼠标光标。
- **决策**：在 `Display.RenderedActiveMenu` 里画气泡，随后调用 `box.drawMouse(e.SpriteBatch)` 重绘光标（`ModEntry.cs:291`）。
- **原因**：该事件触发在菜单绘制之后、光标绘制之前；气泡画在此处天然被光标覆盖，再手动补画光标即可。
- **影响文件**：`ModEntry.cs`、`WordOverlay.cs`。
- **替代方案及否决原因**：Harmony postfix `DialogueBox.draw` + 抬高 LayerDepth（0.98/0.99）→ 实测无效，光标仍被压。

## 决策 2：生词本用 `WriteJsonFile/ReadJsonFile`（立即落盘）

- **背景**：需要「收藏即保存」，退出（不睡觉）也不丢。
- **决策**：`helper.Data.WriteJsonFile("data/<SaveFolderName>.json", data)`（`Vocabulary.cs:56-62`）。
- **原因**：`WriteSaveData` 只写 `Game1.CustomData` 内存，随游戏存档周期，退出即丢。
- **影响文件**：`Vocabulary.cs`、`VocabData.cs`。
- **替代方案及否决原因**：`WriteSaveData` → 退出即丢，不可靠。

## 决策 3：词典采用 v2 四列格式 `word \t pos \t translation \t tag`

- **背景**：需要分离词性与释义，并支持专有名词标记。
- **决策**：`DictionaryData.Load` 解析 4 列（`DictionaryData.cs:62-64`），`Entry` 含 `Pos/Translation/Tag`。
- **原因**：v1（3 列，词性内嵌释义）无法准确区分词性/义项；v2 由词库搭建侧收敛生成。
- **影响文件**：`DictionaryData.cs`、`assets/dictionary.tsv`、`docs/specs/词典数据格式.md`。
- **替代方案及否决原因**：运行时用 `ExtractPos/ExtractMeaning` 拆分 → 只能粗略切分，已删除。

## 决策 4：移除等级过滤

- **背景**：曾按 ECDICT `tag` 只显示「达到所选等级」的词。
- **决策**：彻底移除（`IsLevelMatch`/`EnabledLevels`/GMCM 等级项均已删）。
- **原因**：① 查词是**悬停主动行为**，不存在信息污染，过滤无收益；② `tag` 是考纲等级 ≠ 常见度，空 tag（52.8%）被误杀，导致 `channeled`/`pelican` 无气泡。
- **影响文件**：`WordOverlay.cs`、`ModConfig.cs`、`ModEntry.cs`。
- **替代方案及否决原因**：按 `frq` 词频过滤 → 数据可用但用户已明确「不做分级」。

## 决策 5：知识卡片 = 重放原生 `drawHoverText` + 捕获 `DrawString`

- **背景**：卡片要「和原生 Tooltip 一致」（类别/增益/价格/描述），且要支持任意文字悬浮查词。
- **决策**：
  - Harmony postfix 捕获 `drawHoverText` 全部参数（`object[] __args`）；
  - 卡片 `draw` 里用固定 `overrideX/overrideY` **重放**原生方法（`KnowledgeCardMenu.cs:72`），类别/增益/价格自动齐全；
  - 重放期间用 `TooltipTextCapture` 捕获 3 个 `DrawString` 重载的文本+位置，建词矩形。
- **原因**：自己重排类别/增益/价格极其复杂；重放原生方法零维护且完全一致。
- **影响文件**：`KnowledgeCardMenu.cs`、`TooltipTextCapture.cs`、`ModEntry.cs`（3 个 DrawString hook）。
- **替代方案及否决原因**：自绘完整 tooltip → 需复刻 buff/价格布局，成本高、易过时。

## 决策 6：GMCM 用「最小接口 + 运行时 `GetApi` 动态代理」

- **背景**：GMCM 不在 nuget.org，不想引 NuGet/DLL。
- **决策**：在 `IApi.cs` 定义最小 `IApi`（`RegisterModConfig`/`SetDefaultIngameOptinValue`/`RegisterSimpleOption`/`RegisterChoiceOption`），运行时 `helper.ModRegistry.GetApi<IApi>("spacechase0.GenericModConfigMenu")`。
- **原因**：SMAPI 的 `GetApi<T>` 按方法名+签名动态代理，不要求接口同名；GMCM 是软依赖（`IsRequired:false`）。
- **影响文件**：`IApi.cs`、`ModEntry.cs`、`manifest.json`。
- **替代方案及否决原因**：NuGet 包（需自定义 feed）/ 引 DLL（需本地 DLL）→ 复杂且不宜随包分发。

## 决策 7：字体子集化（按词典实际用字）

- **背景**：完整 CJK 字体 15.6MB，发布包过大。
- **决策**：用 `fonttools` 按词典+ASCII+标点（3850 字符）裁剪，`Font.ttf` 15.6MB → **2.08MB**。
- **原因**：mod 只渲染词典释义，用不到全部 CJK 字形。
- **影响文件**：`assets/Font.ttf`、`tools/subset_font.py`（词库搭建侧）。
- **替代方案及否决原因**：保留全量字体 → 体积超标。

## 决策 8：变形词补入（ECDICT `exchange`）

- **背景**：`channeled`/`went`/`channels` 等变形查不到。
- **决策**：用 `exchange` 的 `p/d/i`（过去式/过去分词/现在分词）与 `3/s`（三单/复数）补词；已在词典中的词不覆盖；`-s` 歧义展示原型合并义项。
- **原因**：覆盖衍生词；`-s` 歧义不强行消歧（避免错误义项）。
- **影响文件**：`assets/dictionary.tsv`（5.26 万词）、`tools/add_inflections.py`（词库搭建侧）。
- **替代方案及否决原因**：运行时词形还原 + 词性标注/WSD → 复杂且易错，暂不做。

## 决策 9：人物名称过滤用 `Data/NPCGiftTastes`

- **背景**：对话出现 NPC 名（Penny）会给出错误词义（便士）。
- **决策**：从 `Data/NPCGiftTastes` 取 NPC 名集合，命中跳过；`Data/Characters` 在 1.6 加载失败。
- **原因**：`Data/NPCGiftTastes` 是稳定的 `Dictionary<string,string>`。
- **影响文件**：`ModEntry.cs` `LoadNpcNames`、`WordOverlay.cs` `IsIgnoredName`。

## 决策 10：语言校验（仅英文启用）

- **背景**：mod 面向英文游戏查中文释义。
- **决策**：`LocalizedContentManager.CurrentLanguageCode == LanguageCode.en` 才启用（`WordOverlay.cs` `IsEnglish`）。
- **原因**：非英文游戏下查词无意义（且中文/其它字体布局不同）。
- **影响文件**：`WordOverlay.cs`。

## 决策 11：气泡 0 延迟隐藏（不实现「延迟 N 秒」）

- **背景**：冻结需求 F5 / §3.3 要求鼠标同时离开单词与气泡后延迟 N 秒隐藏。
- **决策**：改为**立即隐藏（0 延迟）**，靠「词 ∪ 气泡 ∪ 桥接矩形」连通判定避免误消失（`WordOverlay.cs` 连通判定 + `ComputeBubbleLayout`；卡片为 `KnowledgeCardMenu.ComputeBridge`）。
- **原因**：4px 间隙已被桥接矩形覆盖，实测无闪烁；延迟只会让气泡在屏幕上残留，反而碍眼。
- **影响文件**：`WordOverlay.cs`、`KnowledgeCardMenu.cs`、需求文档 §9-2。
- **替代方案及否决原因**：可配置延迟 → 配置项膨胀且默认值难定，暂不做。

## 决策 12：气泡背景用自制 PNG，而非对话九宫格

- **背景**：冻结需求 §3.6 要求复用 `LooseSprites/dialogueBox` 九宫格纹理。
- **决策**：改用 `assets/bubble_default.png` / `bubble_sandy.png`，仍用 `IClickableMenu.drawTextureBox` 九宫格绘制；游戏 `mouseCursors` 纹理保留为加载失败时的回退（`WordOverlay.cs` `Draw`）。
- **原因**：对话纹理为深色底，深色中文释义对比度不足；自制浅色纸张底更易读，并支持 `BubbleStyle` 双风格。
- **影响文件**：`WordOverlay.cs`、`BubbleRenderer.cs`、`assets/bubble_*.png`、需求文档 §9-3。

## 决策 13：不实现 Alt+点击触发

- **背景**：冻结需求 F1 要求「hover + Alt+点击」，二者可分别开关。
- **决策**：**仅 hover**，Alt+点击本期不做。
- **原因**：hover 已覆盖全部查词场景；Alt+点击需额外拦截点击并判断修饰键，与「星标点击吞掉」「翻页/跳过打字机」的点击分层冲突，收益低。
- **影响文件**：需求文档 §9-1。
- **替代方案及否决原因**：用 `Suppress` 再判修饰键 → 与现有 `BeforeClick` 吞点击逻辑耦合，易引入翻页 bug。

## 决策 14：MinimumGameVersion 收紧到 1.6.15

- **背景**：实现依赖游戏私有字段 `Game1._activeClickableMenu`（反射）与 `drawHoverText` 参数索引 0/13/14 的重放。
- **决策**：manifest `MinimumGameVersion` 由 `1.6.0` → `1.6.15`。
- **原因**：旧版本未实测，私有字段/签名可能不同，宁可拒绝加载也不静默失效。
- **影响文件**：`manifest.json`、`docs/planning/开发计划与节点控制.md` §9。
- **注意**：`dist/` 内 0.5.0 发布包仍是旧 manifest，发布前必须重新打包。

## 决策 15：文本面扩展「按文本面切片」，不按绘制 API 分阶段

- **背景**：原计划 §8.4 按「阶段2 SpriteText → 阶段3 SpriteFont → 打磨」推进，备份分支（`7e2b7ee`）据此重写并回滚。
- **决策**：
  - 先建**统一每帧文本注册表**（`SpriteText.drawString` + 4 个 `SpriteBatch.DrawString` 重载），再按文本面逐个交付：信件 → 任务日志/订单板 → 菜单长尾 → 世界层 → 交互；
  - `WordOverlay` / `KnowledgeCardMenu` 保持原路径，只把词布局抽成共享纯函数；
  - 新文本面白名单启用（GMCM 默认关），不全局接管点击。
- **原因（Mono.Cecil 实测）**：
  - `QuestLog`、`ShopMenu`、`JunimoNoteMenu`、`LetterViewerMenu` 同一菜单混用两种 API，按 API 切会导致重复实现；
  - `SpriteBatch.DrawString` 全游戏只有 4 个重载（第 4 个尚缺 hook），SpriteFont 面并不构成独立阶段；
  - HUD toast 是 `Utility.drawTextWithShadow`（SpriteFont）、电视是 `Game1.drawObjectDialogue`（DialogueBox，已覆盖），原清单归属错误。
- **影响文件**：`docs/planning/开发计划与节点控制.md` §8、`TextCapture.cs`、`ExtendedTextOverlay.cs`、`ModEntry.cs`。
- **替代方案及否决原因**：逐菜单自绘适配（路线 B）→ 工作随菜单数线性增长；全局点击接管（备份分支做法）→ 与游戏原生交互冲突，已回滚。

## 决策 16：SpriteFont 文本用「行内前缀 MeasureString」定位

- **背景**：扩展文本面的词矩形需要从 `SpriteBatch.DrawString` 的参数重建；最初按「词宽 + 空格宽」累加。
- **决策**：每个词的位置 = `行首 X + font.MeasureString(该词之前的行内文本).X * scale`，宽度 = `MeasureString(word).X * scale`。
- **原因（实测）**：MonoGame 的 `MeasureString` 对每个字符都累加 `Spacing` 与左右边距；分段累加会在每个词边界少计 2×`Spacing`（对话字体 `Spacing=-2` → 4px/词），导致矩形逐词右漂、单字母最明显。改用前缀测量后与 `DrawString` 推进一致，误差收敛为字形左留白（约 3px）。
- **影响文件**：`ExtendedTextOverlay.BuildSpriteFontHits`。
- **替代方案及否决原因**：`MeasureString(word)+MeasureString(" ")` → 见上，累计漂移；逐字形 hook → 侵入性过大。

## 决策 17：调试键 `F10` 不进 GMCM，只做开发对位工具

- **背景**：扩展文本面需要验证「词矩形是否与实机文字贴合」，但普通玩家不需要看到矩形。
- **决策**：
  - `F10`（`DebugTextRectsKey`）只放在 `config.json`，**不注册进 GMCM**；
  - 开启时绘制半透明矩形，并把捕获行与词矩形写入 SMAPI 日志（`[诊断]` 前缀）；
  - 关闭且 `ExtendLookup=Off` 时，`TextCapture.Enabled=false`，捕获完全不运行（零开销）；
  - 只读绘制，不修改游戏状态、不写存档、不新增文件。
- **原因**：调试 UI 若暴露给玩家容易被误开；而排查对齐问题必须能「看见」矩形。日志走 SMAPI 既有通道，不额外落盘。
- **影响文件**：`ModConfig.cs`、`ModEntry.cs`、`ExtendedTextOverlay.cs`、`README.md`、`docs/ai/HANDOVER_2026-11-23.md`。
- **替代方案及否决原因**：把矩形开关放进 GMCM → 误触风险 + 玩家困惑；不提供调试手段 → 每次对齐问题只能靠猜。

## 决策 18：扩展文本面「白名单 + 只读气泡」

- **背景**：阶段 0~2 覆盖信件与任务日志，后续还有订单板/商店/世界层。
- **决策**：
  - 只有 `ExtendedTextOverlay.AllowedMenus` 里的菜单类型才捕获与命中；不在白名单时零捕获；
  - 扩展面只画「词性 + 释义」气泡，**不画星标、不拦截点击**；收藏/交互留到阶段 5 统一做；
  - 命中矩形裁剪到当前菜单矩形，被 scissor 裁掉的文字不响应。
- **原因**：避免重蹈备份分支「全局接管点击」导致交互冲突的覆辙；也让每个新文本面可以独立验收。
- **影响文件**：`ExtendedTextOverlay.cs`、`ModEntry.cs`。

## 决策 19：商店 tooltip 不接管，物品描述用 F1 知识卡片间接查词

- **背景**：阶段 3b-4 接入 `ShopMenu` 时，正文（物品名/价格/以物易物数量/缺货提示）都是像素字体 `SpriteText.drawString`，可复用现有捕获与布局；但物品**描述与售价**只存在于 `IClickableMenu.drawHoverText`/`drawToolTip`（`ShopMenu.draw` 内 1 处 `drawHoverText` + 2 处 `drawToolTip`），且背包格 tooltip 与商店 tooltip 同屏并存。要把 tooltip 纳入查词，需要额外的捕获路径（重放/Hook `drawHoverText`）与命中容器，属于架构性改动，而非「白名单一行」。
- **决策**：商店**只做正文**查词；tooltip（物品描述/售价/背包格）**不接管**。需要查物品描述时，用户按 `F1` 打开知识卡片，卡片内描述走既有 `TooltipTextCapture` 路径可查词（用户已确认此替代路径可接受）。
- **原因**：① 收益低——tooltip 描述本可通过 F1 卡片查到，重复实现一次悬停查词边际收益小；② 风险高——tooltip 每帧随鼠标重绘，捕获矩形与鼠标位置重合，易出现「气泡压 tooltip / 悬停即抖」；且需新增 Hook 或改动捕获闸门，可能回归对话/卡片路径；③ 已有替代路径——F1 知识卡片对任何物品都能重放原生 tooltip 并查词。
- **影响**：`ExtendedTextOverlay.AllowedMenus` 只加 `ShopMenu`（正文）；商店 tooltip、背包 tooltip、`TooltipTextCapture`、`ModEntry` 的 `drawHoverText` Hook 均**零改动**；`ExtendLookup=Off` 时行为与之前完全一致。
- **替代方案及否决原因**：在商店内捕获 `drawHoverText`/`drawToolTip` 的绘制文本（复用 `TooltipTextCapture`）→ 否决：需要新的 Hook/捕获路径 + 与商店 tooltip 的绘制顺序/光标重绘协调，且会与背包格 tooltip 同屏产生双 tooltip 冲突，超出本阶段范围。

## 决策 20：扩展面星标复用对话路径的 Vocabulary 与气泡布局

- **背景**：阶段 5b 要给白名单菜单（信件/任务日志/订单板/木匠/任务板/发货箱/丢物列表/商店正文）的查词气泡加星标收藏；对话路径已有成熟的星标语义（`WordOverlay.ToggleStar`）与视觉（`BubbleRenderer` 的 `showStar/starred`）。
- **决策**：
  - `ExtendedTextOverlay.Init` 直接持有与对话路径**同一个** `Vocabulary` 实例，不新建收藏体系，不改 `VocabData`/`Exporter`；
  - 气泡布局复用 `BubbleRenderer.ComputeLayout(..., showStar: true)` 与 `Draw(..., starred, ..., showStar: true)`，星标命中用既有 `BubbleLayout.StarBox`（24×24，气泡左侧内边距内）；
  - 左键菜单 prefix：先 `TryToggleStar`（命中星标则收藏/取消并吞掉），否则气泡区域只吞不 toggle；右键 prefix 只吞不 toggle（与对话路径一致，星标仅响应左键）；
  - `VocabEntry.Sentence` 取命中词所在捕获行文本（`SpriteWordHit.LineText`，经 `WordOverlay.CleanSentence` 清理 `^`/`^^`），`FirstSeen` 用当前游戏年/季/日；toggle 走 `Vocabulary.Add/Remove`，内部立即 `WriteJsonFile`（沿用决策 2）。
- **原因**：① 复用已验证的语义与布局，数据格式/导出零改动，F9 导出天然包含扩展面收藏；② 星标与气泡同属一个覆盖层，命中框天然一致，避免第二套坐标与桥接；③ 立即写盘避免退出丢词。
- **影响文件**：`ExtendedTextOverlay.cs`、`ModEntry.cs`、`SpriteTextWordLayout.cs`（`SpriteWordHit.LineText`）、`WordOverlay.cs`（`CleanSentence` 仅提升可见性为 `internal static`）。`BubbleRenderer.cs`、生词本模型/导出、其它菜单均零改动。
- **替代方案及否决原因**：新建扩展面专用收藏列表 → 否决：会出现两套生词本与导出分叉，与对话收藏不一致；把点击处理做进 `BubbleRenderer` → 否决：渲染器不应持有菜单/生词本状态，且 5a 的统一吞点击已建在菜单 prefix 上；给扩展面另算一套星标坐标 → 否决：`BubbleLayout.StarBox` 已是唯一事实来源，重复计算易漂移。



