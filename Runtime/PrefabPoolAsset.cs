using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public class PrefabPoolAsset<T> : ScriptableObject, IPrefabPool<T>
        where T : Component
    {
        [SerializeField] protected PrefabPool<T> _pool;

        public T Get()
        {
            return _pool.Get();
        }

        public void Release(T instance)
        {
            _pool.Release(instance);
        }

        public bool TryGetPooled(out T instance)
        {
            return _pool.TryGetPooled(out instance);
        }

        void OnDisable()
        {
            _pool.Dispose();
        }
    }

    [CreateAssetMenu(menuName = "PrefabPool/PrefabPoolAsset")]
    public class PrefabPoolAsset : PrefabPoolAsset<Transform>
    {
    }
}
