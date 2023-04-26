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
        public bool IsDangerous;

        [JsonConstructor]
        public BundleData(bool isDangerous)
        {
            IsDangerous = isDangerous;
        }
    }
}
