using System;
using System.Collections.Generic;
using System.IO;
using StardewModdingAPI;

namespace BilingualLearning;

/// <summary>
/// 精简英中词典（v2）。数据文件 dictionary.tsv，每行：word \t pos \t translation \t tag。
/// </summary>
public class DictionaryData
{
    public readonly struct Entry
    {
        public readonly string Pos;         // 词性（n./vt./人名 等，可空）
        public readonly string Translation; // 中文释义（收敛后 1~2 义项）
        public readonly string Tag;         // 难度标签（gk/cet4/... 空格分隔，含 prop）

        public Entry(string pos, string translation, string tag)
        {
            this.Pos = pos;
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

    public bool Contains(string word)
    {
        return this.entries.ContainsKey(word);
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
            if (parts.Length < 4)
                continue;

            string word = parts[0].Trim();
            string pos = parts[1].Trim();
            string translation = parts[2].Trim();
            string tag = parts[3].Trim();

            if (word.Length == 0 || translation.Length == 0)
                continue;

            data.entries[word] = new Entry(pos, translation, tag);
        }

        monitor.Log($"词典加载完成：{data.Count} 词条", LogLevel.Info);
        return data;
    }
}
