using UnityEngine;
using UnityEngine.UI;
using CementTools.Modules.NotificationModule;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using System;
using Il2CppTMPro;

namespace CementTools.ModMenuTools
{
    // a class which handles the UI in the mod menu for a specific mod
    public class ModUI : MonoBehaviour
    {
        public ModUI(IntPtr ptr) : base(ptr) { }

        ModFile modFile;
        public Il2CppReferenceField<Transform> _parameterParent;
        public Il2CppReferenceField<Toggle> parameterToggle;
        public Il2CppReferenceField<Toggle> modFileToggle;
        public Il2CppReferenceField<TMP_Text> _name;

        public Transform GetParameterParent()
        {
            return _parameterParent;
        }

        public void SetValues(ModFile file, bool enabled, string name)
        {
            modFile = file;
            modFileToggle.Value.isOn = enabled;
            _name.Value.text = name;
        }

        // update height is used, because when toggling the mod, the UI's height doesn't automatically shrink or grow
        public void UpdateHeight()
        {
            RectTransform parentTransform = (RectTransform)(transform.parent);
            float y;
            if (_parameterParent.Value.gameObject.activeSelf)
            {
                y = 100f + 110f * _parameterParent.Value.childCount;
            }
            else
            {
                y = 100f;
            }

            parentTransform.sizeDelta = new Vector3(parentTransform.sizeDelta.x, y);
            LayoutRebuilder.ForceRebuildLayoutImmediate(parentTransform.parent as RectTransform);
        }
 
        public void ToggleParameters()
        {
            _parameterParent.Value.gameObject.SetActive(parameterToggle.Value.isOn);
            UpdateHeight();
        }

        bool turningOn;
        // tries to toggle a mod
        public void ToggleMod()
        {   
            // prevents refire when forcefully keeping a mod enabled
            if (turningOn) return;
            
            // if toggling on, it will enable all required mods
            // when toggling off it loops through all the mods dependant on it, and checks if all of them are disabled
            // before disabling it
            if (modFileToggle.Value.isOn)
            {
                bool ran = false;
                foreach (ModFile required in modFile.requiredMods)
                {
                    CementTools.Cement.Log($"REQUIRED: {required.path}");
                    if (required.GetBool("Disabled"))
                    {
                        ModMenu.Singleton.SetModActive(required, true);
                        ran = true;
                    }
                }
                if (ran)
                {
                    string modName = modFile.GetString("Name");
                    NotificationModule.Send($"{modName}", $"{modName} requires other mods in order to work, so they were automatically turned on for you. You're welcome!", 4f);
                }
            }
            else
            {
                foreach (ModFile required in modFile.requiredBy)
                {
                    if (!required.GetBool("Disabled"))
                    {
                        CementTools.Cement.Log($"REQUIRED BY: {required.path}");
                        turningOn = true;
                        modFileToggle.Value.isOn = true;
                        modFile.SetBool("Disabled", false);
                        turningOn = false;

                        string modName = modFile.GetString("Name");
                        string requiredName = required.GetString("Name");
                        NotificationModule.Send($"Can't disable {modName}", $"{modName} is required by {requiredName} in order to function correctly.", 4f);
                        return;
                    }
                }
            }
            
            modFile.SetBool("Disabled", !modFileToggle.Value.isOn);
        }
    }
}