using Il2CppInterop.Runtime.InteropTypes.Fields;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CementTools.ModMenuTools
{
    public class BoolParameterUI : ParameterUI
    {
        private Il2CppReferenceField<Toggle> _toggle;
        private Il2CppReferenceField<TMP_Text> _paramName;

        public BoolParameterUI(IntPtr ptr) : base(ptr)
        {
        }

        public override string GetValue()
        {
            if (_toggle.Value.isOn)
            {
                return "true";
            }
            return "false";
        }

        public override void SetValues(string name, string value)
        {
            if (value == "true")
            {
                _toggle.Value.isOn = true;
            }
            else
            {   
                _toggle.Value.isOn = false;
            }
            
            _paramName.Value.text = name;
        }
    }
}
