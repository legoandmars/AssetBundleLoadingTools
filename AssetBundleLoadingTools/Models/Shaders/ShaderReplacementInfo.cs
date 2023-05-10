using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Shaders
{
    public class ShaderReplacementInfo
    {
        public bool AllShadersReplaced { get; set; }
        public List<string> MissingShaderNames { get; set; }

        public ShaderReplacementInfo(bool allShadersReplaced, List<string>? missingShaderNames = null) 
        {
            if (missingShaderNames == null) missingShaderNames = new List<string>();

            AllShadersReplaced = allShadersReplaced;
            MissingShaderNames = missingShaderNames;
        }
    }
}
