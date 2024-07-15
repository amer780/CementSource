using HarmonyLib;
using Il2CppGB.UI.Utils.Settings;
using Il2CppTMPro;
using UnityEngine;
using UnityEngine.Localization.Components;
using UnityEngine.UI;

namespace CementGB.Mod.Patches;

public static class RootSettingsMenuPatch
{
    private static bool _onEnableExecuted = false;

    [HarmonyPatch(typeof(OptionsMenu), nameof(OptionsMenu.OnEnable))]
    private static class OnEnable
    {
        private static void Postfix(OptionsMenu __instance)
        {
            var castedInstance = __instance.TryCast<RootSettingsMenu>();

            if (_onEnableExecuted || castedInstance == null) return;
            _onEnableExecuted = true;

            var inputButton = castedInstance.transform.Find("Input");
            var cementButton = Object.Instantiate(inputButton.gameObject, castedInstance.transform);

            cementButton.name = "CementMenuButton";
            cementButton.GetComponent<TextMeshProUGUI>().text = "Cement";

            Object.Destroy(cementButton.GetComponent<LocalizeStringEvent>());

            // Remove click events
            var cementButtonComp = cementButton.GetComponent<Button>();
            cementButtonComp.onClick = new Button.ButtonClickedEvent();

            // TODO: add custom events for in-game menu

            cementButton.transform.position = new Vector3(8.85f, -3.2669f, -0.11f);
        }
    }
}
