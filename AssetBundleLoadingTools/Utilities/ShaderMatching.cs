using AssetBundleLoadingTools.Models.Properties;
using AssetBundleLoadingTools.Models.Shaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace AssetBundleLoadingTools.Utilities
{
    internal static class ShaderMatching
    {

        // for deserialization/cache; when we don't need detailed info on shader matches
        internal static bool ShaderInfosMatchOptimized(CompiledShaderInfo shaderInfo, CompiledShaderInfo otherShaderInfo)
        {
            return ShaderNameAndPropertiesMatch(shaderInfo.Name, otherShaderInfo.Name, shaderInfo.Properties, otherShaderInfo.Properties);
        }

        internal static bool ShaderInfoIsForShader(CompiledShaderInfo shaderInfo, Shader shader)
        {
            return ShaderNameAndPropertiesMatch(shaderInfo.Name, shader.name, shaderInfo.Properties, GetShaderProperties(shader));
        }

        private static bool ShaderNameAndPropertiesMatch(string name, string otherName, List<ShaderProperty> properties, List<ShaderProperty> otherProperties)
        {

            if (name != otherName || properties.Count != otherProperties.Count)
            {
                return false;
            }

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
                        break;
                    }
                }

                if (exists == false) return false;
            }

            return true;
        }

        internal static ShaderMatchInfo? ShaderInfosMatch(CompiledShaderInfo shaderInfo, CompiledShaderInfo otherShaderInfo)
        {
            if (shaderInfo.Name != otherShaderInfo.Name)
            {
                return null;
            }

            ShaderMatchInfo? matchInfo = new(otherShaderInfo);

            // this does not use variants for now as it's a bit too unstable between the three systems

            // actually check the other properties instead of just count
            // if "smart" checking/debugging is unimplemented, you can check count and call it a day
            // however, we need to check the otherShaderInfo properties to see if there's any *new* properties
            // if (shaderInfo.Properties.Count != otherShaderInfo.Properties.Count) matchesFully = false;

            foreach (var property in shaderInfo.Properties)
            {
                bool exists = false;
                foreach (var otherProperty in otherShaderInfo.Properties)
                {
                    // Impossible to have multiple properties with the same name
                    if(property.Name == otherProperty.Name)
                    {
                        PropertyConflictType? conflictType = null;
                        if (property.DisplayName != otherProperty.DisplayName && property.PropertyType != otherProperty.PropertyType)
                        {
                            conflictType = PropertyConflictType.DisplayNameAndType;
                        }
                        else if (property.DisplayName != otherProperty.DisplayName)
                        {
                            conflictType = PropertyConflictType.DisplayName;
                        }
                        else if (property.PropertyType != otherProperty.PropertyType)
                        {
                            conflictType = PropertyConflictType.Type;
                        }

                        if (conflictType != null)
                        {
                            matchInfo.PropertyConflictInfos.Add(new(property, otherProperty, conflictType.Value));
                        }

                        exists = true;
                        break;
                    }
                }

                if (exists == false)
                {
                    matchInfo.PropertiesMissingFromMatchShader.Add(property);
                }
            }

            foreach (var otherProperty in otherShaderInfo.Properties)
            {
                bool exists = false;
                foreach (var property in shaderInfo.Properties)
                {
                    if (property.Name == otherProperty.Name)
                    {
                        exists = true;
                        break;
                    }
                }
                if (exists == false)
                {
                    matchInfo.PropertiesMissingFromShader.Add(otherProperty);
                }
            }

            return matchInfo;
        }

        /*internal static bool ShadersMatch(Shader shader, Shader otherShader)
        {
            if (shader.name != otherShader.name) return false;
            var otherProperties = GetShaderProperties(otherShader);

            return Matches(otherShader.name, otherProperties);
        }*/

        internal static List<ShaderProperty> GetShaderProperties(Shader shader)
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
