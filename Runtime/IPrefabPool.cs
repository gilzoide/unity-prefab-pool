namespace Gilzoide.PrefabPool
{
    public interface IPrefabPool<T>
    {
        T Get();
        void Release(T instance);
        bool TryGetPooled(out T instance);
    }
}
