using System.Net;
using System;
using System.Threading.Tasks;

public static class DownloadHelper
{
    public static async Task<bool> DownloadFile(string link, string path, DownloadProgressChangedEventHandler progressChanged)
    {
        WebClient client = new WebClient();

        client.Proxy = null;
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