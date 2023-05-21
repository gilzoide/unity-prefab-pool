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
    public class PrefabPool<T> : IDisposable, ISerializationCallbackReceiver
        where T : Component
    {
        [Tooltip("Prefab which instances will be pooled.")]
        [SerializeField] protected T _prefab;

        [Header("Prewarm")]
        [Tooltip("Number of instances that will be spawned when the pool is created. "
            + "Instances are spawned asynchronously. "
            + "At most 'Objects Per Frame' objects will be prewarmed per frame.")]
        [SerializeField, Min(0)] protected int _initialObjectCount;
        
        [Tooltip("Maximum number of instances that will be spawned per frame when pool is created. "
            + "If zero, all items will be prewarmed in a single frame.")]
        [SerializeField, Min(0)] protected int _objectsPerFrame;

        public T Prefab
        {
            get => _prefab;
            set => _prefab = value;
        }
        
        protected CancellationTokenSource _cancelOnDispose = new CancellationTokenSource();

        private ObjectPool<T> Pool => _pool != null ? _pool : (_pool = CreatePool());
        private ObjectPool<T> _pool;


        public PrefabPool() {}
        public PrefabPool(T prefab, int initialObjectCount = 0, int objectsPerFrame = 0, bool prewarm = true)
        {
            _prefab = prefab;
            _initialObjectCount = initialObjectCount;
            _objectsPerFrame = objectsPerFrame;

            if (prewarm)
            {
                Prewarm();
            }
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

        public async void Prewarm()
        {
            await Task.Yield();
            await PrewarmAsync(_initialObjectCount, _objectsPerFrame, _cancelOnDispose.Token);
        }

        public async Task PrewarmAsync(int count, int itemsPerBatch = 0, CancellationToken cancellationToken = default)
        {
            if (count <= 0 || !Application.isPlaying)
            {
                return;
            }

            if (itemsPerBatch <= 0)
            {
                itemsPerBatch = count;
            }

            using var _ = ListPool<T>.Get(out List<T> list);
            int batchCount = Mathf.CeilToInt((float) count / (float) itemsPerBatch);
            for (int batch = 0; batch < batchCount && !cancellationToken.IsCancellationRequested; batch++)
            {
                for (int i = 0; i < itemsPerBatch && Pool.CountAll < count; i++)
                {
                    T instance = Pool.Get();
                    OnReturnToPool(instance);
                    list.Add(instance);
                }
                await Task.Yield();
            }

            list.ForEach(Pool.Release);
        }

        public void Dispose()
        {
            if (_cancelOnDispose != null)
            {
                _cancelOnDispose.Dispose();
                _cancelOnDispose = null;
            }
            if (_pool != null)
            {
                _pool.Dispose();
                _pool = null;
            }
        }


        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            Prewarm();
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
