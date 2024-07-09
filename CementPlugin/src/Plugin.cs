using MelonLoader;
using MelonLoader.Utils;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace CementGB.Plugin;

public class Plugin : MelonPlugin
{
    public static readonly string userDataPath = Path.Combine(MelonEnvironment.UserDataDirectory, "CementGB");
    public static readonly string cementBinPath = Path.Combine(userDataPath, "bin");

    public static bool OfflineMode => _offlineModePref.GetValueAsString() == "true";


    private static readonly MelonPreferences_Category _melonCat = MelonPreferences.CreateCategory("cement_prefs", "CementGB");
    private static readonly MelonPreferences_Entry _offlineModePref = _melonCat.CreateEntry(nameof(_offlineModePref), false);

    public override async void OnPreModsLoaded()
    {
        base.OnPreModsLoaded();

        
    }

    public static async Task<bool> CheckForUpdates()
    {
        return false;
    }
}
