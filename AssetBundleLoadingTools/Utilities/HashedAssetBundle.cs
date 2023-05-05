using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AssetBundleLoadingTools.Utilities
{
    public class HashedAssetBundle
    {
        internal static Dictionary<AssetBundle, string> HashedBundles = new();
        internal static Dictionary<AssetBundle, byte[]> BundleBytes = new();

        public static AssetBundle LoadFromMemory(byte[] binary)
        {
            var hash = AssetBundleHashing.FromBytes(binary);
            var bundle = AssetBundle.LoadFromMemory(binary);

            HashedBundles.Add(bundle, hash);
            BundleBytes.Add(bundle, binary);
            return bundle;
        }
    }
}
