using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Shaders
{
    [Serializable]
    public class ShaderVariant
    {
        [JsonProperty]
        public List<string> Keywords { get; set; }

        public ShaderVariant(List<string> keywords)
        {
            Keywords = keywords;
        }

        [JsonConstructor]
        public ShaderVariant() { }
    }
}
