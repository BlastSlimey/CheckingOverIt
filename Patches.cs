
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using BepInEx.Logging;
using HarmonyLib;
using I2.Loc;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public static class PatchesHandler {

    // 0.1.0 locations
    public static ManualLogSource Logger;
    public static List<Trigger> CheckedTriggers = [];

    // 0.1.0 locations
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

    // 0.2.0 locations
    public static Trigger Barrels = new(21, 18, 32, 30, "Barrels");
    public static Trigger FirstLamp = new(28, 27, 70.7f, 69.9f, "First Lamp");
    public static Trigger BigBalloon = new(43, 37, 134, 124, "Big Balloon");
    public static Trigger Pliers = new(35, 27, 131, 118, "Pliers");
    public static Trigger Car = new(-15, -35, 152, 144, "Car");
    public static Trigger Anvil = new(68.8f, 65.8f, 244, 243, "Anvil");
    public static Trigger TelephoneBooth = new(88, 85, 273, 267, "Telephone Booth");
    public static Trigger ShoppingCart = new(51.7f, 49, 321, 318.4f, "Shopping Cart");
    public static Trigger SexyHikingCharacter = new(74, 68, 305, 304, "Sexy Hiking Character");

    public static void DeleteAllAndSave() {
        PlayerPrefs.DeleteKey("NumSaves"+GetUniqueSlot());
        PlayerPrefs.DeleteKey("SaveGame0"+GetUniqueSlot());
        PlayerPrefs.DeleteKey("SaveGame1"+GetUniqueSlot());
        PlayerPrefs.Save();
    }

    public static void DeleteAll() {
        PlayerPrefs.DeleteKey("NumSaves"+GetUniqueSlot());
        PlayerPrefs.DeleteKey("SaveGame0"+GetUniqueSlot());
        PlayerPrefs.DeleteKey("SaveGame1"+GetUniqueSlot());
    }

    public static void DeleteSaveGame01() {
        PlayerPrefs.DeleteKey("SaveGame0"+GetUniqueSlot());
        PlayerPrefs.DeleteKey("SaveGame1"+GetUniqueSlot());
    }

    public static string GetSaveGame0() {
        return PlayerPrefs.GetString("SaveGame0"+GetUniqueSlot());
    }

    public static string GetSaveGame1() {
        return PlayerPrefs.GetString("SaveGame1"+GetUniqueSlot());
    }

    public static void SetPlayerPrefsString(string name, string s) {
        PlayerPrefs.SetString(name+GetUniqueSlot(), s);
    }

    public static int GetNumSaves() {
        return PlayerPrefs.GetInt("NumSaves"+GetUniqueSlot());
    }

    public static void SetNumSaves(int i) {
        PlayerPrefs.SetInt("NumSaves"+GetUniqueSlot(), i);
    }

    public static string GetUniqueSlot() {
        if (ConnectionHandler.Success == null) return "";
        else return "-" + ConnectionHandler.Session.RoomState.Seed + "-" + ConnectionHandler.Success.Slot;
    }

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
public static class GravityControlPatch {

    public static GravityControl GlobalGravControl;
    public static readonly System.Random Random = new();
    public static Task GravityTask = null;
    public static bool InSpace = false;

    // This field is also used outside, so we need to keep the reflection
    public static readonly FieldInfo _creditsUp = typeof(GravityControl).GetField("creditsUp", BindingFlags.NonPublic | BindingFlags.Instance);

    public static bool creditsUp() {
        return GlobalGravControl == null ? false : (bool)_creditsUp.GetValue(GlobalGravControl);
    }

    public static void UpdateGravity(bool creditsUp) {
        if (creditsUp) {
            Physics2D.gravity = new Vector2(0f, 1.2f);
        } else {
            Physics2D.gravity = new Vector2(
                (Random.Next(3) - 1) * 0.4f * ConnectionHandler.WindTraps, 
                InSpace ? 0 : -50 + (10 * ConnectionHandler.GravityReductions)
            );
        }
        if (ConfigHandler.PrintGravity.Value) PatchesHandler.Logger?.LogInfo("Gravity update: " + Physics2D.gravity.ToString());
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
                    PatchesHandler.SexyHikingCharacter.Check();
                    PatchesHandler.ShoppingCart.Check();
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
                    PatchesHandler.SexyHikingCharacter.Check();
                    PatchesHandler.ShoppingCart.Check();
                    PatchesHandler.TelephoneBooth.Check();
                    InitCompletion(instance);
                }
                break;
            case 5:
                if (ConnectionHandler.GoalHeightReductions >= 5) {
                    PatchesHandler.AntennaTop.Check();
                    PatchesHandler.Temple.Check();
                    PatchesHandler.IceMountain.Check();
                    PatchesHandler.LandingStage.Check();
                    PatchesHandler.Bucket.Check();
                    PatchesHandler.SnakeRide.Check();
                    PatchesHandler.Hat.Check();
                    PatchesHandler.Hedge.Check();
                    PatchesHandler.ChurchTop.Check();
                    PatchesHandler.SexyHikingCharacter.Check();
                    PatchesHandler.ShoppingCart.Check();
                    PatchesHandler.TelephoneBooth.Check();
                    PatchesHandler.Anvil.Check();
                    InitCompletion(instance);
                }
                break;
            case 6:
                if (ConnectionHandler.GoalHeightReductions >= 6) {
                    PatchesHandler.AntennaTop.Check();
                    PatchesHandler.Temple.Check();
                    PatchesHandler.IceMountain.Check();
                    PatchesHandler.LandingStage.Check();
                    PatchesHandler.Bucket.Check();
                    PatchesHandler.SnakeRide.Check();
                    PatchesHandler.Hat.Check();
                    PatchesHandler.Hedge.Check();
                    PatchesHandler.ChurchTop.Check();
                    PatchesHandler.Gargoyle.Check();
                    PatchesHandler.OrangeTable.Check();
                    PatchesHandler.Toilet.Check();
                    PatchesHandler.SexyHikingCharacter.Check();
                    PatchesHandler.ShoppingCart.Check();
                    PatchesHandler.TelephoneBooth.Check();
                    PatchesHandler.Anvil.Check();
                    InitCompletion(instance);
                }
                break;
        }
    }

    public static void InitCompletion(GravityControl instance) {
        if (!creditsUp()) {
            UpdateGravity(true);
            Object.Instantiate(instance.creditsPrefab, instance.creditsParent);
            instance.starNest.SetActive(value: true);
            instance.starNest.GetComponent<MeshRenderer>().sharedMaterial.SetFloat("_Brightness", 0f);
            instance.StartCoroutine("FadeUpStarNest");
            _creditsUp.SetValue(instance, true);
            instance.fgCam.GetComponent<PostProcessVolume>().profile = instance.blurOffProfile;
            instance.progressMeter.Pause(shouldPause: true);
            instance.settingsMenu.canMenu = false;
            // Make save file unique for each multiworld and slot
            PatchesHandler.DeleteAllAndSave();
            // Send checks or goal
            if (ConnectionHandler.Success != null) {
                for (int i = 1; i < 10; i++) {
                    string loc_name = $"Got Over It #{i}";
                    long loc_id = ConnectionHandler.Session.Locations.GetLocationIdFromName("Getting Over It", loc_name);
                    if (ConnectionHandler.Session.Locations.AllMissingLocations.Contains(loc_id)) {
                        ConnectionHandler.CheckLocation(loc_id, loc_name);
                        return;
                    }
                }
                // If no completion check left
                ConnectionHandler.Session.SetGoalAchieved();
            }
        }
    }

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void StartPostfix(GravityControl __instance) {
        PatchesHandler.Logger.LogInfo("Started gravity control");
        GlobalGravControl = __instance;
        UpdateGravity(creditsUp());
        if (GravityTask == null) {
            GravityTask = Task.Run(async () => {
                while (true) {
                    await Task.Delay(Random.Next(3, 15)*1000);
                    UpdateGravity(creditsUp());
                }
            });
        }
    }

    [HarmonyPatch("OnTriggerEnter2D")]
    [HarmonyPrefix]
    private static bool OnTriggerEnter2DPrefix() {
        // This is a replacement for the original function, so always return false
        InSpace = true;
        UpdateGravity(creditsUp());
        return false;
    }

    [HarmonyPatch("OnTriggerExit2D")]
    [HarmonyPrefix]
    private static bool OnTriggerExit2DPrefix(GravityControl __instance, Collider2D coll) {
        // This is a replacement for the original function, so always return false
        InSpace = false;
        if (coll.attachedRigidbody == null) {
            return false;
        }
        if (coll.attachedRigidbody.position.y > __instance.GetComponent<BoxCollider2D>().bounds.max.y - 5f) {
            HandleCompletion(__instance, 0);
        } else {
            UpdateGravity(creditsUp());
        }
        return false;
    }

}

