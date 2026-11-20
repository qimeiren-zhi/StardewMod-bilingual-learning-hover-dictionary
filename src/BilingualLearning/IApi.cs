using System;
using StardewModdingAPI;

namespace GenericModConfigMenu;

/// <summary>Generic Mod Config Menu API 的最小接口（仅本 mod 用到的方法，运行时经 SMAPI GetApi 动态代理）。</summary>
public interface IApi
{
    void RegisterModConfig(IManifest mod, Action revertToDefault, Action saveToFile);

    void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<bool> optionGet, Action<bool> optionSet);
    void RegisterSimpleOption(IManifest mod, string optionName, string optionDesc, Func<SButton> optionGet, Action<SButton> optionSet);
}
