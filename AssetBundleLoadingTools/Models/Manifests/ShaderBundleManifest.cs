using AssetBundleLoadingTools.Models.Shader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Manifests
{
    // Specifically needs a manifest because in-editor serialization is done with unity's JSON
    // Which, *somehow*, still cannot serialize a List<> or Dictionary<>

    [Serializable]
    public class ShaderBundleManifest
    {
        [JsonProperty]
        public List<ShaderBundleManifestEntry> ManifestEntries { get; set; } // hack to work with legacy unity projects

        [JsonIgnore]
        public Dictionary<string, CompiledShaderInfo> ShadersByBundlePath { get; set; }

        [JsonProperty]
        public long TimeSerialized { get; set; }

        [JsonProperty]
        public string UnityVersion { get; set; } // could handle bundles compiled for future versions; probably unnecessary

        [JsonConstructor]
        public ShaderBundleManifest()
        {
            ShadersByBundlePath = new();

            foreach(var entry in ManifestEntries)
            {
                ShadersByBundlePath.Add(entry.BundlePath, entry.ShaderInfo);
            }

            ManifestEntries = null; // literally only needed for serialization
        }
    }
}
