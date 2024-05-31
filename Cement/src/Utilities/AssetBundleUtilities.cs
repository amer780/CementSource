using MelonLoader;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace CementGB.Utilities;

public static class AssetBundleUtilities
{
    public static T? LoadPersistentAsset<T>(this AssetBundle bundle, string name) where T : UnityEngine.Object
    {
        var asset = bundle.LoadAsset(name);

        if (asset != null)
        {
            asset.hideFlags = HideFlags.DontUnloadUnusedAsset;
            return asset.TryCast<T>();
        }

        return null;
    }

    public static void LoadPersistentAssetAsync<T>(this AssetBundle bundle, string name, Action<T>? onLoaded) where T : UnityEngine.Object
    {
        var request = bundle.LoadAssetAsync<T>(name);

        request.add_completed((Il2CppSystem.Action<AsyncOperation>)((a) =>
        {
            if (request.asset == null) return;
            var result = request.asset.TryCast<T>();
            if (result == null) return;
            result.hideFlags = HideFlags.DontUnloadUnusedAsset;
            onLoaded?.Invoke(result);
        }));
    }

    internal static AssetBundle? LoadEmbeddedAssetBundle(Assembly assembly, string name)
    {
        string[] manifestResources = assembly.GetManifestResourceNames();
        AssetBundle? bundle = null;
        if (manifestResources.Contains(name))
        {
            Melon<Mod>.Logger.Msg($"Loading embedded resource data {name}...");
            using var str = assembly.GetManifestResourceStream(name);
            if (str is null) 
            {
                Melon<Mod>.Logger.Warning($"Manifest resource returned null stream. How did this happen?");
                return null; 
            }

            using var memoryStream = new MemoryStream();

            str.CopyTo(memoryStream);
            Melon<Mod>.Logger.Msg("Done!");
            byte[] resource = memoryStream.ToArray();

            Melon<Mod>.Logger.Msg($"Loading assetBundle from data {name}, please be patient...");
            bundle = AssetBundle.LoadFromMemory(resource);
            Melon<Mod>.Logger.Msg("Done!");
        }
        return bundle;
    }
}
