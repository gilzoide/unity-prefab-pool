using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public abstract class APooledBehaviour : MonoBehaviour, IPooledObject
    {
        public PoolHandle PoolSentinel { get; set; }
    }
}
