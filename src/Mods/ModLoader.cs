using CementTools.Helpers;
using CementTools.ModMenuTools;
using CementTools.Modules.InputModule;
using CementTools.Modules.NotificationModule;
using CementTools.Modules.PoolingModule;
using CementTools.Modules.SceneModule;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Injection;
using System.Reflection;
using UnityEngine;

namespace CementTools.ModLoading
{
    public static class ModLoader
    {
        private static GameObject modHolder;

        private static void LoadAllModules()
        {
            modHolder.AddComponent<InputManager>();
            modHolder.AddComponent<CustomSceneManager>();
            modHolder.AddComponent<Pool>();
            modHolder.AddComponent<NotificationModule>();
        }

        public static void Setup()
        {
            modHolder = new GameObject("Cement Mods");
            UnityEngine.Object.DontDestroyOnLoad(modHolder);

            LoadAllModules();

            foreach (string subDirectory in Directory.GetDirectories(Cement.MODBIN_PATH))
            {
                Cement.Log($"PROCESSING SUB {subDirectory}");

                ModFile modFile = Cement.Instance.GetModFileFromName(subDirectory);
                Cement.Log("CREATED MOD FILE. LOADING DLLS...");
                LoadModAssemblies(subDirectory, modFile);
                Cement.Log("FINISHED LOADING DLLS");
                modFile.GotLoaded();
            }

            Cement.Instance.CreateSummary();
            Cement.Log("SETTING UP MOD MENU");
            ModMenu.Singleton.SetupModMenu();
            Cement.Log("DONE SETTING UP MOD MENU");
        }

        private static void LoadModAssemblies(string directory, ModFile modFile)
        {
            string[] assemblyPaths = Directory.GetFiles(directory, "*.dll");
            foreach (string path in assemblyPaths)
            {
                try
                {
                    Assembly assembly = Assembly.LoadFile(path);
                    foreach (AssemblyName referencedAssembly in assembly.GetReferencedAssemblies())
                    {
                        foreach (string sub in Directory.GetDirectories(Cement.MODBIN_PATH))
                        {
                            string assemblyPath = Path.Combine(sub, referencedAssembly.Name + ".dll");
                            if (File.Exists(assemblyPath))
                            {
                                Assembly.LoadFile(assemblyPath);
                                break;
                            }
                        }

                    }
                    foreach (Type type in assembly.GetTypes())
                    {
                        if (typeof(CementMod).IsAssignableFrom(type) || type.IsAssignableFrom(typeof(CementMod)))
                        {
                            try
                            {
                                ClassInjector.RegisterTypeInIl2Cpp(type);
                                CementMod mod = InstantiateMod(type);
                                mod.modDirectoryPath = directory;
                                mod.modFile = modFile;
                                mod.enabled = !modFile.GetBool("Disabled");
                                CementModSingleton.Add(type, mod);
                                Cement.Log($"Succesfully loaded {type.Name}.");
                            }
                            catch (Exception e)
                            {
                                Cement.Log($"Error occurred while loading {type.Name}: {e}");
                                Cement.Instance.AddToSummary(Cement.FAILED_TAG);
                                Cement.Instance.AddToSummary($"Error occurred while loading {type.Name}: {e}\n");
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Cement.Log($"Error loading assembly {path}. {e}");
                    Cement.Instance.AddToSummary(Cement.FAILED_TAG);
                    Cement.Instance.AddToSummary($"Error occurred while loading assembly {IOExtender.GetFileName(path)}.");
                }
            }
        }

        private static CementMod InstantiateMod(Type mod)
        {
            return modHolder.AddComponent(Il2CppType.From(mod)).Cast<CementMod>();
        }
    }
}
