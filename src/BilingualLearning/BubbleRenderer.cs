using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace BilingualLearning;

/// <summary>气泡布局（背景/星标矩形与文字位置），供命中检测与绘制使用。</summary>
public readonly struct BubbleLayout
{
    public readonly Rectangle BubbleBox;
    public readonly Rectangle StarBox;
    public readonly Vector2 TransPos;

    public BubbleLayout(Rectangle bubbleBox, Rectangle starBox, Vector2 transPos)
    {
        this.BubbleBox = bubbleBox;
        this.StarBox = starBox;
        this.TransPos = transPos;
    }
}

/// <summary>气泡渲染器：对话与知识卡片共用的「词性+释义」气泡（背景/星标/中文）。</summary>
public static class BubbleRenderer
{
    private static readonly Rectangle IridiumStarSource = new(346, 392, 8, 8);
    private const int Padding = 12;
    private const int StarSize = 24;
    private const int Gap = 10;

    /// <summary>计算气泡布局（不绘制）。</summary>
    public static BubbleLayout ComputeLayout(Rectangle wordBox, string translation, ChineseFont? chineseFont, bool showStar = true)
    {
        Vector2 transSize = chineseFont != null
            ? chineseFont.Measure(translation)
            : Game1.smallFont.MeasureString(translation);

        int starBlock = showStar ? Gap + StarSize : 0;
        int width = (int)transSize.X + starBlock + Padding * 2;
        int height = (int)Math.Max(transSize.Y, StarSize) + Padding * 2;

        int x = wordBox.X;
        int y = wordBox.Bottom + 4;
        if (x + width > Game1.uiViewport.Width)
        {
            x = Game1.uiViewport.Width - width;
        }
        if (y + height > Game1.uiViewport.Height)
        {
            y = wordBox.Top - height - 4;
        }

        var bubbleBox = new Rectangle(x, y, width, height);
        var starBox = new Rectangle(x + Padding, y + (height - StarSize) / 2, StarSize, StarSize);
        var transPos = new Vector2(x + Padding + (showStar ? StarSize + Gap : 0), y + (height - transSize.Y) / 2);
        return new BubbleLayout(bubbleBox, starBox, transPos);
    }

    /// <summary>绘制气泡。</summary>
    public static void Draw(
        SpriteBatch b,
        BubbleLayout layout,
        string translation,
        bool starred,
        ModConfig config,
        ChineseFont? chineseFont,
        Texture2D? bubbleDefault,
        Texture2D? bubbleSandy,
        bool showStar = true)
    {
        // 背景（按风格）
        bool sandy = config.BubbleStyle == "Sandy's Notes";
        Texture2D? customTex = sandy ? bubbleSandy ?? bubbleDefault : bubbleDefault;
        bool customBg = customTex != null;
        Texture2D bgTex = customTex ?? Game1.mouseCursors;
        Rectangle bgSrc = customBg ? new Rectangle(0, 0, bgTex.Width, bgTex.Height) : new Rectangle(306, 320, 16, 16);
        float bgScale = customBg ? 1f : 2f;
        IClickableMenu.drawTextureBox(b, bgTex, bgSrc, layout.BubbleBox.X, layout.BubbleBox.Y, layout.BubbleBox.Width, layout.BubbleBox.Height, Color.White, bgScale, drawShadow: false, draw_layer: 0.95f);

        // 星标（阶段0 的扩展文本面不绘制星标：只读悬停，不支持收藏）
        if (showStar)
        {
            Color starColor = starred ? Color.White : new Color(90, 90, 90) * 0.6f;
            b.Draw(Game1.mouseCursors, layout.StarBox, IridiumStarSource, starColor, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);
        }


        // 文字
        Color transColor = customBg ? new Color(86, 22, 12) : new Color(240, 240, 240);
        if (chineseFont != null)
        {
            chineseFont.Draw(b, translation, layout.TransPos, transColor, 0.96f);
        }
        else
        {
            b.DrawString(Game1.smallFont, translation, layout.TransPos, transColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.96f);
        }
    }
}
