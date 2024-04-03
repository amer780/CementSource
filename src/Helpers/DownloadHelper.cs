using System.Net;

namespace CementTools.Helpers;

public static class DownloadHelper
{
    public static async Task<bool> DownloadFile(string link, string path, DownloadProgressChangedEventHandler progressChanged)
    {
        WebClient client = new WebClient();
        client.DownloadProgressChanged += progressChanged;

        try
        {
            await client.DownloadFileTaskAsync(new Uri(link), path);
        }
        catch
        {
            CementTools.Cement.Log($"FAILED TO DOWNLOAD FILE");
            return false;
        }

        return true;
    }
}