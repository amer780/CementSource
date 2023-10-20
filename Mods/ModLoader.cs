using UnityEngine;
using CementTools;
using System;
using System.IO;
using System.Reflection;
using CementTools.ModMenuTools;
using CementTools.Modules.InputModule;
using CementTools.Modules.SceneModule;
using CementTools.Modules.PoolingModule;

public static class ModLoader
{
    public static GameObject modHolder;

    public static void LoadAllModules()
    {
        modHolder.AddComponent<InputManager>();
        modHolder.AddComponent<CustomSceneManager>();
        modHolder.AddComponent<Pool>();
    }

    public static void LoadAllMods()
    {
        Cement.Log(System.Reflection.MethodBase.GetCurrentMethod().Name);
        modHolder = new GameObject("Cement Mods");
        modHolder.hideFlags = HideFlags.HideAndDontSave;

        LoadAllModules();
        GameObject.DontDestroyOnLoad(modHolder);

        foreach (string subDirectory in Directory.GetDirectories(Cement.MODBIN_PATH))
        {
            Cement.Log($"PROCESSING SUB {subDirectory}");

            ModFile modFile = Cement.Singleton.GetModFileFromName(subDirectory);
            Cement.Log("CREATED MOD FILE. LOADING DLLS...");
            LoadMods(subDirectory, modFile);
            Cement.Log("FINISHED LOADING DLLS");
        }

        Cement.Singleton.CreateSummary();
        Cement.Log("SETTING UP MOD MENU");
        ModMenu.Singleton.SetupModMenu();
        Cement.Log("DONE SETTING UP MOD MENU");
    }

    private static void LoadMods(string directory, ModFile modFile)
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
                    if (typeof(CementMod).IsAssignableFrom(type) || type.IsAssignableFrom(typeof(Cement)))
                    {
                        try
                        {
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
                            Cement.Singleton.AddToSummary(Cement.FAILED_TAG);
                            Cement.Singleton.AddToSummary($"Error occurred while loading {type.Name}: {e}\n");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Cement.Log($"Error loading assembly {path}. {e}");
                Cement.Singleton.AddToSummary(Cement.FAILED_TAG);
                Cement.Singleton.AddToSummary($"Error occurred while loading assembly {IOExtender.GetFileName(path)}.");
            }
        }
    }

    private static CementMod InstantiateMod(Type mod)
    {
        return modHolder.AddComponent(mod) as CementMod;
    }
}