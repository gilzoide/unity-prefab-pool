using UnityEngine;
using UnityEngine.Pool;

namespace Gilzoide.PrefabPool
{
    public interface IPrefabPool<T> : IObjectPool<T> where T : Object
    {
        T Prefab { get; }
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