[HarmonyPatch(typeof(HammerCollisions))]
public class HammerCollisionsPatch {

    private static Trigger[] Possible40 = [
        PatchesHandler.Tree, PatchesHandler.Paddle, PatchesHandler.ConcretePipe, PatchesHandler.Cup, PatchesHandler.Barrels, 
        PatchesHandler.SnakeRide
    ]; 
    private static Trigger[] Possible117 = [
        PatchesHandler.Trash, PatchesHandler.SecondLamp, PatchesHandler.Bathtub, PatchesHandler.Grill, PatchesHandler.FirstLamp, 
        PatchesHandler.SnakeRide
    ]; 
    private static Trigger[] Possible141 = [
        PatchesHandler.KidsSlide, PatchesHandler.Child, PatchesHandler.Staircase, PatchesHandler.SecurityCam, PatchesHandler.BigBalloon, 
        PatchesHandler.Pliers, PatchesHandler.SnakeRide
    ]; 
    private static Trigger[] Possible215 = [
        PatchesHandler.Toilet, PatchesHandler.OrangeTable, PatchesHandler.Gargoyle, PatchesHandler.ChurchTop, PatchesHandler.Car, 
        PatchesHandler.SnakeRide
    ]; 
    private static Trigger[] Possible274 = [
        PatchesHandler.Hedge, PatchesHandler.Hat, PatchesHandler.Bucket, PatchesHandler.Anvil, PatchesHandler.TelephoneBooth, 
        PatchesHandler.SnakeRide
    ]; 
    private static Trigger[] PossibleElse = [
        PatchesHandler.LandingStage, PatchesHandler.IceMountain, PatchesHandler.Temple, PatchesHandler.AntennaTop, 
        PatchesHandler.ShoppingCart, PatchesHandler.SexyHikingCharacter, PatchesHandler.SnakeRide
    ];

