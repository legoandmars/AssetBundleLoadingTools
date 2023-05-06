using AssetBundleLoadingTools.Models.Shaders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Properties
{
    // mostly used for debug logging. if no debug logging is necessary, we just need PropertyMatchType and MatchShaderInfo
    public class PropertyMatchInfo
    {
        public PropertyMatchType PropertyMatchType { get; set; }

        [JsonIgnore]
        public CompiledShaderInfo? MatchShaderInfo { get; set; }

        public List<ShaderProperty> PropertiesMissingFromShader { get; set; } = new();
        public List<ShaderProperty> PropertiesMissingFromMatchShader { get; set; } = new();
        public List<PropertyConflictInfo> PropertyConflictInfos { get; set; } = new();

        // debugging
        [JsonConstructor]
        public PropertyMatchInfo(
            PropertyMatchType propertyMatchType, 
            CompiledShaderInfo matchShaderInfo, 
            List<ShaderProperty>? propertiesMissingFromShader,
            List<ShaderProperty> propertiesMissingFromMatchShader, 
            List<PropertyConflictInfo> propertyConflictInfos)
        {
            PropertyMatchType = propertyMatchType;
            MatchShaderInfo = matchShaderInfo;
            PropertiesMissingFromShader = propertiesMissingFromShader;
            PropertiesMissingFromMatchShader = propertiesMissingFromMatchShader;
            PropertyConflictInfos = propertyConflictInfos;
        }

        // for non-debugging
        public PropertyMatchInfo(CompiledShaderInfo compiledShaderInfo) 
        {
            PropertyMatchType = PropertyMatchType.FullMatch;
            MatchShaderInfo = compiledShaderInfo;
        }

        public PropertyMatchInfo()
        {
            PropertyMatchType = PropertyMatchType.NoMatch;
        }
    }
}
