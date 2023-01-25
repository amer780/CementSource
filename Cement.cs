using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System;
using System.ComponentModel;

public class CementMod : MonoBehaviour {};

[BepInPlugin("org.gangbeastsmodding.cement.blastfurnaceslag", "Cement", "3.0.0")]
public class Cement : BaseUnityPlugin 
{
    public static Cement Singleton
    {
        get
        {
            return _singleton;
        }
    }

    private static Cement _singleton;

    int totalFiles = 0;
    float downloadProgress = 0;
    int linksDownloaded = 0;
    bool downloadedFiles = false;
    bool loadedMods = false;

    Slider progressBar;
    TMP_Text progressText;
    
    GameObject cementGUI;

    GameObject infectedWithAllSTIs;

    private string MODS_FOLDER_PATH
    {
        get
        {   
            string path = Path.Combine(Application.dataPath, "../", "Mods");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
    
            return path;
        }
    }

    public string MODBIN_PATH
    {
        get
        {
            string path = Path.Combine(Application.dataPath, "modbin");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
    
            return path;
        }
    }

    public string CACHE_PATH
    {
        get
        {
            string path = Path.Combine(Application.dataPath, "modcache");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
    
            return path;
        }
    }

    private void CreateGUI()
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Paths.BepInExRootPath, "plugins", "Cement", "cement"));
        cementGUI = Instantiate(bundle.LoadAsset<GameObject>("CementGUI"));
        DontDestroyOnLoad(cementGUI);
        
        Transform parent = cementGUI.transform.Find("Background").Find("LoadingBar");
        progressBar = parent.GetComponent<Slider>();
        progressText = parent.Find("Fill Area").GetComponentInChildren<TMP_Text>();

        progressText.text = "0%";
        progressBar.value = 0f;
    }

    private string GetLatestVersion(string url)
    {
        WebClient client = new WebClient();
        client.Proxy = null;
        string latestVersion = client.DownloadString(url);
        client.Dispose();

        return latestVersion;
    }

    const string BANNED = "/<>:\"\\|?*";
    private string ToUsableName(string name)
    {
        string newName = name;
        foreach (char c in BANNED)
        {
           newName  = newName.Replace(c, '_');
        }

        Logger.LogInfo($"Usable name: {newName}");
        return newName;
    }

    private async Task DownloadFile(string link, string path)
    {
        float previousChange = 0;
        void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            float currentChange = e.ProgressPercentage * 0.01f;
            downloadProgress += currentChange - previousChange;
            previousChange = currentChange;

            UpdateProgressBar();
        }

        WebClient client = new WebClient();
            
        client.Proxy  = null;
        client.DownloadProgressChanged += ProgressChanged;
        await client.DownloadFileTaskAsync(new System.Uri(link), path);
        client.Dispose();

        linksDownloaded++;
    }

    private string GetNameFromLink(string link)
    {
        string[] split = ToUsableName(link).Split('/');
        return split[split.Length - 1];
    }

    private void DeleteFilesInDirectory(string path)
    {
        foreach(string file in Directory.EnumerateFiles(path))
        {
            File.Delete(file);
        }
    }

    private async Task DownloadLinks(string links, string modName)
    {
        string directoryPath = Path.Combine(CACHE_PATH, modName);

        if (Directory.Exists(directoryPath))
        {
            DeleteFilesInDirectory(directoryPath);
        }
        else
        {
            Directory.CreateDirectory(directoryPath);
        }
            

        string usableName = ToUsableName(modName);
        string[] splitLinks = links.Split(',');
        totalFiles += splitLinks.Length;

        foreach (string link in splitLinks)
        {
            await DownloadFile(link, Path.Combine(directoryPath, GetNameFromLink(link)));
        }
    }

    private async void DownloadMod(string path)
    {
        ModFile modFile = new ModFile(path);
        string name = modFile.GetValue("Name");
        string currentVersion = modFile.GetValue("CurrentVersion");
        string latestVersion = GetLatestVersion(modFile.GetValue("LatestVersion")).Replace("\n", "");
        
        if (latestVersion != currentVersion)
        {
            Logger.LogInfo($"Latest version not installed for {name}");
            await DownloadLinks(modFile.GetValue("Links"), name)
            .ContinueWith(delegate(Task task)
            {
                if (task.Status == TaskStatus.RanToCompletion)
                {
                    modFile.SetValue("CurrentVersion", latestVersion.Replace("\n", ""));
                    modFile.UpdateFile();
                }
            });
        }

        UpdateProgressBar();
    }

    private void DownloadMods(string directory)
    {   
        foreach (string subDirectory in Directory.GetDirectories(directory))
        {
            DownloadMods(subDirectory);
        }

        foreach (string path in Directory.GetFiles(directory))
        {
            ThreadPool.QueueUserWorkItem(delegate(object data)
            {
                Logger.LogInfo("Starting to download mod.");
                DownloadMod(path);
            });
        }
    }

    private void CopyFolderToModBin(string name)
    {
        File.Copy(Path.Combine(CACHE_PATH, name), Path.Combine(MODBIN_PATH, name));
    }

    private void OnApplicationQuit()
    {
        DeleteFilesInDirectory(MODBIN_PATH);
    }

    private void LoadMods()
    {
        infectedWithAllSTIs = new GameObject("Infected With All STIs");
        DontDestroyOnLoad(infectedWithAllSTIs);

        string[] assemblyPaths = Directory.GetFiles(MODBIN_PATH, "*.dll");

        foreach (string path in assemblyPaths)
        {
            try
            {
			    Assembly assembly = Assembly.LoadFile(path);
			    foreach (Type type in assembly.GetTypes())
			    {
				    if (typeof(CementMod).IsAssignableFrom(type))
				    {
					    try
					    {
						    CementMod item = Activator.CreateInstance(type) as CementMod;
                            InstantiateMod(item);
                            Logger.LogInfo($"Succesfully loaded {type.Name}.");
					    }
					    catch (Exception e)
					    {
						    Logger.LogError($"Error occurred while loading {type.Name}: {e}");
                        }
                    }
                }
            }
            catch
            {
                Logger.LogError($"Error loading assembly {path}.");
            }
		}

        cementGUI.SetActive(false);
    }

    private void InstantiateMod(CementMod mod)
    {
        Logger.LogInfo("Instantiating mod.");
        infectedWithAllSTIs.AddComponent(mod.GetType());
    }

    private void Awake()
    {
        CreateGUI();
        DownloadMods(MODS_FOLDER_PATH);
    }

    private void UpdateProgressBar()
    {
        Logger.LogInfo($"{linksDownloaded}/{totalFiles}");
        if (totalFiles > 0)
        {
            float value = downloadProgress / (float)totalFiles;
            progressText.text = $"{Mathf.Round(value * 1000) * 0.1f}%";
            progressBar.value = Mathf.Lerp(progressBar.value, value, 0.2f);
        }
        else
        {
            progressText.text = "100%";
            progressBar.value = 1f;
        }
            
        if (linksDownloaded == totalFiles)
        {
            Logger.LogInfo("All links downloaded!");
            LoadMods();
        }
    }
}