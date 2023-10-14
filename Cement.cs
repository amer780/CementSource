using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using BepInEx;
using System.IO;
using System.Diagnostics;
using System.Net;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using CementTools.ModMenuTools;
using UnityEngine.EventSystems;

namespace CementTools
{
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

        public static bool HasInternet
        {
            get
            {
                return _hasInternet;
            }
        }

        private static bool _hasInternet;

        private static Cement _singleton;

        bool loadedMods = false;

        public int totalMods = 0;
        int totalModsProcessed = 0;

        TMP_Text summary;

        string modMessageText = "";

        string summaryText = $"{FAILED_TAG}<b>IMPORTANT! PLEASE READ!</b></color>\n\nNote: if a mod failed to process, make sure you have a good internet connection or try restarting your game." +
        " If it still doesn't work open the 'Cement' menu from the main menu and clear your cache. If that doesn't work then there is a problem with the mod file, which the mod's creator must fix.\n\n" +
        "If you want assistance with trying to get your mods to work, go to <link=\"website\"><u>our website</u></link>, and join the discord server. " +
        "There is a #modding-questions channel.\n\nAlso everything the Cement Team does is for free. All of us are students or work full time, so we would be really grateful "
        + "if you bought us a coffee, with the following link: <link=\"coffee\"><u>CLICK ME!</u></link>.\n\n";

        Slider progressBar;
        TMP_Text progressText;

        GameObject cementGUI;
        GameObject summaryGUI;
        
        AssetBundle _bundle;

        private Dictionary<string, float> _percentages = new Dictionary<string, float>();
        private Dictionary<string, ModFile> _nameToModFile = new Dictionary<string, ModFile>();

        private float _currentProgressBarValue;

        public const string FAILED_TAG = "<color=#B41447>";
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

        private EventSystem _oldEventSystem;
        private EventSystem _cementEventSystem;
        private bool _usingCementEventSystem;
        private Navigation cementNav;

        // this is where the main processing happens, so look here to see how Cement works.
        private void Awake()
        {
            _singleton = this;
            DontDestroyOnLoad(_singleton);

            _hasInternet = IsConnectedToWifi();
            Cement.Log($"IS CONNECTED TO WIFI? {_hasInternet}");
            SceneManager.sceneLoaded += OnSceneLoaded;

            try
            {
                IOExtender.DeleteFilesInDirectory(MODBIN_PATH);
            }
            catch (Exception e)
            {
                // yowser
            }

            LoadCement();
        }

        private void OnDestroy()
        {
            Logger.LogError($"GETTING DESTROYED {transform.parent.name}");
        }

        private void LoadCement()
        {
            GetLatestCementVersion(delegate (bool succeeded, string latestVersion)
            {
                Cement.Log($"LATEST CEMENT VERSION: {latestVersion}");
                if (succeeded)
                {
                    if (latestVersion != GetCurrentCementVersion())
                    {
                        UpdateCement();
                        return;
                    }
                }

                DeleteTempInstaller();

                try
                {
                    CreateGUI();
                }
                catch (Exception e)
                {
                    Logger.LogError($"Error while creating gui: {e}");
                }
                if (ModsPresent())
                {
                    DownloadManager.DownloadAllModFiles(delegate (bool succeeded2)
                    {
                        Cement.Log($"DONE DOWNLOADING ALL MODS. DID SUCCEED? {succeeded2}");
                        if (succeeded2)
                        {
                            DownloadManager.DownloadMods(MODS_FOLDER_PATH);
                        }
                        else
                        {
                            summaryText += $"\n\n{FAILED_TAG}Failed to download all requireds mods. Try restarting your game, or make sure you have a good internet connection.</color>\n\n";
                            Destroy(cementGUI);
                            LoadAllMods();
                        }
                    });
                }
                else
                {
                    
                }
            });
        }

