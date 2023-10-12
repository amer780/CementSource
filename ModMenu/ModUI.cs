using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CementTools.ModMenuTools
{
    // a class which handles the UI in the mod menu for a specific mod
    public class ModUI : MonoBehaviour
    {
        ModFile modFile;
        [SerializeField] private Transform _parameterParent;
        public Toggle parameterToggle;
        public Toggle modFileToggle;
        [SerializeField] private TMP_Text _name;

        public Transform GetParameterParent()
        {
            return _parameterParent;
        }

        public void SetValues(ModFile file, bool enabled, string name)
        {
            modFile = file;
            modFileToggle.isOn = enabled;
            _name.text = name;
        }

        // update height is used, because when toggling the mod, the UI's height doesn't automatically shrink or grow
        public void UpdateHeight()
        {
            RectTransform parentTransform = (RectTransform)(transform.parent);
            float y = 0f;
            if (_parameterParent.gameObject.activeSelf)
            {
                y = 100f + 110f * _parameterParent.childCount;
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
            _parameterParent.gameObject.SetActive(parameterToggle.isOn);
            UpdateHeight();
        }

        // tries to toggle a mod
        public void ToggleMod()
        {   
            // if toggling on, it will enable all required mods
            // when toggling off it loops through all the mods dependant on it, and checks if all of them are disabled
            // before disabling it
            if (modFileToggle.isOn)
            {
                foreach (ModFile required in modFile.requiredMods)
                {
                    Cement.Log($"REQUIRED: {required.path}");
                    ModMenu.Singleton.SetModActive(required, true);
                } 
            }
            else
            {
                foreach (ModFile required in modFile.requiredBy)
                {
                    if (!required.GetBool("Disabled"))
                    {   
                        Cement.Log($"REQUIRED BY: {required.path}");
                        modFileToggle.isOn = true;
                        modFile.SetBool("Disabled", false);
                        return;
                    }
                }
            }
            
            modFile.SetBool("Disabled", !modFileToggle.isOn);
        }
    }
}