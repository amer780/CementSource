using TMPro;
using UnityEngine;

namespace CementTools.ModMenuTools
{
    public class InputFieldParameterUI : ParameterUI
    {
        [SerializeField] private TMP_InputField _inputField;
        [SerializeField] private TMP_Text _paramName;

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
