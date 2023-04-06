using System.Net;
using CementTools;
using System;

public static class DownloadHelper
{
    public static bool DownloadFile(string link, string path, DownloadProgressChangedEventHandler progressChanged)
    {
        WebClient client = new WebClient();

        client.Proxy = null;
        client.DownloadProgressChanged += progressChanged;

        try
        {
            client.DownloadFile(link, path);
        }
        catch
        {
            Cement.Log($"FAILED TO DOWNLOAD FILE");
            return false;
        }

        return true;
    }
}