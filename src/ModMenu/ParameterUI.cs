using UnityEngine;

namespace CementTools.ModMenuTools
{
    public class ParameterUI : MonoBehaviour // il2cpp monobehaviours can't be abstract (?)
    {
        public ParameterUI(IntPtr ptr) : base(ptr) { }

        public virtual string GetValue() { return ""; }
        public virtual void SetValues(string name, string value) { }
    }
}