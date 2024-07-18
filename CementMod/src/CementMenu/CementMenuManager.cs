#if MELONLOADER
using MelonLoader;
#endif
using UnityEngine;

namespace CementGB.Mod.CementMenu;

#if MELONLOADER
[RegisterTypeInIl2Cpp]
#endif
public class CementMenuManager : MonoBehaviour
{
#if MELONLOADER
    public CementMenuManager(IntPtr ptr) : base(ptr) { }
#endif
}