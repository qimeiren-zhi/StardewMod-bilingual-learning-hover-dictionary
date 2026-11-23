using System;
using System.Collections.Generic;
using GenericModConfigMenu;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace BilingualLearning;

/// <summary>Mod 入口点：注册 Harmony 补丁、覆盖层绘制、生词本、F11 开关、名称过滤、语言校验。</summary>
public class ModEntry : Mod
{
    private static ModEntry? instance;

    private readonly WordOverlay overlay = new();
    private ExtendedTextOverlay? extendedOverlay;
    private Vocabulary? vocabulary;
    private ModConfig config = new();

    private static string? lastTooltipTitle;
    private static string? lastTooltipBody;
    private static object[]? lastTooltipArgs;
    private static System.Reflection.MethodInfo? drawHoverTextMethod;
    private IClickableMenu? pendingRestore;
    private DictionaryData? dictionary;
    private ChineseFont? chineseFont;
    private Texture2D? bubbleDefault;
    private Texture2D? bubbleSandy;

    public override void Entry(IModHelper helper)
    {
        instance = this;

        // 配置 + 词典 + 中文字体 + 生词本
        this.config = helper.ReadConfig<ModConfig>();
        this.dictionary = DictionaryData.Load(helper, this.Monitor);
        this.chineseFont = ChineseFont.Load(helper, this.Monitor, 16f * SpriteText.FontPixelZoom);
        this.vocabulary = new Vocabulary(helper, this.Monitor);

        // 生词本在存档加载后才读取（需存档已加载）
        helper.Events.GameLoop.SaveLoaded += (sender, e) => this.vocabulary?.Load();

        // 人物名称集合（NPC 名，用于过滤）
        var nameSet = this.LoadNpcNames(helper);

        this.bubbleDefault = this.LoadBubbleTexture(helper, "assets/bubble_default.png");
        this.bubbleSandy = this.LoadBubbleTexture(helper, "assets/bubble_sandy.png");

        this.overlay.Init(this.Monitor, this.dictionary, this.chineseFont, this.vocabulary!, nameSet, this.config, this.bubbleDefault, this.bubbleSandy);

        // 阶段0/2：扩展文本面（信件 / 任务日志）悬停查词
        this.extendedOverlay = new ExtendedTextOverlay();
        this.extendedOverlay.Init(this.Monitor, this.dictionary, this.chineseFont, nameSet, this.config, this.bubbleDefault, this.bubbleSandy);

        // 每帧绘制前清空捕获缓冲（本帧结果在下一帧用于命中）；关闭功能且未开调试时不产生捕获开销
        helper.Events.Display.Rendering += (sender, e) =>
        {
            TextCapture.BeginFrame();
            TextCapture.Enabled = this.config.ExtendLookup == "On"
                || (this.extendedOverlay?.DebugRects ?? false);
        };

        var harmony = new Harmony(this.ModManifest.UniqueID);

        // S2: hover 命中检测
        harmony.Patch(
            original: AccessTools.Method(
                typeof(DialogueBox),
                nameof(DialogueBox.performHoverAction),
                new[] { typeof(int), typeof(int) }
            ),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.AfterHover))
        );

        // S3: 星标点击分层（命中星标时吞掉点击，不翻页）
        harmony.Patch(
            original: AccessTools.Method(
                typeof(DialogueBox),
                nameof(DialogueBox.receiveLeftClick),
                new[] { typeof(int), typeof(int), typeof(bool) }
            ),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.BeforeClick))
        );

        // S3: 气泡覆盖层绘制
        helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;

        // 知识卡片：观察 drawHoverText（只读参数，不改变游戏逻辑）
        var drawHoverText = FindDrawHoverTextMethod();
        if (drawHoverText != null)
        {
            drawHoverTextMethod = drawHoverText;
            harmony.Patch(
                original: drawHoverText,
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.AfterDrawHoverText))
            );
        }
        else
        {
            this.Monitor.Log("未找到 IClickableMenu.drawHoverText，知识卡片将不可用", LogLevel.Warn);
        }

        // 捕获卡片重放期间所有 DrawString 文本的位置（用于全文字悬浮查词）
        var drawStringSignatures = new[]
        {
            // 4 参数 string（drawHoverText 的标题）
            new[] { typeof(SpriteFont), typeof(string), typeof(Vector2), typeof(Color) },
            // 9 参数 string（Utility.drawTextWithShadow：类别/增益/价格）
            new[] { typeof(SpriteFont), typeof(string), typeof(Vector2), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) },
            // 4 参数 StringBuilder（Item.drawTooltip：物品描述）
            new[] { typeof(SpriteFont), typeof(System.Text.StringBuilder), typeof(Vector2), typeof(Color) }
        };
        foreach (var signature in drawStringSignatures)
        {
            var method = AccessTools.Method(typeof(SpriteBatch), "DrawString", signature);
            if (method != null)
            {
                harmony.Patch(
                    original: method,
                    postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.AfterDrawStringArgs))
                );
            }
        }

        // 阶段0：统一文本捕获（SpriteText.drawString + 4 个 DrawString 重载）
        var spriteTextDrawString = AccessTools.Method(typeof(SpriteText), "drawString");
        if (spriteTextDrawString != null)
        {
            harmony.Patch(
                original: spriteTextDrawString,
                postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.AfterSpriteTextDrawString))
            );
        }
        else
        {
            this.Monitor.Log("未找到 SpriteText.drawString，扩展文本面将不可用", LogLevel.Warn);
        }

        var captureSignatures = new[]
        {
            // 4 参数 string
            new[] { typeof(SpriteFont), typeof(string), typeof(Vector2), typeof(Color) },
            // 9 参数 string
            new[] { typeof(SpriteFont), typeof(string), typeof(Vector2), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) },
            // 4 参数 StringBuilder
            new[] { typeof(SpriteFont), typeof(System.Text.StringBuilder), typeof(Vector2), typeof(Color) },
            // 9 参数 StringBuilder（卡片未覆盖的第 4 个重载）
            new[] { typeof(SpriteFont), typeof(System.Text.StringBuilder), typeof(Vector2), typeof(Color), typeof(float), typeof(Vector2), typeof(float), typeof(SpriteEffects), typeof(float) }
        };
        foreach (var signature in captureSignatures)
        {
            var method = AccessTools.Method(typeof(SpriteBatch), "DrawString", signature);
            if (method != null)
            {
                harmony.Patch(
                    original: method,
                    postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.AfterDrawStringCapture))
                );
            }
        }

        // F11/F9 快捷键
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;

        // 知识卡片关闭后恢复原菜单
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;

        // GMCM 配置菜单（可选前置）：在 GameLaunched（所有 mod 初始化完成后）再注册
        helper.Events.GameLoop.GameLaunched += (sender, e) => this.RegisterConfigMenu(helper);
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        // 知识卡片被关闭（activeClickableMenu 变 null）后，恢复打开前的菜单
        if (this.pendingRestore != null && Game1.activeClickableMenu == null)
        {
            this.Helper.Reflection.GetField<IClickableMenu>(typeof(Game1), "_activeClickableMenu").SetValue(this.pendingRestore);
            this.pendingRestore = null;
        }
    }

    private Texture2D? LoadBubbleTexture(IModHelper helper, string path)
    {
        try
        {
            return helper.ModContent.Load<Texture2D>(path);
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"加载气泡背景失败（{path}）：{ex.Message}", LogLevel.Warn);
            return null;
        }
    }

    private static System.Reflection.MethodInfo? FindDrawHoverTextMethod()
    {
        foreach (var m in AccessTools.GetDeclaredMethods(typeof(IClickableMenu)))
        {
            if (m.Name == "drawHoverText"
                && m.GetParameters().Length > 2
                && m.GetParameters()[1].ParameterType == typeof(string))
            {
                return m;
            }
        }
        return null;
    }

    private static void AfterDrawStringArgs(object[] __args)
    {
        if (!TooltipTextCapture.Capturing || __args.Length < 3)
        {
            return;
        }
        if (!(__args[0] is SpriteFont font) || !(__args[2] is Vector2 pos))
        {
            return;
        }

        string? text = __args[1] as string;
        if (text == null && __args[1] is System.Text.StringBuilder sb)
        {
            text = sb.ToString();
        }
        if (!string.IsNullOrEmpty(text))
        {
            TooltipTextCapture.Record(font, text, pos);
        }
    }

    /// <summary>阶段0：记录 SpriteText 绘制（只读，不改变游戏绘制）。</summary>
    private static void AfterSpriteTextDrawString(object[] __args)
    {
        if (__args.Length < 12 || !ShouldCapture())
        {
            return;
        }
        if (!(__args[1] is string text)
            || !(__args[2] is int x)
            || !(__args[3] is int y)
            || !(__args[4] is int characterPosition)
            || !(__args[5] is int width)
            || !(__args[10] is int drawBgScroll))
        {
            return;
        }

        TextCapture.RegisterSpriteText(text, x, y, characterPosition, width, drawBgScroll);
    }

    /// <summary>阶段0：记录 SpriteFont 绘制（四个重载共用）。</summary>
    private static void AfterDrawStringCapture(object[] __args)
    {
        if (__args.Length < 4 || !ShouldCapture())
        {
            return;
        }
        if (!(__args[0] is SpriteFont font) || !(__args[2] is Vector2 position))
        {
            return;
        }

        string? text = __args[1] as string;
        if (text == null && __args[1] is System.Text.StringBuilder sb)
        {
            text = sb.ToString();
        }
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        Vector2 origin = Vector2.Zero;
        float rotation = 0f;
        float scale = 1f;
        if (__args.Length >= 9)
        {
            if (__args[4] is float r)
            {
                rotation = r;
            }
            if (__args[5] is Vector2 o)
            {
                origin = o;
            }
            if (__args[6] is float s)
            {
                scale = s;
            }
        }

        TextCapture.RegisterSpriteFont(text, font, position, origin, rotation, scale);
    }

    private static bool ShouldCapture()
    {
        // 卡片重放 tooltip 期间由 TooltipTextCapture 负责，避免双重记录
        if (TooltipTextCapture.Capturing || Game1.activeClickableMenu is KnowledgeCardMenu)
        {
            return false;
        }

        // 只覆盖白名单菜单（阶段0/2：信件、任务日志），其它菜单不产生捕获开销
        if (!ExtendedTextOverlay.IsAllowedMenu(Game1.activeClickableMenu))
        {
            return false;
        }

        return LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en;
    }

    private static void AfterDrawHoverText(object[] __args)
    {
        if (__args.Length <= 9)
        {
            return;
        }

        // 商店：物品 tooltip（drawToolTip → drawHoverText，参数 9 是 Item）之后，
        // base.draw 里还会为店长问候语再调用一次 drawHoverText（参数 9 不是 Item）。
        // 后者会覆盖缓存导致 F1 打开问候语卡片，这里直接忽略非物品 tooltip。
        if (Game1.activeClickableMenu is ShopMenu && __args[9] is not Item)
        {
            return;
        }

        lastTooltipBody = __args[1] as string;
        lastTooltipTitle = __args[6] as string;
        lastTooltipArgs = (object[])__args.Clone();
    }

    private void RegisterConfigMenu(IModHelper helper)
    {
        var api = helper.ModRegistry.GetApi<IApi>("spacechase0.GenericModConfigMenu");
        if (api == null)
        {
            return; // 未安装 GMCM
        }

        var manifest = this.ModManifest;
        api.RegisterModConfig(
            manifest,
            () => this.config = new ModConfig(),
            () => helper.WriteConfig(this.config)
        );

        api.SetDefaultIngameOptinValue(manifest, true);

        api.RegisterSimpleOption(manifest, "Toggle key", "Toggle the lookup feature on/off", () => this.config.ToggleKey, value => this.config.ToggleKey = value);
        api.RegisterSimpleOption(manifest, "Export key", "Export the vocabulary", () => this.config.ExportKey, value => this.config.ExportKey = value);
        api.RegisterChoiceOption(manifest, "Export location", "Where to save exported files", () => this.config.ExportLocation, value => this.config.ExportLocation = value, new[] { "Documents", "Desktop", "Downloads", "Mod folder" });
        api.RegisterChoiceOption(manifest, "Bubble style", "Bubble background and text color style", () => this.config.BubbleStyle, value => this.config.BubbleStyle = value, new[] { "Default", "Sandy's Notes" });
        api.RegisterChoiceOption(manifest, "Extended lookup", "Look up words in letters and the quest log (experimental)", () => this.config.ExtendLookup, value => this.config.ExtendLookup = value, new[] { "Off", "On" });
    }

    private HashSet<string> LoadNpcNames(IModHelper helper)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var characters = helper.GameContent.Load<Dictionary<string, string>>("Data/NPCGiftTastes");
            foreach (string name in characters.Keys)
            {
                set.Add(name);
            }
            this.Monitor.Log($"NPC 名称过滤：已加载 {set.Count} 个名称", LogLevel.Info);
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"加载 NPC 名单失败：{ex.Message}", LogLevel.Warn);
        }
        return set;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button == this.config.ToggleKey)
        {
            this.overlay.Enabled = !this.overlay.Enabled;
            this.Monitor.Log($"学习模式：{(this.overlay.Enabled ? "开启" : "关闭")}", LogLevel.Info);
        }
        else if (e.Button == this.config.ExportKey)
        {
            // 导出生词本
            if (this.vocabulary != null && this.vocabulary.Words.Count > 0)
            {
                Exporter.Export(this.vocabulary.Words, this.config.ExportLocation, this.Helper.DirectoryPath, this.Monitor);
            }
            else
            {
                this.Monitor.Log("生词本为空，无需导出", LogLevel.Info);
            }
        }
        else if (e.Button == this.config.DebugTextRectsKey && this.extendedOverlay != null)
        {
            this.extendedOverlay.DebugRects = !this.extendedOverlay.DebugRects;
            this.Monitor.Log($"文本矩形调试：{(this.extendedOverlay.DebugRects ? "开启" : "关闭")}（上帧捕获 {TextCapture.LastCount} 行）", LogLevel.Info);
            if (this.extendedOverlay.DebugRects)
            {
                this.extendedOverlay.DumpDiagnostics();
            }
        }
        else if (e.Button == this.config.LookupKey)
        {
            // 知识卡片：已打开则关闭，否则用上一帧捕获的 tooltip 打开
            if (Game1.activeClickableMenu is KnowledgeCardMenu existing)
            {
                existing.exitThisMenu();
                return;
            }
            if (!string.IsNullOrEmpty(lastTooltipTitle) || !string.IsNullOrEmpty(lastTooltipBody))
            {
                this.pendingRestore = Game1.activeClickableMenu;
                var card = new KnowledgeCardMenu(drawHoverTextMethod, lastTooltipArgs, this.dictionary, this.vocabulary, this.chineseFont, this.config, this.bubbleDefault, this.bubbleSandy);

                // 绕过 Game1.activeClickableMenu setter（它会 Dispose 原菜单），避免关闭后回不到背包
                this.Helper.Reflection.GetField<IClickableMenu>(typeof(Game1), "_activeClickableMenu").SetValue(card);
                this.Monitor.Log($"知识卡片已打开：{lastTooltipTitle}", LogLevel.Info);
            }
            else
            {
                this.Monitor.Log("当前没有可用的 tooltip（先悬停一个物品）", LogLevel.Info);
            }
        }
    }

    private static void AfterHover(DialogueBox __instance, int mouseX, int mouseY)
    {
        instance?.overlay.OnHover(__instance, mouseX, mouseY);
    }

    private static bool BeforeClick(DialogueBox __instance, int x, int y)
    {
        return instance?.overlay.OnClick(__instance, x, y) ?? true;
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is DialogueBox box)
        {
            this.overlay.Draw(e.SpriteBatch);

            // 气泡画完后重绘菜单光标，确保光标浮在气泡之上
            box.drawMouse(e.SpriteBatch);
        }

        // 阶段0/2：扩展文本面（信件 / 任务日志）查词
        if (this.extendedOverlay != null)
        {
            this.extendedOverlay.UpdateAndDraw(e.SpriteBatch);
            if (this.extendedOverlay.HasBubble)
            {
                this.extendedOverlay.DrawCursor(e.SpriteBatch);
            }
        }
    }
}
