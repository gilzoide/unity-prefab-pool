using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public class PrefabPoolAsset<T> : ScriptableObject where T : Component
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
