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
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace AssetBundleLoadingTools.Core
{
    public class ShaderBundleLoader
    {
        public static ShaderBundleLoader Instance { get; private set; } = null!;

        public Shader? InvalidShader = null;

        private readonly List<string> _fileExtensions = new List<string>() { "*.shaderbundle", "*.shaderbundl" };
        private List<ShaderBundleManifest> _manifests = new();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private bool _loaded = false;
        private bool _loadingStarted = false;

        public ShaderBundleLoader() 
        {
            if (!Directory.Exists(Constants.ShaderBundlePath))
            {
                Directory.CreateDirectory(Constants.ShaderBundlePath);
            }

            if (Instance == null)
            {
                Instance = this;
            }
        }

        // This ideally should not ever be called! but we need a 
        public void LoadAllBundlesIfNeeded()
        {
            if (_loaded) return;

            if (_loadingStarted)
            {
                // async is already running; cancel

                _cancellationTokenSource.Cancel();
            }

            _loadingStarted = true;

            List<string> files = new();
            foreach (var fileExtension in _fileExtensions)
            {
                files.AddRange(Directory.GetFiles(Constants.ShaderBundlePath, fileExtension));
            }

            // No async HttpClient, don't want to look into other solution right now
            var webBundlesTask = Task.Run(() => ShaderBundleWebService.DownloadAllShaderBundles(Constants.ShaderBundlePath, files));
            webBundlesTask.Wait();
            var webBundles = webBundlesTask.Result;
            files.AddRange(webBundles);

            foreach (var file in files)
            {
                var manifest = ManifestFromZipFile(file);
                var bundleStream = AssetBundleStreamFromZipFile(file);
                if (manifest == null || bundleStream == null) continue;

                var bundle = AssetBundle.LoadFromStream(bundleStream); // Needs main thread
                if (bundle == null) continue;

                manifest.Path = file;
                manifest.AssetBundle = bundle;

                _manifests.Add(manifest);
            }

            foreach (var manifest in _manifests)
            {
                foreach (var shader in manifest.ShadersByBundlePath.Values)
                {
                    if (shader.Name != Constants.InvalidShaderName) continue;

                    LoadReplacementShaderFromBundle(shader, manifest); // Needs main thread
                    InvalidShader = shader.Shader;
                    break;
                }

                if (InvalidShader != null) break;
            }

            Plugin.Log.Info($"Loaded {_manifests.Count} manifests containing {_manifests.SelectMany(x => x.ShadersByBundlePath).ToList().Count} shaders.");
            _loaded = true;
        }

        public async Task LoadAllBundlesIfNeededAsync(CancellationToken? cancellationToken = null)
        {
            if (_loaded) return;

            if (cancellationToken == null)
            {
                cancellationToken = _cancellationTokenSource.Token;
            }

            if (_loadingStarted || cancellationToken.Value.IsCancellationRequested)
            {
                await WaitForBundleToLoad();
                return;
            }

            _loadingStarted = true;

            List<string> files = new();
            foreach(var fileExtension in _fileExtensions)
            {
                files.AddRange(Directory.GetFiles(Constants.ShaderBundlePath, fileExtension));
            }

            // Search for web bundles
            var webBundles = await ShaderBundleWebService.DownloadAllShaderBundles(Constants.ShaderBundlePath, files);
            files.AddRange(webBundles);

            foreach(var file in files)
            {
                if (cancellationToken.Value.IsCancellationRequested)
                {
                    await WaitForBundleToLoad();
                    return;
                }

                var manifest = ManifestFromZipFile(file);
                var bundleStream = AssetBundleStreamFromZipFile(file);
                if (manifest == null || bundleStream == null) continue;

                var bundle = await LoadAssetBundleFromStreamAsync(bundleStream); // Needs main thread
                if (bundle == null) continue;

                manifest.Path = file;
                manifest.AssetBundle = bundle;

                _manifests.Add(manifest);
            }

            foreach(var manifest in _manifests)
            {
                foreach(var shader in manifest.ShadersByBundlePath.Values)
                {
                    if (shader.Name != Constants.InvalidShaderName) continue;

                    if (cancellationToken.Value.IsCancellationRequested)
                    {
                        await WaitForBundleToLoad();
                        return;
                    }

                    await LoadReplacementShaderFromBundleAsync(shader, manifest); // Needs main thread
                    InvalidShader = shader.Shader;
                    break;
                }

                if (InvalidShader != null) break;
            }

            Plugin.Log.Info($"Loaded {_manifests.Count} manifests containing {_manifests.SelectMany(x => x.ShadersByBundlePath).ToList().Count} shaders.");
            _loaded = true;
        }

        public async Task WaitForBundleToLoad()
        {
            while (!_loaded)
            {
                await Task.Delay(10);
            }
            return;
        }

        public (CompiledShaderInfo?, ShaderMatchInfo?) GetReplacementShader(CompiledShaderInfo shaderInfo)
        {
            ShaderMatchInfo? closestMatch = null;
            ShaderBundleManifest? closestManifest = null;

            foreach(var manifest in _manifests)
            {
                foreach(var shader in manifest.ShadersByBundlePath.Values)
                {
                    var match = ShaderMatching.ShaderInfosMatch(shaderInfo, shader);
                    if (match == null) continue;
                    if (match.ShaderMatchType == ShaderMatchType.FullMatch)
                    {
                        LoadReplacementShaderFromBundle(shader, manifest);
                        return (shader, match);
                    }

                    if(closestMatch == null || match.PartialMatchScore < closestMatch.PartialMatchScore)
                    {
                        closestMatch = match;
                        closestManifest = manifest;
                    }
                }
            }
            
            if(closestMatch != null && closestManifest != null)
            {
                LoadReplacementShaderFromBundle(closestMatch.ShaderInfo, closestManifest);
            }
            return (closestMatch?.ShaderInfo, closestMatch);
        }

        // TODO: Optimize on non-debug mode
        public async Task<(CompiledShaderInfo?, ShaderMatchInfo?)> GetReplacementShaderAsync(CompiledShaderInfo shaderInfo)
        {
            ShaderMatchInfo? closestMatch = null;
            ShaderBundleManifest? closestManifest = null;

            foreach (var manifest in _manifests)
            {
                foreach (var shader in manifest.ShadersByBundlePath.Values)
                {
                    var match = ShaderMatching.ShaderInfosMatch(shaderInfo, shader);
                    if (match == null) continue;
                    if (match.ShaderMatchType == ShaderMatchType.FullMatch)
                    {
                        await LoadReplacementShaderFromBundleAsync(shader, manifest);
                        return (shader, match);
                    }

                    if (closestMatch == null || match.PartialMatchScore < closestMatch.PartialMatchScore)
                    {
                        closestMatch = match;
                        closestManifest = manifest;
                    }
                }
            }

            if (closestMatch != null && closestManifest != null)
            {
                await LoadReplacementShaderFromBundleAsync(closestMatch.ShaderInfo, closestManifest);
            }
            return (closestMatch?.ShaderInfo, closestMatch);
        }

        private void LoadReplacementShaderFromBundle(CompiledShaderInfo shaderInfo, ShaderBundleManifest manifest)
        {
            if (shaderInfo.Shader != null) return;
            if (manifest.AssetBundle == null) 
            {
                throw new NullReferenceException(nameof(manifest.AssetBundle));
            }

            foreach (var bundlePathAndShaderInfo in manifest.ShadersByBundlePath)
            {
                if (bundlePathAndShaderInfo.Value != shaderInfo) continue;
                shaderInfo.Shader = manifest.AssetBundle.LoadAsset<Shader>(bundlePathAndShaderInfo.Key);
            }
        }

        private async Task LoadReplacementShaderFromBundleAsync(CompiledShaderInfo shaderInfo, ShaderBundleManifest manifest)
        {
            if (shaderInfo.Shader != null) return;
            if (manifest.AssetBundle == null)
            {
                throw new NullReferenceException(nameof(manifest.AssetBundle));
            }

            foreach (var bundlePathAndShaderInfo in manifest.ShadersByBundlePath)
            {
                if (bundlePathAndShaderInfo.Value != shaderInfo) continue;
                var shader = await LoadShaderFromPathAsync(manifest.AssetBundle, bundlePathAndShaderInfo.Key);
                
                if (shader != null)
                {
                    shaderInfo.Shader = shader;
                }
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

        private async Task<Shader?> LoadShaderFromPathAsync(AssetBundle bundle, string assetName)
        {
            var completion = new TaskCompletionSource<Shader?>();
            var assetLoadRequest = bundle.LoadAssetAsync<Shader>(assetName);

            assetLoadRequest.completed += delegate
            {
                completion.SetResult(assetLoadRequest.asset as Shader);
            };

            return await completion.Task;
        }
    }
}
