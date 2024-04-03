#if UNITY_EDITOR
using System;
using TMPro;

namespace CementTools.ModMenuTools
{
    public class InputFieldParameterUI : ParameterUI
    {
        public TMP_InputField _inputField;
        public TMP_Text _paramName;

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