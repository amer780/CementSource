using Il2CppInterop.Runtime.Injection;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppTMPro;

namespace CementTools.ModMenuTools
{
    public class InputFieldParameterUI : ParameterUI
    {
        public InputFieldParameterUI(IntPtr ptr) : base(ptr) { }

        public InputFieldParameterUI() : base(ClassInjector.DerivedConstructorPointer<InputFieldParameterUI>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        public TMP_InputField _inputField;
        public TMP_Text _paramName;

        private void Start()
        {
            _inputField = GetComponentInChildren<TMP_InputField>();
            _paramName = GetComponentInChildren<TMP_Text>();
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
