using AssetBundleLoadingTools.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AssetBundleLoadingTools.Utilities
{
    internal class ShaderBundleWebService
    {
        static readonly HttpClient client = new HttpClient();
        public static async Task<List<string>?> GetShaderBundles()
        {
            client.BaseAddress = new Uri(Constants.ShaderBundleURL);
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                HttpResponseMessage response = await client.GetAsync(Constants.ShaderBundleDownloadPath);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                // Plugin.Log.Info(responseBody);
                Dictionary<string, List<string>>? shaderBundlePaths = JsonConvert.DeserializeObject<Dictionary<string, List<string>>?>(responseBody);
                if (shaderBundlePaths == null || shaderBundlePaths.Count == 0 || !shaderBundlePaths.ContainsKey("ShaderBundles")) return null;

                return shaderBundlePaths["ShaderBundles"];
            }
            catch (HttpRequestException e)
            {
                Plugin.Log.Info("\nWeb Exception Caught!");
                Plugin.Log.Info("Message : " + e.Message);
                return null;
            }
        }

        public static async Task<byte[]?> GetShaderBundleBytesFromURL(string bundleURL)
        {
            if (bundleURL == null) return null;
            client.BaseAddress = new Uri(Constants.ShaderBundleURL);
            // Call asynchronous network methods in a try/catch block to handle exceptions.
            try
            {
                HttpResponseMessage response = await client.GetAsync(bundleURL);
                response.EnsureSuccessStatusCode();
                var bytes = await response.Content.ReadAsByteArrayAsync();

                return bytes;
            }
            catch (HttpRequestException e)
            {
                Plugin.Log.Info("\nWeb Exception Caught!");
                Plugin.Log.Info("Message : " + e.Message);
                return null;
            }
        }
    }
}
