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
using UnityEngine.InputSystem;

namespace CementTools
{
    public class CementMod : MonoBehaviour
    {
        public string modDirectoryPath;
    };

    public struct ProcessedModData
    {
        public bool succeeded;
        public string modName;
    }


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

        int totalMods = 0;
        int totalModsProcessed = 0;
        
        TMP_Text summary;
        string summaryText = "Note: if a mod failed to process, make sure you have a good internet connection or try restarting your game." + 
        " If it still doesn't work, there is a problem with the mod file, which the mod's creator must fix.\n\n" + 
        "If you want assistance with trying to get your mods to work, go to <link=\"website\"><u>our website</u></link>, and join the discord server. " + 
        "There is a #modding-questions channel.\n\n";

        Slider progressBar;
        TMP_Text progressText;

        GameObject cementGUI;
        GameObject summaryGUI;
        GameObject infectedWithAllSTIs;

        AssetBundle _bundle;
        GameObject[] _bundleObjects;
        List<ProcessedModData> _processedMods = new List<ProcessedModData>();

        private const string FAILED_TAG = "<color=#B41447>";
        private const string SUCCEEDED_TAG = "<color=#4DD11D>";
        public const string MOD_FILE_EXTENSION = "cmt";

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

        public string HIDDEN_MODS_PATH
        {
            get
            {
                string path = Path.Combine(MODS_FOLDER_PATH, ".hidden mods");
                if (!Directory.Exists(path))
                {
                    DirectoryInfo hidden = Directory.CreateDirectory(path);
                    hidden.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }

                return path;
            }
        }

        private void CreateGUI()
        {
            _bundle = AssetBundle.LoadFromFile(Path.Combine(Paths.BepInExRootPath, "plugins", "Cement", "cement"));
            cementGUI = Instantiate(_bundle.LoadAsset<GameObject>("CementLoadingScreen"));
            summaryGUI = Instantiate(_bundle.LoadAsset<GameObject>("CementSummaryCanvas"));
            DontDestroyOnLoad(cementGUI);

            Transform parent = cementGUI.transform.Find("Background").Find("LoadingBar");
            progressBar = parent.GetComponent<Slider>();
            progressText = parent.Find("Fill Area").GetComponentInChildren<TMP_Text>();

            progressText.text = "0%";
            progressBar.value = 0f;
        }

        private void CreateSummary()
        {
            if (summaryGUI == null)
            {
                return;
            }

            summaryGUI.SetActive(true);

            DontDestroyOnLoad(summaryGUI);

            Transform background = summaryGUI.transform.Find("Scroll View");
            summary = background.Find("Viewport/Content").GetComponent<TMP_Text>();
            Button okButton = background.Find("OK").GetComponent<Button>();

            summary.text = summaryText;

            okButton.onClick.AddListener(delegate()
            {
                Destroy(summaryGUI);
            });
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
                newName = newName.Replace(c, '_');
            }

