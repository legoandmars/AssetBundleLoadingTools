using AssetBundleLoadingTools.Models.Bundles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AssetBundleLoadingTools.Utilities
{
    // Mirror existing AssetBundle API for easiest transition
    // Ideally handle everything like hashing internally
    public class SafeAssetBundle
    {
        //internal static Dictionary<AssetBundle, string> HashedBundles = new();
        //internal static Dictionary<AssetBundle, byte[]> BundleBytes = new();

        private string _hash; // needed for cache. probably no reasonable way around reading the data twice
        private AssetBundle _bundle;
        private AssetBundleInitializationType _initializationType;
        private string _bundlePath; // I really hate this. Unfortunately, assettools.net NEEDS a path to extract bundles. Toni please save me
        // If we have to keep the _bundlePath bit, maybe add a way to disable it, it will hurt performance unnecessarily if you don't plan on using shader replacement

        public SafeAssetBundle(AssetBundle bundle, string path)
        {
            _initializationType = AssetBundleInitializationType.File;
            _hash = AssetBundleHashing.FromFile(path);
            _bundle = bundle;
            _bundlePath = path;
        }

        public SafeAssetBundle(AssetBundle bundle, byte[] binary)
        {
            _initializationType = AssetBundleInitializationType.Memory;
            _hash = AssetBundleHashing.FromBytes(binary);
            _bundle = bundle;

            _bundlePath = Path.GetTempFileName();
            File.WriteAllBytes(_bundlePath, binary);
        }

        public SafeAssetBundle(AssetBundle bundle, MemoryStream stream)
        {
            _initializationType = AssetBundleInitializationType.Stream;
            _hash = AssetBundleHashing.FromStream(stream);
            _bundle = bundle;

            _bundlePath = Path.GetTempFileName();
            File.WriteAllBytes(_bundlePath, stream.ToArray());
            stream.Dispose();
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
            return new SafeAssetBundle(AssetBundle.LoadFromFile(path, crc, offset), path);
        }

        public static SafeAssetBundle LoadFromMemory(byte[] binary) => LoadFromMemory(binary, 0u);
        public static SafeAssetBundle LoadFromMemory(byte[] binary, uint crc)
        {
            var bundle = AssetBundle.LoadFromMemory(binary, crc);
            return new SafeAssetBundle(bundle, binary);
        }

        public static SafeAssetBundle LoadFromStream(Stream stream) => LoadFromStream(stream, 0u, 0u);
        public static SafeAssetBundle LoadFromStream(Stream stream, uint crc) => LoadFromStream(stream, crc, 0u);
        public static SafeAssetBundle LoadFromStream(Stream stream, uint crc, uint managedReadBufferSize)
        {
            var bundle = AssetBundle.LoadFromStream(stream, crc, managedReadBufferSize);

            // return a different stream
            var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);

            // **pretty* sure we don't need to dispose stream but it's worth double checking
            return new SafeAssetBundle(bundle, memoryStream);
        }
    }
}
