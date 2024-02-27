using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CementTools.ModMenuTools
{
    public class BoolParameterUI : ParameterUI
    {
        private Toggle _toggle; // TODO: assign
        private TMP_Text _paramName; // TODO: assign

        public BoolParameterUI(IntPtr intPtr) : base(intPtr)
        {
        }

        public override string GetValue()
        {
            if (_toggle.isOn)
            {
                return "true";
            }
            return "false";
        }

        public override void SetValues(string name, string value)
        {
            if (value == "true")
            {
                _toggle.isOn = true;
            }
            else
            {   
                _toggle.isOn = false;
            }
            
            _paramName.text = name;
        }
    }
}
