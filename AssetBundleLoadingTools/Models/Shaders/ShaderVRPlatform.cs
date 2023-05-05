using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Shader
{
    [System.Serializable]
    public enum ShaderVRPlatform
    {
        SinglePass,
        SinglePassInstanced,
        MultiPass // ? unsure if this is used anywhere
    }
}
