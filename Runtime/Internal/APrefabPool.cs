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
    public abstract class APrefabPool<T> : IPrefabPool<T>, IDisposable where T : Object
    {
        public abstract T Prefab { get; }

        public int CountAll => _pool?.CountAll ?? 0;
        public int CountActive => _pool?.CountActive ?? 0;
        public int CountInactive => _pool?.CountInactive ?? 0;

        private ObjectPool<T> Pool => _pool != null ? _pool : (_pool = CreatePool());
        private ObjectPool<T> _pool;

        protected CancellationTokenSource CancelOnDispose => _cancelOnDispose != null ? _cancelOnDispose : (_cancelOnDispose = new());
        private CancellationTokenSource _cancelOnDispose;

        ~APrefabPool()
        {
            Dispose();
        }

        public T Get()
        {
            _ = Get(out T instance);
            return instance;
        }

        public PooledObject<T> Get(out T instance)
        {
            PooledObject<T> poolSentinel = Pool.Get(out instance);
            using (GetInstanceObjects(instance, out List<IPooledObject> list, out _))
            {
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    IPooledObject obj = list[i];
                    obj.PoolSentinel = new PoolSentinel
                    {
                        Pool = this,
                        PooledObject = instance,
                    };
                }
            }
            return poolSentinel;
        }

        public void Release(Object instance)
        {
            if (instance is T tInstance)
            {
                Release(tInstance);
            }
            else
            {
                Debug.LogWarningFormat("Ignoring incompatible pooled object: expected type '{0}' but found '{1}'", typeof(T), instance.GetType());
            }
        }

        public void Release(T instance)
        {
            Pool.Release(instance);
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

        public virtual void Dispose()
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
            return new ObjectPool<T>(Create, OnGet, OnRelease, Object.Destroy);
        }

        protected T Create()
        {
            T instance = Object.Instantiate(Prefab);
            instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            return instance;
        }

        protected void OnGet(T instance)
        {
            using (GetInstanceObjects(instance, out List<IPooledObject> list, out GameObject gameObject))
            {
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    IPooledObject poolObject = list[i];
                    poolObject.OnGetFromPool();
                }
                if (gameObject)
                {
                    gameObject.SetActive(true);
                }
            }
        }

        protected void OnRelease(T instance)
        {
            using (GetInstanceObjects(instance, out List<IPooledObject> list, out GameObject gameObject))
            {
                if (gameObject)
                {
                    gameObject.SetActive(false);
                }
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    IPooledObject poolObject = list[i];
                    poolObject.PoolSentinel = default;
                    poolObject.OnReleaseToPool();
                }
            }
        }

        private PooledObject<List<IPooledObject>> GetInstanceObjects(T instance, out List<IPooledObject> objs, out GameObject gameObject)
        {
            var pooledList = ListPool<IPooledObject>.Get(out objs);
            if (instance is GameObject go)
            {
                gameObject = go;
                gameObject.GetComponentsInChildren(true, objs);
            }
            else if (instance is Component component)
            {
                gameObject = component.gameObject;
                gameObject.GetComponentsInChildren(true, objs);
            }
            else if (instance is IPooledObject poolObject)
            {
                gameObject = null;
                objs.Add(poolObject);
            }
            else
            {
                gameObject = null;
            }
            return pooledList;
        }

        private async Task PrewarmAsyncInternal(int count, int instancesPerFrame, CancellationToken cancellationToken)
        {
            if (CountAll >= count)
            {
                return;
            }

            if (instancesPerFrame <= 0)
            {
                instancesPerFrame = count;
            }

            using (ListPool<T>.Get(out List<T> list))
            while (true)
            {
                for (int i = 0; i < instancesPerFrame; i++)
                {
                    if (Pool.CountAll >= count)
                    {
                        list.ForEach(Pool.Release);
                        return;
                    }
                    cancellationToken.ThrowIfCancellationRequested();
                    T instance = Pool.Get();
                    OnRelease(instance);
                    list.Add(instance);
                }
                await Task.Yield();
            }
        }
    }
}