    [HarmonyPatch("OnCollisionEnter2D")]
    [HarmonyPostfix]
    private static void OnCollisionEnter2DPostfix(HammerCollisions __instance, ref Collider2D ___myCollider) {

        Vector2 position = ___myCollider.attachedRigidbody.position;
        if (ConfigHandler.PrintHammerCollision.Value) PatchesHandler.Logger?.LogInfo("Hammer collided: " + position.ToString());

        Trigger[] possible;
        if (position.y <= 40) possible = Possible40;
        else if (position.y <= 117) possible = Possible117;
        else if (position.y <= 141) possible = Possible141;
        else if (position.y <= 215.5f) possible = Possible215;
        else if (position.y <= 274.5f) possible = Possible274;
        else possible = PossibleElse;

        foreach (Trigger t in possible) {
            if (t.IsTriggered(position.x, position.y)) {
                t.Check();
            }
        }

    }
    
    [HarmonyPatch("OnCollisionExit2D")]
    [HarmonyPostfix]
    private static void OnCollisionExit2DPostfix(HammerCollisions __instance, Collision2D coll, ref Collider2D ___myCollider) {
        
        Vector2 position = ___myCollider.attachedRigidbody.position;

        if (position.y >= 357.6f) {
            GravityControlPatch.HandleCompletion(GravityControlPatch.GlobalGravControl, 1);
        } else if (position.y >= 329.5f) {
            GravityControlPatch.HandleCompletion(GravityControlPatch.GlobalGravControl, 2);
        } else if (position.y >= 282.5f) {
            GravityControlPatch.HandleCompletion(GravityControlPatch.GlobalGravControl, 3);
        } else if (position.y >= 255) {
            GravityControlPatch.HandleCompletion(GravityControlPatch.GlobalGravControl, 4);
        } else if (position.y >= 195) {
            GravityControlPatch.HandleCompletion(GravityControlPatch.GlobalGravControl, 5);
        } else if (position.y >= 157) {
            GravityControlPatch.HandleCompletion(GravityControlPatch.GlobalGravControl, 6);
        }
        
    }

}

