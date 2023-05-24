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

        protected CancellationTokenSource CancelOnDispose => _cancelOnDispose != null ? _cancelOnDispose : (_cancelOnDispose = new());
        private CancellationTokenSource _cancelOnDispose;

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

        public async void Prewarm(int count, int instancesPerFrame = 0)
        {
            try
            {
                await PrewarmAsync(count, instancesPerFrame);
            }
            catch (OperationCanceledException) {}
        }

        public Task PrewarmAsync(int count, int instancesPerFrame)
        {
            return PrewarmAsyncInternal(count, instancesPerFrame, CancelOnDispose.Token);
        }

        public async Task PrewarmAsync(int count, int instancesPerFrame, CancellationToken cancellationToken)
        {
            using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancelOnDispose.Token);
            await PrewarmAsyncInternal(count, instancesPerFrame, CancelOnDispose.Token);
        }

        public void Clear()
        {
            _pool?.Clear();
        }

        public void Dispose()
        {
            if (_cancelOnDispose != null)
            {
                _cancelOnDispose.Cancel();
                _cancelOnDispose = null;
            }
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

        private async Task PrewarmAsyncInternal(int count, int instancesPerFrame, CancellationToken cancellationToken)
        {
            if (CountAll >= count)
            {
                return;
            }

            using var returnListToPoolOnDispose = ListPool<T>.Get(out List<T> list);
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

            for (int i = 0, imax = list.Count; i < imax; i++)
            {
                Pool.Release(list[i]);
            }
        }

    }

    [Serializable]
    public class PrefabPool : PrefabPool<GameObject>
    {
    }
}
