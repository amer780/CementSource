using System;
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
}
