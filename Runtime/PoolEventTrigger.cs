using UnityEngine.Events;

namespace Gilzoide.PrefabPool
{
    public class PoolEventTrigger : PooledBehaviour
    {
        public UnityEvent OnGet;
        public UnityEvent OnRelease;

        public void OnGetFromPool()
        {
            OnGet.Invoke();
        }

        public void OnReleaseToPool()
        {
            OnRelease.Invoke();
        }
    }
}
