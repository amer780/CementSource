using System.Collections.Generic;
using UnityEngine;
using BepInEx;
using System.IO;
using System.Diagnostics;
using System.Threading;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace CementTools
{
    public class CementMod : MonoBehaviour
    {
        public string modDirectoryPath;
    };

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
        bool loadedMods = false;

        int totalMods = 0;
        int totalModsProcessed = 0;

        TMP_Text summary;

        string modMessageText = "";

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

        private Dictionary<string, float> _percentages = new Dictionary<string, float>();

        private float _currentProgressBarValue;

        private const string FAILED_TAG = "<color=#B41447>";
        private const string SUCCEEDED_TAG = "<color=#4DD11D>";
        public const string MOD_FILE_EXTENSION = "cmt";

        private const string CEMENT_VERSION_URL = "https://raw.githubusercontent.com/CementGB/cementresources/main/BepInEx/plugins/Cement/version";

        public static string MODS_FOLDER_PATH
        {
            get
            {
                string path = Path.Combine(Application.dataPath, "../", "Mods");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public static string MODBIN_PATH
        {
            get
            {
                string path = Path.Combine(Application.dataPath, "modbin");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public static string CACHE_PATH
        {
            get
            {
                string path = Path.Combine(Application.dataPath, "modcache");
                if (!Directory.Exists(path))
                    Directory.CreateDirectory(path);

                return path;
            }
        }

        public static string HIDDEN_MODS_PATH
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

        public static string CEMENT_PATH
        {
            get
            {
                string path = Path.Combine(Application.dataPath, "..", "BepInEx", "plugins", "Cement");
                if (!Directory.Exists(path))
                {
                    DirectoryInfo hidden = Directory.CreateDirectory(path);
                    hidden.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                }

                return path;
            }
        }

        public void Log(object o)
        {
            Logger.LogInfo(o);
        }

        public static void Log(params object[] objects)
        {
            foreach (object o in objects)
            {
                Cement.Singleton.Log(o);
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

            _bundle.Unload(false);
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

            summary.text = summaryText + "\n" + modMessageText;

            okButton.onClick.AddListener(delegate ()
            {
                if (SceneManager.GetActiveScene().name == "Menu")
                {
                    Destroy(summaryGUI);
                }
            });
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

        private void FinishedDownloadingMod(ProcessedModData data)
        {
            if (data.succeeded)
            {
                summaryText += SUCCEEDED_TAG + $"Successfully processed {data.name}.";
                CopyCacheToBin(data.directoryName);
            }
            else
            {
                if (data.name == null)
                {
                    data.name = "incorrectly formatted mod";
                }
                summaryText += FAILED_TAG + $"Failed to process {data.name}.";
            }

            summaryText += "</color>\n";
            if (data.message != null)
            {
                modMessageText += $"<b>{data.name} message</b>:\n\n{data.message}\n\n";
            }
            totalModsProcessed++;
            UpdateProgressBar();
        }

        private void OnProgress(string mod, float percentage)
        {
            Cement.Log($"MOD {mod} IS {percentage}% DONE");
            _percentages[mod] = percentage;
            UpdateProgressBar();
        }

        private void HandleDownloadMod(string pathToMod)
        {
            totalMods++;
            ModDownloadHandler handler = new ModDownloadHandler(pathToMod);

            Cement.Log($"CREATING HANDLER FOR MOD {pathToMod}");

            handler.OnProgress += (float percentage) => OnProgress(pathToMod, percentage);
            handler.OnRequiresMod += HandleDownloadMod;
            handler.Download(FinishedDownloadingMod);
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
                    HandleDownloadMod(path);
                });
            }
        }

        private void CopyCacheToBin(string directoryName)
        {
            string cachePath = Path.Combine(CACHE_PATH, directoryName);
            string binPath = Path.Combine(MODBIN_PATH);

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
            try
            {
                DirectoryExtender.DeleteFilesInDirectory(MODBIN_PATH);
            }
            catch (Exception e)
            {
            }
        }

        private void LoadMods()
        {
            infectedWithAllSTIs = new GameObject("Infected With All STIs");
            DontDestroyOnLoad(infectedWithAllSTIs);

            string[] assemblyPaths = Directory.GetFiles(MODBIN_PATH, "*.dll");
            foreach (string path in assemblyPaths)
            {
                Logger.LogInfo(path);
                try
                {
                    Assembly assembly = Assembly.LoadFile(path);
                    foreach (AssemblyName referencedAssembly in assembly.GetReferencedAssemblies())
                    {
                        string assemblyPath = Path.Combine(MODBIN_PATH, referencedAssembly.Name + ".dll");
                        if (File.Exists(assemblyPath))
                            Assembly.LoadFile(assemblyPath);
                    }
                    foreach (Type type in assembly.GetTypes())
                    {
                        Logger.LogInfo(type.Assembly.FullName);
                        if (typeof(CementMod).IsAssignableFrom(type) || type.IsAssignableFrom(typeof(Cement)))
                        {
                            Logger.LogInfo(type);
                            try
                            {
                                CementMod item = Activator.CreateInstance(type) as CementMod;
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
                catch (Exception e)
                {
                    Logger.LogError($"Error loading assembly {path}. {e}");
                    summaryText += FAILED_TAG;
                    summaryText += $"Error occurred while loading assembly {GetFileName(path)}.";
                }
            }

            Destroy(cementGUI);
            CreateSummary();
        }

        private void InstantiateMod(CementMod mod)
        {
            infectedWithAllSTIs.AddComponent(mod.GetType());
        }

        private string GetCurrentCementVersion()
        {
            return File.ReadAllText(Path.Combine(CEMENT_PATH, "version"));
        }

        private void GetLatestCementVersion(Action<bool, string> callback)
        {
            PersistentWebClient client = new PersistentWebClient();
            client.OnPersistentDownloadStringComplete += callback;
            client.DownloadStringPersistent(CEMENT_VERSION_URL);
        }

        private void UpdateCement()
        {
            Process.Start(Path.Combine(CEMENT_PATH, "CementInstaller.exe"));
        }

        private void Awake()
        {
            _singleton = this;

            GetLatestCementVersion(delegate (bool succeeded, string latestVersion)
            {
                if (succeeded)
                {
                    if (latestVersion == GetCurrentCementVersion())
                    {
                        UpdateCement();
                        return;
                    }
                }

                try
                {
                    CreateGUI();
                }
                catch (Exception e)
                {
                    Logger.LogError($"Error while creating gui: {e}");
                }
                DownloadMods(MODS_FOLDER_PATH);
            });
        }

        private void UpdateProgressBar()
        {
            float value = 0f;
            foreach (float percentage in _percentages.Values)
            {
                value += percentage;
            }
            value /= totalMods;

            _currentProgressBarValue = value / 100f;
            progressText.text = $"{Mathf.Round(value * 10) * 0.1f}%";

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

            if (progressBar != null)
            {
                progressBar.value = Mathf.Lerp(progressBar.value, _currentProgressBarValue, 10f * Time.deltaTime);
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

            switch (linkId)
            {
                case "website":
                    Application.OpenURL("https://cementgb.github.io");
                    break;
            }
        }
    }
}