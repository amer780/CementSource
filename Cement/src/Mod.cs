using MelonLoader;
using MelonLoader.Utils;
using System.IO;
using System.Net.Http;

namespace CementGB;

public static class BuildInfo
{
    public const string Name = "Cement";
    public const string Author = "HueSamai // dotpy";
    public const string Description = null;
    public const string Company = "CementGB";
    public const string Version = "4.0.0";
    public const string DownloadLink = "https://api.github.com/repos/HueSamai/CementSource/releases/";
}

public class Mod : MelonPlugin
{
    public static readonly string userDataPath = Path.Combine(MelonEnvironment.UserDataDirectory, "CementGB");

    public static bool OfflineMode => _offlineModePref.GetValueAsString() == "true";

    internal static readonly HttpClient updaterClient = new();

    private static readonly MelonPreferences_Category _melonCat = MelonPreferences.CreateCategory("cement_prefs", "CementGB");
    private static readonly MelonPreferences_Entry _offlineModePref = _melonCat.CreateEntry(nameof(_offlineModePref), false); 

    public override void OnPreInitialization()
    {
        base.OnPreInitialization();

        
    }

    public override void OnInitializeMelon()
    {
        base.OnInitializeMelon();

        Directory.CreateDirectory(userDataPath);
    }

    public override void OnPreModsLoaded()
    {
        base.OnPreModsLoaded();

        
    }
}