﻿using AssetBundleLoadingTools.Models.Shaders;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;

namespace AssetBundleLoadingTools.Models.Manifests
{
    // Specifically needs a manifest because in-editor serialization is done with unity's JSON
    // Which, *somehow*, still cannot serialize a List<> or Dictionary<>

    internal class ShaderBundleManifest
    {
        [JsonProperty("ManifestEntries")]
        private List<ShaderBundleManifestEntry>? _manifestEntries { get; set; } // hack to work with legacy unity projects

        [JsonIgnore]
        public Dictionary<string, CompiledShaderInfo> ShadersByBundlePath { get; set; }

        [JsonIgnore]
        public string? Path { get; set; }

        [JsonIgnore]
        public AssetBundle? AssetBundle { get; set; }

        public long TimeSerialized { get; set; }

        public string UnityVersion { get; set; } // could handle bundles compiled for future versions; probably unnecessary

        [JsonConstructor]
        public ShaderBundleManifest(List<ShaderBundleManifestEntry> manifestEntries, long timeSerialized, string unityVersion)
        {
            TimeSerialized = timeSerialized;
            UnityVersion = unityVersion;

            ShadersByBundlePath = new();

            foreach(var entry in manifestEntries)
            {
                ShadersByBundlePath.Add(entry.BundlePath, entry.ShaderInfo);
            }

            _manifestEntries = null; // literally only needed for serialization
        }

        // mostly for debugging, this will never be serialized in plugin
        [OnSerializing] 
        public void OnSerializing(StreamingContext context)
        {
            _manifestEntries = new();
            foreach(var entry in ShadersByBundlePath)
            {
                _manifestEntries.Add(new(entry.Key, entry.Value));
            }
        }
    }
}
