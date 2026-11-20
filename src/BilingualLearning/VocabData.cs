using System.Collections.Generic;

namespace BilingualLearning;

/// <summary>生词本根数据（按存档隔离，序列化为 JSON）。</summary>
public class VocabularyData
{
    public List<VocabEntry> Words { get; set; } = new();
}

/// <summary>单条生词记录（F8）。</summary>
public class VocabEntry
{
    public string Word { get; set; } = "";
    public string Pos { get; set; } = "";
    public string Translation { get; set; } = "";
    public string Sentence { get; set; } = "";
    public FirstSeen FirstSeen { get; set; } = new();
}

/// <summary>首次出现游戏时间（第 X 年 X 季第 X 日）。</summary>
public class FirstSeen
{
    public int Year { get; set; }
    public string Season { get; set; } = "";
    public int Day { get; set; }
}
