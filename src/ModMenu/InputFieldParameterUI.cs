using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppTMPro;
using System;

namespace CementTools.ModMenuTools
{
    public class InputFieldParameterUI : ParameterUI
    {
        public InputFieldParameterUI(IntPtr ptr) : base(ptr) { }

        public Il2CppReferenceField<TMP_InputField> _inputField;
        public Il2CppReferenceField<TMP_Text> _paramName;

        public override string GetValue()
        {
            return _inputField.Value.text;
        }

        public override void SetValues(string name, string value)
        {
            _inputField.Value.text = value;
            _paramName.Value.text = name;
        }
    }
}
