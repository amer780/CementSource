using UnityEngine;

namespace CementTools.ModMenuTools
{
    public abstract class ParameterUI : MonoBehaviour
    {
        public abstract string GetValue();
        public abstract void SetValues(string name, string value);
    }
}