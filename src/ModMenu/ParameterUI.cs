using System;
using UnityEngine;

namespace CementTools.ModMenuTools
{
    public abstract class ParameterUI : MonoBehaviour
    {
        public ParameterUI(IntPtr ptr) : base(ptr) { }

        public abstract string GetValue();
        public abstract void SetValues(string name, string value);
    }
}