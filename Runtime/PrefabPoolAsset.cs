using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

namespace Gilzoide.PrefabPool
{
    public class PrefabPoolAsset<T> : ScriptableObject, IObjectPool<T>
        where T : Object
    {
        [SerializeField] protected PrefabPool<T> _pool = new();

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

        void OnEnable()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.playModeStateChanged += mode =>
            {
                if (mode == UnityEditor.PlayModeStateChange.EnteredPlayMode)
                {
                    Prewarm();
                }
                else if (mode == UnityEditor.PlayModeStateChange.ExitingPlayMode)
                {
                    Dispose();
                }
            };
#else
            Prewarm();
#endif
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

        public void Release(T instance)
        {
            _pool.Release(instance);
        }

        public bool TryGetPooled(out T instance)
        {
            return _pool.TryGetPooled(out instance);
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

    [CreateAssetMenu(menuName = "PrefabPool/PrefabPoolAsset")]
    public class PrefabPoolAsset : PrefabPoolAsset<GameObject>
    {
    }
}
