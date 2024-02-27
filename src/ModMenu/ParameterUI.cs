using System;
using UnityEngine;

namespace CementTools.ModMenuTools
{
    [MelonLoader.RegisterTypeInIl2Cpp]
    public abstract class ParameterUI : MonoBehaviour
    {
        public ParameterUI(IntPtr intPtr) : base(intPtr) { }

        public abstract string GetValue();
        public abstract void SetValues(string name, string value);
    }
}