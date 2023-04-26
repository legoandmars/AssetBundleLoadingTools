using AssetBundleLoadingTools.Config;
using AssetBundleLoadingTools.Core;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPA.Utilities;
using IPALogger = IPA.Logging.Logger;

namespace AssetBundleLoadingTools
{
    // Formatted as a plugin to support adding future planned BSML UI & Harmony Patches
    // This could probably be made more generic but we're running on borrowed time currently
    // Really most of this (except the public APIs in Utilities) needs to be made non-static
    [Plugin(RuntimeOptions.SingleStartInit)]
    internal class Plugin
    {
        internal static Plugin Instance { get; private set; }
        /// <summary>
        /// Use to send log messages through BSIPA.
        /// </summary>
        internal static IPALogger Log { get; private set; }

        internal static PluginConfig Config { get; private set; }

        [Init]
        public Plugin(IPALogger logger, IPA.Config.Config config)
        {
            Instance = this;
            Log = logger;
            Config = config.Generated<PluginConfig>();
            
            BundleCache.ReadCache();
        }

        [OnStart]
        public void OnApplicationStart()
        {

        }

        [OnExit]
        public void OnApplicationQuit()
        {

        }

    }
}
