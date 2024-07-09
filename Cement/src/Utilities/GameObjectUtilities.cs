using MelonLoader;
using System;
using UnityEngine;

namespace CementGB.Mod.Utilities;

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
