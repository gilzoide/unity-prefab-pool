using System;
using Object = UnityEngine.Object;

namespace Gilzoide.PrefabPool
{
    public struct PoolSentinel : IDisposable
    {
        public IPrefabPool Pool;
        public Object PooledObject;

        public PoolSentinel(IPrefabPool pool, Object pooledObject)
        {
            Pool = pool;
            PooledObject = pooledObject;
        }

        public void ReturnToPool()
        {
            if (Pool != null && PooledObject != null)
            {
                Pool.Release(PooledObject);
                this = default;
            }
        }

        public void Dispose()
        {
            ReturnToPool();
        }
    }
}
