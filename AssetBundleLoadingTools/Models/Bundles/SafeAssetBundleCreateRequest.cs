using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AssetBundleLoadingTools.Models.Bundles
{
    // wacky ass unity assetbundle async api middleware
    public class SafeAssetBundleCreateRequest
    {
        public event Action<SafeAssetBundleCreateRequest> completed;

        public SafeAssetBundleCreateRequest(AssetBundleCreateRequest assetBundleCreateRequest)
        {
            //assetBundleCreateRequest.completed += completed;
        }
    }
}
