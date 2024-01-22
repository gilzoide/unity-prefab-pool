using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public abstract class APooledBehaviour : MonoBehaviour, IPooledObject
    {
        public PoolHandle PoolHandle { get; set; }

        public virtual void OnGetFromPool() {}
        public virtual void OnReleaseToPool() {}
    }
}
