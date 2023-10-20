using HarmonyLib;
using System.IO;
using UnityEngine;

namespace CementTools.Patches
{
    public static class Patch_Credits
    {
        static TextAsset textAsset = null;
        [HarmonyPatch(typeof(DisplayCredits), "Update")]
        [HarmonyPostfix]
        private static void Patch_Update(DisplayCredits __instance)
        {
            if (textAsset == null)
            {
                textAsset = new TextAsset(File.ReadAllText(Path.Combine(Cement.CEMENT_PATH, "CreditsText.txt")) + __instance.textFile.text);
            }
            __instance.textFile = textAsset;
        }
    }
}
