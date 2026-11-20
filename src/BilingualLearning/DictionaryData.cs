using System;
using System.Collections.Generic;
using System.IO;
using StardewModdingAPI;

namespace BilingualLearning;

/// <summary>
/// 精简英中词典（S4）。数据文件 dictionary.tsv，每行：word \t 释义 \t tag。
/// </summary>
public class DictionaryData
{
    public readonly struct Entry
    {
        public readonly string Translation; // 词性 + 中文释义（首行）
        public readonly string Tag;         // 难度标签（gk/cet4/... 空格分隔）

        public Entry(string translation, string tag)
        {
            this.Translation = translation;
            this.Tag = tag;
        }
    }

    private readonly Dictionary<string, Entry> entries = new(StringComparer.OrdinalIgnoreCase);

    public int Count => this.entries.Count;

    public bool TryGet(string word, out Entry entry)
    {
        return this.entries.TryGetValue(word, out entry);
    }

    public static DictionaryData Load(IModHelper helper, IMonitor monitor)
    {
        var data = new DictionaryData();
        string path = Path.Combine(helper.DirectoryPath, "assets", "dictionary.tsv");

        if (!File.Exists(path))
        {
            monitor.Log($"词典文件不存在：{path}", LogLevel.Warn);
            return data;
        }

        foreach (string raw in File.ReadLines(path))
        {
            if (string.IsNullOrWhiteSpace(raw))
                continue;

            string[] parts = raw.Split('\t');
            if (parts.Length < 2)
                continue;

            string word = parts[0].Trim();
            string translation = parts[1].Trim();
            string tag = parts.Length >= 3 ? parts[2].Trim() : "";

            if (word.Length == 0)
                continue;

            data.entries[word] = new Entry(translation, tag);
        }

        monitor.Log($"词典加载完成：{data.Count} 词条", LogLevel.Info);
        return data;
    }
}
