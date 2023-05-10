using AssetBundleLoadingTools.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;


namespace AssetBundleLoadingTools.Models.Shaders
{
    public class CompiledShaderInfo
    {
        [JsonIgnore]
        public Shader Shader { get; set; }

        public string Name { get; set; }

        public List<ShaderProperty> Properties { get; set; }

        public ShaderVariantInfo VariantInfo { get; set; }
        
        [JsonIgnore]
        public bool IsSinglePassInstancedSupported { get; private set; }

        // **Technically** this can be set to check whatever
        // For now, we're just going to hardcode the variant check for Beat Saber
        // Maybe eventually this can cover other things like Platform (PC/Android?)
        [JsonIgnore]
        public bool IsSupported
        { 
            get 
            {
                return VariantInfo.IsSinglePassInstanced ||
                       IsSinglePassInstancedSupported;
            } 
        }

        [JsonConstructor]
        public CompiledShaderInfo(string name, List<ShaderProperty> properties, ShaderVariantInfo variantInfo) 
        {
            Name = name;
            Properties = properties;
            VariantInfo = variantInfo;

            // shader always set by ShaderBundleLoader before call
            Shader = null!;
        }

        // WARNING: If you call this constructor on anything but the main thread, it will crash
        public CompiledShaderInfo(Shader shader, List<string> keywords)
        {
            Shader = shader;
            Name = shader.name;
            VariantInfo = new ShaderVariantInfo(keywords);
            Properties = ShaderMatching.GetShaderProperties(shader);
            IsSinglePassInstancedSupported = ShaderReader.IsSinglePassInstancedSupported(shader);

            // TODO: implement cache somewhere here(?)
        }
    }
}
