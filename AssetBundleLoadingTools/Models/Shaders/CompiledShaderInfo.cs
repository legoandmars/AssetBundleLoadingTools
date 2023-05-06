using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;


namespace AssetBundleLoadingTools.Models.Shader
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

        private bool Matches(string otherName, List<ShaderProperty> otherProperties)
        {
            if (otherName != Name) return false;

            bool matchesFully = true;

            // name matches; check if serialized properties are the same
            // this does not use variants for now as it's a bit too unstable between the three systems
            // if (otherProperties.Count != Properties.Count) return false;
            if (otherProperties.Count != Properties.Count) matchesFully = false;

            foreach (var property in Properties)
            {
                if (matchesFully == false) break;
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

                if (exists == false) matchesFully = false;
            }

            if(matchesFully == false)
            {
                Console.WriteLine($"Name matches between {Name} and {otherName}; properties don't; debug info:");
                Console.WriteLine(GetPropertyDiffs(Properties, otherProperties));

                // log
                return false;
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

        // used for debug logging, see property differences
        // maybe eventually use for smart replacement (if _Tex and _MainTex are the only differing properties, it should be fixable by setting shader properties)
        private static string GetPropertyDiffs(List<ShaderProperty> properties, List<ShaderProperty> otherProperties)
        {
            string propertyDiffs = "";

            foreach (var property in properties)
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

                    if (property.Name == otherProperty.Name)
                    {
                        // property exists
                        if (property.DisplayName != otherProperty.DisplayName)
                        {
                            propertyDiffs += $"Property {property.Name} DisplayName ({property.DisplayName}) is different on Replacement ({otherProperty.DisplayName})\n";
                        }
                        if (property.PropertyType != otherProperty.PropertyType)
                        {
                            propertyDiffs += $"Property {property.Name} Type ({property.PropertyType}) is different on Replacement ({otherProperty.PropertyType})\n";
                        }
                    }
                }

                if (exists == false)
                {
                    propertyDiffs += $"Property {property.Name} does not exist on Replacement\n";
                }
            }

            foreach (var otherProperty in otherProperties)
            {
                bool exists = false;
                if (!properties.Any(x => x.Name == otherProperty.Name))
                {
                    propertyDiffs += $"Replacement property {otherProperty.Name} does not exist on Original\n";
                }
            }

            return propertyDiffs;
        }
    }
}
