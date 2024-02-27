using System;
using UnityEngine;

namespace CementTools
{
    [MelonLoader.RegisterTypeInIl2Cpp]
    public class CementMod : MonoBehaviour
    {
        public CementMod(IntPtr intPtr) : base(intPtr) { }

        public string modDirectoryPath;
        public ModFile modFile;
    }
}