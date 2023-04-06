using System.IO;
using System.Net;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using CementTools;

public struct ProcessedModData
{
    public bool succeeded;
    public string name;
    public string directoryName;
    public string message;
}

public class ModDownloadHandler
{
    private string _pathToMod;
    public event Action<float> OnProgress;

    private Dictionary<string, float> _percentages = new Dictionary<string, float>();
    private int _numberOfLinks;

    public ModDownloadHandler(string pathToMod)
    {
        _pathToMod = pathToMod;
    }

    private string GetLatestVersion(string url)
    {
        if (url == null)
        {
            return "Latest";
        }
        
        try
        {   
            WebClient client = new WebClient();
            client.Proxy = null;
            return client.DownloadString(url);
        }
        catch
        {
            return null;
        }
    }

    public void Download(Action<ProcessedModData> callback)
    {
        ProcessedModData data = new ProcessedModData();
        data.succeeded = false;

        ModFile modFile = new ModFile(_pathToMod);

        string name = modFile.GetValue("Name");
        string author = modFile.GetValue("Author");

        if (name == null || author == null)
        {
            data.name = "Unknown";
            data.message = null;
            callback.Invoke(data);
            return;
        }

        string currentVersion = modFile.GetValue("CurrentVersion");
        string latestVersion;
        string modMessage = modFile.GetValue("Message");

        data.name = name;
        data.message = modMessage;

        string directoryName = $"{LinkHelper.ToUsableName(author)}.{LinkHelper.ToUsableName(name)}";
        data.directoryName = directoryName;


        Cement.Log($"GETTING LATEST VERSION FOR {_pathToMod}...");
        if (Cement.HasInternet)
        {
            latestVersion = GetLatestVersion(modFile.GetValue("LatestVersion"));
        }
        else
        {
            latestVersion = "";
        }

        if (latestVersion == null)
        {
            Cement.Log("FAILED!");
            OnProgress(100f);
            callback.Invoke(data);
            return;
        }
        latestVersion = latestVersion.Replace("\n", "");
        Cement.Log($"SUCCEEDED {latestVersion}!");

        if (latestVersion != currentVersion)
        {
            Cement.Log($"DOWNLOADING LINKS FOR MOD {_pathToMod}!");
            if (Cement.HasInternet)
            {
                bool succeeded = DownloadLinks(modFile.GetValue("Links"), directoryName);
                if (succeeded)
                {
                    Cement.Log($"SUCCEEDED!");
                    modFile.SetValue("CurrentVersion", latestVersion);
                    modFile.UpdateFile();

                    data.succeeded = true;

                    OnProgress(100f);
                    callback.Invoke(data);
                }
                else
                {
                    Cement.Log($"FAILED!");
                    OnProgress(100f);
                    callback.Invoke(data);
                }
            }
            else 
            {
                if (currentVersion != null)
                {
                    OnProgress(100f);
                    data.succeeded = true;
                    callback.Invoke(data);
                }
                else
                {
                    OnProgress(100f);
                    callback.Invoke(data);
                }
            }
        }
        else
        {
            OnProgress(100f);
            data.succeeded = true;
            callback.Invoke(data);
        }
    }

    private void ProgressChanged(string link, float percentage)
    {
        _percentages[link] = percentage;

        float totalPercentages = 0;
        foreach (float p in _percentages.Values)
        {
            totalPercentages += p;
        }

        OnProgress.Invoke(totalPercentages / _numberOfLinks);
    }

    private bool DownloadLinks(string links, string directoryName)
    {
        string directoryPath = Path.Combine(Cement.CACHE_PATH, directoryName);

        if (Directory.Exists(directoryPath))
        {
            DirectoryExtender.DeleteFilesInDirectory(directoryPath);
        }
        Directory.CreateDirectory(directoryPath);

        string[] splitLinks = links.Split(',');
        _numberOfLinks = splitLinks.Length;
        foreach (string link in splitLinks)
        {
            if (LinkHelper.IsLinkToMod(link)) // already downloaded mods so can ignore
            {
                _percentages[link] = 100f;
                continue;
            }

            bool succeeded = DownloadHelper.DownloadFile(link, Path.Combine(directoryPath, LinkHelper.GetNameFromLink(link)),
            delegate (object sender, DownloadProgressChangedEventArgs eventArgs)
            {
                Cement.Log($"PROGRESS CHANGED: {eventArgs.ProgressPercentage}");
                ProgressChanged(link, eventArgs.ProgressPercentage);
            });

            if (!succeeded)
            {
                return false;
            }
        }

        return true;
    }
}