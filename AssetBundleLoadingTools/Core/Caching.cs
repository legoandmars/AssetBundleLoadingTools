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

        private static readonly string _cachedBundleDataPath = Path.Combine(Constants.CachePath, "AssetBundleHashData.dat");
        private static readonly string _cachedShaderDataPath = Path.Combine(Constants.CachePath, "AssetBundleShaderData.dat");
        private static readonly string _warningPath = Path.Combine(Constants.CachePath, "IMPORTANT_WARNING.txt");
        private static bool _cacheAwaitingWrite = false;

        // warning is likely unnecessary but might reduce the odds of people using the cache to allow malicious assetbundles
        private const string _warningText = "WARNING: UNLESS YOU KNOW WHAT YOU ARE DOING, DO ***NOT*** CHANGE ANYTHING IN THIS FOLDER.\nIF SOMEONE TOLD YOU TO CHANGE/PASTE SOMETHING HERE, THEY COULD BE TRICKING YOU INTO INSTALLING MALWARE ON YOUR SYSTEM.";

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
            if (!File.Exists(_warningPath))
            {
                File.WriteAllText(_warningPath, _warningText);
            }

            File.WriteAllText(_cachedShaderDataPath, JsonConvert.SerializeObject(_cachedBundleShaderData));
        }


        internal static void AddShaderDataToCache(string hash, BundleShaderData data)
        {
            if(_cachedBundleShaderData.TryGetValue(hash, out BundleShaderData existingData))
            {
                if(existingData == data)
                {
                    Console.WriteLine("YOU ARE JUST OVERWRITING DATA!");
                    _cacheAwaitingWrite = true;
                    return;
                }
                // could be multiple assets in same assetbundle hash
                foreach(var existingInfo in existingData.CompiledShaderInfos)
                {
                    if(!data.CompiledShaderInfos.Any(x => ShaderMatching.ShaderInfosMatchOptimized(x, existingInfo)))
                    {
                        data.CompiledShaderInfos.Add(existingInfo);
                    }
                }

                data.NeedsReplacing = data.CompiledShaderInfos.All(x => x.IsSupported);

                _cachedBundleShaderData[hash] = data;
                _cacheAwaitingWrite = true;
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
