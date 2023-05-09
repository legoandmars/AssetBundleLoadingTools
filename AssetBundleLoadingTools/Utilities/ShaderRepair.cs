using AssetBundleLoadingTools.Core;
using AssetBundleLoadingTools.Models.Shaders;
using IPA.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace AssetBundleLoadingTools.Utilities
{
    public static class ShaderRepair
    {
        public static bool FixShadersOnGameObject(GameObject gameObject)
        {
            MainThreadCheck();

            var materials = GetMaterialsFromGameObject(gameObject);
            var shaderInfos = GetShaderInfosFromMaterials(materials);

            return ReplaceShaders(materials, shaderInfos);
        }

        public static async Task<bool> FixShadersOnGameObjectAsync(GameObject gameObject)
        {
            MainThreadCheck();
            await ShaderBundleLoader.Instance.WaitForWebBundles(); // wait to catch up on "new" online bundles 

            // a bit expensive due to the GetComponentsInChildren call, but can't be awaited
            var materials = GetMaterialsFromGameObject(gameObject);
            var shaderInfos = GetShaderInfosFromMaterials(materials);

            return await ReplaceShadersAsync(materials, shaderInfos);
        }

        public static bool FixShaderOnMaterial(Material material) => FixShadersOnMaterials(new List<Material>() { material });
        public static bool FixShadersOnMaterials(List<Material> materials)
        {
            MainThreadCheck();

            var shaderInfos = GetShaderInfosFromMaterials(materials);

            return ReplaceShaders(materials, shaderInfos);
        }

        public static async Task<bool> FixShaderOnMaterialAsync(Material material) => await FixShadersOnMaterialsAsync(new List<Material>() { material });
        public static async Task<bool> FixShadersOnMaterialsAsync(List<Material> materials)
        {
            MainThreadCheck();
            await ShaderBundleLoader.Instance.WaitForWebBundles(); // wait to catch up on "new" online bundles 

            var shaderInfos = GetShaderInfosFromMaterials(materials);

            return await ReplaceShadersAsync(materials, shaderInfos);
        }

        private static void MainThreadCheck()
        {
            if (!UnityGame.OnMainThread)
            {
                throw new InvalidOperationException("ShaderRepair methods must be called from the main thread.");
            }
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

                if (Plugin.Config.ShaderDebugging)
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

                if (Plugin.Config.ShaderDebugging)
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

        private static void FixAndDownloadLegacyShaders(GameObject gameObject)
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
        }
    }
}
