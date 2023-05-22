using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Gilzoide.PrefabPool
{
    [Serializable]
    public class PrefabPool<T> : IPrefabPool<T>, IDisposable
        where T : Component
    {
        [Tooltip("Prefab which instances will be pooled.")]
        [SerializeField] protected T _prefab;

        public T Prefab
        {
            get => _prefab;
            set => _prefab = value;
        }

        private ObjectPool<T> Pool => _pool != null ? _pool : (_pool = CreatePool());
        private ObjectPool<T> _pool;

        public PrefabPool() {}
        public PrefabPool(T prefab)
        {
            _prefab = prefab;
        }

        public T Get()
        {
            var pooledObject = Pool.Get(out T instance);
            if (instance is IPrefabPoolBehaviour pooledBehaviour)
            {
                pooledBehaviour.PooledObject = pooledObject;
            }
            return instance;
        }

        public void Release(T instance)
        {
            Pool.Release(instance);
        }

        public bool TryGetPooled(out T instance)
        {
            if (Pool.CountInactive > 0)
            {
                instance = Pool.Get();
                return true;
            }
            else
            {
                instance = default;
                return false;
            }
        }

        public async Task PrewarmAsync(int count, int itemsPerBatch, CancellationToken cancellationToken = default)
        {
            if (count <= Pool.CountAll || !Application.isPlaying)
            {
                return;
            }

            if (itemsPerBatch <= 0)
            {
                itemsPerBatch = count;
            }

            using var _ = ListPool<T>.Get(out List<T> list);
            int batchCount = Mathf.CeilToInt((float) count / (float) itemsPerBatch);
            for (int batch = 0; batch < batchCount; batch++)
            {
                await Task.Yield();
                for (int i = 0; i < itemsPerBatch && Pool.CountAll < count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    T instance = Pool.Get();
                    OnReturnToPool(instance);
                    list.Add(instance);
                }
            }

            list.ForEach(Pool.Release);
        }

        public void Clear()
        {
            _pool?.Clear();
        }

        public void Dispose()
        {
            if (_pool != null)
            {
                _pool.Dispose();
                _pool = null;
            }
        }

        protected ObjectPool<T> CreatePool()
        {
            return new ObjectPool<T>(Create, OnTakeFromPool, OnReturnToPool, Object.Destroy);
        }

        protected T Create()
        {
            return Object.Instantiate(_prefab);
        }

        protected void OnTakeFromPool(T instance)
        {
            instance.gameObject.SetActive(true);
            if (instance is IPrefabPoolBehaviour pooledBehaviour)
            {
                pooledBehaviour.OnTakeFromPool();
            }
        }

        protected void OnReturnToPool(T instance)
        {
            instance.gameObject.SetActive(false);
            if (instance is IPrefabPoolBehaviour pooledBehaviour)
            {
                pooledBehaviour.OnReturnToPool();
            }
        }
    }

    [Serializable]
    public class PrefabPool : PrefabPool<Transform>
    {
    }
}
