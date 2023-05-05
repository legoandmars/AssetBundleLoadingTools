using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AssetBundleLoadingTools.Models.Bundles
{
    public class BundleSafetyData
    {
        public string Path { get; set; }
        public bool IsDangerous { get; set; }

        [JsonConstructor]
        public BundleSafetyData(string path, bool isDangerous)
        {
            Path = path;
            IsDangerous = isDangerous;
        }
    }
}
