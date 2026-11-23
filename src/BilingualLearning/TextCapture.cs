using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.BellsAndWhistles;

namespace BilingualLearning;

/// <summary>文本矩形的来源绘制入口。</summary>
public enum TextSource
{
    /// <summary><c>SpriteText.drawString</c>（位图字体，对话框/信件/菜单长文本）。</summary>
    SpriteText,

    /// <summary><c>SpriteBatch.DrawString</c>（SpriteFont，tooltip/菜单标签）。</summary>
    SpriteFont
}

/// <summary>一次绘制调用记录下来的文本与屏幕矩形。</summary>
public sealed class CapturedText
{
    public string Text = "";
    public TextSource Source;
    public Rectangle Bounds;

    /// <summary>SpriteText：换行宽；SpriteFont：0。</summary>
    public int WrapWidth;

    /// <summary>SpriteText：<c>^</c> 换行后重置到的原始 x（与 Bounds.X 在被钳制时不同）。</summary>
    public int LineOriginX;

    /// <summary>SpriteText：已绘制字符数（打字机）；SpriteFont：int.MaxValue。</summary>
    public int CharacterPosition = int.MaxValue;

    public SpriteFont? Font;
    public float Scale = 1f;
}

/// <summary>
/// 每帧文本注册表：绘制期间记录「文本 + 屏幕矩形」，下一帧做悬停命中。
/// 只在绘制时写入、查询时读取；<see cref="Suppressed"/> 用于屏蔽自身气泡绘制产生的递归捕获。
/// </summary>
public static class TextCapture
{
    private static readonly List<CapturedText> Current = new();
    private static readonly List<CapturedText> Previous = new();

    /// <summary>上一帧完整捕获结果（本帧命中检测使用）。</summary>
    public static IReadOnlyList<CapturedText> LastFrame => Previous;

    /// <summary>为 true 时暂停捕获（绘制自带气泡、知识卡片重放期间）。</summary>
    public static bool Suppressed { get; set; }

    /// <summary>功能总开关：关闭且未开调试时不产生任何捕获开销（每帧由 ModEntry 设置）。</summary>
    public static bool Enabled { get; set; }

    /// <summary>当前帧捕获数量（调试显示用）。</summary>
    public static int CurrentCount => Current.Count;

    /// <summary>上一帧捕获数量。</summary>
    public static int LastCount => Previous.Count;

    /// <summary>帧开始：把本帧结果转为「上一帧」，清空缓冲。</summary>
    public static void BeginFrame()
    {
        Previous.Clear();
        Previous.AddRange(Current);
        Current.Clear();
    }

    /// <summary>记录一次 <c>SpriteText.drawString</c> 调用（仅处理 drawBGScroll == -1 的普通文本）。</summary>
    public static void RegisterSpriteText(string? text, int x, int y, int characterPosition, int width, int drawBgScroll)
    {
        if (!Enabled || Suppressed || string.IsNullOrEmpty(text) || drawBgScroll != -1)
        {
            return;
        }

        int viewportWidth = Game1.graphics.GraphicsDevice.Viewport.Width;

        // 复刻 drawString 内部对 position.X / width 的修正（SpriteText.cs:507-532）
        int effectiveX = x;
        int effectiveWidth = width;
        if (width == -1)
        {
            effectiveWidth = viewportWidth - x;
        }
        if (effectiveX + effectiveWidth > viewportWidth - 4)
        {
            effectiveX = viewportWidth - effectiveWidth - 4;
        }
        if (effectiveX < 0)
        {
            effectiveX = 0;
        }

        // 复刻 y 偏移（FontPixelZoom < 4 时整体下移）
        float zoom = SpriteText.FontPixelZoom;
        if (zoom < 4f)
        {
            y += (int)((4f - zoom) * 4f);
        }

        // 去掉换行符（drawString 内部 s.Replace(Environment.NewLine, "")）
        string cleaned = text.Replace("\r", "").Replace("\n", "");
        if (cleaned.Length == 0)
        {
            return;
        }

        Add(new CapturedText
        {
            Text = cleaned,
            Source = TextSource.SpriteText,
            Bounds = new Rectangle(effectiveX, y, effectiveWidth, 0),
            WrapWidth = effectiveWidth,
            LineOriginX = x,
            CharacterPosition = characterPosition <= 0 ? cleaned.Length : Math.Min(characterPosition, cleaned.Length)
        });
    }

    /// <summary>记录一次 <c>SpriteBatch.DrawString</c> 调用。</summary>
    public static void RegisterSpriteFont(string? text, SpriteFont? font, Vector2 position, Vector2 origin, float rotation, float scale)
    {
        if (!Enabled || Suppressed || string.IsNullOrEmpty(text) || font == null)
        {
            return;
        }

        // 旋转文本的矩形计算复杂，第一版跳过（游戏中这类调用极少）
        if (Math.Abs(rotation) > 0.001f)
        {
            return;
        }

        if (scale <= 0f)
        {
            scale = 1f;
        }

        Vector2 size = font.MeasureString(text) * scale;
        Vector2 topLeft = position - origin * scale;
        var bounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)Math.Ceiling(size.X), (int)Math.Ceiling(size.Y));

        Add(new CapturedText
        {
            Text = text,
            Source = TextSource.SpriteFont,
            Bounds = bounds,
            CharacterPosition = text.Length,
            Font = font,
            Scale = scale
        });
    }

    private static void Add(CapturedText item)
    {
        // 阴影/重绘去重：同文本同字体、位置相近则视为同一行
        for (int i = 0; i < Current.Count; i++)
        {
            CapturedText existing = Current[i];
            if (existing.Source != item.Source || existing.Text != item.Text)
            {
                continue;
            }
            if (existing.Font != item.Font)
            {
                continue;
            }
            if (Math.Abs(existing.Bounds.X - item.Bounds.X) <= 4 && Math.Abs(existing.Bounds.Y - item.Bounds.Y) <= 4)
            {
                // 阴影先画、正文后画：保留后到的正文位置（去掉 −2/−3 的阴影偏移）
                Current[i] = item;
                return;
            }
        }

        Current.Add(item);
    }
}
