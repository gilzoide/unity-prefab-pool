using System;
using UnityEditor;

namespace Gilzoide.PrefabPool.Extensions
{
    public static class IDisposableExtensions
    {
#if UNITY_EDITOR
        public static void DisposeWhenExitingPlayMode(this IDisposable pool, PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.ExitingPlayMode)
            {
                pool.Dispose();
            }
        }
#endif
    }
}
