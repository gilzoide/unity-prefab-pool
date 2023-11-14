using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public abstract class APooledBehaviour : MonoBehaviour, IPooledObject
    {
        public PoolSentinel PoolSentinel { get; set; }
    }
}
