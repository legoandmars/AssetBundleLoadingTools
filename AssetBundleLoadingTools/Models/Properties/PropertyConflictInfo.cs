using AssetBundleLoadingTools.Models.Shaders;
using Newtonsoft.Json;

namespace AssetBundleLoadingTools.Models.Properties
{
    internal class PropertyConflictInfo
    {
        public ShaderProperty ShaderProperty { get; set; }
        public ShaderProperty MatchShaderProperty { get; set; }
        public PropertyConflictType PropertyConflictType { get; set; }

        [JsonConstructor]
        public PropertyConflictInfo(ShaderProperty shaderProperty, ShaderProperty matchShaderProperty, PropertyConflictType propertyConflictType) 
        {
            ShaderProperty = shaderProperty;
            MatchShaderProperty = matchShaderProperty;
            PropertyConflictType = propertyConflictType;
        }
    }
}
