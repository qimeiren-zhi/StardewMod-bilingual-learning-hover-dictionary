using System;
using HarmonyLib;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;

namespace BilingualLearning;

/// <summary>Mod 入口点：注册 Harmony 补丁与覆盖层绘制事件。</summary>
public class ModEntry : Mod
{
    private static ModEntry? instance;

    private readonly WordOverlay overlay = new();

    public override void Entry(IModHelper helper)
    {
        instance = this;
        var dictionary = DictionaryData.Load(helper, this.Monitor);
        this.overlay.Init(this.Monitor, dictionary);

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

        // S3: 气泡覆盖层绘制。用 RenderedActiveMenu（早于光标绘制），画在此处的气泡天然被光标覆盖。
        helper.Events.Display.RenderedActiveMenu += this.OnRenderedActiveMenu;
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

            // 气泡画完后重绘菜单光标（layer 1.0），确保光标浮在气泡之上。
            // 因为 RenderedActiveMenu 触发于 DialogueBox.draw（末尾已画过 drawMouse）之后，
            // 气泡会盖住那个光标，故需在气泡之后重画一次。
            box.drawMouse(e.SpriteBatch);
        }
    }
}
