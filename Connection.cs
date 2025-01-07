
using System;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using BepInEx.Logging;

public class ConnectionHandler {

    public static ArchipelagoSession Session;
    public static LoginSuccessful Success;
    public static ManualLogSource Logger;
    public static int GravityReductions = 0;
    public static int GoalHeightReductions = 0;
    public static int WindTraps = 0;

    public static void Connect(ManualLogSource logger, Action updateGravity) {

        Logger = logger;

        string[] list = ConfigHandler.ConnectionsList.Value.Split([";;;"], StringSplitOptions.None);
        int active = ConfigHandler.ActiveSlot.Value;
        if (active < -1) {
            Logger.LogWarning($"Illegal ActiveSlot value: {active}");
            return;
        } else if (active >= list.Length) {
            Logger.LogWarning($"ActiveSlot value out of range: {active}");
            return;
        } else if (active == -1) {
            Logger.LogInfo("Connecting disabled as per config");
            return;
        }
        string[] activeList = list[active].Split([",,,"], StringSplitOptions.None);
        if (activeList.Length < 2) {
            Logger.LogWarning($"Chosen slot contains not enough information");
            return;
        } else if (activeList.Length == 2) {
            activeList.Append("");
        }

        Session = ArchipelagoSessionFactory.CreateSession(activeList[1]);
        LoginResult result;
        try {
            result = Session.TryConnectAndLogin(
                "Getting Over It", activeList[0], ItemsHandlingFlags.AllItems, password: activeList[2]
            );
        } catch (Exception e) {
            result = new LoginFailure(e.GetBaseException().Message);
        }

        if (!result.Successful) {
            LoginFailure failure = (LoginFailure)result;
            string errorMessage = $"Failed to Connect to {activeList[1]} as {activeList[0]}:";
            foreach (string error in failure.Errors) {
                errorMessage += $"\n    {error}";
            }
            foreach (ConnectionRefusedError error in failure.ErrorCodes) {
                errorMessage += $"\n    {error}";
            }
        } else {
            Success = (LoginSuccessful)result;
            logger.LogInfo("Connection successful");
            Session.Items.ItemReceived += (receivedItemsHelper) => {
                logger.LogInfo($"Received {receivedItemsHelper.DequeueItem().ItemName}");
                GravityReductions = 0;
                GoalHeightReductions = 0;
                WindTraps = 0;
                foreach (ItemInfo item in Session.Items.AllItemsReceived) {
                    if (item.ItemName.Equals("Gravity Reduction")) GravityReductions++;
                    else if (item.ItemName.Equals("Goal Height Reduction")) GoalHeightReductions++;
                    else if (item.ItemName.Equals("Wind Trap")) WindTraps++;
                }
                if (GravityReductions > 4) GravityReductions = 4;
                if (GoalHeightReductions > 4) GoalHeightReductions = 4;
                if (WindTraps > 6) WindTraps = 6;
                logger.LogInfo(
                    $"Gravity Reductions: {GravityReductions}, Goal Height Reductions: {GoalHeightReductions}, Wind Traps: {WindTraps}"
                );
                updateGravity();
            };
        }

    }

    public static void CheckLocation(string name) {
        CheckLocation(Session.Locations.GetLocationIdFromName("Getting Over It", name), name);
    }

    public static void CheckLocation(long id, string name = null) {
        Logger.LogInfo($"Checking location {(name == null ? id : name)}");
        Session.Locations.CompleteLocationChecks([id]);
    }

}
