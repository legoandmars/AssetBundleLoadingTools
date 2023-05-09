using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;

namespace AssetBundleLoadingTools.Utilities
{
    public static class AssetBundleExtensions
    {
        public static async Task<AssetBundle?> LoadFromFileAsync(string path)
        {
            TaskCompletionSource<AssetBundle> taskCompletionSource = new();
            var bundleRequest = AssetBundle.LoadFromFileAsync(path);
            bundleRequest.completed += delegate 
            { 
                taskCompletionSource.SetResult(bundleRequest.assetBundle); 
            };

            return await taskCompletionSource.Task;
        }

        public static async Task<AssetBundle?> LoadFromMemoryAsync(byte[] binary)
        {
            TaskCompletionSource<AssetBundle> taskCompletionSource = new();
            var bundleRequest = AssetBundle.LoadFromMemoryAsync(binary);
            bundleRequest.completed += delegate
            {
                taskCompletionSource.SetResult(bundleRequest.assetBundle);
            };

            return await taskCompletionSource.Task;
        }

        public static async Task<AssetBundle?> LoadFromStreamAsync(Stream stream)
        {
            TaskCompletionSource<AssetBundle> taskCompletionSource = new();
            var bundleRequest = AssetBundle.LoadFromStreamAsync(stream);
            bundleRequest.completed += delegate
            {
                taskCompletionSource.SetResult(bundleRequest.assetBundle);
            };

            return await taskCompletionSource.Task;
        }

        public static async Task<T?> LoadAssetAsync<T>(AssetBundle assetBundle, string path) where T : Object
        {
            TaskCompletionSource<T> taskCompletionSource = new();
            var assetRequest = assetBundle.LoadAssetAsync<T>(path);
            assetRequest.completed += delegate
            {
                taskCompletionSource.SetResult((T)assetRequest.asset);
            };

            return await taskCompletionSource.Task;
        }
    }
}
