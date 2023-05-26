using UnityEngine;
using UnityEngine.Pool;

namespace Gilzoide.PrefabPool
{
    public interface IPrefabPool
    {
        int CountAll { get; }
        int CountActive { get; }
        int CountInactive { get; }

        void Clear();
        void Release(Object instance);
    }

    public interface IPrefabPool<T> : IPrefabPool where T : Object
    {
        T Prefab { get; }
        T Get();
        PoolSentinel Get(out T instance);
        void Release(T instance);
    }

    public static class IPrefabPoolExtensions
    {
        public static bool TryGetPooled<T>(this IObjectPool<T> pool, out T instance) where T : class
        {
            if (pool.CountInactive > 0)
            {
                instance = pool.Get();
                return true;
            }
            else
            {
                instance = null;
                return false;
            }
        }
    }
}
