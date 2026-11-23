# 已修 Bug 模式库（Bilingual Learning）

> 症状 → 触发条件 → 根因 → 修复方式 → 涉及文件 → 验证方法 → 搜索关键词。
> 「搜索关键词」用于快速定位相关代码/日志，新会话可先 `rg` 这些词。

| # | 症状 | 触发条件 | 根因 | 修复方式 | 涉及文件 | 验证方法 | 搜索关键词 |
|---|---|---|---|---|---|---|---|
| 1 | mod 加载崩溃：`InvalidOperationException: Can't use Helper.Data.ReadSaveData when a save file isn't loaded` | 在 `Entry()` 里调 `vocabulary.Load()` | mod 加载时存档未加载 | 推迟到 `GameLoop.SaveLoaded` 再读 | `ModEntry.cs:45` `Vocabulary.cs:26` | 启动进入存档无崩溃，控制台有「生词本加载完成」 | `ReadSaveData` `SaveLoaded` `当存档未加载` `before a save` |
| 2 | 生词本退出即丢（不睡觉退出重进词没了） | 用 `WriteSaveData` 存生词本 | `WriteSaveData` 只写内存 `Game1.CustomData`，随游戏存档周期 | 改 `WriteJsonFile`/`ReadJsonFile` 立即落盘 | `Vocabulary.cs:56` `Vocabulary.cs:62` | 收藏→退出重进→星标仍亮；`data/<存档id>.json` 有内容 | `WriteSaveData` `WriteJsonFile` `ReadJsonFile` `GetSavePath` `落盘` |
| 3 | `加载 NPC 名单失败：failed loading content asset 'Data/Characters'` | `LoadNpcNames` 用 `Data/Characters` | 1.6 资源类型不符 | 改用 `Data/NPCGiftTastes` | `ModEntry.cs` `LoadNpcNames` | 悬停 Penny/Sam 不弹释义；控制台无该告警 | `Data/Characters` `Data/NPCGiftTastes` `加载 NPC 名单失败` |
| 4 | 控制台报 `Tried to access a mod-provided API before all mods were initialized` | `Entry()` 里调 `GetApi`（GMCM） | 其他 mod 未初始化 | 推迟到 `GameLoop.GameLaunched` | `ModEntry.cs:124` | 启动无该报错；GMCM 配置页出现 | `GetApi` `GameLaunched` `before all mods were initialized` |
| 5 | GMCM 配置页中文标签显示为符号/乱码 | 用中文做 GMCM 标签 | 英文游戏字体无中文字形 | 标签改英文（Toggle key/Export key/Export location/Bubble style） | `ModEntry.cs` `RegisterConfigMenu` | GMCM 页英文正常显示 | `Toggle key` `Export location` `Bubble style` `GMCM` `乱码` |
| 6 | GMCM 游戏内设置页不可用（仅标题页可用） | 未 opt-in | GMCM 默认不允许游戏内编辑 | `SetDefaultIngameOptinValue(manifest, true)` | `ModEntry.cs` `RegisterConfigMenu` `IApi.cs` | 游戏内按 E 打开 GMCM 能看到本 mod | `SetDefaultIngameOptinValue` `ingame` |
| 7 | 大量常见/变形词无气泡（`channeled`/`pelican`），词量似变少 | 等级过滤开启 + 词 tag 为空 | 把「空 tag」当「低于阈值」，ECDICT 空 tag 实为生僻/变形/专有词（占 52.8%） | **移除等级过滤**（悬停是主动行为，无需过滤） | `WordOverlay.cs`（`IsLevelMatch` 已删）、`ModConfig.cs`（`EnabledLevels` 已删） | 悬停 `channeled`/`pelican` 出气泡 | `IsLevelMatch` `EnabledLevels` `无标签` `等级过滤` `52.8` |
| 8 | 光标被气泡遮挡（光标画在气泡下面） | 气泡画在 `DialogueBox.draw` 末尾光标之后 | 游戏光标绘制时机/层 | `RenderedActiveMenu` 画完气泡后 `box.drawMouse(e.SpriteBatch)` 重绘光标 | `ModEntry.cs:291` `OnRenderedActiveMenu` | 气泡上光标可见 | `drawMouse` `RenderedActiveMenu` `光标` |
| 9 | 鼠标从单词移向气泡时气泡消失 | 命中只认词矩形 | 词与气泡间有 4px 间隙 + 气泡区域不算命中 | 「词 ∪ 气泡 ∪ 桥接矩形」连通判定；零延迟 | `WordOverlay.cs`（`bridgeRect`）、`KnowledgeCardMenu.cs:216`（`ComputeBridge`） | 鼠标移入气泡不消失，能点星标 | `bridgeRect` `ComputeBridge` `连通` `桥接` `死区` |
| 10 | Python/构建报 `PermissionError` / `UnauthorizedAccessException: Access to the path '...bubble_default.png' is denied` | 从附件复制的 PNG 继承只读属性 | 文件只读 | 清除只读（`$_.IsReadOnly = $false`，源码目录 + 游戏 Mods 目录两处） | `assets/bubble_*.png` | 能覆盖写入、构建部署成功 | `IsReadOnly` `只读` `UnauthorizedAccessException` `PermissionError` |
| 11 | 知识卡片里只有物品**标题**能查词，类别/增益/价格/描述不能 | 只 hook 了 4 参数 `string` 的 `SpriteBatch.DrawString` | `Utility.drawTextWithShadow` 用 **9 参数**重载、`Item.drawTooltip` 用 **StringBuilder** 重载 | 同时 hook 3 个重载，统一 `AfterDrawStringArgs` 记录 | `ModEntry.cs:110` `ModEntry.cs:164` `TooltipTextCapture.cs` | 卡片内类别/增益/价格/描述都能悬停查词 | `AfterDrawStringArgs` `DrawString` `StringBuilder` `drawTextWithShadow` `DrawString` 重载 |
| 12 | 知识卡片内点击气泡/星标无反应（死区） | 气泡超出卡片矩形 | 游戏只把点击传给 `isWithinBounds` 范围内的菜单 | `override bool isWithinBounds(...) => true` | `KnowledgeCardMenu.cs:296` | 星标可点击收藏/取消 | `isWithinBounds` `死区` `receiveLeftClick` |
| 13 | 知识卡片气泡里显示的是英文单词而不是中文释义 | 绘制时传了 `hoveredWord` 而非翻译 | 变量传错 | 存 `hoveredTranslation` 并传入 `BubbleRenderer.Draw` | `KnowledgeCardMenu.cs` `draw`/`UpdateHover` | 气泡显示「n. xxx」而非单词 | `hoveredTranslation` `BubbleRenderer.Draw` `hoveredWord` |
| 14 | 自定义菜单中光标不显示 | 用 `IClickableMenu` 自定义菜单 | 自定义菜单需自己画光标 | `draw` 末尾 `this.drawMouse(b)` | `KnowledgeCardMenu.cs` `draw` | 卡片上光标可见 | `drawMouse` `光标` `IClickableMenu` |
| 15 | 无中文解义的 `is` 等词不显示（预期行为，非 bug） | 词典未收录 | 未收录词按 F13 不显示气泡 | 无需修（F13 设计） | `WordOverlay.cs` `TryHitWord` | 未收录词无气泡 | `TryHitWord` `未收录` `F13` |
| 16 | 行为差异：`saw` 显示「锯子」（see 的过去式应为「看见」） | 同形异义词 | 词典按拼写精确匹配，无上下文消歧 | 未修（已知局限，需上下文/WSD） | `dictionary.tsv` | 悬停 `saw`（see 过去式）显示「n. 锯子」 | `saw` `同形` `WSD` `歧义` |
