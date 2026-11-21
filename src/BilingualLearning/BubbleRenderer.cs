using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;

namespace BilingualLearning;

/// <summary>气泡渲染器：对话与知识卡片共用的「词性+释义」气泡（背景/星标/中文）。</summary>
public static class BubbleRenderer
{
    private static readonly Rectangle IridiumStarSource = new(346, 392, 8, 8);

    /// <summary>在单词下方（或视口上方）绘制气泡，返回气泡矩形。</summary>
    public static Rectangle Draw(
        SpriteBatch b,
        Rectangle wordBox,
        string translation,
        bool starred,
        ModConfig config,
        ChineseFont? chineseFont,
        Texture2D? bubbleDefault,
        Texture2D? bubbleSandy)
    {
        Vector2 transSize = chineseFont != null
            ? chineseFont.Measure(translation)
            : Game1.smallFont.MeasureString(translation);

        const int padding = 12;
        const int starSize = 24;
        const int gap = 10;

        int width = (int)transSize.X + gap + starSize + padding * 2;
        int height = System.Math.Max((int)transSize.Y, starSize) + padding * 2;

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
        var starBox = new Rectangle(x + padding, y + (height - starSize) / 2, starSize, starSize);
        var transPos = new Vector2(x + padding + starSize + gap, y + (height - transSize.Y) / 2);

        // 背景（按风格）
        bool sandy = config.BubbleStyle == "Sandy's Notes";
        Texture2D? customTex = sandy ? bubbleSandy ?? bubbleDefault : bubbleDefault;
        bool customBg = customTex != null;
        Texture2D bgTex = customTex ?? Game1.mouseCursors;
        Rectangle bgSrc = customBg ? new Rectangle(0, 0, bgTex.Width, bgTex.Height) : new Rectangle(306, 320, 16, 16);
        float bgScale = customBg ? 1f : 2f;
        IClickableMenu.drawTextureBox(b, bgTex, bgSrc, x, y, width, height, Color.White, bgScale, drawShadow: false, draw_layer: 0.95f);

        // 星标
        Color starColor = starred ? Color.White : new Color(90, 90, 90) * 0.6f;
        b.Draw(Game1.mouseCursors, starBox, IridiumStarSource, starColor, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);

        // 文字
        Color transColor = customBg ? new Color(86, 22, 12) : new Color(240, 240, 240);
        if (chineseFont != null)
        {
            chineseFont.Draw(b, translation, transPos, transColor, 0.96f);
        }
        else
        {
            b.DrawString(Game1.smallFont, translation, transPos, transColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.96f);
        }

        return bubbleBox;
    }
}
