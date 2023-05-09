using AssetBundleLoadingTools.Config;
using AssetBundleLoadingTools.Core;
using HarmonyLib;
using IPA;
using IPA.Config.Stores;
using IPA.Utilities;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using IPALogger = IPA.Logging.Logger;

namespace AssetBundleLoadingTools
{
    // Formatted as a plugin to support adding future planned BSML UI & Harmony Patches
    // This could probably be made more generic but we're running on borrowed time currently
    // Really most of this (except the public APIs in Utilities) needs to be made non-static
    [Plugin(RuntimeOptions.SingleStartInit)]
    internal class Plugin
    {
        internal static Plugin Instance { get; private set; } = null!;
        private static readonly Harmony harmony = new("com.legoandmars.assetbundleloadingtools");

        /// <summary>
        /// Use to send log messages through BSIPA.
        /// </summary>
        internal static IPALogger Log { get; private set; } = null!;

        internal static PluginConfig Config { get; private set; } = null!;
        private static Timer? _debuggerWriteTimer { get; set; }

        [Init]
        public void Init(IPALogger logger, IPA.Config.Config config)
        {
            Instance = this;
            Log = logger;
            Config = config.Generated<PluginConfig>();

            if (Config.EnableShaderDebugging)
            {
                _debuggerWriteTimer = new Timer(ShaderDebugger.SerializeDebuggingInfo, null, 30000, 30000);
            }
        }


        [OnStart]
        public void OnApplicationStart()
        {
            harmony.PatchAll();
        }

        [OnExit]
        public void OnApplicationQuit()
        {
            _debuggerWriteTimer?.Dispose();
            harmony.UnpatchSelf();
        }
    }
}
