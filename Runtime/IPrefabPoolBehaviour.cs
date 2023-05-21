using System;

namespace Gilzoide.PrefabPool
{
    public interface IPrefabPoolBehaviour
    {
        IDisposable PooledObject { get; set; }
        void OnTakeFromPool() {}
        void OnReturnToPool() {}
    }

    public static class IPooledComponentExtensions
    {
        public static void ReturnToPool(this IPrefabPoolBehaviour pooledComponent)
        {
            if (pooledComponent.PooledObject != null)
            {
                pooledComponent.PooledObject.Dispose();
                pooledComponent.PooledObject = null;
            }
        }
    }
}
