using StardewModdingAPI;

namespace BilingualLearning;

/// <summary>mod 配置（config.json，GMCM 可编辑）。</summary>
public class ModConfig
{
    public SButton ToggleKey { get; set; } = SButton.F11;
    public SButton ExportKey { get; set; } = SButton.F9;

    /// <summary>导出位置：Documents / Desktop / Downloads / Mod folder。</summary>
    public string ExportLocation { get; set; } = "Documents";

    /// <summary>气泡风格：Default（深色）/ Sandy's Notes（浅色）。</summary>
    public string BubbleStyle { get; set; } = "Default";
}
