using MelonLoader;
using System;
using System.IO;
using System.Text;
using System.Net.Http;
using System.Threading.Tasks;
using Unity.Services.Wire.Internal;

namespace CementGB;

public class AutoUpdater
{
    private readonly MelonBase _itemToUpdate;

    public bool MelonHasDownloadLink => !string.IsNullOrWhiteSpace(_itemToUpdate.Info.DownloadLink);

    public AutoUpdater(MelonBase itemToUpdate)
    {
        _itemToUpdate = itemToUpdate;
    }

    public async Task UpdateItem()
    {
        if (!MelonHasDownloadLink) return;
        if (Mod.OfflineMode) return;

        var localVersion = _itemToUpdate.Info.Version;

        try
        {
            string responseBody = await Mod.updaterClient.GetStringAsync(_itemToUpdate.Info.DownloadLink);
            Melon<Mod>.Logger.Msg($"Successfully parsed string response from mod download link ({_itemToUpdate.Info.DownloadLink})");
            
        }
        catch (Exception ex)
        {
            Melon<Mod>.Logger.Error($"An error occured performing HTTP GET request to mod {_itemToUpdate.Info.Name}, download link {_itemToUpdate.Info.DownloadLink}: ", ex);
        }
    }
}
