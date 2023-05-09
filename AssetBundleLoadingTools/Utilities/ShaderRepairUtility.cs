using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;
using AssetsTools.NET;
using AssetsTools.NET.Extra;
using AssetBundleLoadingTools.Models.Shaders;
using AssetBundleLoadingTools.Core;
using System.IO;
using Caching = AssetBundleLoadingTools.Core.Caching;
using AssetBundleLoadingTools.Models.Bundles;
using System.Security.Policy;
using IPA.Utilities;

namespace AssetBundleLoadingTools.Utilities
{
    public static class ShaderRepairUtility
    {
        public static bool FixShadersOnGameObject(GameObject gameObject)
        {
            if (!UnityGame.OnMainThread)
            {
                throw new InvalidOperationException("ShaderRepair methods must be called from the main thread.");
            }

            ShaderBundleLoader.Instance.LoadAllBundlesIfNeeded();

            // a bit expensive due to the GetComponentsInChildren call, but can't be awaited
            var materials = GetMaterialsFromGameObject(gameObject);
            var shaderInfos = GetShaderInfosFromMaterials(materials);

            return ReplaceShaders(materials, shaderInfos);
        }

        public static async Task<bool> FixShadersOnGameObjectAsync(GameObject gameObject)
        {
            if (!UnityGame.OnMainThread)
            {
                throw new InvalidOperationException("ShaderRepair methods must be called from the main thread.");
            }

            await ShaderBundleLoader.Instance.LoadAllBundlesIfNeededAsync();

            // a bit expensive due to the GetComponentsInChildren call, but can't be awaited
            var materials = GetMaterialsFromGameObject(gameObject);
            var shaderInfos = GetShaderInfosFromMaterials(materials);

            return await ReplaceShadersAsync(materials, shaderInfos);
        }

        public static bool FixShaderOnMaterial(Material material) => FixShadersOnMaterials(new List<Material>() { material });
        public static bool FixShadersOnMaterials(List<Material> materials)
        {
            if (!UnityGame.OnMainThread)
            {
                throw new InvalidOperationException("ShaderRepair methods must be called from the main thread.");
            }

            ShaderBundleLoader.Instance.LoadAllBundlesIfNeeded();

            // a bit expensive due to the GetComponentsInChildren call, but can't be awaited
            var shaderInfos = GetShaderInfosFromMaterials(materials);

            return ReplaceShaders(materials, shaderInfos);
        }

        public static async Task<bool> FixShaderOnMaterialAsync(Material material) => await FixShadersOnMaterialsAsync(new List<Material>() { material });
        public static async Task<bool> FixShadersOnMaterialsAsync(List<Material> materials)
        {
            if (!UnityGame.OnMainThread)
            {
                throw new InvalidOperationException("ShaderRepair methods must be called from the main thread.");
            }

            await ShaderBundleLoader.Instance.LoadAllBundlesIfNeededAsync();

            // a bit expensive due to the GetComponentsInChildren call, but can't be awaited
            var shaderInfos = GetShaderInfosFromMaterials(materials);

            return await ReplaceShadersAsync(materials, shaderInfos);
        }

        private static async Task<bool> ReplaceShadersAsync(List<Material> materials, List<CompiledShaderInfo> shaderInfos)
        {
            foreach (var shaderInfo in shaderInfos)
            {
                if (shaderInfo.IsSupported) continue;

                var (replacement, replacementMatchInfo) = await ShaderBundleLoader.Instance.GetReplacementShaderAsync(shaderInfo); // main async difference is how assetbundle.load is called

                foreach (var material in materials)
                {
                    if (material.shader != shaderInfo.Shader) continue;

                    if (replacement != null)
                    {
                        material.shader = replacement.Shader;
                    }
                    else
                    {
                        material.shader = ShaderBundleLoader.Instance.InvalidShader;
                    }
                }

                if (Plugin.Config.EnableShaderDebugging)
                {
                    ShaderDebugger.AddInfoToDebugging(new ShaderDebugInfo(shaderInfo, replacementMatchInfo));
                }
            }

            return true;
        }

