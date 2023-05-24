#if HAVE_ADDRESSABLES
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Object = UnityEngine.Object;

namespace Gilzoide.PrefabPool
{
    [Serializable]
    public class AddressablePrefabPool<T> : APrefabPool<T> where T : Object
    {
        [Tooltip("Prefab which instances will be pooled. "
            + "The addressable asset will be loaded when the first instance is created and released when the pool is disposed.")]
        [SerializeField] protected AssetReferenceT<T> _prefab;

        public AddressablePrefabPool() : base() {}
        public AddressablePrefabPool(AssetReferenceT<T> prefab) : base()
        {
            _prefab = prefab;
        }

        public override T Prefab => _prefab.IsValid()
            ? (T) _prefab.Asset
            : _prefab.LoadAssetAsync().WaitForCompletion();

        public override void Dispose()
        {
            base.Dispose();
            if (_prefab.IsValid())
            {
                _prefab.ReleaseAsset();
            }
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