[HarmonyPatch(typeof(Loader))]
public class LoaderPatch {

    [HarmonyPatch("Start")]
    [HarmonyPostfix]
    private static void StartPostfix(Loader __instance, ref bool ___canContinue) {
        // only thing to change is condition of whether a bool is true
        // This is for patching the save file being unnique to the multiworld and slot
        ___canContinue = PatchesHandler.GetSaveGame0().Length + PatchesHandler.GetSaveGame1().Length > 0;
    }

    [HarmonyPatch("StartGame")]
    [HarmonyPrefix]
    private static bool StartGamePrefix(Loader __instance, ref bool ___safeToClick, ref int ___menuItemClicked) {
        // This is a replacement for the original function, so always return false
        // This is for patching the save file being unnique to the multiworld and slot
        if (!___safeToClick) return false;
        TextMeshProUGUI[] componentsInChildren = __instance.menu.GetComponentsInChildren<TextMeshProUGUI>();
        for (int i = 0; i < componentsInChildren.Length; i++) {
            if (componentsInChildren[i].text == ScriptLocalization.START_NEW_GAME) ___menuItemClicked = i;
        }
        PatchesHandler.DeleteAllAndSave();
        __instance.ContinueGame();
        return false;
    }

}

[HarmonyPatch(typeof(PlayerControl))]
public class PlayerControlPatch {
    
    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    private static bool UpdatePrefix(PlayerControl __instance, ref int ___numWins) {
        // This is a replacement for the original function, so always return false
        // This is for patching the save file being unnique to the multiworld and slot
        if (___numWins > 0 && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKey(KeyCode.R)) {
            PatchesHandler.DeleteAllAndSave();
            SceneManager.LoadScene("Mian");
        }
        __instance.fakeCursor.position = new Vector3(
            __instance.fakeCursor.position.x, __instance.fakeCursor.position.y, __instance.tip.position.z - 1f
        );
        return false;
    }

}

[HarmonyPatch(typeof(RewardLogic))]
public class RewardLogicPatch {

    [HarmonyPatch("EnableYes")]
    [HarmonyPostfix]
    private static void EnableYesPostfix(RewardLogic __instance) {
        // This is for preventing players from entering the secret at the top
        __instance.yes.interactable = false;
    }

}

[HarmonyPatch(typeof(Saviour))]
public class SaviourPatch {