        private static bool ReplaceShaders(List<Material> materials, List<CompiledShaderInfo> shaderInfos)
        {
            foreach (var shaderInfo in shaderInfos)
            {
                if (shaderInfo.IsSupported) continue;

                var (replacement, replacementMatchInfo) = ShaderBundleLoader.Instance.GetReplacementShader(shaderInfo); // main async difference is how assetbundle.load is called

                foreach (var material in materials)
                {
                    if (material.shader != shaderInfo.Shader) continue;

                    if (replacement != null)
                    {
                        material.shader = replacement.Shader;
                    }
                    else
                    {
                        material.shader = ShaderBundleLoader.Instance.InvalidShader;
                    }
                }

                if (Plugin.Config.EnableShaderDebugging)
                {
                    ShaderDebugger.AddInfoToDebugging(new ShaderDebugInfo(shaderInfo, replacementMatchInfo));
                }
            }

            return true;
        }

        private static List<CompiledShaderInfo> GetShaderInfosFromMaterials(List<Material> materials) 
        {
            var shaders = materials.Select(x => x.shader).Distinct();
            List<CompiledShaderInfo> shaderInfos = new();

            foreach (var shader in shaders)
            {
                var shaderInfo = new CompiledShaderInfo(shader, GetKeywordsFromShader(shader));
                shaderInfos.Add(shaderInfo);
            }

            return shaderInfos;
        }

        private static List<Material> GetMaterialsFromGameObject(GameObject gameObject)
        {
            List<Material> sharedMaterials = new();

            // TODO: Is there anything that uses shaders that isn't a renderer?
            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null || material.shader == null) continue;

