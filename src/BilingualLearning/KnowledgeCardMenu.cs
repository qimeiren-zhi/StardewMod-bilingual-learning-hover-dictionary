using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace BilingualLearning;

/// <summary>
/// 知识卡片菜单（方案 A）：悬停物品时按 F1 打开。
/// 原生 Tooltip 风格（menuTexture 九宫格）+ 卡片外深色蒙版；打开菜单即暂停游戏。
/// 卡片正文/标题上可悬停英文单词查看中文释义（复用气泡渲染器）。
/// </summary>
public class KnowledgeCardMenu : IClickableMenu
{
    private readonly string title;
    private readonly string body;
    private readonly DictionaryData? dictionary;
    private readonly Vocabulary? vocabulary;
    private readonly ChineseFont? chineseFont;
    private readonly ModConfig config;
    private readonly Texture2D? bubbleDefault;
    private readonly Texture2D? bubbleSandy;

    private Rectangle cardRect;
    private Vector2 titlePos;
    private Vector2 bodyPos;
    private readonly List<(string Word, Rectangle Rect)> wordRects = new();
    private string? hoveredWord;
    private Rectangle hoveredWordBox;
    private string? hoveredTranslation;

    public KnowledgeCardMenu(
        string title,
        string body,
        DictionaryData? dictionary,
        Vocabulary? vocabulary,
        ChineseFont? chineseFont,
        ModConfig config,
        Texture2D? bubbleDefault,
        Texture2D? bubbleSandy)
        : base(0, 0, 0, 0)
    {
        this.title = title ?? "";
        this.body = body ?? "";
        this.dictionary = dictionary;
        this.vocabulary = vocabulary;
        this.chineseFont = chineseFont;
        this.config = config;
        this.bubbleDefault = bubbleDefault;
        this.bubbleSandy = bubbleSandy;
        this.ComputeLayout();
        this.initializeUpperRightCloseButton();
    }

    private void ComputeLayout()
    {
        int width = Math.Min(720, Math.Max(320, Game1.uiViewport.Width - 128));
        string wrapped = Game1.parseText(this.body, Game1.smallFont, width - 32);
        Vector2 titleSize = Game1.dialogueFont.MeasureString(this.title);
        Vector2 bodySize = Game1.smallFont.MeasureString(wrapped);

        int height = (int)(titleSize.Y + 12 + bodySize.Y) + 48;
        int x = (Game1.uiViewport.Width - width) / 2;
        int y = (Game1.uiViewport.Height - height) / 2;

        this.xPositionOnScreen = x;
        this.yPositionOnScreen = y;
        this.width = width;
        this.height = height;
        this.cardRect = new Rectangle(x, y, width, height);

        this.titlePos = new Vector2(x + 16, y + 16);
        this.bodyPos = new Vector2(x + 16, this.titlePos.Y + titleSize.Y + 12);

        this.BuildWordRects(wrapped);
    }

    private void BuildWordRects(string wrappedBody)
    {
        this.wordRects.Clear();
        this.AddWords(this.title, this.titlePos, Game1.dialogueFont);
        this.AddWords(wrappedBody, this.bodyPos, Game1.smallFont);
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

    public override void draw(SpriteBatch b)
    {
        // 卡片外深色蒙版
        b.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.65f
        );

        // 原生 Tooltip 风格卡片
        IClickableMenu.drawTextureBox(
            b,
            Game1.menuTexture,
            new Rectangle(0, 256, 60, 60),
            this.cardRect.X,
            this.cardRect.Y,
            this.cardRect.Width,
            this.cardRect.Height,
            Color.White,
            1f,
            drawShadow: false
        );

        // 标题
        b.DrawString(Game1.dialogueFont, this.title, this.titlePos, Game1.textColor);

        // 正文（换行）
        string wrapped = Game1.parseText(this.body, Game1.smallFont, this.cardRect.Width - 32);
        b.DrawString(Game1.smallFont, wrapped, this.bodyPos, Game1.textColor);

        // 悬浮查词气泡
        this.UpdateHover();
        if (this.hoveredWord != null && this.hoveredTranslation != null)
        {
            bool starred = this.vocabulary?.Contains(this.hoveredWord) ?? false;
            BubbleRenderer.Draw(
                b,
                this.hoveredWordBox,
                this.hoveredTranslation,
                starred,
                this.config,
                this.chineseFont,
                this.bubbleDefault,
                this.bubbleSandy
            );
        }

        this.upperRightCloseButton?.draw(b);
    }

    private void UpdateHover()
    {
        this.hoveredWord = null;
        this.hoveredTranslation = null;
        if (this.dictionary == null)
        {
            return;
        }

        Point mouse = Game1.getMousePosition();
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

            this.hoveredWord = normalized;
            this.hoveredWordBox = rect;
            this.hoveredTranslation = string.IsNullOrEmpty(entry.Pos)
                ? entry.Translation
                : $"{entry.Pos} {entry.Translation}";
            return;
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
}
