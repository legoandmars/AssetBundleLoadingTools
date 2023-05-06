using AssetBundleLoadingTools.Models.Shaders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Bundles
{
    // Specifically cache because assettools.NET takes SO. LONG.
    // Might not really be needed if we can do everything super fast at runtime
    // TODO: Find a way to conditionally not need CompiledShaderInfos if NeedsShaderReplacement is false
    // It's a lot of data. Maybe too much, this could cause performance problems

    public class BundleShaderData
    {
        // TODO: Replce with just ShaderVariantInfo in cache instead of entire shader infos
        // The other data is not really used
        // Name isn't enough to make sure CompiledShaderInfos is the same; we'll need to hash it based on name + properties
        public List<CompiledShaderInfo> CompiledShaderInfos { get; set; }
        public bool NeedsReplacing { get; set; } = true;

        [JsonConstructor]
        public BundleShaderData(List<CompiledShaderInfo> compiledShaderInfos, bool needsReplacing)
        {
            CompiledShaderInfos = compiledShaderInfos;
            NeedsReplacing = needsReplacing;
        }
    }
}
