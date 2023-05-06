using AssetBundleLoadingTools.Models.Shaders;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Properties
{
    public class PropertyConflictInfo
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
