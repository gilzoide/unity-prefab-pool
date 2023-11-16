using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gilzoide.PrefabPool.Internal
{
    public abstract class APrefabPoolAsset<T, TPool> : ScriptableObject, IPrefabPool<T>, IDisposable
        where T : Object
        where TPool : APrefabPool<T>, new()
    {
        [SerializeField] protected TPool _pool = new TPool();

        [Header("Prewarm")]
        [Tooltip("Number of instances that will be spawned when the pool is created. "
            + "Instances are spawned asynchronously. "
            + "At most 'Objects Per Frame' objects will be prewarmed per frame.")]
        [SerializeField, Min(0)] protected int _initialObjectCount;

        [Tooltip("Maximum number of instances that will be spawned per frame when pool is created. "
            + "If zero, all items will be prewarmed in a single frame.")]
        [SerializeField, Min(0)] protected int _objectsPerFrame;

        public int CountAll => _pool.CountAll;
        public int CountActive => _pool.CountActive;
        public int CountInactive => _pool.CountInactive;

        public TPool Pool => _pool;

        void OnEnable()
        {
#if UNITY_EDITOR
            Application.quitting += Dispose;
            if (Application.isPlaying)
#endif
            {
                Prewarm();
            }
        }

        void OnDisable()
        {
#if UNITY_EDITOR
            Application.quitting -= Dispose;
#endif
            Dispose();
        }

        void OnDestroy()
        {
            Dispose();
        }

        public T Get()
        {
            return _pool.Get();
        }

        public PoolHandle Get(out T instance)
        {
            return _pool.Get(out instance);
        }

        public void Release(PoolHandle sentinel)
        {
            _pool.Release(sentinel);
        }

        public void Release(T instance)
        {
            _pool.Release(instance);
        }

        public void Clear()
        {
            _pool.Clear();
        }

        public void Prewarm()
        {
            _pool.Prewarm(_initialObjectCount, _objectsPerFrame);
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
