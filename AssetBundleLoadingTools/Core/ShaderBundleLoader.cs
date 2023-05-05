using System;
using System.Collections.Generic;
using System.IO;
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
            Debug.Log("Attempting to load bundle..");
            //var bundle = await LoadAssetBundleFromPathAsync(path);
            //Debug.Log("BUNDLE LOADED! Deserializing time...");
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
