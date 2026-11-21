using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BilingualLearning;

/// <summary>
/// 在知识卡片重放 tooltip 期间，捕获所有 DrawString 文本及其绘制位置，
/// 以便对「类别/增益/价格」等全部文字建词矩形做悬浮查词。
/// </summary>
public static class TooltipTextCapture
{
    private static readonly List<(string Text, Vector2 Position, SpriteFont Font)> Items = new();

    public static bool Capturing { get; private set; }

    public static void Begin()
    {
        Items.Clear();
        Capturing = true;
    }

    public static void End()
    {
        Capturing = false;
    }

    public static void Record(SpriteFont font, string text, Vector2 position)
    {
        if (Capturing && !string.IsNullOrEmpty(text))
        {
            Items.Add((text, position, font));
        }
    }

    /// <summary>去重后的文本列表（同文本+字体+位置相近的只保留一个，去掉阴影重复绘制）。</summary>
    public static List<(string Text, Vector2 Position, SpriteFont Font)> GetDistinct()
    {
        var best = new Dictionary<(string, SpriteFont, int, int), (string, Vector2, SpriteFont)>();
        foreach ((string text, Vector2 pos, SpriteFont font) in Items)
        {
            var key = (text, font, (int)(pos.X / 6), (int)(pos.Y / 6));
            if (!best.TryGetValue(key, out var existing) || pos.X + pos.Y < existing.Item2.X + existing.Item2.Y)
            {
                best[key] = (text, pos, font);
            }
        }
        return new List<(string, Vector2, SpriteFont)>(best.Values);
    }
}
