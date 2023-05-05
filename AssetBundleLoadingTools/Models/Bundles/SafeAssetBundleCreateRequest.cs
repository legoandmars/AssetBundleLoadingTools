using AssetBundleLoadingTools.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AssetBundleLoadingTools.Models.Bundles
{
    // wacky ass unity assetbundle async api middleware
    public class SafeAssetBundleCreateRequest : CustomYieldInstruction
    {
        private AssetBundleCreateRequest _createRequest;
        private SafeAssetBundle _assetBundle;
        private bool _finished = false;

        // fired to user
        public event Action<SafeAssetBundleCreateRequest> completed;

        // properties named lowercase for API compatibilty
        // public AssetBundle assetBundle => _createRequest?.assetBundle;
        public SafeAssetBundle assetBundle => _assetBundle;
        public bool isDone => _finished;
        public float progress => _createRequest.progress;
        public int priority => _createRequest.priority;
        public bool allowSceneActivation => _createRequest.allowSceneActivation;

        private string? _path;
        private byte[]? _binary;
        private MemoryStream? _stream;

        // yield await compatibility
        public override bool keepWaiting => _finished;

        public SafeAssetBundleCreateRequest(AssetBundleCreateRequest assetBundleCreateRequest, string path)
        {
            _path = path;
            _createRequest = assetBundleCreateRequest;
            _createRequest.completed += OnInternalCreateRequestCompleted;
        }
        public SafeAssetBundleCreateRequest(AssetBundleCreateRequest assetBundleCreateRequest, byte[] binary)
        {
            _binary = binary;
            _createRequest = assetBundleCreateRequest;
            _createRequest.completed += OnInternalCreateRequestCompleted;
        }
        public SafeAssetBundleCreateRequest(AssetBundleCreateRequest assetBundleCreateRequest, MemoryStream stream)
        {
            _stream = stream;
            _createRequest = assetBundleCreateRequest;
            _createRequest.completed += OnInternalCreateRequestCompleted;
        }

        // post-assetbundle load, used internally for safe check
        internal async void OnInternalCreateRequestCompleted(AsyncOperation asyncOperation)
        {
            if(_createRequest.assetBundle != null)
            {
                var type = GetInitializationType();
                var hash = await GetHashAsync(type);
                var bundlePath = await GetBundlePathAsync(type);
                _assetBundle = new SafeAssetBundle(_createRequest.assetBundle, type, hash, bundlePath);
            }
            // do stuff
            _finished = true;
            completed?.Invoke(this);
        }

        private AssetBundleInitializationType GetInitializationType()
        {
            if(_path != null)
            {
                return AssetBundleInitializationType.File;
            }
            else if(_binary != null)
            {
                return AssetBundleInitializationType.Memory;
            }
            else if(_stream != null)
            {
                return AssetBundleInitializationType.Stream;
            }
            else
            {
                throw new Exception("SafeAssetBundleCreateRequest has no type");
            }
        }

        private async Task<string> GetHashAsync(AssetBundleInitializationType initializationType)
        {
            if(initializationType == AssetBundleInitializationType.File)
            {
                return await Task.Run(() => AssetBundleHashing.FromFile(_path));
            }
            else if (initializationType == AssetBundleInitializationType.Memory)
            {
                return await Task.Run(() => AssetBundleHashing.FromBytes(_binary));
            }
            else
            {
                return await Task.Run(() => AssetBundleHashing.FromStream(_stream));
            }
        }

        private async Task<string> GetBundlePathAsync(AssetBundleInitializationType initializationType)
        {
            if (initializationType == AssetBundleInitializationType.File)
            {
                return _path;
            }
            else if(initializationType == AssetBundleInitializationType.Memory)
            {
                return await Task.Run(() =>
                {
                    var bundlePath = Path.GetTempFileName();
                    File.WriteAllBytes(bundlePath, _binary);

                    return bundlePath;
                });
            }
            else
            {
                var bundlePath = await Task.Run(() =>
                {
                    var hash = AssetBundleHashing.FromStream(_stream);
                    var bundlePath = Path.GetTempFileName();
                    File.WriteAllBytes(bundlePath, _stream.ToArray());

                    return bundlePath;
                });

                _stream.Dispose(); // do outside task
                return bundlePath;
            }
        }
    }
}
