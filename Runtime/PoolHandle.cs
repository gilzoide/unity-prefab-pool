using System;
using Object = UnityEngine.Object;

namespace Gilzoide.PrefabPool
{
    public struct PoolHandle : IDisposable
    {
        public IPrefabPool Pool;
        public Object PooledObject;

        public PoolHandle(IPrefabPool pool, Object pooledObject)
        {
            Pool = pool;
            PooledObject = pooledObject;
        }

        public void ReturnToPool()
        {
            if (Pool != null && PooledObject != null)
            {
                Pool.Release(this);
                this = default;
            }
        }

        public void Dispose()
        {
            ReturnToPool();
        }
    }
}
