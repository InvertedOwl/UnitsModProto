using HarmonyLib;
using MelonLoader;
using Protobot;
using Protobot.InputEvents;
using Protobot.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace TestMod
{
    public static class BuildInfo
    {
        public const string Name = "CustomSnapping"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "Allows user to add custom snapping distances"; // Description for the Mod.  (Set as null if none)
        public const string Author = "InvertedOwl"; // Author of the Mod.  (MUST BE SET)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "0.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class Mod : MelonMod
    {
        public static float translation = 0.125f;
        public static float rotation = 15;

        public override void OnApplicationStart() // Runs after Game Initialization.
        {
            var h = new HarmonyLib.Harmony("InvertedOwlPatch");
        }

        public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
            if (PlayerPrefs.HasKey("translation"))
            {
                translation = PlayerPrefs.GetFloat("translation");
            }
            if (PlayerPrefs.HasKey("rotation"))
            {
                rotation = PlayerPrefs.GetFloat("rotation");
            }

            GameObject SelectionToggle = FindInActiveObjectByName("Selection Toggle");
            GameObject newToggle = GameObject.Instantiate(SelectionToggle, SelectionToggle.transform.parent);
            newToggle.transform.localPosition = new Vector2(75, 53.5f);
            newToggle.name = "Mods Toggle";
            newToggle.transform.GetChild(1).GetComponent<Text>().text = "Mods";

            GameObject SelectionMenu = FindInActiveObjectByName("Selection Prefs Menu");
            GameObject newMenu = GameObject.Instantiate(SelectionMenu, SelectionMenu.transform.parent);
            newMenu.transform.localPosition = new Vector2(-175, 0);
            newMenu.name = "Mods Prefs Menu";
            newMenu.transform.GetChild(0).GetComponent<Text>().text = "Unit Movement";

            GameObject.Destroy(newMenu.transform.GetChild(2).gameObject);
            GameObject.Destroy(newMenu.transform.GetChild(3).gameObject);
            GameObject.Destroy(newMenu.transform.GetChild(4).gameObject);
            GameObject.Destroy(newMenu.transform.GetChild(5).gameObject);
            GameObject shor = newMenu.transform.GetChild(1).gameObject;
            shor.name = "Input 1";

            GameObject.Destroy(shor.GetComponent<RebindUI>());
            GameObject.Destroy(shor.transform.GetChild(3).gameObject);
            shor.transform.GetChild(0).GetComponent<Text>().text = "Translation Unit";

            GameObject.Destroy(shor.transform.GetChild(2).GetComponent<Button>());
            InputField f = shor.AddComponent<InputField>();
            f.textComponent = shor.transform.GetChild(2).GetChild(0).GetComponent<Text>();
            f.text = translation + "";
            f.onEndEdit.AddListener(delegate
            {
                OnTranslationInput(f);
            });
            //shor.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = "0.125";

            GameObject shor2 = GameObject.Instantiate(shor, shor.transform.parent);
            shor2.transform.localPosition = new Vector2(0, 127.6f - 35);

            shor2.transform.GetChild(0).GetComponent<Text>().text = "Rotation Deg";
            shor2.transform.GetComponent<InputField>().text = rotation + "";
            shor2.transform.GetComponent<InputField>().onEndEdit.AddListener(delegate
            {
                OnRotationInput(shor2.transform.GetComponent<InputField>());
            });



            Toggle t = newToggle.GetComponent<Toggle>();
            t.onValueChanged.RemoveAllListeners();
            t.onValueChanged.AddListener(delegate
            {
                OnModsToggle(t, newMenu, SelectionMenu);
            });
        
        }

        public void OnModsToggle(Toggle t, GameObject menu, GameObject se)
        {
            if (t.isOn)
            {
                menu.SetActive(true);
                se.SetActive(false);
            } else
            {
                menu.SetActive(false);
            }
        }

        public void OnTranslationInput(InputField f)
        {
            float o = 0.125f;
            float.TryParse(f.text, out o);
            Mod.translation = o;
            PlayerPrefs.SetFloat("translation", o);
        }
        public void OnRotationInput(InputField f)
        {
            float o = 15;
            float.TryParse(f.text, out o);
            Mod.rotation = o;
            PlayerPrefs.SetFloat("rotation", o);
        }


        [HarmonyPatch(typeof(PositionTool), "MoveToPos")]
        class PositionPatch
        {
            [HarmonyPrefix]
            internal static bool Prefix(Vector3 pos, ref MovementManager ___movementManager, ref Vector3 __result)
            {
                if (PositionTool.snapping)
                {
                    pos = pos.Round(Mod.translation);
                }
                ___movementManager.MoveTo(pos);
                __result = pos;
                return false;
            }
        }

        [HarmonyPatch(typeof(RotateRing), "Rotate")]
        class RotationPatch
        {
            [HarmonyPrefix]
            internal static bool Prefix( ref Vector3 ___initMouseVector, ref Camera ___refCamera, 
                ref Vector3 ___initRotVector, ref Quaternion ___initRot, ref MovementManager ___movementManager, ref Quaternion ___finalRotation, RotateRing __instance)
            {
                Vector3 MouseVector = (MouseInput.Position -___refCamera.WorldToScreenPoint(__instance.transform.position)).normalized;
                float num = Vector2.SignedAngle(MouseVector, ___initMouseVector);
                if (RotateRing.snapping)
                {
                    num = Mathf.Round(num * 1f / Mod.rotation) * Mod.rotation;
                }
                float f = Vector3.Dot(___refCamera.transform.forward, ___initRotVector);
                Quaternion rotation = Quaternion.AngleAxis(num, -___initRotVector * Mathf.Sign(f)) * ___initRot;
                ___movementManager.RotateTo(rotation);
               ___finalRotation = rotation;
                return false;
            }
        }

        GameObject FindInActiveObjectByName(string name)
        {
            Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    if (objs[i].name == name)
                    {
                        return objs[i].gameObject;
                    }
                }
            }
            return null;
        }
    }
}