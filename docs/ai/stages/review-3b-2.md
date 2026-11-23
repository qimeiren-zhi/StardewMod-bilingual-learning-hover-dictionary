# 阶段 3b-2 评审（Billboard 悬停查词）

角色：只读评审 SubAgent；分支 `stage/3b-2-billboard`，比对 `master...HEAD`。

```text
VERDICT: PASS

BLOCKING: 无。
- 越界：无。`git diff --name-status master...HEAD` = `M src/BilingualLearning/ExtendedTextOverlay.cs` + `A docs/ai/stages/stage-3b-2.md`，共 2 文件；manifest.json / csproj / 版本号 / dist / bin / obj / references 均未改（name-only 为空）。无删除：numstat 3+/2− 为「注释行替换 + CarpenterMenu 补逗号」，4 个既有菜单条目全部保留。
- 白名单：仅新增 1 个类型（ExtendedTextOverlay.cs:26 `typeof(Billboard)`），并列于 LetterViewerMenu/QuestLog/SpecialOrdersBoard/CarpenterMenu 之后（:22-25）。
- 注释：:14 追加「阶段3b-2：小镇任务板/日历（Billboard）」，与既有阶段措辞一致、指向准确。
- 闸门/布局未动：`git diff ... -- src/` 中不出现 IsEnabled/ShouldCapture/RebuildWordRects/BuildSpriteFontHits/TextCapture；IsAllowedMenu（:126-141）仍按 AllowedMenus 做 IsInstanceOfType，普通泛化，无特例分支；Rectangle 裁剪（:281、:299、:314）与 ≤4px 去重（TextCapture.cs:159-171）复用现状。
- 构建：0 error（1 个已知 CS9057 警告）。命令 `$env:DOTNET_ROOT='C:\Users\zhiqi\dotnet'; dotnet build ... -c Debug -p:EnableModDeploy=false` →「已成功生成」。
- 部署一致性：bin\Debug\BilingualLearning.dll 与 Mods\BilingualLearning\BilingualLearning.dll SHA256 均为 BB6E0CC…C66842（一致），故报告 §2「未部署」为执行时事实，主控补的带部署构建已对齐。
- 类型解析：`using StardewValley.Menus`（:8）已覆盖 Billboard；src 内无同名 Billboard 冲突，编译 0 error 佐证唯一解析。

NON_BLOCKING:
1. stage-3b-2.md §0 元信息仍为 `QUEUED` / `<待派发>`，§3 模板未填（本文件承担评审结论）；建议主控在验收时统一回填 §0/§4。
2. 未实机验证（报告 §2 RISKS 已明示）：「未部署」已由主控构建补齐，但 5 条 USER_TESTS 仍需真人跑，尤其 #2 长描述压底部 Accept Quest 的视觉遮挡——只读气泡不拦截点击（按钮仍可点），风险可接受。
3. 日历分支噪声评估：该分支新增可见文本仅季节名 + UI:Billboard_Year（日号烘在 LooseSprites\Billboard 贴图、无文本），故 Capture 不新增误命中面；季节词命中词典会出气泡，属预期。drawHoverText（生日/事件）不接管为设计内。
4. 多行描述由单次 Utility.drawTextWithShadow 画出，BuildSpriteFontHits（:355-361）按 `^`/换行 + `row*LineSpacing` 推 y；若实际行距与 Font.LineSpacing 有差，行位置可能像素级偏移——建议 F10 实机对位确认（不改代码，仅观察）。

EVIDENCE（关键行/输出）:
- `git rev-parse --abbrev-ref HEAD` → stage/3b-2-billboard；工作树干净。
- `git diff --stat master...HEAD` → 2 files changed, 135 insertions(+), 2 deletions(-)。
- src diff 内容：仅 :14 注释一行替换；:25 `typeof(CarpenterMenu)` → 增逗号 + :26 `typeof(Billboard)`。
- 构建尾部：「已成功生成。… 1 个警告 / 0 个错误」。
- SHA256 双路径一致（见上）。
- 报告 §2 与 diff 对账：源码 1 文件 3+/2−、分支 2 文件、白名单 4 旧 + 1 新、未动闸门 —— 与实测逐项吻合。
```
