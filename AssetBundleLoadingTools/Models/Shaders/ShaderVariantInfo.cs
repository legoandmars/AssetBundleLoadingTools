using AssetBundleLoadingTools.Core;
using AssetBundleLoadingTools.Models.Shaders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Shader
{
    [Serializable]
    public class ShaderVariantInfo
    {
        // WARNING: Avoid using variants/keywords for comparison purposes
        // The format is not consistent between loaded shaders
        [JsonProperty]
        private List<ShaderVariant> _variants { get; set; }

        // Platforms is serialized seperately, even though we ***COULD*** probably infer it from keywords
        // Not 100% confident in the keyword extraction we have right now
        [JsonProperty]
        public List<ShaderVRPlatform> Platforms { get; set; }

        [JsonIgnore]
        public bool IsSinglePass => Platforms.Contains(ShaderVRPlatform.SinglePass);

        [JsonIgnore]
        public bool IsSinglePassInstanced => Platforms.Contains(ShaderVRPlatform.SinglePassInstanced);

        public bool CompiledWithKeyword(string keyword)
        {
            return _variants.Any(x => x.Keywords.Contains(keyword));
        }

        public ShaderVariantInfo(List<List<string>> variantKeywordsList)
        {
            _variants = variantKeywordsList.Select(x => new ShaderVariant(x)).ToList();
            Platforms = new List<ShaderVRPlatform>();
            if (CompiledWithKeyword(Constants.SinglePassKeyword)) Platforms.Add(ShaderVRPlatform.SinglePass);
            if (CompiledWithKeyword(Constants.SinglePassInstancedKeyword)) Platforms.Add(ShaderVRPlatform.SinglePassInstanced);
        }

        [JsonConstructor]
        public ShaderVariantInfo() { }
    }
}
