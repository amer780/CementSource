using CementTools.ModMenuTools;
using CementTools.Modules.InputModule;
using CementTools.Modules.NotificationModule;
using CementTools.Modules.PoolingModule;
using CementTools.Modules.SceneModule;
using Il2CppInterop.Runtime.Injection;
using MelonLoader;
using UnityEngine;

namespace CementTools
{
    public static class BuildInfo
    {
        public const string Name = "Cement";
        public const string Description = null;
        public const string Author = "HueSamai";
        public const string Company = "CementGB";
        public const string Version = "3.14.0";
        public const string DownloadLink = null;
    }

    public class Mod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            HarmonyInstance.PatchAll();

            #region register_types_il2cpp
            ClassInjector.RegisterTypeInIl2Cpp<Cement>();
            ClassInjector.RegisterTypeInIl2Cpp<CementMod>();
            ClassInjector.RegisterTypeInIl2Cpp<Pool>();
            ClassInjector.RegisterTypeInIl2Cpp<Notification>();
            ClassInjector.RegisterTypeInIl2Cpp<NotificationModule>();
            ClassInjector.RegisterTypeInIl2Cpp<InputManager>();
            ClassInjector.RegisterTypeInIl2Cpp<CustomSceneManager>();
            ClassInjector.RegisterTypeInIl2Cpp<ModUI>();
            ClassInjector.RegisterTypeInIl2Cpp<ParameterUI>();
            ClassInjector.RegisterTypeInIl2Cpp<InputFieldParameterUI>();
            ClassInjector.RegisterTypeInIl2Cpp<BoolParameterUI>();
            #endregion
        }

        public override void OnLateInitializeMelon()
        {
            UnityEngine.Object.DontDestroyOnLoad(new GameObject("CementGlobal").AddComponent<Cement>());
        }
    }
}
