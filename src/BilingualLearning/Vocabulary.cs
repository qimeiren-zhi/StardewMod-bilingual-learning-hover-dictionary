using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StardewModdingAPI;

namespace BilingualLearning;

/// <summary>生词本管理器：按存档隔离、立即落盘（F15/F6/F8）。</summary>
public class Vocabulary
{
    private readonly IModHelper helper;
    private readonly IMonitor monitor;
    private VocabularyData data = new();

    public Vocabulary(IModHelper helper, IMonitor monitor)
    {
        this.helper = helper;
        this.monitor = monitor;
    }

    public int Count => this.data.Words.Count;

    public IReadOnlyList<VocabEntry> Words => this.data.Words;

    public void Load()
    {
        this.data = this.helper.Data.ReadJsonFile<VocabularyData>(this.GetSavePath()) ?? new VocabularyData();
        this.monitor.Log($"生词本加载完成：{this.data.Words.Count} 词", LogLevel.Info);
    }

    public bool Contains(string word)
    {
        return this.data.Words.Any(w => string.Equals(w.Word, word, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(VocabEntry entry)
    {
        if (this.Contains(entry.Word))
        {
            return;
        }
        this.data.Words.Add(entry);
        this.Save();
    }

    public void Remove(string word)
    {
        int removed = this.data.Words.RemoveAll(w => string.Equals(w.Word, word, StringComparison.OrdinalIgnoreCase));
        if (removed > 0)
        {
            this.Save();
        }
    }

    private void Save()
    {
        this.helper.Data.WriteJsonFile(this.GetSavePath(), this.data);
    }

    /// <summary>按存档隔离的词典文件路径（独立文件、立即落盘，不随游戏存档周期）。</summary>
    private string GetSavePath()
    {
        return $"data/{Constants.SaveFolderName}.json";
    }
}
