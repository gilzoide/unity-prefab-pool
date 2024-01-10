#if (UNITY_EDITOR || DEVELOPMENT_BUILD) && !PREFAB_POOL_NO_DEBUG_LOGS
    #define PREFAB_POOL_DEBUG_LOGS
#endif
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Gilzoide.PrefabPool.Extensions;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

namespace Gilzoide.PrefabPool.Internal
{
    public abstract class APrefabPool<T> : IPrefabPool<T>, IDisposable where T : Object
    {
        public abstract T GetPrefab();

        public int CountAll => CountActive + CountInactive;
        public int CountActive => _activeObjects.Count;
        public int CountInactive => _inactiveObjects.Count;

        public IReadOnlyCollection<T> ActiveInstances => _activeObjects;
        public IReadOnlyCollection<T> InactiveInstances => _inactiveObjects;
        IReadOnlyCollection<Object> IPrefabPool.ActiveInstances => ActiveInstances;
        IReadOnlyCollection<Object> IPrefabPool.InactiveInstances => InactiveInstances;

        protected CancellationTokenSource CancelOnDispose => _cancelOnDispose ??= new CancellationTokenSource();
        protected CancellationTokenSource _cancelOnDispose;

        private readonly Stack<T> _inactiveObjects = new Stack<T>();
        private readonly SortedObjectList<T> _activeObjects = new SortedObjectList<T>();

        ~APrefabPool()
        {
            Dispose();
        }

        public T Get()
        {
            _ = Get(out T instance);
            return instance;
        }

        public PoolHandle Get(out T instance)
        {
            if (!_inactiveObjects.TryPop(out instance))
            {
                instance = CreateInstance();
            }
            _activeObjects.Add(instance);

            var poolSentinel = new PoolHandle(this, instance);
            using (instance.GetInterfaceObjects(out List<IPooledObject> list, out GameObject gameObject))
            {
                if (gameObject)
                {
                    gameObject.SetActive(true);
                }
                foreach (IPooledObject poolObject in list)
                {
                    poolObject.PoolHandle = poolSentinel;
                    poolObject.OnGetFromPool();
                }
            }
            return poolSentinel;
        }

        public void Release(PoolHandle sentinel)
        {
            if (sentinel.Pool != this)
            {
                LogWarning($"Trying to release a {nameof(PoolHandle)} from another pool.");
                return;
            }

            Object instance = sentinel.PooledObject; 
            if (instance is T tInstance)
            {
                Release(tInstance);
            }
            else
            {
                LogWarning($"Ignoring incompatible pooled object: expected type '{typeof(T)}' but found '{instance.GetType()}'");
            }
        }

        public void Release(T instance)
        {
            if (instance == null)
            {
                return;
            }

            if (!_activeObjects.Remove(instance))
            {
                LogWarning($"Releasing an instance that was not active.");
            }

            _inactiveObjects.Push(instance);
            using (instance.GetInterfaceObjects(out List<IPooledObject> list, out GameObject gameObject))
            {
                foreach (IPooledObject poolObject in list)
                {
                    poolObject.PoolHandle = default;
                    poolObject.OnReleaseToPool();
                }
                if (gameObject)
                {
                    gameObject.SetActive(false);
                }
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
            using (CancellationTokenSource linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, CancelOnDispose.Token))
            {
                await PrewarmAsyncInternal(count, instancesPerFrame, linkedCancellation.Token);
            }
        }

        public void Clear()
        {
            foreach (T instance in _activeObjects)
            {
                DestroyInstance(instance);
            }
            _activeObjects.Clear();

            foreach (T instance in _inactiveObjects)
            {
                DestroyInstance(instance);
            }
            _inactiveObjects.Clear();
        }

        public virtual void Dispose()
        {
            if (_cancelOnDispose != null)
            {
                _cancelOnDispose.Cancel();
                _cancelOnDispose = null;
            }
            Clear();
        }

        protected T CreateInstance()
        {
            T prefab = GetPrefab();
            T instance = Object.Instantiate(prefab);
            instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            return instance;
        }

        protected void DestroyInstance(Object instance)
        {
            if (instance == null)
            {
                return;
            }

            Object objectToDestroy = instance is Component component
                ? component.gameObject
                : instance;
            if (Application.isPlaying)
            {
                Object.Destroy(objectToDestroy);
            }
            else
            {
                Object.DestroyImmediate(objectToDestroy);
            }
        }

        [Conditional("PREFAB_POOL_DEBUG_LOGS")]
        protected void LogWarning(string message)
        {
            Debug.LogWarning($"[PrefabPool] {message}", this as Object);
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

            while (true)
            {
                for (int i = 0; i < instancesPerFrame; i++)
                {
                    if (CountAll >= count)
                    {
                        return;
                    }
                    cancellationToken.ThrowIfCancellationRequested();

                    T instance = CreateInstance();
                    if (instance.TryGetGameObject(out GameObject gameObject))
                    {
                        gameObject.SetActive(false);
                    }
                    _inactiveObjects.Push(instance);
                }
                await Task.Yield();
            }
        }
    }
}
