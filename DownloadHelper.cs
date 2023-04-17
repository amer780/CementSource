using System.Net;
using CementTools;
using System;
using System.Threading.Tasks;
using System.Threading;

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
            Cement.Log($"FAILED TO DOWNLOAD FILE");
            return false;
        }

        return true;
    }
}