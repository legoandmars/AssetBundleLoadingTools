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

namespace AssetBundleLoadingTools.Utilities
{
    public static class ShaderRepairUtility
    {
        // Ideally "new" fixed shaders will be distributed in easy to open AssetBundles that you can just load if necessary
        // I've been doing this internally for testing with the following structure:
        // Prefab Assets/_Shaders.prefab
        //    - GameObject ShaderParent
        //       - List<GameObject> ShaderObject
        // 
        // To load these shader assetbundles, I'm just using gameObject.GetComponentsInChildren<Renderer>()
        // definitely not ideal!
        // It would almost certainly be better to have a format that's just directly adding a bunch of different shaders to the assetbundle when building in editor
        // Then you can just do AssetBundle.LoadAllAssets<Shader>() and you're off to the races
        // I'm curious if you would run into problems with the "add a bunch of assets to the assetbundle individually" method after you started getting into the hundreds of shaders in a single file, though...

        private static AssetsManager _assetsManager = new();

        // really needs to be expanded into multiple methods
        internal static bool FixLegacyShaders(GameObject gameObject, string assetBundlePath, string hash)
        {
            if (gameObject == null) return false;

            Dictionary<string, List<string>>? keywords = null;
            // var keywords = LoadShaderKeywordsFromBundle(assetBundlePath);
            bool fixable = true;

            List<Material> sharedMaterials = new();
            List<Shader> shaders = new();
            List<CompiledShaderInfo> shaderInfos = new();

            // TODO: Is there anything that uses shaders that isn't a renderer?
            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null || material.shader == null) continue;

                    if (!sharedMaterials.Contains(material)) sharedMaterials.Add(material);
                    if (!shaders.Contains(material.shader)) shaders.Add(material.shader);
                }
            }

            // setup cache
            // TODO: own method
            BundleShaderData? shaderCache = null;

            if (Plugin.Config.EnableCache)
            {
                shaderCache = Caching.GetCachedBundleShaderData(hash);
                if(shaderCache == null)
                {
                    shaderCache = new BundleShaderData(new List<CompiledShaderInfo>(), true);
                }
            }

            bool cacheEnabled = shaderCache != null && Plugin.Config.EnableCache;

            if (cacheEnabled && !shaderCache!.NeedsReplacing)
            {
                return true;
            }

            foreach (var shader in shaders)
            {
                // convert to infos; first check cache
                bool foundCachedShader = false;

                if (cacheEnabled)
                {
                    foreach (var cachedShader in shaderCache!.CompiledShaderInfos)
                    {
                        if(cachedShader == null) continue;
                        if(ShaderMatching.ShaderInfoIsForShader(cachedShader, shader))
                        {
                            // match found, no need to get properties and keywords
                            // create new object to avoid modifying cached objects
                            var newShaderInfo = new CompiledShaderInfo(cachedShader.Name, cachedShader.Properties, cachedShader.VariantInfo);
                            newShaderInfo.Shader = shader;

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

                    if (keywords.TryGetValue(shader.name, out var shaderKeywords))
                    {
                        var shaderInfo = new CompiledShaderInfo(shader, shaderKeywords);
                        shaderInfos.Add(shaderInfo);

                        if (cacheEnabled)
                        {
                            // cache shader
                            shaderCache!.CompiledShaderInfos.Add(shaderInfo);
                        }
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
                        foreach (var material in sharedMaterials)
                        {
                            if (material.shader == shaderInfo.Shader)
                            {
                                material.shader = ShaderBundleLoader.InvalidShader;
                            }
                        }
                    }
                    else
                    {
                        foreach (var material in sharedMaterials)
                        {
                            if (material.shader == shaderInfo.Shader)
                            {
                                Debug.Log($"Successfully replaced {shaderInfo.Name} with {replacement.Name}");
                                material.shader = replacement.Shader;
                            }
                        }
                    }

                    if (Plugin.Config.EnableShaderDebugging)
                    {
                        ShaderDebugger.AddInfoToDebugging(hash, new ShaderDebugInfo(shaderInfo, replacementMatchInfo));
                    }
                    // material.shader = shader;
                }
            }

            return fixable;
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
