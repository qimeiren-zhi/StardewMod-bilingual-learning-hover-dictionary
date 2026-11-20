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
/// 命中判定与视觉绘制分离：桥接矩形（BridgeRect）仅参与命中判定，不绘制。
/// </summary>
public class WordOverlay
{
    private IMonitor monitor = null!;
    private DictionaryData? dictionary;
    private ChineseFont? chineseFont;
    private Vocabulary? vocabulary;
    private HashSet<string>? nameSet;
    private ModConfig config = new();

    /// <summary>学习模式总开关（F11）。</summary>
    public bool Enabled = true;

    // —— 覆盖层状态 ——
    private string? hoveredWord;
    private string? hoveredTranslation;
    private Rectangle wordBox;
    private Rectangle bubbleBox;
    private Rectangle starBox;
    private Rectangle bridgeRect; // 命中判定用，不绘制
    private Vector2 transPos;

    /// <summary>铱星物品品质星的源矩形（Game1.mouseCursors）。</summary>
    private static readonly Rectangle IridiumStarSource = new(346, 392, 8, 8);

    public void Init(IMonitor monitor, DictionaryData dictionary, ChineseFont? chineseFont, Vocabulary vocabulary, HashSet<string> nameSet, ModConfig config)
    {
        this.monitor = monitor;
        this.dictionary = dictionary;
        this.chineseFont = chineseFont;
        this.vocabulary = vocabulary;
        this.nameSet = nameSet;
        this.config = config;
    }

    /// <summary>S2：hover 命中检测（Harmony postfix on performHoverAction）。</summary>
    public void OnHover(DialogueBox box, int mouseX, int mouseY)
    {
        if (!this.Enabled || !IsEnglish())
        {
            this.hoveredWord = null;
            return;
        }

        // 生命周期/门槛 1：转场中 → 清空，防止旧气泡穿透到下一个对话
        if (box.transitioning)
        {
            this.hoveredWord = null;
            return;
        }

        // 生命周期/门槛 2：打字机未完成（新对话/新页开头 characterIndexInDialogue 归零）→ 清空，防残留
        string text = box.getCurrentString();
        if (string.IsNullOrEmpty(text) || box.characterIndexInDialogue < text.Length - 1)
        {
            this.hoveredWord = null;
            return;
        }

        // 连通判定：当前单词 ∪ 气泡 ∪ 桥接矩形 → 保持显示（优先于命中，解决气泡覆盖下一行时的跳词）
        if (this.hoveredWord != null &&
            (this.wordBox.Contains(mouseX, mouseY) ||
             this.bubbleBox.Contains(mouseX, mouseY) ||
             this.bridgeRect.Contains(mouseX, mouseY)))
        {
            return;
        }

        // 命中新单词
        if (TryHitWord(box, text, mouseX, mouseY, out string word, out Rectangle rect))
        {
            if (word != this.hoveredWord)
            {
                this.monitor.Log($"S2 命中单词：'{word}' @ {rect}", LogLevel.Info);

                // S4 查词：命中新词时查词典并记录结果
                if (this.dictionary != null && this.dictionary.TryGet(word, out var entry))
                {
                    this.monitor.Log($"S4 查词：'{word}' → [{entry.Tag}] {entry.Pos} {entry.Translation}", LogLevel.Info);
                }
                else
                {
                    this.monitor.Log($"S4 查词：'{word}' → 未收录", LogLevel.Info);
                }
            }
            this.hoveredWord = word;
            this.wordBox = rect;
            this.ComputeBubbleLayout();
            return;
        }

        // 离开连通区域且未命中新词 → 立即隐藏（零延迟）
        this.hoveredWord = null;
    }

    /// <summary>S3：点击分层。返回 false 表示吞掉本次点击（不翻页）。</summary>
    public bool OnClick(DialogueBox box, int x, int y)
    {
        if (!this.Enabled || !IsEnglish() || this.hoveredWord == null)
        {
            return true;
        }

        if (this.starBox.Contains(x, y))
        {
            this.ToggleStar(box, this.hoveredWord);
            return false; // 吞掉点击
        }

        return true; // 放行给游戏默认逻辑（翻页 / 跳过打字机）
    }

