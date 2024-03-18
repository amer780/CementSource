#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

namespace CementTools.ModMenuTools
{
    // a class which handles the UI in the mod menu for a specific mod
    public class ModUI : MonoBehaviour
    {
        public Transform _parameterParent;
        public Toggle parameterToggle;
        public Toggle modFileToggle;
        public TMP_Text _name;

        public Transform GetParameterParent()
        {
            return null;
        }

        public void UpdateHeight()
        {
            
        }
 
        public void ToggleParameters()
        {
            
        }

        // tries to toggle a mod
        public void ToggleMod()
        {   
            
        }
    }
}
#endif