using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace BilingualLearning;

/// <summary>
/// 把查词扩展到非对话框的菜单文本面。
/// 阶段0：信件 / 秘密纸条 / 日志残页；阶段2：任务日志；阶段3a：特殊订单板；阶段3b-1：木匠/建造菜单；阶段3b-2：小镇任务板/日历（Billboard）；阶段3b-3：发货箱日终结算 / 丢物列表；阶段3b-4：商店正文（物品名/价格/以物易物/缺货提示，tooltip 不接管，见 DECISIONS 19）。
/// 只读悬停、不含星标交互；命中范围裁剪到当前菜单矩形内（滚动列表中不命中被裁掉的文字）。
/// </summary>
public class ExtendedTextOverlay
{
    /// <summary>当前启用的文本面白名单。</summary>
    private static readonly Type[] AllowedMenus =
    {
        typeof(LetterViewerMenu),
        typeof(QuestLog),
        typeof(SpecialOrdersBoard),
        typeof(CarpenterMenu),
        typeof(Billboard),
        typeof(ShippingMenu),
        typeof(ItemListMenu),
        typeof(ShopMenu)
    };

    /// <summary>白名单的只读视图，供 ModEntry 注册点击吞掉补丁（阶段 5a）。</summary>
    internal static IReadOnlyList<Type> SupportedMenus => AllowedMenus;

    private IMonitor monitor = null!;
    private DictionaryData? dictionary;
    private ChineseFont? chineseFont;
    private HashSet<string>? nameSet;
    private ModConfig config = new();
    private Texture2D? bubbleDefault;
    private Texture2D? bubbleSandy;

    private object? trackedMenu;
    private readonly List<SpriteWordHit> wordRects = new();

    private string? hoveredWord;
    private string? hoveredTranslation;
    private Rectangle hoveredWordBox;
    private Rectangle bridgeRect;
    private BubbleLayout bubbleLayout;
    private bool hasBubble;

    /// <summary>是否绘制命中矩形（F10 切换，用于实机对位）。</summary>
    public bool DebugRects { get; set; }

    /// <summary>本轮气泡是否已经打印过一次光标诊断（F10 时每个气泡只打一次，避免刷屏）。</summary>
    private bool cursorDiagLogged;

    /// <summary>当前是否正在显示气泡（用于决定是否重绘光标）。</summary>
    public bool HasBubble => this.hasBubble;

    public void Init(
        IMonitor monitor,
        DictionaryData? dictionary,
        ChineseFont? chineseFont,
        HashSet<string> nameSet,
        ModConfig config,
        Texture2D? bubbleDefault,
        Texture2D? bubbleSandy)
    {
        this.monitor = monitor;
        this.dictionary = dictionary;
        this.chineseFont = chineseFont;
        this.nameSet = nameSet;
        this.config = config;
        this.bubbleDefault = bubbleDefault;
        this.bubbleSandy = bubbleSandy;
    }

    /// <summary>一次性诊断输出（F10 开启时调用）：把捕获行与词矩形写进 SMAPI 日志，便于实机对位。</summary>
    public void DumpDiagnostics()
    {
        if (this.monitor == null)
        {
            return;
        }

        this.monitor.Log(
            $"[诊断] 菜单={Game1.activeClickableMenu?.GetType().Name} 捕获={TextCapture.LastCount} 行 " +
            $"zoom={SpriteText.FontPixelZoom} viewport={Game1.uiViewport.Width}x{Game1.uiViewport.Height}",
            LogLevel.Info);

        int lineIndex = 0;
        foreach (CapturedText line in TextCapture.LastFrame)
        {
            if (lineIndex++ >= 8)
            {
                this.monitor.Log("[诊断] …（仅显示前 8 行）", LogLevel.Info);
                break;
            }

            string preview = line.Text.Length > 44 ? line.Text.Substring(0, 44) + "…" : line.Text;
            string fontInfo = "-";
            if (line.Font != null)
            {
                string which = line.Font == Game1.dialogueFont
                    ? "dialogueFont"
                    : (line.Font == Game1.smallFont ? "smallFont" : "otherFont");
                float spaceWidth = line.Font.MeasureString(" ").X;
                fontInfo = $"{which} spacing={line.Font.Spacing} lineSpacing={line.Font.LineSpacing} space={spaceWidth:0.##}";
            }
            this.monitor.Log(
                $"[诊断] {line.Source} rect={line.Bounds} wrap={line.WrapWidth} originX={line.LineOriginX} " +
                $"cp={line.CharacterPosition} scale={line.Scale} font={fontInfo} text='{preview}'",
                LogLevel.Info);

            List<SpriteWordHit> hits = line.Source == TextSource.SpriteText
                ? SpriteTextWordLayout.Build(line.Text, line.Bounds.X, line.LineOriginX, line.Bounds.Y, line.WrapWidth, line.CharacterPosition)
                : BuildSpriteFontHits(line);

            int wordIndex = 0;
            foreach (SpriteWordHit hit in hits)
            {
                if (wordIndex++ >= 30)
                {
                    this.monitor.Log("[诊断]   …（仅显示前 30 词）", LogLevel.Info);
                    break;
                }
                this.monitor.Log($"[诊断]   '{hit.Text}' @ {hit.Box}", LogLevel.Info);
            }
        }
    }