                    if (!sharedMaterials.Contains(material)) sharedMaterials.Add(material);
                }
            }

            return sharedMaterials;
        }

        private static List<string> GetKeywordsFromShader(Shader shader)
        {
            // Hardcoded for now, eventually swapped in for pointer magic

            return new List<string> { Constants.SinglePassKeyword };
        }


        // i am so tired i'm just copy-pasting rn
        // eventually this will not be copy-pasted and will use shared methods
        internal static bool ReplaceShaderOnMaterial(Material material, string assetBundlePath, string hash)
        {

            Dictionary<string, List<string>>? keywords = null;

            List<CompiledShaderInfo> shaderInfos = new();

            // setup cache
            // TODO: own method
            BundleShaderData? shaderCache = null;

            if (Plugin.Config.EnableCache)
            {
                shaderCache = Caching.GetCachedBundleShaderData(hash);
                if (shaderCache == null)
                {
                    shaderCache = new BundleShaderData(new List<CompiledShaderInfo>(), true);
                }
            }

            bool cacheEnabled = shaderCache != null && Plugin.Config.EnableCache;

            if (cacheEnabled && !shaderCache!.NeedsReplacing)
            {
                return true;
            }


            // convert to infos; first check cache
            bool foundCachedShader = false;

            if (cacheEnabled)
            {
                foreach (var cachedShader in shaderCache!.CompiledShaderInfos)
                {
                    if (cachedShader == null) continue;
                    if (ShaderMatching.ShaderInfoIsForShader(cachedShader, material.shader))
                    {
                        // match found, no need to get properties and keywords
                        // create new object to avoid modifying cached objects
                        var newShaderInfo = new CompiledShaderInfo(cachedShader.Name, cachedShader.Properties, cachedShader.VariantInfo);
                        newShaderInfo.Shader = material.shader;

                        shaderInfos.Add(newShaderInfo);

                        foundCachedShader = true;
                        break;
                    }
                }
            }

            if (!foundCachedShader)
            {
                if (keywords == null)
                {
                    // load keywords for model... this is some SLOW SHIT right now!
                    keywords = LoadShaderKeywordsFromBundle(assetBundlePath);
                }

                if (keywords.TryGetValue(material.shader.name, out var shaderKeywords))
                {
                    var shaderInfo = new CompiledShaderInfo(material.shader, shaderKeywords);
                    shaderInfos.Add(shaderInfo);

                    if (cacheEnabled)
                    {
                        // cache shader
                        shaderCache!.CompiledShaderInfos.Add(shaderInfo);
                    }
                }
            }

            if (cacheEnabled)
            {
                shaderCache!.NeedsReplacing = shaderCache.CompiledShaderInfos.Any(x => !x.IsSupported);
                Caching.AddShaderDataToCache(hash, shaderCache);
            }

            foreach (var shaderInfo in shaderInfos)
            {
                // shader replacement pass
                if (!shaderInfo.IsSupported)
                {
                    Debug.Log("HUH");
                    // Debug.Log("NOT SUPPORTED! REPLACE!");
                    // try to get replacement
                    var (replacement, replacementMatchInfo) = ShaderBundleLoader.GetReplacementShader(shaderInfo);
                    if (replacement == null)
                    {
                        if (material.shader == shaderInfo.Shader)
                        {
                            material.shader = ShaderBundleLoader.InvalidShader;
                        }
                    }
                    else
                    {
                        if (material.shader == shaderInfo.Shader)
                        {
                            Debug.Log($"Successfully replaced {shaderInfo.Name} with {replacement.Name}");
                            material.shader = replacement.Shader;
                        }
                    }

                    if (Plugin.Config.EnableShaderDebugging)
                    {
                        ShaderDebugger.AddInfoToDebugging(hash, new ShaderDebugInfo(shaderInfo, replacementMatchInfo));
                    }
                    // material.shader = shader;
                }
            }

            return true; // temporary
        }

        private static Shader? GetShaderReplacement(CompiledShaderInfo shaderInfo)
        {
            // unimplemented
            return null;
        }

        // very temporary; just wanted temporary trail support without having to call keywords many times
        private static Dictionary<string, Dictionary<string, List<string>>> _localKeywordsCache = new(); // what the fuck

        // Not quite the same as global keywords, this includes properties too
        private static Dictionary<string, List<string>> LoadShaderKeywordsFromBundle(string assetBundlePath)
        {
            if(_localKeywordsCache.TryGetValue(assetBundlePath, out var list)) 
            { 
                return list; 
            }
            // Fairly scuffed solution using AssetsTools.NET. this might break things, but it's the only reasonable option unless somebody can figure out how to get the data from DirectX using native code

            var bundle = _assetsManager.LoadBundleFile(assetBundlePath, true);
            var assetsFileInstance = _assetsManager.LoadAssetsFileFromBundle(bundle, 0, false);
            var assetsFile = assetsFileInstance.file;

            Dictionary<string, List<string>> keywords = new();

            foreach (var shaderInfo in assetsFile.GetAssetsOfType(AssetClassID.Shader))
            {
                var shaderBase = _assetsManager.GetBaseField(assetsFileInstance, shaderInfo);

                var shaderName = shaderBase["m_ParsedForm"]["m_Name"].AsString;
                keywords[shaderName] = new List<string>();

                var subShaders = shaderBase["m_ParsedForm"]["m_SubShaders.Array"].Children;
                foreach(var subShader in subShaders)
                {
                    var passes = subShader["m_Passes.Array"].Children;
                    foreach(var pass in passes)
                    {
                        var nameIndices = pass["m_NameIndices.Array"].Children;
                        foreach(var nameIndex in nameIndices)
                        {
                            var keywordName = nameIndex["first"].AsString;
                            if(!keywords[shaderName].Contains(keywordName)) keywords[shaderName].Add(keywordName);
                        }
                    }
                }
            }

            _localKeywordsCache.Add(assetBundlePath, keywords);
            return keywords;
        }


        public static bool FixAndDownloadLegacyShaders(GameObject gameObject, string assetBundlePath, string hash)
        {
            // unimplemented
            // hopefully this will eventually:
            // 1) fix the shaders it can
            // 2) if any shaders are missing, ask for consent to download more / upload model metadata
            // 3) if permission granted, check for shaders in API
            // 4) if shaders exist in API, download and cache locally for future use

            // I am unsure if it is 100% necessary to ask for a download prompt, as it *would* be smoother to do everything automatically
            // However, I am worried about a situation such as: somebody has a private/secret saber, a shader is broken, and they are understandably upset about some data being uploaded
            // Maybe just check the model's hash for matches, and if *that* exists don't prompt, but otherwise do

            // The actual necessary parameters you would need to send to the web api:
            // Hash, List<Shader> = (ShaderName, List<Properties>)
            // Properties list is very important because some people edit shaders with the same name to have slightly different properties based on usecase

            return FixLegacyShaders(gameObject, assetBundlePath, hash);
        }
    }
}
