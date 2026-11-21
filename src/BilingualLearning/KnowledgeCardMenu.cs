using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace BilingualLearning;

/// <summary>
/// 知识卡片菜单（方案 A）：悬停物品时按 F1 打开。
/// 原生 Tooltip 风格（menuTexture 九宫格）+ 卡片外深色蒙版；打开菜单即暂停游戏。
/// </summary>
public class KnowledgeCardMenu : IClickableMenu
{
    private readonly string title;
    private readonly string body;
    private Rectangle cardRect;

    public KnowledgeCardMenu(string title, string body)
        : base(0, 0, 0, 0)
    {
        this.title = title ?? "";
        this.body = body ?? "";
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
    }

    public override void draw(SpriteBatch b)
    {
        // 卡片外深色蒙版
        b.Draw(
            Game1.fadeToBlackRect,
            new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height),
            Color.Black * 0.65f
        );

        // 原生 Tooltip 风格卡片（与 IClickableMenu.drawHoverText 用同一纹理）
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

        // 标题（物品名，粗体大字，与原生 tooltip 一致）
        Vector2 titlePos = new Vector2(this.cardRect.X + 16, this.cardRect.Y + 16);
        b.DrawString(Game1.dialogueFont, this.title, titlePos, Game1.textColor);
        Vector2 titleSize = Game1.dialogueFont.MeasureString(this.title);

        // 正文（描述，小字，换行，与原生 tooltip 一致）
        string wrapped = Game1.parseText(this.body, Game1.smallFont, this.cardRect.Width - 32);
        Vector2 bodyPos = new Vector2(this.cardRect.X + 16, titlePos.Y + titleSize.Y + 12);
        b.DrawString(Game1.smallFont, wrapped, bodyPos, Game1.textColor);

        // 关闭按钮
        this.upperRightCloseButton?.draw(b);
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
