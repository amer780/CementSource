using System.IO;
using System.Net;
using System.Collections.Generic;
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
    public event Action<string> OnRequiresMod;
    public event Action<float> OnProgress;

    private Dictionary<string, float> _percentages = new Dictionary<string, float>();
    private int _numberOfLinks;

    public ModDownloadHandler(string pathToMod)
    {
        _pathToMod = pathToMod;
    }

    private void IsConnectedToWifi(Action<bool> callback)
    {
        PersistentWebClient client = new PersistentWebClient();
        client.OnPersistentDownloadStringComplete += delegate (bool succeeded, string _)
        {
            callback.Invoke(succeeded);
        };
        client.DownloadStringPersistent("https://www.google.com");
    }

    private void GetLatestVersion(string url, Action<bool, string> callback)
    {
        if (url == null)
        {
            callback.Invoke(true, "Latest");
            return;
        }

        PersistentWebClient client = new PersistentWebClient();
        client.Proxy = null;
        client.OnPersistentDownloadStringComplete += delegate (bool succeeded, string result)
        {
            callback.Invoke(succeeded, result);
        };
        client.DownloadStringPersistent(url);
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

        string directoryName = $"{ToUsableName(author)}.{ToUsableName(name)}";
        data.directoryName = directoryName;

        Cement.Log("CHECKING IF CONNECTED TO WIFI...");
        IsConnectedToWifi(delegate (bool succeeded)
        {
            if (!succeeded)
            {
                Cement.Log("NOT CONNECTED");
                if (currentVersion != null)
                {
                    data.succeeded = true;
                }

                OnProgress(100f);
                callback.Invoke(data);
                return;
            }

            Cement.Log("CONNECTED");

            Cement.Log($"GETTING LATEST VERSION FOR {_pathToMod}...");
            GetLatestVersion(modFile.GetValue("LatestVersion"), delegate (bool succeeded2, string latestVersion2)
            {
                if (!succeeded2)
                {
                    Cement.Log("FAILED!");
                    OnProgress(100f);
                    callback.Invoke(data);
                    return;
                }
                latestVersion = latestVersion2.Replace("\n", "");
                Cement.Log($"SUCCEEDED {latestVersion}!");

                if (latestVersion != currentVersion)
                {
                    Cement.Log($"DOWNLOADING LINKS FOR MOD {_pathToMod}!");
                    DownloadLinks(modFile.GetValue("Links"), directoryName, delegate (bool succeeded3)
                    {   
                        if (succeeded3)
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
                    });
                }
                else
                {
                    OnProgress(100f);
                    data.succeeded = true;
                    callback.Invoke(data);
                }
            });
        });
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

    private void DownloadFile(string link, string path, Action<bool> callback)
    {
        PersistentWebClient client = new PersistentWebClient();

        client.Proxy = null;
        client.OnPersistentDownloadFileComplete += delegate (bool succeeded)
        {
            callback.Invoke(succeeded);
        };
        client.DownloadProgressChanged += delegate(object sender, DownloadProgressChangedEventArgs eventArgs)
        {
            Cement.Log($"PROGRESS CHANGED: {eventArgs.ProgressPercentage}");
            ProgressChanged(link, eventArgs.ProgressPercentage);
        };
        client.DownloadFilePersistent(link, path);
    }

    private void DownloadLinks(string links, string directoryName, Action<bool> callback)
    {
        string directoryPath = Path.Combine(Cement.CACHE_PATH, directoryName);

        if (Directory.Exists(directoryPath))
        {
            DirectoryExtender.DeleteFilesInDirectory(directoryPath);
        }
        Directory.CreateDirectory(directoryPath);

        string[] splitLinks = links.Split(',');
        _numberOfLinks = splitLinks.Length;
        HandleDownloadingLink(directoryPath, splitLinks, callback);
    }

    private void HandleDownloadingLink(string directoryPath, string[] links, Action<bool> callback, int i = 0)
    {
        if (i >= links.Length)
        {
            callback.Invoke(true);
            return;
        }

        string link = links[i];

        if (IsLinkToMod(link))
        {
            HandleDownloadingModFromLink(link, delegate (bool succeeded)
            {
                if (succeeded)
                {
                    HandleDownloadingLink(directoryPath, links, callback, i + 1);
                }
                else
                {
                    callback.Invoke(false);
                }
            });
        }
        else
        {
            DownloadFile(link, Path.Combine(directoryPath, GetNameFromLink(link)), delegate (bool succeeded)
            {
                if (succeeded)
                {
                    HandleDownloadingLink(directoryPath, links, callback, i + 1);
                }
                else
                {
                    callback.Invoke(false);
                }
            });
        }
    }

    private bool IsLinkToMod(string link)
    {
        string[] split = link.Split('.');
        return split[split.Length - 1] == Cement.MOD_FILE_EXTENSION;
    }

    private string GetNameFromLink(string link)
    {
        string[] split = link.Split('/');
        return ToUsableName(split[split.Length - 1]);
    }

    const string BANNED = "/<>:\"\\|?*";
    private string ToUsableName(string name)
    {
        string newName = name;
        foreach (char c in BANNED)
        {
            newName = newName.Replace(c, '_');
        }

        newName = URLManager.URLToNormal(newName);
        return newName;
    }

    private void HandleDownloadingModFromLink(string link, Action<bool> callback)
    {
        string nameFromLink = GetNameFromLink(link);
        string pathToMod = Path.Combine(Cement.HIDDEN_MODS_PATH, nameFromLink);

        bool file1Exists = File.Exists(pathToMod);
        bool file2Exists = File.Exists(Path.Combine(Cement.MODS_FOLDER_PATH, nameFromLink));

        if (file1Exists || file2Exists) // doesn't need to install the mod file if it already exists
        {
            _percentages[link] = 100f;
            callback.Invoke(true);
            return;
        }

        DownloadFile(link, pathToMod, delegate (bool succeeded)
        {
            if (succeeded)
            {
                OnRequiresMod.Invoke(pathToMod);
                callback.Invoke(true);
            }
            else
            {
                callback.Invoke(false);
            }
        });
    }
}