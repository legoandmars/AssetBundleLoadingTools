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

        private bool Matches(string otherName, List<ShaderProperty> otherProperties)
        {
            if (otherName != Name) return false;

            // name matches; check if serialized properties are the same
            // this does not use variants for now as it's a bit too unstable between the three systems

            if (otherProperties.Count != Properties.Count) return false;
            foreach (var property in Properties)
            {
                bool exists = false;
                foreach (var otherProperty in otherProperties)
                {
                    // unsure if display name necessary
                    if (
                        property.Name == otherProperty.Name &&
                        property.DisplayName == otherProperty.DisplayName &&
                        property.PropertyType == otherProperty.PropertyType)
                    {
                        exists = true;
                    }
                }

                if (exists == false) return false;
            }

            return true;
        }

        // deep match
        public bool MatchesOtherShader(UnityEngine.Shader otherShader)
        {
            if (otherShader.name != Name) return false;
            var otherProperties = GetShaderProperties(otherShader);
            
            return Matches(otherShader.name, otherProperties);
        }

        public bool MatchesShaderInfo(CompiledShaderInfo otherCompiledShaderInfo) => Matches(otherCompiledShaderInfo.Name, otherCompiledShaderInfo.Properties);

        // could eventually need variant/subshader/pass info if there's collisions
        // a shader should ALWAYS match if it's being replaced - if somebody's adding properties, the shader likely works differently, and it **SHOULD NOT** replace it
    }
}
