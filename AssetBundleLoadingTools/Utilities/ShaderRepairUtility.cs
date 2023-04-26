using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

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


        public static bool FixLegacyShaders(GameObject gameObject, string hash)
        {
            if (gameObject == null) return false;

            List<Material> sharedMaterials = new();

            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>(true))
            {
                if (renderer == null) continue;
                foreach (var material in renderer.sharedMaterials)
                {
                    if (material != null && material.shader != null && !sharedMaterials.Contains(material)) sharedMaterials.Add(material);
                }
            
            }

            foreach (var material in sharedMaterials)
            {
                var shader = material.shader;

                if (!ShaderSupported(shader))
                {
                    // TODO: replace shader
                    // below could come in useful for shader keyword checking? idk
                    //shader.keywordSpace.keywordNames
                }
            }

            return true;
        }

        private static bool ShaderSupported(Shader shader)
        {
            // Honestly i'm not really sure how to do this!
            // The built in shader properties like isSupported don't quite work for what we need
            // So we need to actually check the keywords the shader was compiled against...
            // Just going off of Unity Version or whatever would be too unreliable, and we don't want to replace *every* shader if some will already be compiled for the proper render target
            // I thought it would be easy but unity provides **absolutely ZERO** way to see what variants a shader has, much less which keywords each of those variants has
            // we need *some* way to quantify if the shader supports UNITY_SINGLE_PASS_STEREO, STEREO_INSTANCING_ON, or both.
            // This info is easily available deep within the assetbundle, but I'm not sure if there's a sane way to get it
            // https://i.imgur.com/qD0zXvf.png
            // https://i.imgur.com/a9nCfug.png
            // We might have to end up using AssetTools.NET, which I ***REALLY*** want to avoid because it will add a lot of unnecessary complexity and overhead
            // Alternatively, I could be overthinking this, and there might be some simple solution staring me in the face!

            return true;
        }

        public static bool FixAndDownloadLegacyShaders(GameObject gameObject, string hash)
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

            return FixLegacyShaders(gameObject, hash);
        }
    }
}
