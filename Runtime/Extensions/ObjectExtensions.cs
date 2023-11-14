using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Gilzoide.PrefabPool.Extensions
{
    public static class ObjectExtensions
    {
        public static bool TryGetGameObject(this Object instance, out GameObject gameObject)
        {
            if (instance is GameObject go)
            {
                gameObject = go;
                return true;
            }
            else if (instance is Component component)
            {
                gameObject = component.gameObject;
                return true;
            }
            else
            {
                gameObject = null;
                return false;
            }
        }

#pragma warning disable UNT0014  // "Invalid type for call to GetComponent"
        public static PooledObject<List<T>> GetInterfaceObjects<T>(this Object instance, out List<T> objs, out GameObject gameObject)
        {
            var pooledList = ListPool<T>.Get(out objs);
            if (instance is GameObject go)
            {
                gameObject = go;
                gameObject.GetComponentsInChildren(true, objs);
            }
            else if (instance is Component component)
            {
                gameObject = component.gameObject;
                gameObject.GetComponentsInChildren(true, objs);
            }
            else if (instance is T poolObject)
            {
                gameObject = null;
                objs.Add(poolObject);
            }
            else
            {
                gameObject = null;
            }
            return pooledList;
        }
#pragma warning restore UNT0014
    }
}
