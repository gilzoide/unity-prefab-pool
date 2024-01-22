using UnityEngine.Events;

namespace Gilzoide.PrefabPool
{
    public class PoolEventTrigger : APooledBehaviour
    {
        public UnityEvent OnGet;
        public UnityEvent OnRelease;

        public override void OnGetFromPool()
        {
            OnGet.Invoke();
        }

        public override void OnReleaseToPool()
        {
            OnRelease.Invoke();
        }
    }
}
