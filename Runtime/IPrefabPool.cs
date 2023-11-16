using System.Collections.Generic;
using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public interface IPrefabPool
    {
        int CountAll { get; }
        int CountActive { get; }
        int CountInactive { get; }

        IReadOnlyCollection<Object> ActiveInstances { get; }
        IReadOnlyCollection<Object> InactiveInstances { get; }

        void Clear();
        void Release(PoolHandle handle);
    }

    public interface IPrefabPool<T> : IPrefabPool
        where T : Object
    {
        new IReadOnlyCollection<T> ActiveInstances { get; }
        new IReadOnlyCollection<T> InactiveInstances { get; }

        T Get();
        PoolHandle Get(out T instance);
        void Release(T instance);
    }

    public static class IPrefabPoolExtensions
    {
        public static bool TryGetPooled<T>(this IPrefabPool<T> pool, out T instance)
            where T : Object
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
