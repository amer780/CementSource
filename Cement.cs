using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Threading;
using UnityEngine.UI;
using TMPro;

[BepInPlugin("org.gangbeastsmodding.cement.blastfurnaceslag", "Cement", "2.0.0")]
public class Cement : BaseUnityPlugin 
{
    int totalFiles = 0;
    float linksDownloaded = 0;
    int modsLoaded = 0;
    bool loaded = false;

    Slider progressBar;
    TMP_Text progressText;
    
    GameObject cementGUI;

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

    private void CreateGUI()
    {
        AssetBundle bundle = AssetBundle.LoadFromFile(Path.Combine(Paths.BepInExRootPath, "plugins", "Cement", "cement"));
        cementGUI = Instantiate(bundle.LoadAsset<GameObject>("CementGUI"));
        DontDestroyOnLoad(cementGUI);
        
        Transform parent = cementGUI.transform.Find("Background").Find("LoadingBar");
        progressBar = parent.GetComponent<Slider>();
        progressText = parent.Find("Fill Area").Find("Fill").GetComponentInChildren<TMP_Text>();
    }

    private string[] GetModLinks(string directory)
    {   
        Logger.LogInfo("Getting links!");

        List<string> modLinks = new List<string>();
        foreach (string subDirectory in Directory.GetDirectories(directory))
        {
            modLinks.AddRange(GetModLinks(subDirectory));
        }

        foreach (string modPath in Directory.GetFiles(directory))
        {
            Logger.LogInfo($"Processing file {modPath}");
            foreach (string line in File.ReadLines(modPath))
            {
                Logger.LogInfo($"Processing line {line}");
                modLinks.Add(line.Replace("\n", ""));
            }
        }

        totalFiles = modLinks.Count;
        return modLinks.ToArray();
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

    private void DownloadFile(string link, string path)
    {
        float previousChange = 0;
        void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            float currentChange = e.ProgressPercentage * 0.01f;
            linksDownloaded += currentChange - previousChange;
            previousChange = currentChange;
        }

        async Task Do()
        {
            Logger.LogInfo($"{link}");
            WebClient client = new WebClient();
            
            client.DownloadProgressChanged += ProgressChanged;
            Logger.LogInfo($"Downloading {link} to {path}");
            await client.DownloadFileTaskAsync(new System.Uri(link), path);
        }

        Thread thread = new Thread(() => Do());
        thread.Start();
    }

    private void DownloadFiles()
    {
        Logger.LogInfo("Downloading files!");

        string[] mods = GetModLinks(MODS_FOLDER_PATH);
        totalFiles = mods.Length;

        foreach (string modLink in mods)
        {
            Logger.LogInfo($"Processing link {modLink}");

            string filePath = Path.Combine(MODBIN_PATH, ToUsableName(modLink));
            if (!File.Exists(filePath))
            {
                DownloadFile(modLink, filePath);
            }
            else
            {
                totalFiles--;
            }
        }
    }

    private void Awake()
    {
        CreateGUI();
        DownloadFiles();
    }

    private void Update()
    {   
        if (progressBar == null)
        {
            return;
        }

        if (!loaded)
        {
            float value = 1;
            if (totalFiles > 0)
            {
                value = linksDownloaded / (float)totalFiles;
                progressText.text = $"{Mathf.Round(value * 1000) * 0.1f}%";
                progressBar.value = Mathf.Lerp(progressBar.value, value, 20 * Time.deltaTime);
            }
            
            if (value == 1)
            {
                loaded = true;
                cementGUI.SetActive(false);
            }
        }
    }
}