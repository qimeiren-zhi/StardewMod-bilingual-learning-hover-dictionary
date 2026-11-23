using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;

namespace BilingualLearning;

/// <summary>一次 <see cref="SpriteText.drawString"/> 排版后的单词包围盒。</summary>
public sealed class SpriteWordHit
{
    public string Text = "";
    public Rectangle Box;
}

/// <summary>
/// 复刻 <see cref="SpriteText.drawString"/> 的拉丁分支布局，产出逐词矩形。
/// 逻辑与 <c>WordOverlay.BuildWordHits</c> 等价，额外支持 <c>characterPosition</c>（打字机截断）。
/// </summary>
public static class SpriteTextWordLayout
{
    /// <param name="s">完整文本（调用方已按需处理 ^ 换行）。</param>
    /// <param name="originX">换行后回到的左边（= drawString 的 x 参数，已含内部修正）。</param>
    /// <param name="originY">首行顶部（= drawString 的 y 参数，已含内部修正）。</param>
    /// <param name="wrapWidth">换行宽度（= drawString 的 width 参数，已含内部修正）。</param>
    /// <param name="characterPosition">已绘制的字符数（打字机；默认全部）。</param>
    public static List<SpriteWordHit> Build(string s, int originX, int originY, int wrapWidth, int characterPosition = int.MaxValue)
    {
        return Build(s, originX, originX, originY, wrapWidth, characterPosition);
    }

    /// <summary>
    /// 带「行首原点」的版本：<paramref name="startX"/> 是首行的实际起点（可能被 drawString 内部钳制），
    /// <paramref name="lineOriginX"/> 是 <c>^</c> 换行/自动换行后重置到的原始 x。
    /// </summary>
    public static List<SpriteWordHit> Build(string s, int startX, int lineOriginX, int originY, int wrapWidth, int characterPosition = int.MaxValue)
    {
        var result = new List<SpriteWordHit>();
        if (string.IsNullOrEmpty(s))
        {
            return result;
        }

        float zoom = SpriteText.FontPixelZoom;
        float lineHeight = 18f * zoom;
        float charHeight = 16f * zoom;

        float posX = startX;
        float posY = originY;
        SpriteWordHit? current = null;
        int limit = System.Math.Min(s.Length, characterPosition);

        void EndWord()
        {
            if (current != null)
            {
                result.Add(current);
                current = null;
            }
        }

        for (int i = 0; i < limit; i++)
        {
            char c = s[i];

            if (c == '^')
            {
                EndWord();
                posY += lineHeight;
                posX = lineOriginX;
                continue;
            }

            // 换行判定（与 drawString 一致：相对「原始 x」的偏移）
            if (SpriteText.positionOfNextSpace(s, i, (int)(posX - lineOriginX), 0) >= wrapWidth)
            {
                EndWord();
                posY += lineHeight;
                posX = lineOriginX;
                if (c == ' ')
                {
                    continue; // 行首空格跳过
                }
            }

            if (c == ' ')
            {
                EndWord();
            }
            else
            {
                if (current == null)
                {
                    current = new SpriteWordHit
                    {
                        Text = "",
                        Box = new Rectangle((int)posX, (int)posY, 0, (int)charHeight)
                    };
                }
                current.Text += c;

                int glyphRight = (int)(posX + 8f * zoom + SpriteText.getWidthOffsetForChar(c) * zoom);
                current.Box = new Rectangle(current.Box.X, current.Box.Y, glyphRight - current.Box.X, current.Box.Height);
            }

            // 推进（与 drawString 686-693 行一致）
            if (i < s.Length - 1)
            {
                posX += 8f * zoom + SpriteText.getWidthOffsetForChar(s[i + 1]) * zoom;
            }
            posX += SpriteText.getWidthOffsetForChar(c) * zoom;
        }

        EndWord();
        return result;
    }
}
