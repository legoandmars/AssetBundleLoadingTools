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
    // mostly used for debug logging. if no debug logging is necessary, we just need PropertyMatchType and MatchShaderInfo (unless we want smarter shader matching)
    // i would like to do "smart" shader matching eventually (such as replacing _Tex with _MainTex if it's the only difference)
    // this should mostly be set up for that, but a system for the actual rules needs to be developed, and I don't feel like it right now
    public class ShaderMatchInfo
    {
        [JsonIgnore]
        public CompiledShaderInfo ShaderInfo { get; set; }

        [JsonIgnore]
        public ShaderMatchType ShaderMatchType => PartialMatchScore == 0 ? ShaderMatchType.FullMatch : ShaderMatchType.PartialMatch;

        public List<ShaderProperty> PropertiesMissingFromShader { get; set; } = new();
        public List<ShaderProperty> PropertiesMissingFromMatchShader { get; set; } = new();
        public List<PropertyConflictInfo> PropertyConflictInfos { get; set; } = new();

        [JsonIgnore]
        public int PartialMatchScore
        {
            get
            {
                int score = 0;
                // If it's just NEW properties on the replacement shader, it's *PROBABLY* fine
                // for the purposes of replacement it could be not ideal; eg somebody adding a displacement float with a weird default value
                score += PropertiesMissingFromShader.Count;
                // very bad, replacement shader is probably old and missing crucial data
                score += PropertiesMissingFromMatchShader.Count;
                // might be fine if it's display name. type being different is Very Bad and might break things
                score += PropertyConflictInfos.Count;
                return score;
            }
        }

        // debugging
        /*[JsonConstructor]
        public ShaderMatchInfo(
            List<ShaderProperty> propertiesMissingFromShader,
            List<ShaderProperty> propertiesMissingFromMatchShader, 
            List<PropertyConflictInfo> propertyConflictInfos)
        {
            PropertiesMissingFromShader = propertiesMissingFromShader;
            PropertiesMissingFromMatchShader = propertiesMissingFromMatchShader;
            PropertyConflictInfos = propertyConflictInfos;
        }*/

        // for non-debugging
        public ShaderMatchInfo(CompiledShaderInfo shaderInfo) 
        {
            ShaderInfo = shaderInfo;
        }
    }
}
