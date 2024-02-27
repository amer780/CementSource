using System;
using TMPro;
using UnityEngine;

namespace CementTools.ModMenuTools
{
    public class InputFieldParameterUI : ParameterUI
    {
        private TMP_InputField _inputField; // TODO: assign
        private TMP_Text _paramName; // TODO: assign

        public InputFieldParameterUI(IntPtr intPtr) : base(intPtr)
        {
        }

        public override string GetValue()
        {
            return _inputField.text;
        }

        public override void SetValues(string name, string value)
        {
            _inputField.text = value;
            _paramName.text = name;
        }
    }
}
