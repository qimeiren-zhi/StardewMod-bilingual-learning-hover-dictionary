using System;
using System.Collections.Generic;
using GenericModConfigMenu;
using HarmonyLib;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;

namespace BilingualLearning;

/// <summary>Mod 入口点：注册 Harmony 补丁、覆盖层绘制、生词本、F11 开关、名称过滤、语言校验。</summary>
public class ModEntry : Mod
{
    private static ModEntry? instance;

    private readonly WordOverlay overlay = new();
    private Vocabulary? vocabulary;
    private ModConfig config = new();

    public override void Entry(IModHelper helper)
    {
        instance = this;

        // 配置 + 词典 + 中文字体 + 生词本
        this.config = helper.ReadConfig<ModConfig>();
        var dictionary = DictionaryData.Load(helper, this.Monitor);
        var chineseFont = ChineseFont.Load(helper, this.Monitor, 16f * SpriteText.FontPixelZoom);
        this.vocabulary = new Vocabulary(helper, this.Monitor);

        // 生词本在存档加载后才读取（需存档已加载）
        helper.Events.GameLoop.SaveLoaded += (sender, e) => this.vocabulary?.Load();

        // 人物名称集合（NPC 名，用于过滤）
        var nameSet = this.LoadNpcNames(helper);

        var bubbleDefault = this.LoadBubbleTexture(helper, "assets/bubble_default.png");
        var bubbleSandy = this.LoadBubbleTexture(helper, "assets/bubble_sandy.png");

        this.overlay.Init(this.Monitor, dictionary, chineseFont, this.vocabulary!, nameSet, this.config, bubbleDefault, bubbleSandy);

        var harmony = new Harmony(this.ModManifest.UniqueID);

        // S2: hover 命中检测
        harmony.Patch(
            original: AccessTools.Method(
                typeof(DialogueBox),
                nameof(DialogueBox.performHoverAction),
                new[] { typeof(int), typeof(int) }
            ),
            postfix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.AfterHover))
        );

        // S3: 星标点击分层（命中星标时吞掉点击，不翻页）
        harmony.Patch(
            original: AccessTools.Method(
                typeof(DialogueBox),
                nameof(DialogueBox.receiveLeftClick),
                new[] { typeof(int), typeof(int), typeof(bool) }
            ),
            prefix: new HarmonyMethod(typeof(ModEntry), nameof(ModEntry.BeforeClick))
        );

        // S3: 气泡覆盖层绘制
        helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;

        // F11/F9 快捷键
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;

        // GMCM 配置菜单（可选前置）：在 GameLaunched（所有 mod 初始化完成后）再注册
        helper.Events.GameLoop.GameLaunched += (sender, e) => this.RegisterConfigMenu(helper);
    }

    private Texture2D? LoadBubbleTexture(IModHelper helper, string path)
    {
        try
        {
            return helper.ModContent.Load<Texture2D>(path);
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"加载气泡背景失败（{path}）：{ex.Message}", LogLevel.Warn);
            return null;
        }
    }

    private void RegisterConfigMenu(IModHelper helper)
    {
        var api = helper.ModRegistry.GetApi<IApi>("spacechase0.GenericModConfigMenu");
        if (api == null)
        {
            return; // 未安装 GMCM
        }

        var manifest = this.ModManifest;
        api.RegisterModConfig(
            manifest,
            () => this.config = new ModConfig(),
            () => helper.WriteConfig(this.config)
        );

        api.SetDefaultIngameOptinValue(manifest, true);

        api.RegisterSimpleOption(manifest, "Toggle key", "Toggle the lookup feature on/off", () => this.config.ToggleKey, value => this.config.ToggleKey = value);
        api.RegisterSimpleOption(manifest, "Export key", "Export the vocabulary", () => this.config.ExportKey, value => this.config.ExportKey = value);
        api.RegisterChoiceOption(manifest, "Export location", "Where to save exported files", () => this.config.ExportLocation, value => this.config.ExportLocation = value, new[] { "Documents", "Desktop", "Downloads", "Mod folder" });
        api.RegisterChoiceOption(manifest, "Bubble style", "Bubble background and text color style", () => this.config.BubbleStyle, value => this.config.BubbleStyle = value, new[] { "Default", "Sandy's Notes" });
    }

    private HashSet<string> LoadNpcNames(IModHelper helper)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        try
        {
            var characters = helper.GameContent.Load<Dictionary<string, string>>("Data/NPCGiftTastes");
            foreach (string name in characters.Keys)
            {
                set.Add(name);
            }
            this.Monitor.Log($"NPC 名称过滤：已加载 {set.Count} 个名称", LogLevel.Info);
        }
        catch (Exception ex)
        {
            this.Monitor.Log($"加载 NPC 名单失败：{ex.Message}", LogLevel.Warn);
        }
        return set;
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (e.Button == this.config.ToggleKey)
        {
            this.overlay.Enabled = !this.overlay.Enabled;
            this.Monitor.Log($"学习模式：{(this.overlay.Enabled ? "开启" : "关闭")}", LogLevel.Info);
        }
        else if (e.Button == this.config.ExportKey)
        {
            // 导出生词本
            if (this.vocabulary != null && this.vocabulary.Words.Count > 0)
            {
                Exporter.Export(this.vocabulary.Words, this.config.ExportLocation, this.Helper.DirectoryPath, this.Monitor);
            }
            else
            {
                this.Monitor.Log("生词本为空，无需导出", LogLevel.Info);
            }
        }
    }

    private static void AfterHover(DialogueBox __instance, int mouseX, int mouseY)
    {
        instance?.overlay.OnHover(__instance, mouseX, mouseY);
    }

    private static bool BeforeClick(DialogueBox __instance, int x, int y)
    {
        return instance?.overlay.OnClick(__instance, x, y) ?? true;
    }

    private void OnRenderedActiveMenu(object? sender, RenderedActiveMenuEventArgs e)
    {
        if (Game1.activeClickableMenu is DialogueBox box)
        {
            this.overlay.Draw(e.SpriteBatch);

            // 气泡画完后重绘菜单光标，确保光标浮在气泡之上
            box.drawMouse(e.SpriteBatch);
        }
    }
}
