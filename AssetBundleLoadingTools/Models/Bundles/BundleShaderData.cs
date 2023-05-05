﻿using AssetBundleLoadingTools.Models.Shader;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Models.Bundles
{
    // Specifically cache because assettools.NET takes SO. LONG.
    // Might not really be needed if we can do everything super fast at runtime
    // TODO: Find a way to conditionally not need CompiledShaderInfos if NeedsShaderReplacement is false
    // It's a lot of data. Maybe too much, this could cause performance problems

    [Serializable]
    public class BundleShaderData
    {
        [JsonProperty]
        public string Path { get; set; }

        [JsonProperty]
        public List<CompiledShaderInfo> CompiledShaderInfos { get; set; }

        [JsonProperty]
        public bool NeedsReplacing { get; set; }

        public BundleShaderData(string path, List<CompiledShaderInfo> compiledShaderInfos, bool needsReplacing)
        {
            Path = path;
            CompiledShaderInfos = compiledShaderInfos;
            NeedsReplacing = needsReplacing;
        }

        [JsonConstructor]
        public BundleShaderData() { }
    }
}