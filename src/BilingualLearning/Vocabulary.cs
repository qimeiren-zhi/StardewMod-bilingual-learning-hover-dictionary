using System;
using System.Linq;
using StardewModdingAPI;

namespace BilingualLearning;

/// <summary>生词本管理器：按存档隔离读写（F15），记录单词/词性/释义/原句/时间（F8）。</summary>
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

    public void Load()
    {
        this.data = this.helper.Data.ReadSaveData<VocabularyData>("vocabulary") ?? new VocabularyData();
        this.monitor.Log($"生词本加载完成：{this.data.Words.Count} 词", LogLevel.Info);
    }

    public bool Contains(string word)
    {
        return this.data.Words.Any(w => string.Equals(w.Word, word, StringComparison.OrdinalIgnoreCase));
    }

    public void Add(VocabEntry entry)
    {
        if (this.Contains(entry.Word))
            return;

        this.data.Words.Add(entry);
        this.Save();
    }

    public void Remove(string word)
    {
        int removed = this.data.Words.RemoveAll(w => string.Equals(w.Word, word, StringComparison.OrdinalIgnoreCase));
        if (removed > 0)
            this.Save();
    }

    private void Save()
    {
        this.helper.Data.WriteSaveData("vocabulary", this.data);
    }
}
