using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class AddressableAssetManager
{
    public static async Task<T> LoadFromAddressable<T>(this string assetPath)
    {
        // 加载指定路径的资源
        AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>(assetPath);
        await handle.Task;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            T resource = handle.Result;
            return resource;

        }
        else
        {
            Debug.LogError($"Failed to load asset at path: {assetPath}");
            return default;
        }
    }

    public static void Release<T>(T asset)
    {
        // 释放资源
        Addressables.Release(asset);
    }


}