#if HAVE_ADDRESSABLES
using Gilzoide.PrefabPool.Internal;
using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public class AddressablePrefabPoolComponent<T> : APrefabPoolComponent<T, AddressablePrefabPool<T>>
        where T : Object
    {
    }

    public class AddressablePrefabPoolComponent : AddressablePrefabPoolComponent<GameObject>
    {
    }
}
#endif