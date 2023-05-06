using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Config
{
    public class PluginConfig
    {
        public virtual bool EnableCache { get; set; } = true;
        public virtual bool EnableShaderDebugging { get; set; } = true;
    }
}
