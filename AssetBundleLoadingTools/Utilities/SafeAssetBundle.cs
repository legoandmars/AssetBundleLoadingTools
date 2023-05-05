using AssetBundleLoadingTools.Models.Bundles;
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
    // Mirror existing AssetBundle API for easiest transition
    // Ideally handle everything like hashing internally
    public class SafeAssetBundle
    {
        //internal static Dictionary<AssetBundle, string> HashedBundles = new();
        //internal static Dictionary<AssetBundle, byte[]> BundleBytes = new();

        private AssetBundle _bundle;
        private string _hash; // needed for cache. probably no reasonable way around reading the data twice
        private AssetBundleInitializationType _initializationType;
        private string _bundlePath; // I really hate this. Unfortunately, assettools.net NEEDS a path to extract bundles. Toni please save me
        // If we have to keep the _bundlePath bit, maybe add a way to disable it, it will hurt performance unnecessarily if you don't plan on using shader replacement

        public SafeAssetBundle(AssetBundle bundle, AssetBundleInitializationType initializationType, string hash, string path)
        {
            _bundle = bundle;
            _initializationType = initializationType;
            _hash = hash;
            _bundlePath = path;
        }

        // i have **NEVER** seen anybody actually use these crc/offset methods
        // but they should PROBABLY stay there for compatibility
        public static SafeAssetBundle LoadFromFile(string path) => LoadFromFile(path, 0u, 0uL);
        public static SafeAssetBundle LoadFromFile(string path, uint crc) => LoadFromFile(path, crc, 0uL);
        public static SafeAssetBundle LoadFromFile(string path, uint crc, ulong offset)
        {
            // TODO: test exceptions are the same
            // loadfromfile stuff here
            var bundle = AssetBundle.LoadFromFile(path, crc, offset);
            return new SafeAssetBundle(bundle, AssetBundleInitializationType.File, AssetBundleHashing.FromFile(path), path);
        }

        public static SafeAssetBundle LoadFromMemory(byte[] binary) => LoadFromMemory(binary, 0u);
        public static SafeAssetBundle LoadFromMemory(byte[] binary, uint crc)
        {
            var bundle = AssetBundle.LoadFromMemory(binary, crc);

            var bundlePath = Path.GetTempFileName();
            File.WriteAllBytes(bundlePath, binary);

            return new SafeAssetBundle(bundle, AssetBundleInitializationType.Memory, AssetBundleHashing.FromBytes(binary), bundlePath);
        }

        public static SafeAssetBundle LoadFromStream(Stream stream) => LoadFromStream(stream, 0u, 0u);
        public static SafeAssetBundle LoadFromStream(Stream stream, uint crc) => LoadFromStream(stream, crc, 0u);
        public static SafeAssetBundle LoadFromStream(Stream stream, uint crc, uint managedReadBufferSize)
        {
            var bundle = AssetBundle.LoadFromStream(stream, crc, managedReadBufferSize);

            // return a different stream
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            var hash = AssetBundleHashing.FromStream(memoryStream);
            var bundlePath = Path.GetTempFileName();
            File.WriteAllBytes(bundlePath, memoryStream.ToArray());
            memoryStream.Dispose();

            // **pretty* sure we don't need to dispose stream but it's worth double checking
            return new SafeAssetBundle(bundle, AssetBundleInitializationType.Stream, hash, bundlePath);
        }

        public T LoadAsset<T>(string name) where T : Object
        {
            return AssetBundleExtensions.LoadAssetSafe<T>(_bundle, name, _hash);
        }
    }
}
