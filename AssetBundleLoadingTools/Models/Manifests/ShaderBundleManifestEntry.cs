using AssetBundleLoadingTools.Models.Shaders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Manifests
{
    public class ShaderBundleManifestEntry
    {
        public string BundlePath { get; set; }

        public CompiledShaderInfo ShaderInfo { get; set; }

        [JsonConstructor]
        public ShaderBundleManifestEntry(string bundlePath, CompiledShaderInfo shaderInfo)
        {
            BundlePath = bundlePath;
            ShaderInfo = shaderInfo;
        }
    }
}
