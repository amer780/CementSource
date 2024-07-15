using CementGB.Mod.Utilities;
using Il2CppInterop.Runtime;
using MelonLoader;
using MelonLoader.Utils;
using System.IO;
using UnityEngine;

namespace CementGB.Mod;

public static class BuildInfo
{
    public const string Name = "Cement";
    public const string Author = "HueSamai // dotpy";
    public const string Description = null;
    public const string Company = "CementGB";
    public const string Version = "4.0.0";
    public const string DownloadLink = "https://api.github.com/repos/HueSamai/CementSource/releases/latest";
}

public class Mod : MelonMod
{
    public static readonly string userDataPath = Path.Combine(MelonEnvironment.UserDataDirectory, "CementGB");

    internal static GameObject? cementCompContainer;

    private static readonly MelonPreferences_Category _melonCat = MelonPreferences.CreateCategory("cement_prefs", "CementGB");
    private static readonly MelonPreferences_Entry _offlineModePref = _melonCat.CreateEntry(nameof(_offlineModePref), false);

    public override void OnInitializeMelon()
    {
        base.OnInitializeMelon();

        Directory.CreateDirectory(userDataPath);
        _melonCat.SetFilePath(Path.Combine(userDataPath, "CementPrefs.cfg"));

        CreateCementComponents();
    }

    private static void CreateCementComponents()
    {
        if (cementCompContainer != null) return;

        cementCompContainer = new("CementGB");
        Object.DontDestroyOnLoad(cementCompContainer);
        cementCompContainer.hideFlags = HideFlags.DontUnloadUnusedAsset;

        // Attach Cement MonoBehaviours
        cementCompContainer.AddComponent(Il2CppType.Of<GameObjectUtilities>());
    }
}