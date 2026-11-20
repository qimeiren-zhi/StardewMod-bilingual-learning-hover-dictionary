using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

/// <summary>
/// 从 ECDICT 的 ecdict.csv 提取精简词典。
/// 列：word(0) phonetic(1) definition(2) translation(3) pos(4) collins(5) oxford(6) tag(7) bnc(8) frq(9) ...
/// 输出：word \t 释义首行 \t tag
/// 子集规则：tag 非空（gk/cet4/cet6/ky/toefl/ielts/gre 等），或 frq ∈ (0, 30000]（高频常用词）。
/// </summary>
internal static class Program
{
    private static int Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.Error.WriteLine("用法: build-dict <input.csv> <output.tsv> [maxFrq]");
            return 1;
        }

        string input = args[0];
        string output = args[1];
        int maxFrq = args.Length >= 3 && int.TryParse(args[2], out int m) ? m : 30000;

        int included = 0;
        int total = 0;

        using var reader = new StreamReader(input, Encoding.UTF8);
        using var writer = new StreamWriter(output, false, new UTF8Encoding(false));

        string header = reader.ReadLine();
        if (header == null)
        {
            Console.Error.WriteLine("空文件");
            return 1;
        }

        string line;
        while ((line = reader.ReadLine()) != null)
        {
            total++;
            var f = ParseCsvLine(line);
            if (f.Count < 10)
                continue;

            string word = f[0].Trim();
            string translation = f[3].Trim();
            string tag = f[7].Trim();
            int frq = int.TryParse(f[9].Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out int n) ? n : 0;

            bool tagged = tag.Length > 0;
            bool common = frq > 0 && frq <= maxFrq;
            if (!tagged && !common)
                continue;
            if (word.Length == 0)
                continue;

            // 释义首行（ECDICT 里词性之间用字面 "\n" 分隔）
            string first = translation;
            int nl = first.IndexOf("\\n", StringComparison.Ordinal);
            if (nl >= 0)
                first = first.Substring(0, nl).Trim();

            first = first.Replace('\t', ' ').Replace('\r', ' ').Replace('\n', ' ').Trim();
            if (first.Length == 0)
                first = translation;

            writer.Write(word);
            writer.Write('\t');
            writer.Write(first);
            writer.Write('\t');
            writer.Write(tag);
            writer.Write('\n');
            included++;
        }

        Console.WriteLine($"总词条: {total}, 纳入: {included}（tag非空 或 frq<={maxFrq}）");
        return 0;
    }

    private static List<string> ParseCsvLine(string line)
    {
        var fields = new List<string>();
        var sb = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        sb.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    sb.Append(c);
                }
            }
            else
            {
                if (c == '"')
                    inQuotes = true;
                else if (c == ',')
                {
                    fields.Add(sb.ToString());
                    sb.Clear();
                }
                else
                    sb.Append(c);
            }
        }

        fields.Add(sb.ToString());
        return fields;
    }
}
