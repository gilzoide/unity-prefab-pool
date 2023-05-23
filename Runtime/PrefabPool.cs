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
    public class PrefabPool<T> : IObjectPool<T>, IDisposable
        where T : Object
    {
        [Tooltip("Prefab which instances will be pooled.")]
        [SerializeField] protected T _prefab;

        public T Prefab
        {
            get => _prefab;
            set => _prefab = value;
        }

        public int CountAll => _pool?.CountAll ?? 0;
        public int CountActive => _pool?.CountActive ?? 0;
        public int CountInactive => _pool?.CountInactive ?? 0;

        private ObjectPool<T> Pool => _pool != null ? _pool : (_pool = CreatePool());
        private ObjectPool<T> _pool;

        public PrefabPool() {}
        public PrefabPool(T prefab)
        {
            _prefab = prefab;
        }

        public T Get()
        {
            _ = Get(out T instance);
            return instance;
        }

        public PooledObject<T> Get(out T instance)
        {
            var pooledObject = Pool.Get(out instance);
            if (instance is IPrefabPoolObject obj)
            {
                obj.PooledObject = pooledObject;
            }
            return pooledObject;
        }

        public void Release(T instance)
        {
            Pool.Release(instance);
        }

        public bool TryGetPooled(out T instance)
        {
            if (CountInactive > 0)
            {
                instance = Get();
                return true;
            }
            else
            {
                instance = default;
                return false;
            }
        }

        public async void Prewarm(int count, int instancesPerFrame = 0, CancellationToken cancellationToken = default)
        {
            try
            {
                await PrewarmAsync(count, instancesPerFrame, cancellationToken);
            }
            catch (OperationCanceledException) {}
        }

        public async Task PrewarmAsync(int count, int instancesPerFrame = 0, CancellationToken cancellationToken = default)
        {
            if (count <= Pool.CountAll || !Application.isPlaying)
            {
                return;
            }

            if (instancesPerFrame <= 0)
            {
                instancesPerFrame = count;
            }

            using var _ = ListPool<T>.Get(out List<T> list);
            int batchCount = Mathf.CeilToInt((float) count / (float) instancesPerFrame);
            for (int batch = 0; batch < batchCount; batch++)
            {
                for (int i = 0; i < instancesPerFrame && Pool.CountAll < count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    T instance = Pool.Get();
                    OnReturnToPool(instance);
                    list.Add(instance);
                }
                await Task.Yield();
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
            T instance = Object.Instantiate(_prefab);
            instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            return instance;
        }

        protected void OnTakeFromPool(T instance)
        {
            if (instance is IPrefabPoolObject pooledObject)
            {
                pooledObject.OnTakeFromPool();
            }

            if (instance is Component component)
            {
                component.gameObject.SetActive(true);
            }
            else if (instance is GameObject gameObject)
            {
                gameObject.SetActive(true);
            }
        }

        protected void OnReturnToPool(T instance)
        {
            if (instance is Component component)
            {
                component.gameObject.SetActive(false);
            }
            else if (instance is GameObject gameObject)
            {
                gameObject.SetActive(false);
            }

            if (instance is IPrefabPoolObject pooledObject)
            {
                pooledObject.PooledObject = null;
                pooledObject.OnReturnToPool();
            }
        }
    }

    [Serializable]
    public class PrefabPool : PrefabPool<GameObject>
    {
    }
}
