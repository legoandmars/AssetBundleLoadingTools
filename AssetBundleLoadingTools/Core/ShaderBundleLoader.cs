using AssetBundleLoadingTools.Models.Manifests;
using AssetBundleLoadingTools.Models.Shaders;
using AssetBundleLoadingTools.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AssetBundleLoadingTools.Core
{
    public class ShaderBundleLoader
    {
        private readonly List<string> _fileExtensions = new List<string>() { "*.shaderbundle", "*.shaderbundl" };

        // TODO: unstatic :(
        private static List<ShaderBundleManifest> manifests = new();
        public ShaderBundleLoader() 
        {

            if (!Directory.Exists(Constants.ShaderBundlePath))
            {
                Directory.CreateDirectory(Constants.ShaderBundlePath);
            }
        }

        public void LoadAllBundles()
        {
            List<string> files = new();
            foreach(var fileExtension in _fileExtensions)
            {
                files.AddRange(Directory.GetFiles(Constants.ShaderBundlePath, fileExtension));
            }

            foreach(var file in files)
            {
                LoadBundle(file);
            }

            return;
        }

        private void LoadBundle(string path)
        {
            var manifest = ManifestFromZipFile(path);
            if (manifest == null) return;
            manifest.Path = path;

            // TODO: Once there's an async API available, it should be relatively straightforward to wait to load these until the first request
            // Really shouldn't need to load these right away *at all*
            var bundleStream = AssetBundleStreamFromZipFile(path);
            if(bundleStream == null) return;
            var bundle = AssetBundle.LoadFromMemory(bundleStream.ToArray());
            manifest.AssetBundle = bundle;

            //Debug.Log("why>?")
            manifests.Add(manifest);

            return;
        }

        public static CompiledShaderInfo? GetReplacementShader(CompiledShaderInfo shaderInfo)
        {
            ShaderMatchInfo? closestMatch = null;
            ShaderBundleManifest? closestManifest = null;

            foreach(var manifest in manifests)
            {
                foreach(var shader in manifest.ShadersByBundlePath.Values)
                {
                    var match = ShaderMatching.ShaderInfosMatch(shaderInfo, shader);
                    if (match == null) continue;
                    if (match.ShaderMatchType == ShaderMatchType.FullMatch)
                    {
                        LoadReplacementShaderFromBundle(shader, manifest);
                        return shader;
                    }

                    if(closestMatch == null || match.PartialMatchScore < closestMatch.PartialMatchScore)
                    {
                        closestMatch = match;
                        closestManifest = manifest;
                    }
                }
            }
            
            if(closestMatch != null && closestMatch.ShaderMatchType == ShaderMatchType.PartialMatch)
            {
                LogPartialShaderInfo(closestMatch);
            }

            if(closestMatch != null && closestManifest != null)
            {
                LoadReplacementShaderFromBundle(closestMatch.ShaderInfo, closestManifest);
            }
            return closestMatch?.ShaderInfo;
        }

        // for debugging
        public static void LogPartialShaderInfo(ShaderMatchInfo matchInfo)
        {
            Plugin.Log.Info($"Failed to find full replacement for {matchInfo.ShaderInfo.Name}; only partial match found");
            Plugin.Log.Info(JsonConvert.SerializeObject(matchInfo));
        }

        // TODO: Async
        private static void LoadReplacementShaderFromBundle(CompiledShaderInfo shaderInfo, ShaderBundleManifest manifest)
        {
            if (shaderInfo.Shader == null)
            {
                if (manifest.AssetBundle == null)
                    throw new NullReferenceException(nameof(manifest.AssetBundle));

                foreach(var bundlePathAndShaderInfo in manifest.ShadersByBundlePath)
                {
                    if (bundlePathAndShaderInfo.Value != shaderInfo) continue;
                    shaderInfo.Shader = manifest.AssetBundle.LoadAsset<Shader>(bundlePathAndShaderInfo.Key);
                }
                //manifest.AssetBundle.LoadAssetAsyncSafe<Shader>(manifest.)
                // load from bundle

            }
        }

        private ShaderBundleManifest? ManifestFromZipFile(string path)
        {
            using ZipArchive archive = ZipFile.OpenRead(path);

            var jsonEntry = archive.Entries.FirstOrDefault(i => i.Name == Constants.ManifestFileName);
            var bundleEntry = archive.Entries.FirstOrDefault(i => i.Name == Constants.BundleFileName);
            if (jsonEntry == null || bundleEntry == null) return null;

            using var manifestStream = new StreamReader(jsonEntry.Open(), Encoding.Default);
            string manifestString = manifestStream.ReadToEnd();
            ShaderBundleManifest manifest = JsonConvert.DeserializeObject<ShaderBundleManifest>(manifestString);

            return manifest;
        }

        /*private async Task<AssetBundle?> AssetBundleFromZipFileAsync(string path)
        {
            var stream = await AssetBundleStreamFromZipFileAsync(path);
            if (stream == null) return null;
            return await LoadAssetBundleFromStreamAsync(stream);
        }*/

        private MemoryStream? AssetBundleStreamFromZipFile(string path)
        {
            using ZipArchive archive = ZipFile.OpenRead(path);

            var jsonEntry = archive.Entries.FirstOrDefault(i => i.Name == Constants.ManifestFileName);
            var bundleEntry = archive.Entries.FirstOrDefault(i => i.Name == Constants.BundleFileName);
            if (jsonEntry == null || bundleEntry == null) return null;

            var seekableStream = new MemoryStream();
            bundleEntry.Open().CopyTo(seekableStream);
            seekableStream.Position = 0;
            
            return seekableStream;
        }

        private async Task<AssetBundle?> LoadAssetBundleFromStreamAsync(Stream stream)
        {
            var completion = new TaskCompletionSource<AssetBundle?>();
            var assetLoadRequest = AssetBundle.LoadFromStreamAsync(stream);

            assetLoadRequest.completed += delegate
            {
                completion.SetResult(assetLoadRequest.assetBundle);
            };

            return await completion.Task;
        }
    }
}
