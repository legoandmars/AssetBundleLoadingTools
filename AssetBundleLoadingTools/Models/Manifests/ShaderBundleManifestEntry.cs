using AssetBundleLoadingTools.Models.Shaders;
using Newtonsoft.Json;

namespace AssetBundleLoadingTools.Models.Manifests
{
    internal class ShaderBundleManifestEntry
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
