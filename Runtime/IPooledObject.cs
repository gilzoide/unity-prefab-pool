namespace Gilzoide.PrefabPool
{
    public interface IPooledObject
    {
        PoolHandle PoolHandle { get; set; }
        void OnGetFromPool();
        void OnReleaseToPool();
    }

    public static class IPooledObjectExtensions
    {
        public static void ReturnToPool(this IPooledObject pooledObject)
        {
            pooledObject.PoolHandle.ReturnToPool();
        }
    }
}
