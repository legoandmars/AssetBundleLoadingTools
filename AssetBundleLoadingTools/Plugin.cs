using AssetBundleLoadingTools.Core;
using IPA;
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

        [Init]
        public Plugin(IPALogger logger)
        {
            Instance = this;
            Log = logger;
            
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
