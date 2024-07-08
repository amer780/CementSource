using HarmonyLib;
using Il2CppGB.UI.Utils.Settings;
using UnityEngine;

namespace CementGB.Patches;

public static class RootSettingsMenuPatch
{
    private static GameObject? cmtMenuOpenButton;

    [HarmonyPatch(typeof(RootSettingsMenu), nameof(RootSettingsMenu.OnEnable))]
    [HarmonyPostfix]
    public static void OnEnable(RootSettingsMenu __instance)
    {
        
    }

    [HarmonyPatch(typeof(RootSettingsMenu), nameof(RootSettingsMenu.OnDisable))]
    [HarmonyPostfix]
    public static void OnDisable(RootSettingsMenu __instance)
    {

    }
}
