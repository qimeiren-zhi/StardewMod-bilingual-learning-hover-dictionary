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
/// 直接重放原生的 IClickableMenu.drawHoverText（固定坐标），所以类别/增益/价格等与原生 Tooltip 完全一致；
/// 卡片外加深色蒙版；正文/标题上可悬停英文单词查看中文释义。
/// </summary>
public class KnowledgeCardMenu : IClickableMenu
{
    private readonly MethodInfo? tooltipMethod;
    private readonly object[]? tooltipArgs;
    private readonly string title;
    private readonly string body;
    private readonly string categoryName;
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
    private Rectangle hoveredWordBox;
    private string? hoveredTranslation;

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
        this.title = (tooltipArgs != null && tooltipArgs.Length > 6 ? tooltipArgs[6] as string : null) ?? "";
        this.body = (tooltipArgs != null && tooltipArgs.Length > 1 ? tooltipArgs[1] as string : null) ?? "";
        this.categoryName = (tooltipArgs != null && tooltipArgs.Length > 9 && tooltipArgs[9] is Item it) ? it.getCategoryName() : "";
        this.dictionary = dictionary;
        this.vocabulary = vocabulary;
        this.chineseFont = chineseFont;
        this.config = config;
        this.bubbleDefault = bubbleDefault;
        this.bubbleSandy = bubbleSandy;

        // 固定居中偏左上位置（重放的原生 tooltip 以此为左上角）
        this.cardX = Math.Max(24, Game1.uiViewport.Width / 2 - 250);
        this.cardY = Math.Max(24, Game1.uiViewport.Height / 2 - 220);

        this.xPositionOnScreen = this.cardX;
        this.yPositionOnScreen = this.cardY;
        this.width = 500;
        this.height = 440;

        this.ComputeWordRects();
    }

    private void ComputeWordRects()
    {
        this.wordRects.Clear();

        // 标题：drawHoverText 在 (x+16, y+16+4) 绘制
        this.AddWords(this.title, new Vector2(this.cardX + 16, this.cardY + 20), Game1.dialogueFont);

        // 描述：标题高度 +（有类别时再 + 一行）+ 固定偏移
        float titleH = Game1.dialogueFont.MeasureString(this.title).Y;
        float smallH = Game1.smallFont.MeasureString("T").Y;
        float descY = this.cardY + titleH + (string.IsNullOrEmpty(this.categoryName) ? 0 : smallH) + 36;
        this.AddWords(this.body, new Vector2(this.cardX + 16, descY), Game1.smallFont);
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

        // 重放原生 Tooltip（完整内容：标题/类别/描述/增益/价格），固定坐标
        if (this.tooltipMethod != null && this.tooltipArgs != null)
        {
            object[] args = (object[])this.tooltipArgs.Clone();
            args[0] = b;              // 用当前 SpriteBatch
            args[13] = this.cardX;    // overrideX
            args[14] = this.cardY;    // overrideY
            this.tooltipMethod.Invoke(null, args);
        }

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

        // 自定义菜单必须自己画光标
        this.drawMouse(b);
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
