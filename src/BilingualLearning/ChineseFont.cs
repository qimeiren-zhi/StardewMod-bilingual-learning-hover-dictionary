using System.IO;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;

namespace BilingualLearning;

/// <summary>中文字体渲染（S1）：用 FontStashSharp 加载 Font.ttf 并绘制中文词性/释义。</summary>
public class ChineseFont
{
    private readonly SpriteFontBase font;

    private ChineseFont(SpriteFontBase font)
    {
        this.font = font;
    }

    public static ChineseFont? Load(IModHelper helper, IMonitor monitor, float size = 22f)
    {
        string path = Path.Combine(helper.DirectoryPath, "assets", "Font.ttf");

        if (!File.Exists(path))
        {
            monitor.Log($"字体文件不存在：{path}", LogLevel.Warn);
            return null;
        }

        var fontSystem = new FontSystem();
        fontSystem.AddFont(File.ReadAllBytes(path));
        var font = fontSystem.GetFont(size);

        monitor.Log($"中文字体加载完成（FontStashSharp + Font.ttf，字号 {size}）", LogLevel.Info);
        return new ChineseFont(font);
    }

    public Vector2 Measure(string text)
    {
        return this.font.MeasureString(text);
    }

    public void Draw(SpriteBatch b, string text, Vector2 position, Color color, float layerDepth = 0.96f)
    {
        this.font.DrawText(b, text, position, color, layerDepth: layerDepth);
    }
}
