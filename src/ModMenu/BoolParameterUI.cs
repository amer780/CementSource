using Il2CppInterop.Runtime.Injection;
using Il2CppTMPro;
using UnityEngine.UI;

namespace CementTools.ModMenuTools
{
    public class BoolParameterUI : ParameterUI
    {
        public Toggle _toggle;
        public TMP_Text _paramName;

        public BoolParameterUI(IntPtr ptr) : base(ptr)
        {
        }

        public BoolParameterUI() : base(ClassInjector.DerivedConstructorPointer<BoolParameterUI>())
        {
            ClassInjector.DerivedConstructorBody(this);
        }

        private void Start()
        {
            _paramName = GetComponentInChildren<TMP_Text>();
            _toggle = GetComponentInChildren<Toggle>();
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
