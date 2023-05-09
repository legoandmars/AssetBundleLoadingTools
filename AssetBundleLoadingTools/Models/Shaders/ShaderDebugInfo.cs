using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Shaders
{
    public class ShaderDebugInfo
    {
        public CompiledShaderInfo ShaderInfo { get; set; }
        public ShaderMatchInfo? ShaderMatchInfo { get; set; }

        public ShaderDebugInfo(CompiledShaderInfo shaderInfo, ShaderMatchInfo? shaderMatchInfo) 
        {
            ShaderInfo = shaderInfo;
            ShaderMatchInfo = shaderMatchInfo;
        }
    }
}