        private void LoadAllMods()
        {
            Destroy(cementGUI);
            ModLoader.LoadAllMods();
        }

        public ModFile GetModFileFromName(string name)
        {
            return _nameToModFile[name];
        }

        public void AddToSummary(string text)
        {
            summaryText += text;
        }

        public void UseCementEventSystem()
        {
            _usingCementEventSystem = true;
        }

        public void RevertEventSystem()
        {
            _usingCementEventSystem = false;
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

        private void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
        {
            if (scene.name == "Menu")
            {
                Cement.Log("Spawned cement button is false");
                StartCoroutine(WaitUntilTextSectionExists(SpawnInCementButton));
            }
        }

        private IEnumerator WaitUntilTextSectionExists(Action callback)
        {
            Cement.Log("Waiting for TextSection...");
            yield return new WaitUntil(() => GameObject.Find("TextSection") != null);
            GameObject g = GameObject.Find("TextSection");
            Cement.Log("Waiting for Local...");
            yield return new WaitUntil(() => GameObject.Find("TextSection").transform.Find("Local") != null);
            Cement.Log("Found Local!");
            callback.Invoke();
        }

        private void SpawnInCementButton()
        {
            Cement.Log("SPAWNING IN CEMENT BUTTON");
            Transform textSection = GameObject.Find("TextSection").transform;
            if (textSection == null)
            {
                Cement.Log("TEXT SELECTION IS NULL");
            }
            Cement.Log("TEXT SECTION IS NOT NULL");

            textSection.localPosition += new Vector3(0, 10f, 0);

            GameObject localButton = textSection.Find("Local").gameObject;
            GameObject creditsButton = textSection.Find("Credits").gameObject;
            GameObject quitButton = textSection.Find("Quit").gameObject;

            Cement.Log("LOCAL BUTTON!");
            GameObject cementButton = Instantiate(localButton, textSection);

            cementButton.name = "Cement";
            cementButton.GetComponent<TMP_Text>().text = "Cement";
            cementButton.transform.SetSiblingIndex(textSection.childCount - 4);

            Button button = cementButton.GetComponent<Button>();

            FieldInfo onClickForButton = typeof(Button).GetField("m_OnClick", BindingFlags.Instance | BindingFlags.NonPublic);
            Button.ButtonClickedEvent onClickedEvent = new Button.ButtonClickedEvent();
            onClickForButton.SetValue(button, onClickedEvent);

            button.onClick.AddListener(ModMenu.Singleton.Enable);

            // fix navigation
            cementNav = cementButton.GetComponent<Button>().navigation;
            Cement.Log("GOT CEMENT NAV");
            Navigation creditsNav = creditsButton.GetComponent<Button>().navigation;
            Cement.Log("GOT CREDITS NAV");
            Navigation quitNav = quitButton.GetComponent<Button>().navigation;
            Cement.Log("GOT QUIT NAV");

            creditsNav.selectOnDown = button;
            quitNav.selectOnUp = button;
            cementNav.selectOnUp = creditsButton.GetComponent<Button>();
            cementNav.selectOnDown = quitButton.GetComponent<Button>();

            Cement.Log("CHANGED NAVIGATION!");
        }

        private void CreateEventSystem()
        {
            try
            {
                GameObject mainObject = Instantiate(_bundle.LoadAsset<GameObject>("EventSystem"));
                _cementEventSystem = mainObject.GetComponent<EventSystem>();
                InputSystemUIInputModule inputModule = mainObject.GetComponent<InputSystemUIInputModule>();

                FieldInfo currentModule = typeof(EventSystem).GetField("m_CurrentInputModule", BindingFlags.NonPublic | BindingFlags.Instance);
                currentModule.SetValue(_cementEventSystem, inputModule);

                _cementEventSystem.enabled = true;

                DontDestroyOnLoad(mainObject);
            }
            catch (Exception e)
            {
                Cement.Log($"ERROR WHILE CREATING EVENT SYSTEM: {e}");
            }
        }

        private void CreateGUI()
        {
            _bundle = AssetBundle.LoadFromFile(Path.Combine(Paths.BepInExRootPath, "plugins", "Cement", "cement"));
            cementGUI = Instantiate(_bundle.LoadAsset<GameObject>("CementLoadingScreen"));
            summaryGUI = Instantiate(_bundle.LoadAsset<GameObject>("CementSummaryCanvas"));

            Button okButton = summaryGUI.transform.Find("Scroll View").Find("OK").GetComponent<Button>();
            okButton.onClick.AddListener(CloseSummaryMenu);

            DontDestroyOnLoad(cementGUI);

            CreateEventSystem();

            Transform parent = cementGUI.transform.Find("Background").Find("LoadingBar");
            progressBar = parent.GetComponent<Slider>();
            progressText = parent.Find("Fill Area").GetComponentInChildren<TMP_Text>();

            progressText.text = "0%";
            progressBar.value = 0f;

            try 
            {
                new ModMenu(_bundle);
            }
            catch(Exception e)
            {
                Cement.Log($"EXCEPTION OCCURRED WHILE CREATING MOD MENU {e}");
            }

            _bundle.Unload(false);
        }

        public static void CloseSummaryMenu()
        {
            Cement.Log($"CLICKED OK BUTTON! CURRENT SCENE: {SceneManager.GetActiveScene().name}");
            // checks if it is the menu, so that you can't click the ok button while the loading screen is active.
            if (SceneManager.GetActiveScene().name == "Menu")
            {
                Destroy(Cement.Singleton.summaryGUI);
                Cement.Singleton.RevertEventSystem();
            }
        }

        public void CreateSummary()
        {
            if (summaryGUI == null)
            {
                return;
            }

            summaryGUI.SetActive(true);

            DontDestroyOnLoad(summaryGUI);

            Transform background = summaryGUI.transform.Find("Scroll View");
            summary = background.Find("Viewport/Content").GetComponent<TMP_Text>();

            summary.text = "";
            if (!HasInternet)
            {
                summary.text = $"{FAILED_TAG}<b>NOT CONNECTED TO WIFI</color></b>\n\n";
            }
            summary.text += summaryText + "\n" + modMessageText;

            UseCementEventSystem();
        }

        private bool ModsPresent()
        {
            return Directory.GetFiles(MODS_FOLDER_PATH, $"*.{MOD_FILE_EXTENSION}").Length > 0;
        }


        private void CopyCacheToBin(string directoryName, string pathToMod)
        {
            string cachePath = Path.Combine(CACHE_PATH, directoryName);
            string binPath = Path.Combine(MODBIN_PATH, IOExtender.GetFileName(pathToMod).Split('.')[0]);

            _nameToModFile[binPath] = ModFile.Get(pathToMod);

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
                string name = IOExtender.GetFileName(path);
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
                IOExtender.DeleteFilesInDirectory(MODBIN_PATH);
            }
            catch (Exception e)
            {
                // yowser
            }
        }

