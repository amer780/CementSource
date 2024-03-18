#if UNITY_EDITOR
using System;

namespace CementTools.ModMenuTools
{
    public class InputFieldParameterUI : ParameterUI
    {
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