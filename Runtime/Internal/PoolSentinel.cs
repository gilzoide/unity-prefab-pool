using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public struct PoolSentinel
    {
        public IPrefabPool Pool;
        public Object PooledObject;

        public bool CanRelease => Pool != null && PooledObject != null;

        public void ReturnToPool()
        {
            if (CanRelease)
            {
                Pool.Release(PooledObject);
                this = default;
            }
        }
    }
}
