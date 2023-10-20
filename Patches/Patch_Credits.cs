using HarmonyLib;
using System;
using System.IO;
using UnityEngine;

namespace CementTools.Patches
{
    // this is HUGE
    public static class Patch_Credits
    {
        static TextAsset textAsset = null;
        [HarmonyPatch(typeof(DisplayCredits), "Update")]
        [HarmonyPostfix]
        private static void Patch_Update(DisplayCredits __instance)
        {
            if (textAsset == null)
            {
                try
                {
                    textAsset = new TextAsset(File.ReadAllText(Path.Combine(Cement.CEMENT_PATH, "CreditsText.txt")) + "\n\n" + __instance.textFile.text);
                    __instance.textFile = textAsset;
                    __instance.Reset();
                }
                catch (UnauthorizedAccessException)
                {
                    textAsset = null;
                }
            }
        }
    }
}
