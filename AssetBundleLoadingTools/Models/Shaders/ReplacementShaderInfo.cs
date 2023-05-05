using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Shader
{
    [Serializable]
    public class ReplacementShaderInfo
    {
        [JsonProperty]
        public string BundleName { get; set; }

        [JsonProperty]
        public string ShaderBundlePath { get; set; }
        /*
        public string Name { get; set; }
        public string BundlePath { get; set; }
        public List<ShaderProperty> Properties { get; set; }

        public List<ShaderVRPlatform> Platforms { get; set; }

        [JsonConstructor]
        public ReplacementShaderInfo(string name, List<ShaderProperty> properties, List<ShaderVRPlatform> platforms)
        {
            Name = name;
            Properties = properties;
            Platforms = platforms;
        }*/
    }
}
