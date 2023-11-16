#if HAVE_ADDRESSABLES
using System;
using Gilzoide.PrefabPool.Internal;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Gilzoide.PrefabPool
{
    [Serializable]
    public class AddressablePrefabPool<T> : APrefabPool<T>
        where T : Object
    {
        [Tooltip("Prefab which instances will be pooled. "
            + "The addressable asset will be loaded when the first instance is created and released when the pool is disposed.")]
        [SerializeField] protected AssetReferenceT<T> _prefabReference;

        private bool _hasLoadedAsset = false;

        public AddressablePrefabPool() : base() {}
        public AddressablePrefabPool(AssetReferenceT<T> prefabReference) : base()
        {
            _prefabReference = prefabReference;
        }

        public AssetReferenceT<T> PrefabReference
        {
            get => _prefabReference;
            set
            {
                Dispose();
                _prefabReference = value;
            }
        }

        public override T GetPrefab()
        {
            if (_hasLoadedAsset && _prefabReference.IsValid())
            {
                return (T) _prefabReference.Asset;
            }
            else
            {
                T prefab = _prefabReference.LoadAssetAsync().WaitForCompletion();
                _hasLoadedAsset = true;
                return prefab;
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            if (_hasLoadedAsset && _prefabReference.IsValid())
            {
                _prefabReference.ReleaseAsset();
            }
            _hasLoadedAsset = false;
        }
    }

    [Serializable]
    public class AddressablePrefabPool : AddressablePrefabPool<GameObject>
    {
        public AddressablePrefabPool() : base() {}
        public AddressablePrefabPool(AssetReferenceT<GameObject> prefab) : base(prefab) {}
    }
}
#endif