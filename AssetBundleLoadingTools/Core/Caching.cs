using AssetBundleLoadingTools.Models;
using AssetBundleLoadingTools.Models.Bundles;
using AssetBundleLoadingTools.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Core
{
    internal class Caching
    {
        // Shader and "malicious" caching is relatively different (and currently performed at different steps), so they're cached in seperate areas

        private static readonly string _cachedShaderDataPath = Path.Combine(Constants.CachePath, "AssetBundleShaderData.dat");
        private static bool _cacheAwaitingWrite = false;

        // warning is likely unnecessary but might reduce the odds of people using the cache to allow malicious assetbundles

        private static ConcurrentDictionary<string, BundleShaderData> _cachedBundleShaderData = new();

        // TODO: r/w more smart to avoid unnecessary disk
        // maybe a 1 second timer or on quit or something

        internal static void ReadCache()
        {
            if (File.Exists(_cachedShaderDataPath))
            {
                var cachedData = File.ReadAllText(_cachedShaderDataPath);
                _cachedBundleShaderData = JsonConvert.DeserializeObject<ConcurrentDictionary<string, BundleShaderData>>(cachedData);
                if (_cachedBundleShaderData == null) _cachedBundleShaderData = new();
            }
        }

        internal static void WriteCache(object? _ = null)
        {
            if (!_cacheAwaitingWrite) return;

            if (!Directory.Exists(Constants.CachePath))
            {
                Directory.CreateDirectory(Constants.CachePath);
            }

            // TODO: remove prettyprint for production
            File.WriteAllText(_cachedShaderDataPath, JsonConvert.SerializeObject(_cachedBundleShaderData, Formatting.Indented));

            _cacheAwaitingWrite = false;
        }


        internal static void AddShaderDataToCache(string hash, BundleShaderData data)
        {
            if(_cachedBundleShaderData.TryGetValue(hash, out BundleShaderData existingData) && existingData == data)
            {
                _cacheAwaitingWrite = true;
                // assume changed; don't wanna set up a compare rn
                return;
            }
            else if (_cachedBundleShaderData.TryAdd(hash, data))
            {
                _cacheAwaitingWrite = true;
            }
            else
            {
                Plugin.Log.Warn($"Could not add {hash} to cached bundle data.");
            }
        }

        internal static BundleShaderData? GetCachedBundleShaderData(string hash)
        {
            if (_cachedBundleShaderData.TryGetValue(hash, out BundleShaderData data))
            {
                return data;
            }
            else
            {
                return null;
            }
        }
    }
}
