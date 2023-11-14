using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Gilzoide.PrefabPool.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Gilzoide.PrefabPool.Internal
{
    [Serializable]
    public abstract class APrefabPool<T> : IPrefabPool<T>, IDisposable where T : Object
    {
        public abstract T GetPrefab();

        public int CountAll { get; private set; }
        public int CountActive => CountAll - CountInactive;
        public int CountInactive => _stack.Count;

        protected CancellationTokenSource CancelOnDispose => _cancelOnDispose ??= new();
        protected CancellationTokenSource _cancelOnDispose;

        private readonly Stack<T> _stack = new();

        ~APrefabPool()
        {
            Dispose();
        }

        public T Get()
        {
            _ = Get(out T instance);
            return instance;
        }

        public PoolSentinel Get(out T instance)
        {
            if (!_stack.TryPop(out instance))
            {
                instance = CreateInstance();
            }

            var poolSentinel = new PoolSentinel(this, instance);
            using (instance.GetInterfaceObjects(out List<IPooledObject> list, out GameObject gameObject))
            {
                if (gameObject)
                {
                    gameObject.SetActive(true);
                }
                foreach (IPooledObject poolObject in list)
                {
                    poolObject.PoolSentinel = poolSentinel;
                    poolObject.OnGetFromPool();
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
            if (instance == null)
            {
                return;
            }

            _stack.Push(instance);
            using (instance.GetInterfaceObjects(out List<IPooledObject> list, out GameObject gameObject))
            {
                foreach (IPooledObject poolObject in list)
                {
                    poolObject.PoolSentinel = default;
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
            foreach (T instance in _stack)
            {
                DestroyInstance(instance);
            }
            _stack.Clear();
            CountAll = 0;
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
            CountAll++;
            T instance = Object.Instantiate(GetPrefab());
            instance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            return instance;
        }

        protected void DestroyInstance(T instance)
        {
            CountAll = Mathf.Max(0, CountAll - 1);
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
                    _stack.Push(instance);
                }
                await Task.Yield();
            }
        }
    }
}
