using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CheckingOverIt;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInProcess("GettingOverIt.exe")]
public class Plugin : BaseUnityPlugin {

    internal static new ManualLogSource Logger;
    public static Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        
    private void Awake() {

        Logger = base.Logger;
        PatchesHandler.Logger = Logger;
        Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
        
        Logger.LogInfo("Initializing config...");
        ConfigHandler.InitConfig(Config);

        Logger.LogInfo("Applying Harmony patches...");
        harmony.PatchAll();

        Logger.LogInfo("Attempting connection...");
        ConnectionHandler.Connect(Logger, GravityControlPatch.UpdateGravity);

    }

}
