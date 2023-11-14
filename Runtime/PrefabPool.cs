using System;
using Gilzoide.PrefabPool.Internal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gilzoide.PrefabPool
{
    [Serializable]
    public class PrefabPool<T> : APrefabPool<T>
        where T : Object
    {
        [Tooltip("Prefab which instances will be pooled.")]
        [SerializeField] protected T _prefab;

        public T Prefab
        {
            get => _prefab;
            set
            {
                Dispose();
                _prefab = value;
            }
        }

        public PrefabPool() : base() {}
        public PrefabPool(T prefab) : base()
        {
            _prefab = prefab;
        }

        public override T GetPrefab()
        {
            return _prefab;
        }
    }

    [Serializable]
    public class PrefabPool : PrefabPool<GameObject>
    {
        public PrefabPool() : base() {}
        public PrefabPool(GameObject prefab) : base(prefab) {}
    }
}
