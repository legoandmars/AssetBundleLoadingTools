using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Shader
{
    [Serializable]
    public class CompiledShaderInfo
    {
        public string Name { get; set; }
        public List<ShaderProperty> Properties { get; set; }

        public List<ShaderVRPlatform> Platforms { get; set; }

        // TODO: "matching shader" check

        [JsonConstructor]
        public CompiledShaderInfo(string name, List<ShaderProperty> properties, List<ShaderVRPlatform> platforms)
        {
            Name = name;
            Properties = properties;
            Platforms = platforms;
        }

        // could eventually need variant/subshader/pass info if there's collisions
        // a shader should ALWAYS match if it's being replaced - if somebody's adding properties, the shader likely works differently, and it **SHOULD NOT** replace it
    }
}
