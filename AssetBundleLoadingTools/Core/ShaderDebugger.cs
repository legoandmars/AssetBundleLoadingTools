using AssetBundleLoadingTools.Models.Shaders;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Core
{
    // essentially just log problems with existing shaders
    public class ShaderDebugger
    {
        private static string _debuggingFilePath = Path.Combine(Constants.DebuggingPath, DateTime.Now.ToFileTimeUtc()+".shaderdebug");
        private static List<ShaderDebugInfo> _shaderDebugInfos = new();
        private static bool _debuggerAwaitingWrite = false;

        public static void AddInfoToDebugging(ShaderDebugInfo shaderDebugInfo)
        {
            if (_shaderDebugInfos.Contains(shaderDebugInfo)) return;

            _shaderDebugInfos.Add(shaderDebugInfo);
            _debuggerAwaitingWrite = true;
        }

        internal static void SerializeDebuggingInfo(object? _ = null)
        {
            if (!_debuggerAwaitingWrite) return;

            if (!Directory.Exists(Constants.DebuggingPath))
            {
                Directory.CreateDirectory(Constants.DebuggingPath);
            }

            File.WriteAllText(_debuggingFilePath, JsonConvert.SerializeObject(_shaderDebugInfos, Formatting.Indented));

            _debuggerAwaitingWrite = false;
        }
    }
}
