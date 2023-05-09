namespace AssetBundleLoadingTools.Models.Shaders
{
    internal class ShaderDebugInfo
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
