using AssetBundleLoadingTools.Models.Manifests;
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
        }

        private async void LoadBundle(string path)
        {
            Debug.Log(path);
            Debug.Log("Attempting to load bundle..");

            var manifest = await ManifestFromZipFileAsync(path);
            // serialize
            // Debug.Log(JsonConvert.SerializeObject(manifest, Formatting.Indented));
            //var bundle = await LoadAssetBundleFromPathAsync(path);
            //Debug.Log("BUNDLE LOADED! Deserializing time...");
        }

        private async Task<ShaderBundleManifest?> ManifestFromZipFileAsync(string path)
        {
            var completion = new TaskCompletionSource<ShaderBundleManifest?>();

            using (ZipArchive archive = ZipFile.OpenRead(path))
            {
                var jsonEntry = archive.Entries.FirstOrDefault(i => i.Name == Constants.ManifestFileName);
                var bundleEntry = archive.Entries.FirstOrDefault(i => i.Name == Constants.BundleFileName);
                if (jsonEntry == null || bundleEntry == null)
                {
                    completion.SetResult(null);
                }

                using var manifestStream = new StreamReader(jsonEntry.Open(), Encoding.Default);
                string manifestString = await manifestStream.ReadToEndAsync();
                ShaderBundleManifest manifest = JsonConvert.DeserializeObject<ShaderBundleManifest>(manifestString);

                completion.SetResult(manifest);
            }

            return await completion.Task;
        }


        private async Task<AssetBundle?> LoadAssetBundleFromPathAsync(string path)
        {
            var completion = new TaskCompletionSource<AssetBundle?>();
            var assetLoadRequest = AssetBundle.LoadFromFileAsync(path);

            assetLoadRequest.completed += delegate
            {
                completion.SetResult(assetLoadRequest.assetBundle);
            };

            return await completion.Task;
        }
    }
}
