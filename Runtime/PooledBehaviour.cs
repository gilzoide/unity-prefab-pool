using System;
using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public class PooledBehaviour : MonoBehaviour, IPooledObject
    {
        public PoolSentinel PoolSentinel { get; set; }
    }
}
