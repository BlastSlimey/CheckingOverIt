
using System.Collections.Generic;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PatchesHandler {

    public static ManualLogSource Logger;
    public static GravityControl GravControl;
    public static List<Trigger> CheckedTriggers = [];
    public static Trigger Tree = new(-26, -31, 5, -3, "Tree");
    public static Trigger Paddle = new(1.3f, -2, 5.2f, 4.5f, "Paddle");
    public static Trigger ConcretePipe = new(19, 15.5f, 25.3f, 21.8f, "Concrete Pipe");
    public static Trigger Cup = new(21.5f, 20.4f, 37, 36.2f, "Cup");
    public static Trigger Trash = new(31, 28.1f, 56, 53.7f, "Trash");
    public static Trigger SecondLamp = new(26, 24, 74, 73, "Second Lamp");
    public static Trigger Bathtub = new(24, 20, 116, 114, "Bathtub");
    public static Trigger Grill = new(4, 0, 110, 107, "Grill");
    public static Trigger KidsSlide = new(-7, -15, 140, 123, "Kids Slide");
    public static Trigger Child = new(-14.4f, -15.4f, 136.5f, 135.5f, "Child");
    public static Trigger Staircase = new(23, 17.6f, 130.5f, 123, "Staircase");
    public static Trigger SecurityCam = new(18, 16, 138.5f, 138, "Security Cam");
    public static Trigger Toilet = new(29, 27, 159, 158, "Toilet");
    public static Trigger OrangeTable = new(-2, -4, 165, 161 ,"Orange Table");
    public static Trigger Gargoyle = new(10, 8, 195.5f, 193.5f, "Gargoyle");
    public static Trigger ChurchTop = new(17, 13.5f, 215, 208.4f, "Church Top");
    public static Trigger Hedge = new(34, 30.2f, 218.5f, 216, "Hedge");
    public static Trigger Hat = new(62, 59.9f, 246, 241.8f, "Hat");
    public static Trigger SnakeRide = new(-50, -100, 400, 10, "Snake Ride");
    public static Trigger Bucket = new(24.3f, 10.6f, 274, 266, "Bucket");
    public static Trigger LandingStage = new(10.5f, 1, 285, 275, "Landing Stage");
    public static Trigger IceMountain = new(40.5f, 22.1f, 312, 282.4f, "Ice Mountain");
    public static Trigger Temple = new(59, 52.6f, 330, 318.7f, "Temple");
    public static Trigger AntennaTop = new(65, 60, 360, 357.6f, "Antenna Top");

}

public struct Trigger(float xma, float xmi, float yma, float ymi, string nam) {
    public float xmax = xma;
    public float xmin = xmi;
    public float ymax = yma;
    public float ymin = ymi;
    public string name = nam;
    public bool IsTriggered(float x, float y) {
        return x <= xmax && x >= xmin && y <= ymax && y >= ymin;
    }
    public void Check() {
        if (!PatchesHandler.CheckedTriggers.Contains(this)) {
            PatchesHandler.CheckedTriggers.Add(this);
            ConnectionHandler.CheckLocation(name);
        }
    }
}

[HarmonyPatch(typeof(GravityControl))]
public class GravityControlPatch {

    public static void UpdateGravity() {
        Physics2D.gravity = new Vector2(
            (new System.Random().Next(3) - 1) * 0.4f * ConnectionHandler.WindTraps, 
            -50 + (10 * ConnectionHandler.GravityReductions)
        );
    }

    public static void HandleCompletion(GravityControl instance, int threshold) {
        switch (threshold) {
            case 0:
                InitCompletion(instance);
                break;
            case 1:
                if (ConnectionHandler.GoalHeightReductions >= 1) {
                    InitCompletion(instance);
                }
                break;
            case 2:
                if (ConnectionHandler.GoalHeightReductions >= 2) {
                    PatchesHandler.AntennaTop.Check();
                    InitCompletion(instance);
                }
                break;
            case 3:
                if (ConnectionHandler.GoalHeightReductions >= 3) {
                    PatchesHandler.AntennaTop.Check();
                    PatchesHandler.Temple.Check();
                    InitCompletion(instance);
                }
                break;
            case 4:
                if (ConnectionHandler.GoalHeightReductions >= 4) {
                    PatchesHandler.AntennaTop.Check();
                    PatchesHandler.Temple.Check();
                    PatchesHandler.IceMountain.Check();
                    PatchesHandler.LandingStage.Check();
                    PatchesHandler.Bucket.Check();
                    PatchesHandler.SnakeRide.Check();
                    PatchesHandler.Hat.Check();
                    PatchesHandler.Hedge.Check();
                    PatchesHandler.ChurchTop.Check();
                    InitCompletion(instance);
                }
                break;
        }
    }