    /// <summary>S3/S1：绘制气泡 + 星标 + 中文释义（单行）。图层：背景 0.95 ＜ 星标/文本 0.96。</summary>
    public void Draw(SpriteBatch b)
    {
        if (!this.Enabled || !IsEnglish() || this.hoveredWord == null)
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
            draw_layer: 0.95f
        );

        // 星标（左，0.96，位于背景之上）：点亮=白；未点亮=深灰半透明
        bool lit = this.vocabulary?.Contains(this.hoveredWord) ?? false;
        Color starColor = lit ? Color.White : new Color(90, 90, 90) * 0.6f;
        b.Draw(Game1.mouseCursors, this.starBox, IridiumStarSource, starColor, 0f, Vector2.Zero, SpriteEffects.None, 0.96f);

        // 中文释义（词性 + 释义，单行，0.96）
        if (this.hoveredTranslation != null)
        {
            var transColor = new Color(240, 240, 240);
            if (this.chineseFont != null)
            {
                this.chineseFont.Draw(b, this.hoveredTranslation, this.transPos, transColor, 0.96f);
            }
            else
            {
                b.DrawString(Game1.smallFont, this.hoveredTranslation, this.transPos, transColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0.96f);
            }
        }
    }

    private void ToggleStar(DialogueBox box, string word)
    {
        if (this.vocabulary != null && this.vocabulary.Contains(word))
        {
            this.vocabulary.Remove(word);
            this.monitor.Log($"☆ 已取消收藏：'{word}'", LogLevel.Info);
            return;
        }

        string pos = "";
        string translation = "";
        if (this.dictionary != null && this.dictionary.TryGet(word, out var entry))
        {
            pos = entry.Pos;
            translation = entry.Translation;
        }
        this.vocabulary?.Add(new VocabEntry
        {
            Word = word,
            Pos = pos,
            Translation = translation,
            Sentence = box.getCurrentString(),
            FirstSeen = new FirstSeen
            {
                Year = Game1.year,
                Season = Capitalize(Game1.currentSeason),
                Day = Game1.dayOfMonth
            }
        });
        this.monitor.Log($"★ 已收藏：'{word}' → {pos} {translation}", LogLevel.Info);
    }

    /// <summary>是否英文语言（F14：仅英文启用）。</summary>
    private static bool IsEnglish()
    {
        return LocalizedContentManager.CurrentLanguageCode == LocalizedContentManager.LanguageCode.en;
    }

    /// <summary>6.2 等级过滤：命中任一所选等级即显示；无标签视为低于阈值；专有名词（prop）恒保留。</summary>
    private bool IsLevelMatch(string tag)
    {
        if (this.config.EnabledLevels == null || this.config.EnabledLevels.Count == 0)
        {
            return true; // 未配置等级 → 不过滤
        }
        if (string.IsNullOrEmpty(tag))
        {
            return false; // 无标签 → 低于阈值
        }
        foreach (string t in tag.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            if (t == "prop")
            {
                return true; // 专有名词：保留，按类别显示（NPC 名已由名称过滤先行拦截）
            }
            if (this.config.EnabledLevels.Contains(t))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>是否为应忽略的人物/专有名称。</summary>
    private bool IsIgnoredName(string word)
    {
        if (this.nameSet != null && this.nameSet.Contains(word))
        {
            return true;
        }

        var player = Game1.player;
        if (player != null)
        {
            if (string.Equals(word, player.Name, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (string.Equals(word, player.farmName.Value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            if (string.Equals(word, player.getPetDisplayName(), StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>首字母大写（spring → Spring）。</summary>
    private static string Capitalize(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return s;
        }
        return char.ToUpper(s[0]) + s.Substring(1);
    }

    private void ComputeBubbleLayout()
    {
        string word = this.hoveredWord!;

        // 查词（S4）
        if (this.dictionary != null && this.dictionary.TryGet(word, out var entry))
        {
            this.hoveredTranslation = string.IsNullOrEmpty(entry.Pos)
                ? entry.Translation
                : $"{entry.Pos} {entry.Translation}";
        }
        else
        {
            this.hoveredTranslation = "未收录";
        }

        var transSize = (this.chineseFont != null)
            ? this.chineseFont.Measure(this.hoveredTranslation)
            : Game1.smallFont.MeasureString(this.hoveredTranslation);

        int padding = 8;
        int starSize = 24;
        int gap = 10;

        int width = (int)transSize.X + gap + starSize + padding * 2;
        int height = Math.Max((int)transSize.Y, starSize) + padding * 2;

        int x = this.wordBox.X;
        int y = this.wordBox.Bottom + 4; // 保留 4px 视觉空隙

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

        // 星标在左，中文释义在右（单行）
        this.starBox = new Rectangle(
            x + padding,
            y + (height - starSize) / 2,
            starSize,
            starSize
        );
        this.transPos = new Vector2(
            x + padding + starSize + gap,
            y + (height - transSize.Y) / 2
        );

        // 桥接矩形：覆盖单词与气泡之间的 4px 间隙（气泡可能在单词下方，也可能因视口上移而在上方）
        int bridgeLeft = Math.Max(this.wordBox.X, this.bubbleBox.X);
        int bridgeRight = Math.Min(this.wordBox.Right, this.bubbleBox.Right);
        int bridgeWidth = bridgeRight - bridgeLeft;
        if (bridgeWidth <= 0)
        {
            this.bridgeRect = Rectangle.Empty;
        }
        else if (this.bubbleBox.Top >= this.wordBox.Bottom)
        {
            // 气泡在单词下方
            this.bridgeRect = new Rectangle(bridgeLeft, this.wordBox.Bottom, bridgeWidth, this.bubbleBox.Top - this.wordBox.Bottom);
        }
        else
        {
            // 气泡在单词上方（被视口上移）
            this.bridgeRect = new Rectangle(bridgeLeft, this.bubbleBox.Bottom, bridgeWidth, this.wordBox.Top - this.bubbleBox.Bottom);
        }
    }

    /// <summary>核心命中检测：复刻 SpriteText.drawString 的英文布局，逐字符计算单词包围盒。</summary>
    private bool TryHitWord(DialogueBox box, string text, int mouseX, int mouseY, out string word, out Rectangle rect)
    {
        word = "";
        rect = Rectangle.Empty;

        int wrapWidth;
        int originX = box.x + 8;
        int originY;

        if (box.isQuestion)
        {
            // 问题对话框：提示文本在 (x+8, y+12-(heightForQuestions-height))，换行宽 width-16
            wrapWidth = box.width - 16;
            originY = box.y + 12 - (box.heightForQuestions - box.height);
        }
        else
        {
            wrapWidth = box.isPortraitBox() ? box.width - 460 - 24 : box.width;
            originY = box.y + 8;
        }

        foreach (var hit in BuildWordHits(text, originX, originY, wrapWidth))
        {
            if (hit.Box.Contains(mouseX, mouseY))
            {
                string normalized = NormalizeWord(hit.Text);
                if (normalized.Length == 0)
                {
                    return false; // 全标点，非单词
                }
                if (this.IsIgnoredName(normalized))
                {
                    return false; // 人物/专有名称，不显示释义
                }
                if (this.dictionary == null || !this.dictionary.TryGet(normalized, out var entry))
                {
                    return false; // F13: 未收录词不显示气泡
                }
                if (!this.IsLevelMatch(entry.Tag))
                {
                    return false; // 6.2: 等级过滤（低于所选难度不显示）
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
    /// 逐字符复刻 SpriteText.drawString（拉丁分支）：推进 = 8*zoom + offset(当前) + offset(下一个)；
    /// 换行判定用 positionOfNextSpace。
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
