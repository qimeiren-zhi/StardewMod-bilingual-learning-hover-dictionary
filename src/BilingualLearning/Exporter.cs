using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using StardewModdingAPI;
using StardewValley;

namespace BilingualLearning;

/// <summary>生词导出（F9）：CSV / Markdown / Anki（文本导入格式）。</summary>
public static class Exporter
{
    private static readonly Dictionary<string, string> SeasonNames = new()
    {
        ["Spring"] = "春",
        ["Summer"] = "夏",
        ["Fall"] = "秋",
        ["Winter"] = "冬",
    };

    public static void Export(IReadOnlyList<VocabEntry> words, string location, string modDir, IMonitor monitor)
    {
        string exportDir = ResolveExportDir(location, modDir);
        Directory.CreateDirectory(exportDir);

        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        ExportCsv(words, Path.Combine(exportDir, $"vocabulary-{stamp}.csv"));
        ExportMarkdown(words, Path.Combine(exportDir, $"vocabulary-{stamp}.md"));
        ExportAnki(words, Path.Combine(exportDir, $"vocabulary-{stamp}.anki.txt"));

        monitor.Log($"已导出生词（{words.Count} 词）到 {exportDir}", LogLevel.Info);
        Game1.addHUDMessage(new HUDMessage($"Exported {words.Count} words to {exportDir}"));
    }

    private static string ResolveExportDir(string location, string modDir)
    {
        switch (location)
        {
            case "Desktop":
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "BilingualLearning");
            case "Downloads":
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", "BilingualLearning");
            case "Mod folder":
                return Path.Combine(modDir, "exports");
            default:
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "StardewValley", "BilingualLearning");
        }
    }

    private static string FormatFirstSeen(VocabEntry e)
    {
        string season = SeasonNames.TryGetValue(e.FirstSeen.Season, out string? s) ? s : e.FirstSeen.Season;
        return $"第{e.FirstSeen.Year}年{season}第{e.FirstSeen.Day}日";
    }

    private static void ExportCsv(IReadOnlyList<VocabEntry> words, string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine("word,pos,translation,sentence,firstSeen");
        foreach (VocabEntry w in words)
        {
            sb.AppendLine(string.Join(",", Csv(w.Word), Csv(w.Pos), Csv(w.Translation), Csv(w.Sentence), Csv(FormatFirstSeen(w))));
        }
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true)); // 带 BOM，Excel 打开不乱码
    }

    private static string Csv(string s)
    {
        if (s.Contains(',') || s.Contains('"') || s.Contains('\n'))
        {
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
        return s;
    }

    private static void ExportMarkdown(IReadOnlyList<VocabEntry> words, string path)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# 生词本");
        sb.AppendLine();
        sb.AppendLine("| 单词 | 词性 | 释义 | 例句 | 首次时间 |");
        sb.AppendLine("|---|---|---|---|---|");
        foreach (VocabEntry w in words)
        {
            sb.AppendLine($"| {Md(w.Word)} | {Md(w.Pos)} | {Md(w.Translation)} | {Md(w.Sentence)} | {Md(FormatFirstSeen(w))} |");
        }
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }

    private static string Md(string s)
    {
        return s.Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");
    }

    private static void ExportAnki(IReadOnlyList<VocabEntry> words, string path)
    {
        var sb = new StringBuilder();
        foreach (VocabEntry w in words)
        {
            string front = w.Word;
            string back = string.Join(" ", w.Pos, w.Translation).Trim();
            sb.AppendLine(front + "\t" + back);
        }
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(false));
    }
}