    [HarmonyPatch("Start")]
    [HarmonyPrefix]
    private static bool StartPrefix(
        Saviour __instance, ref int ___savesSinceWrite, ref JointMotor2D ___stillHingeMotor, ref JointMotor2D ___stillSliderMotor,
        ref JointMotor2D ___stillHubMotor, ref SaveState[] ___debugSaves, ref int ___currentSave, ref MemoryStream ___stream,
        ref StreamWriter ___streamWriter, ref XmlSerializer ___serializer, ref int ___frame, ref int ___saveNum, 
        ref bool ___willPlayAnimation
    ) {
        // This is a replacement for the original function, so always return false
        // This is for patching the save file being unnique to the multiworld and slot
        __instance.canReset = true;
		___savesSinceWrite = 20;
		___stillHingeMotor = __instance.hinge.motor;
		___stillHingeMotor.motorSpeed = 0f;
		___stillSliderMotor = ___stillHingeMotor;
		___stillHubMotor = __instance.hubJoint.motor;
		___stillHubMotor.motorSpeed = 0f;
		___stillHubMotor.maxMotorTorque = 100f;
		__instance.cursor.position = __instance.hammer.position;
		__instance.slider.motor = ___stillSliderMotor;
		__instance.hinge.motor = ___stillHingeMotor;
		__instance.hubJoint.motor = ___stillHubMotor;
		___debugSaves = [new(), new()];
		___currentSave = 0;
		___stream = new MemoryStream();
		___streamWriter = new StreamWriter(___stream, Encoding.UTF8);
		___serializer = new XmlSerializer(typeof(SaveState));
		___frame = 0;
		Debug.Log("Application version " + float.Parse(Application.version, CultureInfo.InvariantCulture));
		if (PatchesHandler.GetNumSaves() != 0) ___saveNum = PatchesHandler.GetNumSaves(); 
        else {
			___saveNum = 0;
            PatchesHandler.SetNumSaves(0);
		}
		if (___saveNum > 0) {
			if (__instance.LoadNewestSave()) {
				___willPlayAnimation = false;
				return false;
			}
			Debug.Log("couldn't load save");
			___willPlayAnimation = true;
		} else {
			Debug.Log("no save detected");
			___willPlayAnimation = true;
		}
        return false;
    }

    [HarmonyPatch("SaveGameNow")]
    [HarmonyPrefix]
    private static bool SaveGameNowPrefix(
        Saviour __instance, bool writeToDisk, ref MemoryStream ___stream, ref StreamWriter ___streamWriter, ref int ___currentSave,
        ref SaveState[] ___debugSaves, ref XmlSerializer ___serializer, ref int ___saveNum, ref string ___saveString
    ) {
        // This is a replacement for the original function, so always return false
        // This is for patching the save file being unnique to each multiworld and slot
		___stream = new MemoryStream();
        ___streamWriter = new StreamWriter(___stream, Encoding.UTF8);
		if (___stream == null) return false;
		___currentSave = ___currentSave == 0 ? 1 : 0;
        ___debugSaves[___currentSave] = __instance.Save();
		___serializer.Serialize(___streamWriter, ___debugSaves[___currentSave]);
        ___saveString = Encoding.UTF8.GetString(___stream.GetBuffer());
		if (!___saveString.StartsWith("<?") || !___saveString.EndsWith(">")) {
			___streamWriter.Flush();
			___stream.SetLength(0L);
			Debug.LogWarning("malformed save: " + ___saveString);
			return false;
		}
        PatchesHandler.SetPlayerPrefsString("SaveGame" + ___currentSave, ___saveString);
        PatchesHandler.SetNumSaves(___saveNum);
		if (writeToDisk) PlayerPrefs.Save();
        return false;
    }

    [HarmonyPatch("Update")]
    [HarmonyPrefix]
    private static bool UpdatePrefix(
        Saviour __instance, ref bool ___willPlayAnimation, ref int ___frame, ref int ___savesSinceWrite, ref int ___savesPerWrite
    ) {
        // This is a replacement for the original function, so always return false
        // This is for patching the save file being unnique to each multiworld and slot
		if (___willPlayAnimation) {
			__instance.pc.PlayOpeningAnimation();
            ___willPlayAnimation = false;
			__instance.pc.loadFinished = true;
			__instance.cc.loadFinished = true;
		}
        ___frame++;
		if (___frame > 60 && Time.timeScale > 0f) {
            ___frame = 0;
            ___savesSinceWrite++;
			if (___savesSinceWrite > ___savesPerWrite) {
				__instance.SaveGameNow(writeToDisk: true);
                ___savesSinceWrite = 0;
			} else __instance.SaveGameNow(writeToDisk: false);
		}
		if (Application.isEditor) {
			if (Input.GetKeyDown(KeyCode.L)) __instance.LoadNewestSave();
			if (Input.GetKeyDown(KeyCode.R)) __instance.ResetPlayerButNotDialogue();
			if (Input.GetKeyDown(KeyCode.Minus)) {
				Debug.Log("Deleting old save");
                PatchesHandler.DeleteSaveGame01();
			}
		}
        return false;
    }

