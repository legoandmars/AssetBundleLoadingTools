# AssetBundle Loading Tools
A Beat Saber library that adds utilities to alleviate various problems caused by the Unity 2021 upgrade:
- In Unity 2020+, UnityEvents have [a new field that can be exploited to execute arbitrary code](https://blog.includesecurity.com/2021/06/hacking-unity-games-malicious-unity-game-objects/)
- Because of the change from `Single Pass` to `Single Pass Instanced`, all previously compiled shaders do not show up properly ingame

# Usage
```csharp
using AssetBundleLoadingTools.Utilities;

// ...

// Load AssetBundle
AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);
// AssetBundle file hash is necessary to speed up future asset loading with caches
string hash = AssetBundleHashing.FromFile(bundlePath);
// Safely load asset - this will disable malicious UnityEvents
GameObject asset = AssetBundle.LoadAssetSafe<GameObject>("assets/_example.prefab", hash);
```

# For Developers
If you're planning on testing against the same model repeatedly, make sure to set `EnableCache` to false in `UserData/AssetBundleLoadingTools.json`
# TODO:
- Expand upon shader fixing
- Reformat project so only the absolute necessities (some public APIs in Utilities) are static (maybe use DI too)
- Add a web service to check for converted shaders if not downloaded locally
- Improve caching to more accurately reflect if models are dangerous
- Create pre-written list of safe components that never touch a UnityEvent
- Convert reflection to something more sensible like FieldAccessors or ReflectionUtil if possible
- See if the actual behaviour of LoadAssetAsync can be replicated to make the extension methods 1:1
- Figure out if there's any non-Component/GameObject Objects that can be used to trigger a UnityEvent