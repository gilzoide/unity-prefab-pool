using System;

namespace Gilzoide.PrefabPool
{
    public interface IPooledObject
    {
        IDisposable PoolSentinel { get; set; }
        void OnGetFromPool() {}
        void OnReleaseToPool() {}
    }

    public static class IPooledObjectExtensions
    {
        public static void ReturnToPool(this IPooledObject pooledObject)
        {
            if (pooledObject.PoolSentinel != null)
            {
                pooledObject.PoolSentinel.Dispose();
                pooledObject.PoolSentinel = null;
            }
        }
    }
}
