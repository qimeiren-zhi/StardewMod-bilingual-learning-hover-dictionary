# Nexus 发布文案（Bilingual Learning / 星露谷双语学习）

> 版本 0.5.0。直接复制到 Nexus Mods 的「描述」栏。截图已备 3 张（悬停查词、生词本导出、GMCM 配置页）。

## 一句话简介

边玩《星露谷物语》边学英语：悬停对话单词即现中文释义，星标收藏生词，一键导出复习。

> Learn English while playing Stardew Valley: hover any word in dialogue to see its Chinese meaning, star words into a per-save vocabulary, and export for review.

## 功能

- 🖱 **对话框内悬浮查词**：对话中的英文单词，鼠标悬停即显示「词性 + 中文释义」气泡。
- ⭐ **生词本**：点星标把可选词加入生词本，按存档隔离，记录单词/词性/释义/原句/首次游戏时间。
- 📖 **星露谷专属词典**：基于 ECDICT 精筛 + 释义收敛，覆盖游戏常用词与变形（`went→go`、`channels→channel`），专有名词按类别显示官方译名（NPC 名自动跳过）。
- 📤 **生词本导出**：一键导出 Markdown / CSV / Anki，导出位置可配置。
- ⚙️ **GMCM 配置**：开关/导出快捷键、导出位置可在游戏内配置。

## 环境要求

- Stardew Valley 1.6.x + SMAPI 4.x + .NET 6
- 可选：Generic Mod Config Menu（GMCM）

## 使用

| 操作 | 按键（默认） |
|---|---|
| 查词 | 悬停英文单词 |
| 收藏/取消 | 点击气泡星标 |
| 开/关 | `F11` |
| 导出生词 | `F9` |

游戏语言需设为**英文**（中文等其它语言下自动停用）。

## AI 使用声明（AI Usage Disclosure）

本 mod 标注为 **AI Assisted**。所有核心设计、玩法逻辑与最终实现决策均由开发者完成。

> This mod is tagged **AI Assisted**. All core design, gameplay logic, and final implementation decisions were made by the developer.
>
> - **Code**: AI was used as a coding assistant for C# boilerplate (SMAPI entry, Harmony patch scaffolding), partial UI rendering code, and debugging. All AI-assisted code was reviewed and validated by the developer, who fully understands each module. Core gameplay logic, UI/UX **design decisions**, and in-game testing were done manually.
> - **Dictionary content**: Definitions are sourced from **ECDICT** (a human-curated English–Chinese dictionary). AI was used only to assist in **selecting and converging senses** to fit game context — the translation text originates from ECDICT, not from AI generation.
> - **Assets**: No AI-generated art, textures, or audio. All visual elements reuse the base game's original assets.
> - **Development evidence**: The project maintains complete version-control history (git commits) documenting the human-led development process.

> ⚠️ 请务必在 Nexus Tag 系统勾选 **AI Assisted**；若审核质疑，准备 git 提交记录截图并解释代码。

## 更新日志（CHANGELOG）

### 0.5.0 — 2026-11-21（首个公开版）

- 新增：悬停对话单词显示词性 + 中文释义气泡（覆盖 NPC 对话 / 纯文本对话框 / 问题提示）。
- 新增：星标生词本，按存档隔离，记录单词/词性/释义/原句/首次游戏时间。
- 新增：星露谷专属词典（ECDICT 精筛 + 释义收敛 + 变形覆盖，约 5.2 万词）。
- 新增：生词导出 Markdown / CSV / Anki，导出位置可配置。
- 新增：GMCM 配置（开关/导出快捷键、导出位置）。
- 优化：字体子集化（15.6MB → 2.08MB）。

## 截图

- 悬停查词气泡 + 星标
- 生词本导出效果
- GMCM 配置页

---

### 待补充

- [ ] Logo/图标（**可选**，Nexus 不强制要求，仅影响列表页观感）
