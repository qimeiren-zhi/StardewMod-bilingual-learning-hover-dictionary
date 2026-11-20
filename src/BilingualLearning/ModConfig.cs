using System.Collections.Generic;
using StardewModdingAPI;

namespace BilingualLearning;

/// <summary>mod 配置（config.json，GMCM 可编辑）。</summary>
public class ModConfig
{
    public SButton ToggleKey { get; set; } = SButton.F11;
    public SButton ExportKey { get; set; } = SButton.F9;

    /// <summary>启用的词典难度等级（gk/cet4/cet6/ky/toefl/ielts/gre）。空列表 = 不过滤。</summary>
    public List<string> EnabledLevels { get; set; } = new() { "gk", "cet4", "cet6", "ky", "toefl", "ielts", "gre" };
}
