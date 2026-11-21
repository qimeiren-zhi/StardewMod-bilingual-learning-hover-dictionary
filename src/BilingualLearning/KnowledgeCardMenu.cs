using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace BilingualLearning;

/// <summary>
/// 知识卡片菜单（方案 A）：悬停物品时按 F1 打开。
/// 重放原生 IClickableMenu.drawHoverText（固定坐标）→ 类别/增益/价格与原生一致；
/// 左侧显示物品图标；卡片外深色蒙版；全文字悬浮查词；气泡星标可点击收藏。
/// 鼠标进出气泡用「词 ∪ 气泡 ∪ 桥接矩形」连通判定，避免死区。
/// </summary>
public class KnowledgeCardMenu : IClickableMenu
{
    private const int IconOffset = 112;

    private readonly MethodInfo? tooltipMethod;
    private readonly object[]? tooltipArgs;
    private readonly Item? hoveredItem;
    private readonly DictionaryData? dictionary;
    private readonly Vocabulary? vocabulary;
    private readonly ChineseFont? chineseFont;
    private readonly ModConfig config;
    private readonly Texture2D? bubbleDefault;
    private readonly Texture2D? bubbleSandy;

    private readonly int cardX;
    private readonly int cardY;
    private readonly List<(string Word, Rectangle Rect)> wordRects = new();

    private string? hoveredWord;
    private string? hoveredTranslation;
    private Rectangle hoveredWordBox;
    private BubbleLayout bubbleLayout;
    private Rectangle bridgeRect;
    private bool hasBubble;

    public KnowledgeCardMenu(
        MethodInfo? tooltipMethod,
        object[]? tooltipArgs,
        DictionaryData? dictionary,
        Vocabulary? vocabulary,
        ChineseFont? chineseFont,
        ModConfig config,
        Texture2D? bubbleDefault,
        Texture2D? bubbleSandy)
        : base(0, 0, 0, 0)
    {
        this.tooltipMethod = tooltipMethod;
        this.tooltipArgs = tooltipArgs;
        this.hoveredItem = tooltipArgs != null && tooltipArgs.Length > 9 ? tooltipArgs[9] as Item : null;
        this.dictionary = dictionary;
        this.vocabulary = vocabulary;
        this.chineseFont = chineseFont;
        this.config = config;
        this.bubbleDefault = bubbleDefault;
        this.bubbleSandy = bubbleSandy;

        this.cardX = Math.Max(24, Game1.uiViewport.Width / 2 - 250);
        this.cardY = Math.Max(24, Game1.uiViewport.Height / 2 - 220);
        this.xPositionOnScreen = this.cardX;
        this.yPositionOnScreen = this.cardY;
        this.width = 560;
        this.height = 460;
    }

    public override void draw(SpriteBatch b)
    {
        // 卡片外深色蒙版
        b.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.65f
        );

        // 物品图标（卡片左侧）
        if (this.hoveredItem != null)
        {
            this.hoveredItem.drawInMenu(
                b,
                new Vector2(this.cardX + 4, this.cardY + 12),
                0.75f,
                1f,
                0.95f,
                StackDrawType.Hide,
                Color.White,
                false
            );
        }

        // 重放原生 Tooltip（文字右移给图标让位）
        if (this.tooltipMethod != null && this.tooltipArgs != null)
        {
            object[] args = (object[])this.tooltipArgs.Clone();
            args[0] = b;                          // 当前 SpriteBatch
            args[13] = this.cardX + IconOffset;    // overrideX
            args[14] = this.cardY;                 // overrideY

            TooltipTextCapture.Begin();
            this.tooltipMethod.Invoke(null, args);
            TooltipTextCapture.End();

            this.BuildWordRectsFromCapture();
        }

        // 连通判定 + 悬浮查词气泡
        this.UpdateHover();
        if (this.hasBubble && this.hoveredWord != null && this.hoveredTranslation != null)
        {
            bool starred = this.vocabulary?.Contains(this.hoveredWord) ?? false;
            BubbleRenderer.Draw(
                b,
                this.bubbleLayout,
                this.hoveredTranslation,
                starred,
                this.config,
                this.chineseFont,
                this.bubbleDefault,
                this.bubbleSandy
            );
        }

