using AssetBundleLoadingTools.Models.Shader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Manifests
{
    [Serializable]
    public class ShaderBundleManifestEntry
    {
        [JsonProperty]
        public string BundlePath { get; set; }

        [JsonProperty]
        public CompiledShaderInfo ShaderInfo { get; set; }

        public ShaderBundleManifestEntry(string bundlePath, CompiledShaderInfo shaderInfo)
        {
            BundlePath = bundlePath;
            ShaderInfo = shaderInfo;
        }

        [JsonConstructor]
        public ShaderBundleManifestEntry() { }
    }
}
