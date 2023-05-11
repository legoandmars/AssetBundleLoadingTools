# AssetBundleLoadingTools
A Beat Saber library that adds utilities to alleviate various problems caused by the Unity 2021 upgrade:
- In Unity 2020+, UnityEvents have [a new field that can be exploited to execute arbitrary code](https://blog.includesecurity.com/2021/06/hacking-unity-games-malicious-unity-game-objects/)
- Because of the change from `Single Pass` to `Single Pass Instanced`, all previously compiled shaders do not show up properly ingame

# Features
- Automatically harmony patches `UnityEvent`s to be safe when loaded. If you're maintaining a Beat Saber mod that loads any sort of user-generated AssetBundles, it is **VERY IMPORTANT** you add this library as a dependency to avoid malicious assetbundles from executing code.

- API for fixing broken shaders on AssetBundles built without `Single Pass Instanced` support. Fixes are provided using pre-built `.shaderbundle` files that contain common shaders. Unfortunately, not every shader can be fixed using this method, especially shaders built for a specific purpose or a specific model.

- Easy-to-use `.unitypackage` for exporting `.shaderbundle` files from existing Unity projects & GameObjects.

- Support for auto-updating `.shaderbundle` files when a new public bundle is added to this git repository.

- Fairly verbose `ShaderDebugging` config option that details exactly what shaders are being loaded at runtime, if they're supported, and any differences between the same shader detected in the `.shaderbundle` library, if found.

# Usage
### Example Usage
```csharp
using AssetBundleLoadingTools.Utilities;
using AssetBundleLoadingTools.Models.Shaders;

var assetBundle = await AssetBundleExtensions.LoadFromFileAsync(bundlePath);
var gameObject = await AssetBundleExtensions.LoadAssetAsync<GameObject>(assetName);
// Fix shaders by comparing against .shaderbundle library 
var replacementInfo = await ShaderRepair.FixShadersOnGameObjectAsync(gameObject);

// Handle replacement information
if (!replacementInfo.AllShadersReplaced)
{
    // Not all shaders could be converted. Handle this however you feel is appropriate
    // replacementInfo.MissingShaderNames will let you display the list of shaders that failed to load
}
```

### Advanced Usage
```csharp
using AssetBundleLoadingTools.Utilities;
using AssetBundleLoadingTools.Models.Shaders;

// Some models have custom types that aren't renderers and contain a material
// For example, custom sabers have CustomTrails with a TrailRenderer property
// These can't be automatically found with FixShadersOnGameObject, and it's your reponsibility to manually provide them

// IMPORTANT: Always call only *one* ShaderRepair method for each gameobject.
// If you call FixShadersOnGameObject on a saber then FixShaderOnMaterial on the saber's trails, 
// the second call is missing important material context from the first call

var assetBundle = await AssetBundleExtensions.LoadFromFileAsync(bundlePath);
var gameObject = await AssetBundleExtensions.LoadAssetAsync<GameObject>(assetName);

List<Material> materials = ShaderRepair.GetMaterialsFromGameObjectRenderers(gameObject);

// Manually add CustomTrails to materials list
foreach (var customTrail in gameObject.GetComponentsInChildren<CustomTrail>(true))
{
    if (!materials.Contains(customTrail.TrailMaterial))
    {
        materials.Add(customTrail.TrailMaterial);
    }
}

// Fix shaders by comparing against .shaderbundle library 
var replacementInfo = await ShaderRepair.FixShadersOnMaterialsAsync(materials);
```

### Synchronous Usage
```csharp
using AssetBundleLoadingTools.Utilities;
using AssetBundleLoadingTools.Models.Shaders;

// NOTE: Although the library contains synchronous methods for everything, it's discouraged
// Please use async for model loading & shader replacement whenever possible
// Large models may involve loading a lot of shaders and can cause noticable lag spikes

var assetBundle = AssetBundle.LoadFromFile(bundlePath);
var gameObject = assetBundle.LoadAsset<GameObject>(assetName);
 // Fix shaders by comparing against .shaderbundle library
var replacementInfo = ShaderRepair.FixShadersOnGameObject(gameObject);
```
### AssetBundleExtensions
```csharp
using AssetBundleLoadingTools.Utilities;

// AssetBundleLoadingTools contains some extensions to make awaiting AssetBundles and assets easier
await AssetBundleExtensions.LoadFromFileAsync(path); 
await AssetBundleExtensions.LoadFromMemoryAsync(bytes); 
await AssetBundleExtensions.LoadFromStreamAsync(stream); 

// Generic method for loading assets similar to AssetBundle.LoadAssetAsync<T>
await AssetBundleExtensions.LoadAssetAsync<GameObject>(assetBundle, assetName);
await AssetBundleExtensions.LoadAssetAsync<Material>(assetBundle, materialAssetName);
```

### ShaderRepair
```csharp
using AssetBundleLoadingTools.Utilities;

// ShaderRepair contains multiple ways to fix shaders in legacy AssetBundles
// IMPORTANT: Always call only *one* ShaderRepair method for each gameobject.
// If you call FixShadersOnGameObject on a saber then FixShaderOnMaterial on the saber's trails, 
// the second call is missing important material context from the first call

// Async
await ShaderRepair.FixShadersOnGameObjectAsync(gameObject);
await ShaderRepair.FixShadersOnMaterialsAsync(materials);
await ShaderRepair.FixShaderOnMaterialAsync(material);

// Sync
ShaderRepair.FixShadersOnGameObject(gameObject);
ShaderRepair.FixShadersOnMaterials(materials);
ShaderRepair.FixShaderOnMaterial(material);
```

# TODO:
- Reformat project so only the absolute necessities (some public APIs in Utilities) are static (maybe use DI too)
- Upgrade `.shaderbundle` "web service" from requesting GitHub to actual proper web service
- Add multiple types of default "invalid" shaders and a config option to toggle between. Current is just invisible
- Add ShaderBundleExporter `.unitypackage` to repo, preferably in a dev Unity Project
- Look into ThryEditor compatibilty for ShaderBundleExporter
- Make ShaderBundleExporter export all models with fixed shaders by default when installed (sabers, platforms, etc)