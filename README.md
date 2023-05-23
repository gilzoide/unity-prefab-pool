# Prefab Pool
Prefab instance pool that is configurable in the Inspector, supports any engine `Object` type and is available as a serializable C# class, MonoBehaviour and ScriptableObject.


## Features
- Prefab pools may live either as project assets ([PrefabPoolAsset](Runtime/PrefabPoolAsset.cs)), standalone components in the scene ([PrefabPoolComponent](Runtime/PrefabPoolComponent.cs)), or may be a part of your own scripts ([PrefabPool](Runtime/PrefabPool.cs)), whichever fits best for the use case.
- Supports prewarming instances: configure pools to instantiate a number of prefabs when created, with an optional limit of objects per frame to avoid spikes in CPU usage.
- Supports generic typing for customizing which prefabs can be assigned to the pool.
  By default, the `GameObject` type is used in the non-generic prefab pool classes.
- Optional [IPrefabPoolObject](Runtime/IPrefabPoolObject.cs) interface for customizing what happens when prefab instances are acquired from/returned to the pools.
  Just implement the interface in your script, use a concrete prefab pool with your script type and you're set!


## How to install
Either:
- Install using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html) with the following URL:
  ```
  https://github.com/gilzoide/unity-prefab-pool.git
  ```
- Clone this repository or download a snapshot of it directly inside your project's `Assets` or `Packages` folder.


## Using generic prefab pools
<details>
<summary>Specifying the prefab type for <code>PrefabPool<></code></summary>
To customize the prefab type accepted by a prefab pool, just declare your variable with a concrete version of the <code>PrefabPool<></code> class.

```cs
using Gilzoide.PrefabPool;
using UnityEngine;

public class MyScript : MonoBehaviour
{
    public PrefabPool<Transform> myTransformPool;

    void OnDestroy()
    {
        myTransformPool.Dispose();
    }
}
```
![Inspector showing "myTransformPool" expecting an object of type "Transform"](Extras~/generic-transform-pool.png)
</details>

<details>
<summary>Specifying the prefab type for <code>PrefabPoolComponent<></code></summary>
To customize the prefab type accepted by a prefab pool component, create a concrete class that inherits <code>PrefabPoolComponent<></code>:

```cs
using Gilzoide.PrefabPool;

public class MyScriptPoolComponent : PrefabPoolComponent<MyScript>;
{
}
```
![Inspector showing a prefab pool component expecting a prefab of type "MyScript"](Extras~/generic-pool-component.png)
</details>

<details>
<summary>Specifying the prefab type for <code>PrefabPoolAsset<></code></summary>
To customize the prefab type accepted by a prefab pool asset, create a concrete class that inherits <code>PrefabPoolAsset<></code>:

```cs
using Gilzoide.PrefabPool;
using UnityEngine;

[CreateAssetMenu(menuName = "MyScriptPoolAsset")]
public class MyScriptPoolAsset : PrefabPoolAsset<MyScript>
{
}
```
![Inspector showing a prefab pool asset expecting a prefab of type "MyScript"](Extras~/generic-pool-asset.png)
</details>