namespace AssetBundleLoadingTools.Config
{
    internal class PluginConfig
    {
        public virtual bool EnableCache { get; set; } = true;
        public virtual bool EnableShaderDebugging { get; set; } = true;
        public virtual bool DownloadNewBundles { get; set; } = true;
    }
}
