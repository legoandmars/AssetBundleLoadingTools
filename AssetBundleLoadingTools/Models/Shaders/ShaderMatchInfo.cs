using AssetBundleLoadingTools.Models.Properties;
using AssetBundleLoadingTools.Models.Shaders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Shaders
{
    // mostly used for debug logging. if no debug logging is necessary, we just need PropertyMatchType and MatchShaderInfo
    public class ShaderMatchInfo
    {
        public ShaderMatchType ShaderMatchType { get; set; }

        [JsonIgnore]
        public CompiledShaderInfo? MatchShaderInfo { get; set; }

        public List<ShaderProperty> PropertiesMissingFromShader { get; set; } = new();
        public List<ShaderProperty> PropertiesMissingFromMatchShader { get; set; } = new();
        public List<PropertyConflictInfo> PropertyConflictInfos { get; set; } = new();

        // debugging
        [JsonConstructor]
        public ShaderMatchInfo(
            ShaderMatchType shaderMatchType, 
            CompiledShaderInfo matchShaderInfo, 
            List<ShaderProperty>? propertiesMissingFromShader,
            List<ShaderProperty> propertiesMissingFromMatchShader, 
            List<PropertyConflictInfo> propertyConflictInfos)
        {
            ShaderMatchType = shaderMatchType;
            MatchShaderInfo = matchShaderInfo;
            PropertiesMissingFromShader = propertiesMissingFromShader;
            PropertiesMissingFromMatchShader = propertiesMissingFromMatchShader;
            PropertyConflictInfos = propertyConflictInfos;
        }

        // for non-debugging
        public ShaderMatchInfo(CompiledShaderInfo compiledShaderInfo) 
        {
            ShaderMatchType = ShaderMatchType.FullMatch;
            MatchShaderInfo = compiledShaderInfo;
        }

        public ShaderMatchInfo()
        {
            ShaderMatchType = ShaderMatchType.NoMatch;
        }
    }
}
