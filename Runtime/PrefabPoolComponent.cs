using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public class PrefabPoolComponent<T> : MonoBehaviour where T : Component
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

        void OnDestroy()
        {
            _pool.Dispose();
        }
    }

    public class PrefabPoolComponent : PrefabPoolComponent<Transform>
    {
    }
}
