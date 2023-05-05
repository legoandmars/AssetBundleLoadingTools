using AssetBundleLoadingTools.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Shader
{
    public class ShaderVariantInfo
    {
        // WARNING: Avoid using variants/keywords for comparison purposes
        // The format is not consistent between loaded shaders

        // Originally a list of individual passes/variants but it felt unnecessarily wasteful
        [JsonProperty("Keywords")]
        private List<string> _keywords { get; set; }

        // Platforms is serialized seperately, even though we ***COULD*** probably infer it from keywords
        // Not 100% confident in the keyword extraction we have right now
        public List<ShaderVRPlatform> Platforms { get; set; }

        [JsonIgnore]
        public bool IsSinglePass => Platforms.Contains(ShaderVRPlatform.SinglePass);

        [JsonIgnore]
        public bool IsSinglePassInstanced => Platforms.Contains(ShaderVRPlatform.SinglePassInstanced);

        public bool CompiledWithKeyword(string keyword)
        {
            return _keywords.Any(x => x ==  keyword);
        }

        public ShaderVariantInfo(List<string> variantKeywordsList)
        {
            _keywords = variantKeywordsList;
            Platforms = new List<ShaderVRPlatform>();
            if (CompiledWithKeyword(Constants.SinglePassKeyword)) Platforms.Add(ShaderVRPlatform.SinglePass);
            if (CompiledWithKeyword(Constants.SinglePassInstancedKeyword)) Platforms.Add(ShaderVRPlatform.SinglePassInstanced);
        }

        [JsonConstructor]
        public ShaderVariantInfo(List<string> keywords, List<ShaderVRPlatform> platforms)
        {
            _keywords = keywords;
            Platforms = platforms;
        }
    }
}
