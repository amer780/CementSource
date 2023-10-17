using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace CementTools.ModMenuTools
{
    public class BoolParameterUI : ParameterUI
    {
        [SerializeField] private Toggle _toggle;
        [SerializeField] private TMP_Text _paramName;
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
