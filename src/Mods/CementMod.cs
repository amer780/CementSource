using UnityEngine;

namespace CementTools
{
    public class CementMod : MonoBehaviour
    {
        public CementMod(IntPtr ptr) : base(ptr) { }

        public string modDirectoryPath;
        public ModFile modFile;
    }
}