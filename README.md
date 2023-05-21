# Prefab Pool
Prefab instance pool that is configurable in the Inspector, supports any component type and is available as a serializable C# class, MonoBehaviour and ScriptableObject.


## Features
- Prefab pools may live either as project assets ([PrefabPoolAsset](Runtime/PrefabPoolAsset.cs)), standalone components in the scene ([PrefabPoolComponent](Runtime/PrefabPoolComponent.cs)), or may be a part of your own scripts ([PrefabPool](Runtime/PrefabPool.cs)), whichever fits best for the use case.
- Supports prewarming instances: configure pools to instantiate a number of prefabs when created, with an optional limit of objects per frame to avoid spikes in CPU usage.
- Supports generic typing for customizing which prefabs can be assigned to the pool.
  By default, the `Transform` component is used in the non-generic prefab pool classes.
- Optional [IPrefabPoolBehaviour](Runtime/IPrefabPoolBehaviour.cs) interface for customizing what happens when prefab instances are acquired from/returned to the pools.


## How to install
Either:
- Install using the [Unity Package Manager](https://docs.unity3d.com/Manual/upm-ui-giturl.html) with the following URL:
  ```
  https://github.com/gilzoide/unity-prefab-pool.git
  ```
- Clone this repository or download a snapshot of it directly inside your project's `Assets` or `Packages` folder.