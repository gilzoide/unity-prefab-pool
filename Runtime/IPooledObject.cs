namespace Gilzoide.PrefabPool
{
    public interface IPooledObject
    {
        PoolSentinel PoolSentinel { get; set; }
        void OnGetFromPool() {}
        void OnReleaseToPool() {}
    }

    public static class IPooledObjectExtensions
    {
        public static void ReturnToPool(this IPooledObject pooledObject)
        {
            pooledObject.PoolSentinel.ReturnToPool();
        }
    }
}
