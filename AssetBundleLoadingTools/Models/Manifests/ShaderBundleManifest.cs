using AssetBundleLoadingTools.Models.Shader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

namespace AssetBundleLoadingTools.Models.Manifests
{
    // Specifically needs a manifest because in-editor serialization is done with unity's JSON
    // Which, *somehow*, still cannot serialize a List<> or Dictionary<>

    public class ShaderBundleManifest
    {
        public List<ShaderBundleManifestEntry> ManifestEntries { get; set; } // hack to work with legacy unity projects

        [JsonIgnore]
        public Dictionary<string, CompiledShaderInfo> ShadersByBundlePath { get; set; }

        public long TimeSerialized { get; set; }

        public string UnityVersion { get; set; } // could handle bundles compiled for future versions; probably unnecessary

        [JsonConstructor]
        public ShaderBundleManifest(List<ShaderBundleManifestEntry> manifestEntries, long timeSerialized, string unityVersion)
        {
            TimeSerialized = timeSerialized;
            UnityVersion = unityVersion;

            ShadersByBundlePath = new();

            foreach(var entry in manifestEntries)
            {
                ShadersByBundlePath.Add(entry.BundlePath, entry.ShaderInfo);
            }

            ManifestEntries = null; // literally only needed for serialization
        }

        // mostly for debugging, this will never be serialized in plugin
        [OnSerializing] 
        public void OnSerializing(StreamingContext context)
        {
            ManifestEntries = new();
            foreach(var entry in ShadersByBundlePath)
            {
                ManifestEntries.Add(new(entry.Key, entry.Value));
            }
        }
    }
}
