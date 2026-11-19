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
/// P2 技术 Spike：S2 对话单词命中 + S3 九宫格气泡与铱星星标。
///
/// 说明：本文件是可运行原型。释义/词性为占位（词典接入见 S4，中文 TTF 渲染见 S1），
/// 隐藏延迟与 GMCM 配置等留待 P3/P5。
/// </summary>
public class WordOverlay
{
    private IMonitor monitor = null!;

    // —— 覆盖层状态 ——
    private string? hoveredWord;
    private Rectangle wordBox;
    private Rectangle bubbleBox;
    private Rectangle starBox;
    private readonly HashSet<string> starred = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>铱星物品品质星的源矩形（Game1.mouseCursors）。</summary>
    private static readonly Rectangle IridiumStarSource = new(346, 392, 8, 8);

    public void Init(IMonitor monitor)
    {
        this.monitor = monitor;
    }

    /// <summary>S2：hover 命中检测（Harmony postfix on performHoverAction）。</summary>
    public void OnHover(DialogueBox box, int mouseX, int mouseY)
    {
        if (TryHitWord(box, mouseX, mouseY, out string word, out Rectangle rect))
        {
            if (word != this.hoveredWord)
            {
                this.hoveredWord = word;
                this.monitor.Log($"S2 命中单词：'{word}' @ {rect}", LogLevel.Info);
            }
            this.wordBox = rect;
            this.ComputeBubbleLayout();
            return;
        }

        // 鼠标不在单词上：若仍在气泡区域内，则保持显示（以便点击星标）
        if (this.hoveredWord != null && this.bubbleBox.Contains(mouseX, mouseY))
        {
            return;
        }

        this.hoveredWord = null; // 离开单词与气泡 → 隐藏（延迟隐藏留待 P3）
    }

    /// <summary>S3：点击分层。返回 false 表示吞掉本次点击（不翻页）。</summary>
    public bool OnClick(DialogueBox box, int x, int y)
    {
        if (this.hoveredWord == null)
        {
            return true;
        }

        if (this.starBox.Contains(x, y))
        {
            this.ToggleStar(this.hoveredWord);
            return false; // 吞掉点击
        }

        return true; // 放行给游戏默认逻辑（翻页 / 跳过打字机）
    }

    /// <summary>S3：绘制气泡 + 星标（覆盖层，画在 DialogueBox 之上）。</summary>
    public void Draw(SpriteBatch b, DialogueBox box)
    {
        if (this.hoveredWord == null)
        {
            return;
        }

        // 气泡背景：复用游戏对话框九宫格纹理（mouseCursors 306,320,16,16）
        IClickableMenu.drawTextureBox(
            b,
            Game1.mouseCursors,
            new Rectangle(306, 320, 16, 16),
            this.bubbleBox.X,
            this.bubbleBox.Y,
            this.bubbleBox.Width,
            this.bubbleBox.Height,
            Color.White,
            2f,
            drawShadow: false,
            draw_layer: 0.99f
        );

        // 单词文本（英文；中文释义需 S1 字体，此处占位）
        var textPos = new Vector2(this.bubbleBox.X + 10, this.bubbleBox.Y + 8);
        b.DrawString(Game1.smallFont, this.hoveredWord, textPos, Color.White);

        // 星标：点亮 = 白；未点亮 = 深灰半透明（3.6 描边留待 P3 细化）
        bool lit = this.starred.Contains(this.hoveredWord);
        Color starColor = lit ? Color.White : new Color(90, 90, 90) * 0.6f;
        b.Draw(Game1.mouseCursors, this.starBox, IridiumStarSource, starColor);
    }

    private void ToggleStar(string word)
    {
        if (this.starred.Add(word))
        {
            this.monitor.Log($"★ 已收藏：'{word}'", LogLevel.Info);
        }
        else
        {
            this.starred.Remove(word);
            this.monitor.Log($"☆ 已取消收藏：'{word}'", LogLevel.Info);
        }
    }

