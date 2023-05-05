using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AssetBundleLoadingTools.Models.Bundles
{
    [Serializable]
    public class BundleSafetyData
    {
        [JsonProperty]
        public string Path { get; set; }
        [JsonProperty]
        public bool IsDangerous { get; set; }

        public BundleSafetyData(string path, bool isDangerous)
        {
            Path = path;
            IsDangerous = isDangerous;
        }

        [JsonConstructor]
        public BundleSafetyData() { }
    }
}
