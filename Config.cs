
using System;
using System.Linq;
using BepInEx.Configuration;

public struct ConnectionInfo {

    public string slot;
    public string address;
    public int port;
    public string password;

    public ConnectionInfo(string sl, string ad, int po, string pa) {
        slot = sl;
        address = ad;
        port = po;
        password = pa;
    }

}

public class ConfigHandler {

    public static ConfigEntry<string> ConnectionsList;
    public static ConfigEntry<int> ActiveSlot;
    public static ConfigEntry<string> OfflineItems;
    public static ConfigEntry<bool> PrintHammerCollision;

    public static void InitConfig(ConfigFile Config) {
        ConnectionsList = Config.Bind(
            "General", "ConnectionList", 
            "[{'slot':'SlotName','addressPort':'archipelago.gg:38281','password':'psswrd123'},"+
            "{'slot':'AnotherPlayer','addressPort':'your.APServer.com','password':'top.scrt'},"+
            "{'slot':'Player1','addressPort':'localhost:38281'}]", 
            "A list of connection details, so you don't have to re-enter them every time you play another slot. " + 
            "Has to be formatted as a JSON string."
        );
        ActiveSlot = Config.Bind(
            "General", "ActiveSlot", -1, 
            "The ConnectionList entry to use for connecting to a sever. Begins with 0 as the first entry. Use -1 to disable connecting."
        );
        OfflineItems = Config.Bind(
            "General", "OfflineItems",
            "{'Gravity Reduction':2,'Goal Height Reduction':0,'Wind Trap':0}",
            "Define a set inventory that should be used if the game is not connected to any Archipelago server."
        );
        PrintHammerCollision = Config.Bind(
            "General", "PrintHammerCollision", false, 
            "Print every collision of the hammer with anything to the console. For debug purposes."
        );
    }

}
