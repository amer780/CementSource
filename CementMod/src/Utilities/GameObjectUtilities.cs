using MelonLoader;
using System;
using UnityEngine;

namespace CementGB.Mod.Utilities;

[RegisterTypeInIl2Cpp]
public class GameObjectUtilities : MonoBehaviour
{
    public GameObjectUtilities(IntPtr ptr) : base(ptr) { }

    /// <summary>
    /// The Singleton for this utility script. Make sure to always check if its null first before doing anything!
    /// </summary>
    public static GameObjectUtilities? Instance { get; private set; }

    public void Awake()
    {
        if (Instance != null) Destroy(Instance);

        Instance = this;
    }
}