        private string GetCurrentCementVersion()
        {
            string path = Path.Combine(CEMENT_PATH, "version");
            if (!File.Exists(path))
            {
                return null;
            }
            return File.ReadAllText(path);
        }

        private void GetLatestCementVersion(Action<bool, string> callback)
        {
            PersistentWebClient client = new PersistentWebClient();
            client.OnPersistentDownloadStringComplete += callback;
            client.DownloadStringPersistent(CEMENT_VERSION_URL);
        }

        private void UpdateCement()
        {
            string tempPath = Path.Combine(Application.dataPath, "CementInstaller.exe");

            if (!File.Exists(tempPath))
                File.Copy(Path.Combine(CEMENT_PATH, "CementInstaller.exe"), tempPath);

            Process process = new Process();
            process.StartInfo.FileName = tempPath;
            process.StartInfo.Verb = "runas";
            process.Start();
        }

        private void DeleteTempInstaller()
        {
            string tempPath = Path.Combine(Application.dataPath, "CementInstaller.exe");
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }

        private bool IsConnectedToWifi()
        {
            try
            {
                using (var client = new WebClient())
                using (var stream = client.OpenRead("http://www.google.com"))
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public void FinishedDownloadingMod(ProcessedModData data)
        {
            if (data.succeeded)
            {
                summaryText += SUCCEEDED_TAG + $"Successfully downloaded all files for {data.name}. </color>\n";
                CopyCacheToBin(data.directoryName, data.pathToMod);
            }
            else
            {
                if (data.name == null)
                {
                    data.name = "incorrectly formatted mod";
                }
                summaryText += FAILED_TAG + $"Failed to process {data.name}. </color>\n";
            }

            if (data.message != null)
            {
                modMessageText += $"<b>{data.name} message</b>:\n\n{data.message}\n\n";
            }
            totalModsProcessed++;
            UpdateProgressBar();
        }

        public void OnProgress(string mod, float percentage)
        {
            Cement.Log($"MOD {mod} IS {percentage}% DONE");
            _percentages[mod] = percentage;
            UpdateProgressBar();
        }

        public void UpdateProgressBar()
        {
            float value = 0f;
            foreach (float percentage in _percentages.Values)
            {
                value += percentage;
            }
            value /= totalMods;
            if (value > 100f)
            {
                value = 100f;
            }

            _currentProgressBarValue = value / 100f;
            progressText.text = $"{Mathf.Round(value * 10) * 0.1f}%";

            if (!loadedMods && totalModsProcessed == totalMods)
            {
                loadedMods = true;
                LoadAllMods();
            }
        }   

        bool pressedNo = false;
        private void Update()
        {
            // easter egg for: is this enough cements? obviously no
            if (Keyboard.current.nKey.isPressed && Keyboard.current.oKey.isPressed && !pressedNo)
            {
                pressedNo = true;
                SpawnInCementButton();
            }

            if (Keyboard.current.nKey.wasReleasedThisFrame || Keyboard.current.oKey.wasReleasedThisFrame)
            {
                pressedNo = false;
            }

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleClickingLinks();
            }

            if (progressBar != null)
            {
                progressBar.value = Mathf.Lerp(progressBar.value, _currentProgressBarValue, 10f * Time.deltaTime);
            }

            if (_usingCementEventSystem)
            {
                EventSystem.current = _cementEventSystem;
            }
            else
            {
                if (_oldEventSystem == null)
                {
                    _oldEventSystem = GameObject.Find("Global")?.transform.Find("Input/EventSystem")?.GetComponent<EventSystem>();
                }
                EventSystem.current = _oldEventSystem;
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
                case "coffee":
                    Application.OpenURL("https://www.buymeacoffee.com/cementgb");
                    break;
            }
        }

        public static void Restart()
        {
            string gangBeardPath = Path.Combine(Application.dataPath, "..");
            string arguments = $"/C echo RESTARTING GANG BEASTS && timeout /T 12 /nobreak && cd \"{gangBeardPath}\" && Start-Process \"Gang Beasts.exe\"";
            
            var startInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                UseShellExecute = true,
                Arguments = arguments
            };
            
            Process.Start(startInfo);
            Application.Quit();
        }

        public static void ClearCache()
        {
            IOExtender.DeleteFilesInDirectory(Cement.CACHE_PATH);
            IOExtender.DeleteFilesInDirectory(Cement.HIDDEN_MODS_PATH);
            foreach (CementMod mod in CementModSingleton.GetAll())
            {
                if (File.Exists(mod.modFile.path))
                {
                    mod.modFile.SetString("CurrentVersion", "Null");
                    mod.modFile.UpdateFile();
                } 
            }
            Restart();
        }
    }
}