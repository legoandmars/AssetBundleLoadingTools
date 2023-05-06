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
using AssetBundleLoadingTools.Models.Shader;
using AssetBundleLoadingTools.Core;
using System.IO;

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

        public static bool FixLegacyShaders(GameObject gameObject, string assetBundlePath, string hash)
        {
            if (gameObject == null) return false;
            var keywords = LoadShaderKeywordsFromBundle(assetBundlePath);

            return FixLegacyShadersInternal(gameObject, keywords, hash);

        }

        private static bool FixLegacyShadersInternal(GameObject gameObject, Dictionary<string, List<string>> keywords, string hash)
        {
            bool fixable = true;

            foreach (var shaderKeywordData in keywords)
            {
                Debug.Log($"{shaderKeywordData.Key}: {string.Join(", ", shaderKeywordData.Value)}");
            }

            List<Material> sharedMaterials = new();
            List<CompiledShaderInfo> shaderInfos = new();

            // TODO: Is there anything that uses shaders that isn't a renderer?
            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material == null || material.shader == null) continue;

                    if (!sharedMaterials.Contains(material)) sharedMaterials.Add(material);
                    if (!shaderInfos.Any(x => x.Shader == material.shader) && keywords.TryGetValue(material.shader.name, out var shaderKeywords))
                    {
                        shaderInfos.Add(new(material.shader, shaderKeywords));
                    }
                }
            }

            foreach (var shaderInfo in shaderInfos)
            {
                // shader replacement pass
                if (!shaderInfo.IsSupported)
                {
                    Debug.Log("HUH");
                    // Debug.Log("NOT SUPPORTED! REPLACE!");
                    // try to get replacement
                    var replacement = ShaderBundleLoader.GetReplacementShader(shaderInfo);
                    if (replacement == null) 
                    {
                        foreach (var material in sharedMaterials)
                        {
                            if (material.shader == shaderInfo.Shader)
                            {
                                material.shader = null;
                            }
                        }

                        continue;
                    }

                    foreach (var material in sharedMaterials)
                    {
                        if (material.shader == shaderInfo.Shader)
                        {
                            Debug.Log($"Successfully replaced {shaderInfo.Name} with {replacement.Name}");
                            material.shader = replacement.Shader;
                        }
                    }

                    // material.shader = shader;
                }
            }

            return fixable;
        }

        private static Shader? GetShaderReplacement(CompiledShaderInfo shaderInfo)
        {
            // unimplemented
            return null;
        }

        // Not quite the same as global keywords, this includes properties too
        private static Dictionary<string, List<string>> LoadShaderKeywordsFromBundle(string assetBundlePath)
        {
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