    [HarmonyPatch("LoadNewestSave")]
    [HarmonyPrefix]
    private static bool LoadNewestSavePrefix(Saviour __instance, ref bool __result, ref XmlSerializer ___serializer, ref int ___saveNum) {
        // This is a replacement for the original function, so always return false
        // This is for patching the save file being unnique to each multiworld and slot
		__instance.pc.loadedFromSave = true;
		string @string = PatchesHandler.GetSaveGame0();
		string string2 = PatchesHandler.GetSaveGame1();
		SaveState saveState = null;
		SaveState saveState2 = null;
		bool flag = true;
		bool flag2 = true;
		if (@string.Length > 0) {
			using TextReader textReader = new StringReader(@string);
			if (textReader.Peek() != 60) textReader.Read();
			try {
				saveState = (SaveState)___serializer.Deserialize(textReader);
			} catch (System.InvalidOperationException ex) {
				Debug.LogWarning("failed to load save position 1");
				Debug.LogWarning(ex.Message);
				Debug.LogWarning(ex.StackTrace);
				Debug.LogWarning(textReader);
				flag = false;
			}
		} else {
			flag = false;
		}
		if (string2.Length > 0) {
			using TextReader textReader2 = new StringReader(string2);
			if (textReader2.Peek() != 60) textReader2.Read();
			try {
				saveState2 = (SaveState)___serializer.Deserialize(textReader2);
			} catch (System.InvalidOperationException ex2) {
				Debug.LogWarning("failed to load save position 2");
				Debug.LogWarning(ex2.Message);
				Debug.LogWarning(ex2.StackTrace);
				flag2 = false;
			}
		} else {
			flag2 = false;
		}
		Debug.Log("Save loaded at time: " + System.DateTime.UtcNow.ToString());
		if (flag && (double)float.Parse(saveState.version, CultureInfo.InvariantCulture) < 1.2) {
            __result = false;
            return false;
        }
		if (flag2 && (double)float.Parse(saveState2.version, CultureInfo.InvariantCulture) < 1.2) {
            __result = false;
            return false;
        }
		if (flag && flag2) {
			if (saveState.saveNum > saveState2.saveNum) __instance.Load(saveState);
			else __instance.Load(saveState2);
		} else if (flag2) {
			Debug.LogWarning("loading save 2");
			__instance.Load(saveState2);
			___saveNum = saveState2.saveNum;
		} else {
			if (!flag) {
				Debug.LogWarning("nothing to load");
                __result = false;
				return false;
			}
			Debug.LogWarning("loading save 1 since 2 was not present");
			__instance.Load(saveState);
		}
		__result = true;
        return false;
    }

}

[HarmonyPatch(typeof(Scroller))]
public class ScrollerPatch {

    [HarmonyPatch("QuitToTitle")]
    [HarmonyPrefix]
    private static bool QuitToTitlePrefix(Scroller __instance, ref AudioSource ___aud) {
        // This is a replacement for the original function, so always return false
        // This is for patching the save file being unnique to each multiworld and slot
		___aud.Stop();
		if (SceneManager.GetActiveScene().name == "Mian") {
            PatchesHandler.DeleteAll();
			int @int = PlayerPrefs.GetInt("NumWins");
			@int++;
			PlayerPrefs.SetInt("NumWins", @int);
			Debug.Log("Game Done, deleting saves");
			PlayerPrefs.Save();
			SceneManager.LoadScene("Reward Loader");
		} else {
			SceneManager.LoadScene("Loader");
		}
        return false;
    }

}