    /// <summary>当前菜单是否属于已支持的文本面。</summary>
    public static bool IsAllowedMenu(IClickableMenu? menu)
    {
        if (menu == null)
        {
            return false;
        }

        foreach (Type type in AllowedMenus)
        {
            if (type.IsInstanceOfType(menu))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>每帧（菜单绘制之后）调用：命中检测 + 绘制气泡 + 调试矩形。</summary>
    public void UpdateAndDraw(SpriteBatch b)
    {
        IClickableMenu? menu = Game1.activeClickableMenu;
        if (!ReferenceEquals(menu, this.trackedMenu))
        {
            this.trackedMenu = menu;
            this.Clear();
        }

        bool enabled = this.IsEnabled();
        if (enabled || this.DebugRects)
        {
            this.RebuildWordRects(menu);
        }
        else
        {
            this.wordRects.Clear();
        }

        if (this.DebugRects)
        {
            this.DrawDebugRects(b);
        }

        if (!enabled)
        {
            this.Clear();
            return;
        }

        Point mouse = Game1.getMousePosition();

        // 已有气泡：鼠标仍在「词 ∪ 气泡 ∪ 桥接」内，且该位置的文本未变（防翻页/滚动后残留）→ 保持
        if (this.hasBubble && this.hoveredWord != null && this.hoveredTranslation != null)
        {
            bool inside = this.hoveredWordBox.Contains(mouse)
                || this.bubbleLayout.BubbleBox.Contains(mouse)
                || this.bridgeRect.Contains(mouse);

            if (inside && this.IsHoveredWordStillPresent())
            {
                this.DrawBubble(b);
                return;
            }
            this.Clear();
        }

        foreach (SpriteWordHit hit in this.wordRects)
        {
            if (!hit.Box.Contains(mouse) || !this.TryBuildTranslation(hit.Text, out string translation))
            {
                continue;
            }

            this.hoveredWord = WordOverlay.NormalizeWord(hit.Text);
            this.hoveredTranslation = translation;
            this.hoveredWordBox = hit.Box;
            this.bubbleLayout = BubbleRenderer.ComputeLayout(hit.Box, translation, this.chineseFont, showStar: false);
            this.bridgeRect = ComputeBridge(hit.Box, this.bubbleLayout.BubbleBox);
            this.hasBubble = true;
            this.DrawBubble(b);
            return;
        }
    }

    /// <summary>气泡画完后重绘菜单光标，确保光标浮在气泡之上。</summary>
    public void DrawCursor(SpriteBatch b)
    {
        // 显式忽略 transparency；第三个参数不传，走 drawMouse 默认 cursor=-1 的 snappy/手柄光标选择逻辑。
        Game1.activeClickableMenu?.drawMouse(b, ignore_transparency: true);

        // 仅在 F10 调试开启时每气泡打印一次光标状态，便于判断是否还有 transparency/硬件光标干扰。
        if (this.DebugRects && !this.cursorDiagLogged && this.monitor != null)
        {
            this.cursorDiagLogged = true;
            this.monitor.Log(
                $"[诊断] cursor 菜单={Game1.activeClickableMenu?.GetType().Name} " +
                $"transparency={Game1.mouseCursorTransparency} cursor={Game1.mouseCursor} " +
                $"hardwareCursor={Game1.options.hardwareCursor}",
                LogLevel.Info);
        }
    }

    /// <summary>
    /// 光标是否落在释义气泡内（含词矩形与桥接矩形）。
    /// 供菜单侧吞掉点击使用，避免「气泡盖住商品行时点击穿透、误购下一行商品」。
    /// </summary>
    public bool IsPointerOnBubble(int x, int y)
    {
        var point = new Point(x, y);
        return this.HasBubble
            && (this.bubbleLayout.BubbleBox.Contains(point)
                || this.hoveredWordBox.Contains(point)
                || this.bridgeRect.Contains(point));
    }

    private bool IsEnabled()
    {
        if (this.config.ExtendLookup != "On" || !IsEnglish())
        {
            return false;
        }

        return IsAllowedMenu(Game1.activeClickableMenu);
    }

    private void DrawBubble(SpriteBatch b)
    {
        if (!this.hasBubble || this.hoveredTranslation == null)
        {
            return;
        }

        // 自己的气泡文本不应被再次捕获
        TextCapture.Suppressed = true;
        try
        {
            BubbleRenderer.Draw(
                b,
                this.bubbleLayout,
                this.hoveredTranslation,
                false,
                this.config,
                this.chineseFont,
                this.bubbleDefault,
                this.bubbleSandy,
                showStar: false);
        }
        finally
        {
            TextCapture.Suppressed = false;
        }
    }

    /// <summary>翻页/滚动/重绘后，鼠标位置上的词是否仍是原来那个词（否则旧气泡必须清掉）。</summary>
    private bool IsHoveredWordStillPresent()
    {
        if (this.hoveredWord == null)
        {
            return false;
        }

        var center = new Point(this.hoveredWordBox.Center.X, this.hoveredWordBox.Center.Y);
        foreach (SpriteWordHit hit in this.wordRects)
        {
            if (hit.Box.Contains(center) && WordOverlay.NormalizeWord(hit.Text) == this.hoveredWord)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>把本帧捕获到的所有行展开成词矩形（每帧一次），并裁剪到菜单可视区域。</summary>
    private void RebuildWordRects(IClickableMenu? menu)
    {
        this.wordRects.Clear();
        if (menu == null)
        {
            return;
        }

        var area = new Rectangle(menu.xPositionOnScreen, menu.yPositionOnScreen, menu.width, menu.height);

        // 同一段文字若同时被 SpriteText 与 SpriteFont 捕获，优先用像素字体的几何（避免矩形叠加偏移）
        var spriteTextLines = new HashSet<string>();
        foreach (CapturedText line in TextCapture.LastFrame)
        {
            if (line.Source == TextSource.SpriteText)
            {
                spriteTextLines.Add(NormalizeLineText(line.Text));
            }
        }

        foreach (CapturedText line in TextCapture.LastFrame)
        {
            if (line.Source == TextSource.SpriteText)
            {
                foreach (SpriteWordHit hit in SpriteTextWordLayout.Build(line.Text, line.Bounds.X, line.LineOriginX, line.Bounds.Y, line.WrapWidth, line.CharacterPosition))
                {
                    if (area.Contains(hit.Box))
                    {
                        this.wordRects.Add(hit);
                    }
                }
            }
            else if (line.Font != null)
            {
                if (spriteTextLines.Contains(NormalizeLineText(line.Text)))
                {
                    continue;
                }

                foreach (SpriteWordHit hit in BuildSpriteFontHits(line))
                {
                    if (area.Contains(hit.Box))
                    {
                        this.wordRects.Add(hit);
                    }
                }
            }
        }
    }

    private static string NormalizeLineText(string text)
    {
        return text.Replace("\r", "").Replace("\n", "^").Trim();
    }

    private bool TryBuildTranslation(string raw, out string translation)
    {
        translation = "";
        string normalized = WordOverlay.NormalizeWord(raw);
        if (normalized.Length == 0 || this.IsIgnoredName(normalized))
        {
            return false;
        }
        if (this.dictionary == null || !this.dictionary.TryGet(normalized, out var entry))
        {
            return false;
        }

        translation = string.IsNullOrEmpty(entry.Pos) ? entry.Translation : $"{entry.Pos} {entry.Translation}";
        return true;
    }

    private static List<SpriteWordHit> BuildSpriteFontHits(CapturedText line)
    {
        var result = new List<SpriteWordHit>();
        if (line.Font == null)
        {
            return result;
        }

        float scale = line.Scale <= 0f ? 1f : line.Scale;
        // 游戏用 ^ 表示换行；SpriteFont 行的文本也可能带 ^，统一按换行处理
        string[] rows = line.Text.Replace("\r", "").Replace("\n", "^").Split('^');
        float lineHeight = line.Font.LineSpacing * scale;

        for (int row = 0; row < rows.Length; row++)
        {
            string text = rows[row];
            float y = line.Bounds.Y + row * lineHeight;
            int index = 0;

            while (index < text.Length)
            {
                while (index < text.Length && text[index] == ' ')
                {
                    index++;
                }

                int wordStart = index;
                while (index < text.Length && text[index] != ' ')
                {
                    index++;
                }
                if (index == wordStart)
                {
                    continue;
                }

                string word = text.Substring(wordStart, index - wordStart);

                // 关键：位置用「行内前缀的 MeasureString」得到，和 DrawString 的推进保持一致。
                // （不能用 词宽 + 空格宽 累加：MeasureString 每个字都含 Spacing/边距，累加会每词少算 2×Spacing。）
                float x = line.Bounds.X + line.Font.MeasureString(text.Substring(0, wordStart)).X * scale;
                Vector2 size = line.Font.MeasureString(word) * scale;

                result.Add(new SpriteWordHit
                {
                    Text = word,
                    Box = new Rectangle((int)x, (int)y, (int)Math.Ceiling(size.X), (int)Math.Ceiling(size.Y))
                });
            }
        }

        return result;
    }

    private void DrawDebugRects(SpriteBatch b)
    {
        var color = new Color(0, 255, 0) * 0.35f;
        foreach (SpriteWordHit hit in this.wordRects)
        {
            if (hit.Box.Width <= 0 || hit.Box.Height <= 0)
            {
                continue;
            }
            b.Draw(Game1.staminaRect, hit.Box, null, color, 0f, Vector2.Zero, SpriteEffects.None, 0.999f);
        }
    }

    private bool IsIgnoredName(string word)
    {
        if (this.nameSet != null && this.nameSet.Contains(word))
        {
            return true;
        }

        Farmer? player = Game1.player;
        if (player != null)
        {
            if (string.Equals(word, player.Name, StringComparison.OrdinalIgnoreCase)
                || string.Equals(word, player.farmName.Value, StringComparison.OrdinalIgnoreCase)
                || string.Equals(word, player.getPetDisplayName(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private void Clear()
    {
        this.hoveredWord = null;
        this.hoveredTranslation = null;
        this.hasBubble = false;
        this.bridgeRect = Rectangle.Empty;
        this.cursorDiagLogged = false;
    }

    private static Rectangle ComputeBridge(Rectangle wordBox, Rectangle bubbleBox)
    {
        int left = Math.Max(wordBox.X, bubbleBox.X);
        int right = Math.Min(wordBox.Right, bubbleBox.Right);
        int width = right - left;
        if (width <= 0)
        {
            return Rectangle.Empty;
        }

        if (bubbleBox.Top >= wordBox.Bottom)
        {
            int h = bubbleBox.Top - wordBox.Bottom;
            return h > 0 ? new Rectangle(left, wordBox.Bottom, width, h) : Rectangle.Empty;
        }

        int h2 = wordBox.Top - bubbleBox.Bottom;
        return h2 > 0 ? new Rectangle(left, bubbleBox.Bottom, width, h2) : Rectangle.Empty;
    }

    private static bool IsEnglish()
    {
        return LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en;
    }
}
