using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AssetBundleLoadingTools.Models
{
    [Serializable]
    public class BundleData
    {
        public string Path { get; set; }
        public bool IsDangerous { get; set; }

        [JsonConstructor]
        public BundleData(string path, bool isDangerous)
        {
            Path = path;
            IsDangerous = isDangerous;
        }
    }
}
