using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace AssetBundleLoadingTools.Models.Shaders
{
    public class CompiledShaderInfo
    {
        [JsonIgnore]
        public UnityEngine.Shader Shader { get; set; }

        public string Name { get; set; }

        public List<ShaderProperty> Properties { get; set; }

        public ShaderVariantInfo VariantInfo { get; set; }

        // **Technically** this can be set to check whatever
        // For now, we're just going to hardcode the variant check for Beat Saber
        // Maybe eventually this can cover other things like Platform (PC/Android?)
        [JsonIgnore]
        public bool IsSupported
        { 
            get 
            {
                return VariantInfo.IsSinglePassInstanced;
            } 
        }

        [JsonConstructor]
        public CompiledShaderInfo(string name, List<ShaderProperty> properties, ShaderVariantInfo variantInfo) 
        {
            Name = name;
            Properties = properties;
            VariantInfo = variantInfo;
        }

        public CompiledShaderInfo(UnityEngine.Shader shader, List<string> variants)
        {
            Shader = shader;
            Name = shader.name;
            VariantInfo = new ShaderVariantInfo(variants);
            Properties = GetShaderProperties(shader);

            // TODO: implement cache somewhere here(?)
        }

        private List<ShaderProperty> GetShaderProperties(UnityEngine.Shader shader)
        {
            List<ShaderProperty> properties = new();

            for (int i = 0; i < shader.GetPropertyCount(); i++)
            {
                properties.Add(new(shader.GetPropertyName(i), shader.GetPropertyDescription(i), shader.GetPropertyType(i)));
            }

            return properties;
        }
    }
}