            Logger.LogInfo($"Usable name: {newName}");
            return newName;
        }

        private async Task<bool> DownloadFile(string link, string path)
        {
            float previousChange = 0;
            void ProgressChanged(object sender, DownloadProgressChangedEventArgs e)
            {
                float currentChange = e.ProgressPercentage * 0.01f;
                downloadProgress += currentChange - previousChange;
                previousChange = currentChange;

                if (totalMods > 0)
                    UpdateProgressBar();
            }

            bool succeeded = true;
            try
            {
                WebClient client = new WebClient();

                client.Proxy = null;
                client.DownloadProgressChanged += ProgressChanged;
                await client.DownloadFileTaskAsync(new System.Uri(link), path);
                client.Dispose();
            }
            catch
            {
                succeeded = false;
            }

            linksDownloaded++;
            return succeeded;
        }

        private string GetNameFromLink(string link)
        {
            string[] split = link.Split('/');
            return ToUsableName(split[split.Length - 1]);
        }

        private string GetFileName(string path)
        {
            string fileName = "";
            foreach (char c in path)
            {
                if (c == '\\' || c == '/')
                {
                    fileName = "";
                }
                else
                {
                    fileName += c;
                }
            }

            return fileName;
        }

        private bool IsLinkToMod(string link)
        {
            string[] split = link.Split('.');
            return split[split.Length - 1] == MOD_FILE_EXTENSION;
        }

        private void DeleteFilesInDirectory(string path)
        {
            foreach (string sub in Directory.GetDirectories(path))
            {
                DeleteFilesInDirectory(sub);
            }

            foreach (string file in Directory.EnumerateFiles(path))
            {
                File.Delete(file);
            }
        }

        private async void HandleDownloadMod(string pathToMod, bool copyCacheToBin)
        {
            ProcessedModData data = await DownloadMod(pathToMod, copyCacheToBin);
            if (data.succeeded)
            {
                summaryText += SUCCEEDED_TAG + $"Successfully processed {data.modName}.";
            }
            else
            {
                if (data.modName == null)
                {
                    data.modName = "incorrectly formatted mod";
                }
                summaryText += FAILED_TAG + $"Failed to process {data.modName}.";
            }

            summaryText += "\n";
            totalModsProcessed++;
            UpdateProgressBar();
        }

        private async Task<bool> DownloadLinks(string links, string directoryName)
        {
            string directoryPath = Path.Combine(CACHE_PATH, directoryName);

            if (Directory.Exists(directoryPath))
            {
                DeleteFilesInDirectory(directoryPath);
            }
            else
            {
                Directory.CreateDirectory(directoryPath);
            }

            string[] splitLinks = links.Split(',');
            totalFiles += splitLinks.Length;

            foreach (string link in splitLinks)
            {
                if (IsLinkToMod(link))
                {
                    string pathToMod = Path.Combine(HIDDEN_MODS_PATH, GetNameFromLink(link));
                    bool fileExists = File.Exists(pathToMod);
                    bool succeeded = await DownloadFile(link, pathToMod);
                    if (!succeeded)
                    {
                        return false;
                    }
                    HandleDownloadMod(pathToMod, !fileExists);
                }
                else
                {
                    bool succeeded = await DownloadFile(link, Path.Combine(directoryPath, GetNameFromLink(link)));
                    if (!succeeded)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        private async Task<ProcessedModData> DownloadMod(string path, bool copyCacheToBin)
        {
            totalMods++;
            bool succeeded = true;
            string name = null;
            string directoryName = "";

            try
            {
                ModFile modFile = new ModFile(path);
                name = modFile.GetValue("Name");
                string currentVersion = modFile.GetValue("CurrentVersion");
                string latestVersion = "Latest";

                directoryName = $"{ToUsableName(modFile.GetValue("Author"))}.{ToUsableName(name)}";

                if (modFile.GetValue("LatestVersion") != null)
                {
                    latestVersion = GetLatestVersion(modFile.GetValue("LatestVersion")).Replace("\n", "");
                }

                if (latestVersion != currentVersion)
                {
                    succeeded = await DownloadLinks(modFile.GetValue("Links"), directoryName);
                    if (succeeded)
                    {
                        if (latestVersion != null)
                        {
                            modFile.SetValue("CurrentVersion", latestVersion);
                        }
                        else
                        {
                            modFile.SetValue("CurrentVersion", "Latest");
                        }

                        modFile.UpdateFile();
                    }
                }
            }
            catch
            {
                succeeded = false;
            }

            if (copyCacheToBin && succeeded)
            {
                try
                {
                    CopyCacheToBin(directoryName);
                }
                catch (Exception e)
                {
                    Logger.LogError($"Error occurred trying to copy cache: {e}");
                }
            }

            return new ProcessedModData() { succeeded = succeeded, modName = name };
        }

        private void DownloadMods(string directory)
        {
            foreach (string subDirectory in Directory.GetDirectories(directory))
            {
                DownloadMods(subDirectory);
            }

            foreach (string path in Directory.GetFiles(directory, $"*.{MOD_FILE_EXTENSION}"))
            {
                ThreadPool.QueueUserWorkItem(delegate (object data)
                {
                    HandleDownloadMod(path, true);
                });
            }
        }

        private void CopyCacheToBin(string directoryName)
        {
            string cachePath = Path.Combine(CACHE_PATH, directoryName);
            string binPath = Path.Combine(MODBIN_PATH, directoryName);


            if (!Directory.Exists(cachePath))
            {
                return;
            }

            if (!Directory.Exists(binPath))
            {
                Directory.CreateDirectory(binPath);
            }

            foreach (string path in Directory.GetFiles(cachePath))
            {
                string name = GetFileName(path);
                string binFilePath = Path.Combine(binPath, name);

                if (File.Exists(binFilePath))
                    File.Delete(binFilePath);

                File.Copy(Path.Combine(cachePath, name), binFilePath);
            }
        }

        private void OnApplicationQuit()
        {
            DeleteFilesInDirectory(MODBIN_PATH);
        }

        private void LoadMods()
        {
            infectedWithAllSTIs = new GameObject("Infected With All STIs");
            DontDestroyOnLoad(infectedWithAllSTIs);

            foreach (string subDirectory in Directory.GetDirectories(MODBIN_PATH))
            {
                string[] assemblyPaths = Directory.GetFiles(subDirectory, "*.dll");
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
                                    item.modDirectoryPath = subDirectory;
                                    InstantiateMod(item);
                                    Logger.LogInfo($"Succesfully loaded {type.Name}.");
                                }
                                catch (Exception e)
                                {
                                    Logger.LogError($"Error occurred while loading {type.Name}: {e}");
                                    summaryText += FAILED_TAG;
                                    summaryText += $"Error occurred while loading {type.Name}: {e}\n";
                                }
                            }
                        }
                    }
                    catch
                    {
                        Logger.LogError($"Error loading assembly {path}.");
                        summaryText += FAILED_TAG;
                        summaryText += $"Error occurred while loading assembly {GetFileName(path)}.";
                    }
                }
            }

            Destroy(cementGUI);
            CreateSummary();
        }

        private void InstantiateMod(CementMod mod)
        {
            Logger.LogInfo("Instantiating mod.");
            infectedWithAllSTIs.AddComponent(mod.GetType());
        }

        private void Awake()
        {
            try
            {
                CreateGUI();
            }
            catch (Exception e)
            {
                Logger.LogError($"Error while creating gui: {e}");
            }
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

            if (!loadedMods && totalModsProcessed == totalMods)
            {
                loadedMods = true;
                Logger.LogInfo("All links downloaded!");
                LoadMods();
            }
        }

        private void Update()
        {   
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleClickingLinks();
            }
        }

        private void HandleClickingLinks()
        {
            if (summary == null)
            {
                return;
            }

            int linkIndex = TMP_TextUtilities.FindIntersectingLink(summary, Mouse.current.position.ReadValue(), Camera.current);
            if (linkIndex == -1)
            {
                return;
            }

            string linkId = summary.textInfo.linkInfo[linkIndex].GetLinkID();

            switch(linkId)
            {
                case "website":
                    Application.OpenURL("https://cementgb.github.io");
                    break;
            }
        }
    }
}