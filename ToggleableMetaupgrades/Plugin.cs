using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ToggleableMetaupgrades.Patches;

namespace ToggleableMetaupgrades;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("Aethermancer.exe")]
public class Plugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;

        Harmony harmony = new(MyPluginInfo.PLUGIN_GUID);

        harmony.PatchAll(typeof(MetaUpgradeDependencies));
        harmony.PatchAll(typeof(UpgradeRefunds));

        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_NAME} is loaded!");
    }
}
