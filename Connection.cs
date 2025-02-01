
using System;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using BepInEx.Logging;
using Newtonsoft.Json.Linq;

public class ConnectionHandler {

    public static ArchipelagoSession Session;
    public static LoginSuccessful Success;
    public static ManualLogSource Logger;
    public static int GravityReductions = 2;
    public static int GoalHeightReductions = 0;
    public static int WindTraps = 0;

    public static void Connect(ManualLogSource logger, Action<bool> updateGravity) {

        Logger = logger;
        ProcessOfflineList();

        JArray list = JArray.Parse(ConfigHandler.ConnectionsList.Value);
        int active = ConfigHandler.ActiveSlot.Value;
        if (active < -1) {
            Logger.LogWarning($"Illegal ActiveSlot value: {active}");
            return;
        } else if (active >= list.Count) {
            Logger.LogWarning($"ActiveSlot value out of range: {active}");
            return;
        } else if (active == -1) {
            Logger.LogInfo("Connecting disabled as per config");
            return;
        }
        JToken activeList = list[active];
        string activeAddressPort = activeList.Value<string>("addressPort") ?? "localhost:38281";
        string activeSlot = activeList.Value<string>("slot") ?? "Player1";
        string activePassword = activeList.Value<string>("password") ?? "";

        Session = ArchipelagoSessionFactory.CreateSession(activeAddressPort);
        LoginResult result;
        try {
            result = Session.TryConnectAndLogin(
                "Getting Over It", activeSlot, ItemsHandlingFlags.AllItems, password: activePassword
            );
        } catch (Exception e) {
            result = new LoginFailure(e.GetBaseException().Message);
        }

        if (!result.Successful) {
            LoginFailure failure = (LoginFailure)result;
            string errorMessage = $"Failed to Connect to {activeAddressPort} as {activeSlot}:";
            foreach (string error in failure.Errors) {
                errorMessage += $"\n    {error}";
            }
            foreach (ConnectionRefusedError error in failure.ErrorCodes) {
                errorMessage += $"\n    {error}";
            }
            logger.LogError(errorMessage);
        } else {
            Success = (LoginSuccessful)result;
            logger.LogInfo("Connection successful");
            ProcessReceived();
            Session.Items.ItemReceived += (receivedItemsHelper) => {
                logger.LogInfo($"Received {receivedItemsHelper.DequeueItem().ItemName}");
                ProcessReceived();
                updateGravity(GravityControlPatch.creditsUp());
            };
        }

    }

    public static void ProcessOfflineList() {
        
        Logger.LogInfo("Processing offline inventory...");
        JObject inv = JObject.Parse(ConfigHandler.OfflineItems.Value);
        GravityReductions = inv.Value<int?>("Gravity Reduction") ?? 2;
        GoalHeightReductions = inv.Value<int?>("Goal Height Reduction") ?? 0;
        WindTraps = inv.Value<int?>("Wind Trap") ?? 0;
        if (GravityReductions > 4) GravityReductions = 4;
        if (GoalHeightReductions > 6) GoalHeightReductions = 6;
        if (WindTraps > 6) WindTraps = 6;
        Logger.LogInfo(
            $"[OfflineList] Gravity Reductions: {GravityReductions}, Goal Height Reductions: {GoalHeightReductions}, Wind Traps: {WindTraps}"
        );

    }

    public static void ProcessReceived() {
        
        GravityReductions = 0;
        GoalHeightReductions = 0;
        WindTraps = 0;
        foreach (ItemInfo item in Session.Items.AllItemsReceived) {
            if (item.ItemName.Equals("Gravity Reduction")) GravityReductions++;
            else if (item.ItemName.Equals("Goal Height Reduction")) GoalHeightReductions++;
            else if (item.ItemName.Equals("Wind Trap")) WindTraps++;
        }
        if (GravityReductions > 4) GravityReductions = 4;
        if (GoalHeightReductions > 6) GoalHeightReductions = 6;
        if (WindTraps > 6) WindTraps = 6;
        Logger.LogInfo(
            $"[Received] Gravity Reductions: {GravityReductions}, Goal Height Reductions: {GoalHeightReductions}, Wind Traps: {WindTraps}"
        );

    }

    public static void CheckLocation(string name) {
        if (Success == null) return;
        CheckLocation(Session.Locations.GetLocationIdFromName("Getting Over It", name), name);
    }

    public static void CheckLocation(long id, string name = null) {
        if (Success == null) return;
        Logger.LogInfo($"Checking location {(name == null ? id : name)}");
        Session.Locations.CompleteLocationChecks([id]);
    }

}
