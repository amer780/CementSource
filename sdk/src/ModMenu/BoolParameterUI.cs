#if UNITY_EDITOR
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CementTools.ModMenuTools
{
    public class BoolParameterUI : ParameterUI
    {
        private Toggle _toggle;
        private TMP_Text _paramName;

        public override string GetValue()
        {
            return "";
        }

        public override void SetValues(string name, string value)
        {
            
        }
    }
}
#endif