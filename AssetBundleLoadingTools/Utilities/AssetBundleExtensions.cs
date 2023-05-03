using AssetBundleLoadingTools.Core;
using AssetBundleLoadingTools.Models;
using System.Threading.Tasks;
using UnityEngine;
using Object = UnityEngine.Object;
using Caching = AssetBundleLoadingTools.Core.Caching;

namespace AssetBundleLoadingTools.Utilities
{
    public static class AssetBundleExtensions
    {
        public static T? LoadAssetSafe<T>(this AssetBundle bundle, string path, string hash) where T : Object
        {
            var asset = bundle.LoadAsset<T>(path);
            var gameObject = GameObjectFromAsset(asset);

            CacheAndSanitizeObject(gameObject, path, hash);

            return asset;
           //var manifest = bundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
           // bundle.
           // var hash = manifest.GetAssetBundleHash(bundle.assetbun);
        }

        // I didn't want to try to do whatever the hell async assetbundle loading actually is, so this is just a task
        public static async Task<T?> LoadAssetAsyncSafe<T>(this AssetBundle bundle, string path, string hash) where T : Object
        {
            var completion = new TaskCompletionSource<T?>();
            var assetLoadRequest = bundle.LoadAssetAsync<T>(path);

            assetLoadRequest.completed += delegate
            {
                var gameObject = GameObjectFromAsset(assetLoadRequest.asset);

                CacheAndSanitizeObject(gameObject, path, hash);

                completion.SetResult(assetLoadRequest.asset as T);
            };

            return await completion.Task;
        }
    
        private static GameObject? GameObjectFromAsset<T>(T asset)
        {
            GameObject gameObject;

            if (asset is GameObject assetAsGameObject)
            {
                gameObject = assetAsGameObject;
            }
            else if (asset is Component assetAsComponent)
            {
                gameObject = assetAsComponent.gameObject;
            }
            else
            {
                // TODO: look into non gameobjects/components more
                // there might be things people instantiate (like materials/shaders) that need to be checked(?)
                // I could also see an attacker directly supplying a UnityEvent as the path and somehow finding a way to execute it

                // throw new ArgumentException("Supplied asset is not GameObject/component data");
                gameObject = null;
            }

            return gameObject;
        }

        private static void CacheAndSanitizeObject(GameObject gameObject, string path, string hash)
        {
            if (gameObject == null) return;

            if (!Plugin.Config.EnableCache)
            {
                Sanitization.SanitizeObject(gameObject);
                return;
            }

            var bundleData = Caching.GetCachedBundleData(hash);
            if (bundleData == null)
            {
                var isDangerous = Sanitization.SanitizeObject(gameObject);

                if (bundleData == null)
                {
                    bundleData = new BundleData(path, isDangerous);
                }

                Caching.AddBundleDataToCache(hash, bundleData);
            }
            else if (bundleData.IsDangerous)
            {
                Sanitization.SanitizeObject(gameObject);
            }
        }
    }
}
