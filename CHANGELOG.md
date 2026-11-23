# Changelog

本项目遵循 [Semantic Versioning](https://semver.org/)。

## [Unreleased]

### Added

- 实验性：**信件 / 秘密纸条 / 日志残页 / 任务日志**悬停查词（GMCM 新增 `Extended lookup`，默认 `Off`）。启用后鼠标停在正文单词上即显示「词性 + 释义」气泡；扩展面为只读气泡，暂不支持星标收藏。
- 翻页/滚动后不会残留旧气泡：每帧按当前绘制文本重建词矩形，并校验该位置仍是原词；命中范围裁剪到菜单矩形，被裁掉的文字不响应。
- 统一文本捕获基础设施：`SpriteText.drawString` + 全部 4 个 `SpriteBatch.DrawString` 重载（补上卡片此前未覆盖的 9 参 StringBuilder 重载）。
- 调试键 `F10`：显示/关闭词矩形并输出 `[诊断]` 日志，用于实机对位；不进 GMCM、默认关闭、关闭时零开销（用意见 README「测试键 F10 的用意」）。

### Fixed

- 扩展文本面矩形逐词右漂（每个词约 +4px，单字母最明显）：SpriteFont 文本改用「行内前缀 `MeasureString`」定位，替代「词宽 + 空格宽」累加。
- `drawTextWithShadow` 的阴影与正文被重复捕获、保留阴影导致整体偏移 2~3px：去重改为保留后到的正文。
- `^` 换行在 SpriteFont 文本中被当成普通字符，导致多行文本越往后越偏：统一按换行处理。
- `width=-1` 时游戏内部会钳制 `position.X`、而 `^` 换行重置回原始 `x`：布局区分「行首起点」与「换行原点」。

### Changed

- `manifest.json` 的 `MinimumGameVersion` 由 `1.6.0` 收紧到实测版本 `1.6.15`（实现依赖游戏私有字段 `Game1._activeClickableMenu` 与反射签名）。
- 需求文档新增「§9 实现偏离记录」，把 `0.5.0` 相对冻结需求的三处调整（零延迟隐藏、自制气泡 PNG、Alt+点击未实现）写实，避免后续被当作 bug 回修。
- 开关键改名：`LetterLookup` → `ExtendLookup`（旧字段不再生效，需在 GMCM 重新开启）。

> 本轮无版本号变更，仍为 `0.5.0`；`dist/` 内发布包未重建（见 `docs/planning/开发计划与节点控制.md` §9）。


## [0.5.0] - 2026-11-21

### Added

- 悬停对话单词显示「词性 + 中文释义」气泡（覆盖 NPC 对话 / 纯文本对话框 / 问题提示）。
- 星标生词本：按存档隔离持久化，记录单词/词性/释义/原句/首次游戏时间。
- 星露谷专属词典：基于 ECDICT 精筛 + 释义收敛 + 变形覆盖（约 5.2 万词，含 `went→go`、`channels→channel` 等）。
- 生词导出：Markdown / CSV / Anki，导出位置可配置。
- GMCM 配置：开关/导出快捷键、导出位置。
- 专有名词按类别显示官方译名（NPC 名自动跳过）。
- 查词总开关（默认 `F11`）。

### Changed

- 字体子集化：`Font.ttf` 15.6MB → 2.08MB（按词典实际用字裁剪）。
- 移除等级过滤功能（悬停为主动行为，无需过滤）。
- 气泡中文释义字号对齐对话框原文（48f），颜色沿用原配色。

### Fixed

- 生词本改为立即写盘（不再随游戏存档周期，退出即保存）。
- 导出例句清理游戏对话 `^` 换行/分页标记。
- GMCM 注册推迟到 `GameLaunched`，修复「API before mods initialized」报错。
