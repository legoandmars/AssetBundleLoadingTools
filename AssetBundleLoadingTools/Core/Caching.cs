using AssetBundleLoadingTools.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Core
{
    internal class Caching
    {
        // Shader and "malicious" caching is relatively different (and currently performed at different steps), so they're cached in seperate areas

        private static readonly string _cachePath = Path.Combine(IPA.Utilities.UnityGame.InstallPath, "UserData", "AssetBundleLoadingTools");
        private static readonly string _cachedBundleDataPath = Path.Combine(_cachePath, "AssetBundleHashData.dat");
        private static readonly string _cachedShaderDataPath = Path.Combine(_cachePath, "AssetBundleShaderData.dat");
        private static readonly string _warningPath = Path.Combine(_cachePath, "IMPORTANT_WARNING.txt");

        // warning is likely unnecessary but might reduce the odds of people using the cache to allow malicious assetbundles
        private const string _warningText = "WARNING: UNLESS YOU KNOW WHAT YOU ARE DOING, DO ***NOT*** CHANGE ANYTHING IN THIS FOLDER.\nIF SOMEONE TOLD YOU TO CHANGE/PASTE SOMETHING HERE, THEY COULD BE TRICKING YOU INTO INSTALLING MALWARE ON YOUR SYSTEM.";

        private static ConcurrentDictionary<string, BundleData> _cachedBundleData = new();

        internal static void ReadCache()
        {
            if (File.Exists(_cachedBundleDataPath))
            {
                var cachedData = File.ReadAllText(_cachedBundleDataPath);
                _cachedBundleData = JsonConvert.DeserializeObject<ConcurrentDictionary<string, BundleData>>(cachedData);
                if (_cachedBundleData == null) _cachedBundleData = new();
            }
        }

        private static void WriteCache()
        {
            if (!Directory.Exists(_cachePath)) Directory.CreateDirectory(_cachePath);

            if (!File.Exists(_warningPath)) File.WriteAllText(_warningPath, _warningText);

            File.WriteAllText(_cachedBundleDataPath, JsonConvert.SerializeObject(_cachedBundleData));
        }

        internal static void AddToCache(string hash, BundleData data)
        {
            if (_cachedBundleData.TryGetValue(hash, out BundleData existingData))
            {
                // try to save it
                // this could be a "multiple gameobject LoadAssets in a single" type problem
                // if IsDangerous is EVER true for a hash, it should ALWAYS be true for that hash.
                data.IsDangerous = data.IsDangerous || existingData.IsDangerous;

                _cachedBundleData[hash] = data;

                WriteCache();
            }
            else if (_cachedBundleData.TryAdd(hash, data))
            {
                WriteCache();
            }
            else
            {
                Plugin.Log.Warn($"Could not add {hash} to cached bundle data.");
            }
        }

        internal static BundleData? GetCachedData(string hash)
        {
            if (_cachedBundleData.TryGetValue(hash, out BundleData data))
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
