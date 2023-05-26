using UnityEngine;

namespace Gilzoide.PrefabPool
{
    public class PrefabPoolAsset<T> : APrefabPoolAsset<T, PrefabPool<T>> where T : Object
    {
    }

    [CreateAssetMenu(menuName = "Prefab Pool/Prefab Pool Asset")]
    public class PrefabPoolAsset : PrefabPoolAsset<GameObject>
    {
    }
}
