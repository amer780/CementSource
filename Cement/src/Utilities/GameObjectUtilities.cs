using MelonLoader;
using UnityEngine;

namespace CementGB.Utilities;

[RegisterTypeInIl2Cpp]
public class GameObjectUtilities : MonoBehaviour
{
    public GameObjectUtilities(IntPtr ptr) : base(ptr) { }

    public static GameObjectUtilities? Instance { get; private set; }

    public void Awake()
    {
        if (Instance != null) Destroy(Instance);

        Instance = this;
    }
}
