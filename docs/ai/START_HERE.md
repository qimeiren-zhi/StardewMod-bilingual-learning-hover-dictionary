# 新会话启动指令（START HERE）

> 复制下面代码块作为新会话的第一条指令。它会引导新 Agent 先读交接包、先检索、先报缺口，**不要直接改代码**。

```text
你在维护「星露谷双语学习 Mod」（D:\Project\星露谷双语mod）。
开始任何工作前，请按顺序：
1. 读 AGENTS.md（长期规则/项目地图/构建命令/禁止事项）；
2. 读 docs/ai/HANDOFF.md（当前目标/验收/已改文件/待确认）；
3. 读 docs/ai/BUG_PATTERNS.md 与 docs/ai/DECISIONS.md（避免重复踩坑）；
4. 读 docs/ai/CURRENT_TASK.md 确认当前任务是否仍是该任务；
5. 需要背景时读：docs/requirements/（需求）、docs/planning/（计划）、
   docs/specs/（词典数据格式）、docs/guides/（环境与构建）、docs/release/（发布文案）；
6. 用 rg 检索与任务相关的「相似结构」（气泡渲染、桥接连通、Harmony hook 注册点）；
7. 先只读报告：你看到的缺口、你的假设、以及本次不做什么；
8. 等我确认后再改代码。

硬性约束：
- 构建必须用 C:\Users\zhiqi\dotnet\dotnet.exe（不在 PATH）；游戏运行时不能构建部署；
- 不改 references/decompiled/ 下的反编译参考（gitignored，只读）；
- 不改 bin/obj/dist，不提交 tools/ecdict.csv；
- 改游戏内部字段/方法前，先用 Mono.Cecil 核对签名。
```

## 文档地图

| 位置 | 内容 |
|---|---|
| `AGENTS.md` | 长期规则、项目地图、构建/运行/测试、日志、架构约束、禁止事项 |
| `docs/ai/HANDOFF.md` | 当前目标/非目标、验收、已改文件、失败尝试、下一步、待确认 |
| `docs/ai/BUG_PATTERNS.md` | 已修 Bug 模式库（症状→根因→修复→搜索关键词） |
| `docs/ai/DECISIONS.md` | 架构决策及原因 |
| `docs/ai/CURRENT_TASK.md` | 当前任务卡 |
| `docs/ai/HANDOVER_2026-11-23.md` | 2026-11-23 会话归档（扩展文本面：顺序核查、对齐调试、F10 用意、遗留） |
| `docs/requirements/` | 需求规格说明、资源需求（冻结版） |
| `docs/planning/` | 开发计划与节点控制（含 §9 版本记录） |
| `docs/specs/` | 词典数据格式（离线构建 ↔ 运行时数据契约） |
| `docs/guides/` | 开发环境与构建说明 |
| `docs/release/` | Nexus 发布文案 |
| `assets/art/` | 原始气泡设计稿（二进制） |
| `references/decompiled/` | 游戏反编译参考（gitignored，只读） |
