using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Gilzoide.PrefabPool
{
    public abstract class APrefabPoolComponent<T, TPool> : MonoBehaviour, IPrefabPool<T>
        where T : Object
        where TPool : APrefabPool<T>, new()
    {
        [SerializeField] protected TPool _pool = new();

        [Header("Prewarm")]
        [Tooltip("Number of instances that will be spawned when the pool is created. "
            + "Instances are spawned asynchronously. "
            + "At most 'Objects Per Frame' objects will be prewarmed per frame.")]
        [SerializeField, Min(0)] protected int _initialObjectCount;
        
        [Tooltip("Maximum number of instances that will be spawned per frame when pool is created. "
            + "If zero, all items will be prewarmed in a single frame.")]
        [SerializeField, Min(0)] protected int _objectsPerFrame;

        public int CountAll => _pool.CountAll;
        public int CountActive => _pool.CountActive;
        public int CountInactive => _pool.CountInactive;

        public T Prefab => _pool.Prefab;

        void OnEnable()
        {
            Prewarm();
        }

        void OnDisable()
        {
            Dispose();
        }

        void OnDestroy()
        {
            Dispose();
        }

        public T Get()
        {
            return _pool.Get();
        }

        public PooledObject<T> Get(out T instance)
        {
            return _pool.Get(out instance);
        }

        public void Release(Object instance)
        {
            _pool.Release(instance);
        }

        public void Release(T instance)
        {
            _pool.Release(instance);
        }

        public void Clear()
        {
            _pool.Clear();
        }

        public void Prewarm()
        {
            _pool.Prewarm(_initialObjectCount, _objectsPerFrame);
        }

        public void Dispose()
        {
            _pool.Dispose();
        }
    }
}
