using System;

namespace Gilzoide.PrefabPool
{
    public interface IPrefabPoolObject
    {
        IDisposable PooledObject { get; set; }
        void OnTakeFromPool() {}
        void OnReturnToPool() {}
    }

    public static class IPrefabPoolObjectExtensions
    {
        public static void ReturnToPool(this IPrefabPoolObject pooledComponent)
        {
            if (pooledComponent.PooledObject != null)
            {
                pooledComponent.PooledObject.Dispose();
                pooledComponent.PooledObject = null;
            }
        }
    }
}
