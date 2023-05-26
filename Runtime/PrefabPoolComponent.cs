using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public class PrefabPoolComponent<T> : APrefabPoolComponent<T, PrefabPool<T>> where T : Object
    {
    }

    public class PrefabPoolComponent : PrefabPoolComponent<GameObject>
    {
    }
}
