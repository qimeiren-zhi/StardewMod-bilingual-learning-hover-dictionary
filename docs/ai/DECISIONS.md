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