    public static void InitCompletion(GravityControl instance) {
        if (!(bool)Util.GetPrivateField(typeof(GravityControl), instance, "creditsUp")) {
            Physics2D.gravity = new Vector2(0f, 1.2f);
            Object.Instantiate(instance.creditsPrefab, instance.creditsParent);
            instance.starNest.SetActive(value: true);
            instance.starNest.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Brightness", 0f);
            instance.StartCoroutine("FadeUpStarNest");
            Util.SetPrivateField(typeof(GravityControl), instance, "creditsUp", true);
            instance.fgCam.GetComponent<PostProcessVolume>().profile = instance.blurOffProfile;
            instance.progressMeter.Pause(shouldPause: true);
            instance.settingsMenu.canMenu = false;
            PlayerPrefs.DeleteKey("NumSaves");
            PlayerPrefs.DeleteKey("SaveGame0");
            PlayerPrefs.DeleteKey("SaveGame1");
            PlayerPrefs.Save();
            for (int i = 1; i < 10; i++) {
                string loc_name = $"Got Over It #{i}";
                long loc_id = ConnectionHandler.Session.Locations.GetLocationIdFromName("Getting Over It", loc_name);
                if (ConnectionHandler.Session.Locations.AllMissingLocations.Contains(loc_id)) {
                    ConnectionHandler.CheckLocation(loc_id, loc_name);
                    return;
                }
            }
            // If no completion check left
            StatusUpdatePacket packet = new StatusUpdatePacket();
            packet.Status = ArchipelagoClientState.ClientGoal;
            ConnectionHandler.Session.Socket.SendPacket(packet);
        }
    }

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void Start(GravityControl __instance) {
        UpdateGravity();
        PatchesHandler.GravControl = __instance;
    }

    [HarmonyPatch("OnTriggerExit2D")]
    [HarmonyPrefix]
    private static bool OnTriggerExit2DPrefix() {
        return false;
    }

    [HarmonyPatch("OnTriggerExit2D")]
    [HarmonyPostfix]
    private static void OnTriggerExit2DPostfix(GravityControl __instance, Collider2D coll) {
        if (coll.attachedRigidbody == null) {
            return;
        }
        if (coll.attachedRigidbody.position.y > __instance.GetComponent<BoxCollider2D>().bounds.max.y - 5f) {
            HandleCompletion(__instance, 0);
        } else {
            UpdateGravity();
        }
    }

}

[HarmonyPatch(typeof(GravityControlLite))]
public class GravityControlLitePatch {

    [HarmonyPatch("OnTriggerExit2D")]
    [HarmonyPostfix]
    private static void OnTriggerExit2DPostfix(Collider2D coll) {
        if (coll.attachedRigidbody != null) {
            GravityControlPatch.UpdateGravity();
        }
    }

}

[HarmonyPatch(typeof(HammerCollisions))]
public class HammerCollisionsPatch {

    [HarmonyPatch("OnCollisionEnter2D")]
    [HarmonyPostfix]
    private static void OnCollisionEnter2DPostfix(HammerCollisions __instance, Collision2D coll) {

        Collider2D mycoll = (Collider2D)Util.GetPrivateField(typeof(HammerCollisions), __instance, "myCollider");
        Vector2 position = mycoll.attachedRigidbody.position;
        //PatchesHandler.Logger?.LogInfo(position);

        Trigger[] possible;
        if (position.x <= 0) possible = [
            PatchesHandler.Tree, PatchesHandler.Child, PatchesHandler.SnakeRide, PatchesHandler.OrangeTable, PatchesHandler.KidsSlide, 
            PatchesHandler.Paddle, PatchesHandler.Grill
        ];
        else if (position.x <= 10.5f) possible = [
            PatchesHandler.Paddle, PatchesHandler.Grill, PatchesHandler.Gargoyle, PatchesHandler.LandingStage
        ];
        else if (position.x <= 20) possible = [
            PatchesHandler.ConcretePipe, PatchesHandler.SecurityCam, PatchesHandler.ChurchTop, PatchesHandler.Bathtub, 
            PatchesHandler.Staircase, PatchesHandler.Bucket
        ];
        else if (position.x <= 26) possible = [
            PatchesHandler.Cup, PatchesHandler.SecondLamp, PatchesHandler.Bathtub, PatchesHandler.Staircase, PatchesHandler.Bucket, 
            PatchesHandler.IceMountain
        ];
        else possible = [
            PatchesHandler.Trash, PatchesHandler.Toilet, PatchesHandler.Hedge, PatchesHandler.Hat, PatchesHandler.IceMountain, 
            PatchesHandler.Temple, PatchesHandler.AntennaTop
        ];

        foreach (Trigger t in possible) {
            if (t.IsTriggered(position.x, position.y)) {
                t.Check();
            }
        }

    }
    
    [HarmonyPatch("OnCollisionExit2D")]
    [HarmonyPostfix]
    private static void OnCollisionExit2DPostfix(HammerCollisions __instance, Collision2D coll) {
        
        Collider2D mycoll = (Collider2D)Util.GetPrivateField(typeof(HammerCollisions), __instance, "myCollider");
        Vector2 position = mycoll.attachedRigidbody.position;

        if (position.y >= 357.6f) {
            GravityControlPatch.HandleCompletion(PatchesHandler.GravControl, 1);
        } else if (position.y >= 329.5f) {
            GravityControlPatch.HandleCompletion(PatchesHandler.GravControl, 2);
        } else if (position.y >= 282.5f) {
            GravityControlPatch.HandleCompletion(PatchesHandler.GravControl, 3);
        } else if (position.y >= 195) {
            GravityControlPatch.HandleCompletion(PatchesHandler.GravControl, 4);
        }
        
    }

}

[HarmonyPatch(typeof(RewardLogic))]
public class RewardLogicPatch {

    [HarmonyPatch("EnableYes")]
    [HarmonyPostfix]
    private static void EnableYesPostfix(RewardLogic __instance) {
        __instance.yes.interactable = false;
    }

}
