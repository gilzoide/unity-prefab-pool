#if HAVE_ADDRESSABLES
using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public class AddressablePrefabPoolAsset<T> : APrefabPoolAsset<T, AddressablePrefabPool<T>> where T : Object
    {
    }

    [CreateAssetMenu(menuName = "Prefab Pool/Addressable Prefab Pool Asset")]
    public class AddressablePrefabPoolAsset : AddressablePrefabPoolAsset<GameObject>
    {
    }
}
#endif