        // 自定义菜单必须自己画光标
        this.drawMouse(b);
    }

    private void BuildWordRectsFromCapture()
    {
        this.wordRects.Clear();
        foreach ((string text, Vector2 pos, SpriteFont font) in TooltipTextCapture.GetDistinct())
        {
            this.AddWords(text, pos, font);
        }
    }

    private void AddWords(string text, Vector2 origin, SpriteFont font)
    {
        float lineHeight = font.LineSpacing;
        string[] lines = text.Replace("\r\n", "\n").Split('\n');
        for (int li = 0; li < lines.Length; li++)
        {
            float y = origin.Y + li * lineHeight;
            float x = origin.X;
            foreach (string raw in lines[li].Split(' '))
            {
                if (raw.Length == 0)
                {
                    x += font.MeasureString(" ").X;
                    continue;
                }
                Vector2 size = font.MeasureString(raw);
                this.wordRects.Add((raw, new Rectangle((int)x, (int)y, (int)size.X, (int)size.Y)));
                x += size.X + font.MeasureString(" ").X;
            }
        }
    }

    private void UpdateHover()
    {
        Point mouse = Game1.getMousePosition();

        // 1. 已有气泡：鼠标仍在「词 ∪ 气泡 ∪ 桥接」内 → 保持（避免进出气泡时消失）
        if (this.hasBubble)
        {
            if (this.hoveredWordBox.Contains(mouse)
                || this.bubbleLayout.BubbleBox.Contains(mouse)
                || this.bridgeRect.Contains(mouse))
            {
                return;
            }
        }

        // 2. 查找新词
        this.hasBubble = false;
        this.hoveredWord = null;
        this.hoveredTranslation = null;
        this.bridgeRect = Rectangle.Empty;
        if (this.dictionary == null)
        {
            return;
        }

        foreach ((string word, Rectangle rect) in this.wordRects)
        {
            if (!rect.Contains(mouse))
            {
                continue;
            }

            string normalized = WordOverlay.NormalizeWord(word);
            if (normalized.Length == 0 || !this.dictionary.TryGet(normalized, out var entry))
            {
                return;
            }

            string translation = string.IsNullOrEmpty(entry.Pos)
                ? entry.Translation
                : $"{entry.Pos} {entry.Translation}";

            this.hoveredWord = normalized;
            this.hoveredTranslation = translation;
            this.hoveredWordBox = rect;
            this.bubbleLayout = BubbleRenderer.ComputeLayout(rect, translation, this.chineseFont);
            this.bridgeRect = ComputeBridge(rect, this.bubbleLayout.BubbleBox);
            this.hasBubble = true;
            return;
        }
    }

    /// <summary>桥接矩形：覆盖单词与气泡之间的 4px 间隙（气泡可能在下方或上方）。</summary>
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

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        // 点击气泡星标 → 收藏/取消
        if (this.hasBubble && this.hoveredWord != null && this.bubbleLayout.StarBox.Contains(x, y))
        {
            this.ToggleStar();
            return;
        }
        base.receiveLeftClick(x, y, playSound);
    }

    private void ToggleStar()
    {
        if (this.vocabulary == null || this.hoveredWord == null)
        {
            return;
        }

        if (this.vocabulary.Contains(this.hoveredWord))
        {
            this.vocabulary.Remove(this.hoveredWord);
            return;
        }

        if (this.dictionary != null && this.dictionary.TryGet(this.hoveredWord, out var entry))
        {
            string season = Game1.currentSeason ?? "";
            if (season.Length > 0)
            {
                season = char.ToUpper(season[0]) + season.Substring(1);
            }

            this.vocabulary.Add(new VocabEntry
            {
                Word = this.hoveredWord,
                Pos = entry.Pos,
                Translation = entry.Translation,
                Sentence = this.tooltipArgs != null && this.tooltipArgs.Length > 1
                    ? this.tooltipArgs[1] as string ?? ""
                    : "",
                FirstSeen = new FirstSeen
                {
                    Year = Game1.year,
                    Season = season,
                    Day = Game1.dayOfMonth
                }
            });
        }
    }

    public override void receiveKeyPress(Keys key)
    {
        if (key == Keys.Escape)
        {
            this.exitThisMenu();
            return;
        }
        base.receiveKeyPress(key);
    }

    /// <summary>气泡可能绘制在卡片矩形之外，必须始终接收点击（否则触发不到星标）。</summary>
    public override bool isWithinBounds(int x, int y)
    {
        return true;
    }
}
