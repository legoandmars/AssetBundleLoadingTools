using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace AssetBundleLoadingTools.Models.Shader
{
    [Serializable]
    public class CompiledShaderInfo
    {
        [JsonIgnore]
        public UnityEngine.Shader Shader { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public List<ShaderProperty> Properties { get; set; }

        [JsonProperty]
        public ShaderVariantInfo VariantInfo { get; set; }
        
        public bool ReplacementExistsLocally { get; set; }


        // hash
        // serialized VRProperty
        // for ReplacementShaderInfo
        // BundlePath
        // 
        // TODO: "matching shader" check
        /*
        public CompiledShaderInfo(string name, List<ShaderProperty> properties, ShaderVariantInfo variantInfo, bool replacementExistsLocally)
        {
            Name = name;
            Properties = properties;
            VariantInfo = variantInfo;
            ReplacementExistsLocally = replacementExistsLocally;
        }*/
        [JsonConstructor]
        public CompiledShaderInfo() { }

        public CompiledShaderInfo(UnityEngine.Shader shader, List<string> keywords) : this(shader, new List<List<string>>() { keywords }) { }

        public CompiledShaderInfo(UnityEngine.Shader shader, List<List<string>> variants)
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

        // could eventually need variant/subshader/pass info if there's collisions
        // a shader should ALWAYS match if it's being replaced - if somebody's adding properties, the shader likely works differently, and it **SHOULD NOT** replace it
    }
}
