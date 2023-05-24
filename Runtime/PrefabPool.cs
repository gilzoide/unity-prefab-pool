using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gilzoide.PrefabPool
{
    [Serializable]
    public class PrefabPool<T> : APrefabPool<T> where T : Object
    {
        [Tooltip("Prefab which instances will be pooled.")]
        [SerializeField] protected T _prefab;

        public override T Prefab => _prefab;

        public PrefabPool() : base() {}
        public PrefabPool(T prefab) : base()
        {
            _prefab = prefab;
        }
    }

    [Serializable]
    public class PrefabPool : PrefabPool<GameObject>
    {
        public PrefabPool() : base() {}
        public PrefabPool(GameObject prefab) : base(prefab) {}
    }
}