    private void ComputeBubbleLayout()
    {
        string word = this.hoveredWord!;
        var textSize = Game1.smallFont.MeasureString(word);

        int padding = 8;
        int starSize = 24;
        int gap = 10;

        int width = (int)textSize.X + gap + starSize + padding * 2;
        int height = Math.Max((int)textSize.Y, starSize) + padding * 2;

        int x = this.wordBox.X;
        int y = this.wordBox.Bottom + 4;

        // 防止超出 UI 视口
        if (x + width > Game1.uiViewport.Width)
        {
            x = Game1.uiViewport.Width - width;
        }
        if (y + height > Game1.uiViewport.Height)
        {
            y = this.wordBox.Top - height - 4;
        }

        this.bubbleBox = new Rectangle(x, y, width, height);
        this.starBox = new Rectangle(
            x + width - starSize - padding,
            y + (height - starSize) / 2,
            starSize,
            starSize
        );
    }

    /// <summary>核心命中检测：复刻 SpriteText.drawString 的英文布局，逐字符计算单词包围盒。</summary>
    private static bool TryHitWord(DialogueBox box, int mouseX, int mouseY, out string word, out Rectangle rect)
    {
        word = "";
        rect = Rectangle.Empty;

        if (box.transitioning || box.isQuestion)
        {
            return false;
        }

        string text = box.getCurrentString();
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        // 打字机门槛：文字完全显示后才查词（与游戏 performHoverAction 的门槛一致）
        if (box.characterIndexInDialogue < text.Length - 1)
        {
            return false;
        }

        int wrapWidth = box.isPortraitBox() ? box.width - 460 - 24 : box.width;
        int originX = box.x + 8;
        int originY = box.y + 8;

        foreach (var hit in BuildWordHits(text, originX, originY, wrapWidth))
        {
            if (hit.Box.Contains(mouseX, mouseY))
            {
                string normalized = NormalizeWord(hit.Text);
                if (normalized.Length == 0)
                {
                    return false; // 全标点，非单词
                }
                word = normalized;
                rect = hit.Box;
                return true;
            }
        }

        return false;
    }

    /// <summary>去除单词两侧标点（3.2），保留内部撇号/连字符（如 don't、well-known）。</summary>
    private static string NormalizeWord(string raw)
    {
        int start = 0;
        int end = raw.Length;
        while (start < end && !char.IsLetterOrDigit(raw[start]))
        {
            start++;
        }
        while (end > start && !char.IsLetterOrDigit(raw[end - 1]))
        {
            end--;
        }
        return raw.Substring(start, end - start);
    }

    private sealed class WordHit
    {
        public string Text = "";
        public Rectangle Box;
    }

    /// <summary>
    /// 逐字符复刻 SpriteText.drawString（拉丁分支，见反编译源码 648-695 行）：
    /// 推进 = 8*zoom + offset(当前) + offset(下一个)；换行判定用 positionOfNextSpace。
    /// </summary>
    private static List<WordHit> BuildWordHits(string s, int originX, int originY, int wrapWidth)
    {
        var result = new List<WordHit>();
        float zoom = SpriteText.FontPixelZoom;
        float lineHeight = 18f * zoom;
        float charHeight = 16f * zoom;

        float posX = originX;
        float posY = originY;
        WordHit? current = null;

        void EndWord()
        {
            if (current != null)
            {
                result.Add(current);
                current = null;
            }
        }

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];

            if (c == '^')
            {
                EndWord();
                posY += lineHeight;
                posX = originX;
                continue;
            }

            // 换行判定（与 drawString 673 行一致）
            if (SpriteText.positionOfNextSpace(s, i, (int)(posX - originX), 0) >= wrapWidth)
            {
                EndWord();
                posY += lineHeight;
                posX = originX;
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
                    current = new WordHit
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
