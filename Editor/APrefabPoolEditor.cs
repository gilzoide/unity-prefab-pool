using System.Collections.Generic;
using Gilzoide.PrefabPool.Internal;
using UnityEditor;
using UnityEngine;

namespace Gilzoide.PrefabPool.Editor
{
    public abstract class APrefabPoolEditor : UnityEditor.Editor
    {
        [SerializeField] private bool _foldoutInactive;
        [SerializeField] private bool _foldoutActive;

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (!IsDebugPanelEnabled())
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Debug panel", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            using (new EditorGUI.DisabledScope(true))
            {
                var pool = serializedObject.targetObject as IPrefabPool;
                EditorGUILayout.LabelField($"Total instances: {pool.CountAll}");
                if (_foldoutActive = EditorGUILayout.Foldout(_foldoutActive, $"Active instances: {pool.CountActive}"))
                {
                    DrawObjectList(pool.ActiveInstances);
                }
                if (_foldoutInactive = EditorGUILayout.Foldout(_foldoutInactive, $"Inactive instances: {pool.CountInactive}"))
                {
                    DrawObjectList(pool.InactiveInstances);
                }
            }
            EditorGUI.indentLevel--;
        }

        public override bool RequiresConstantRepaint()
        {
            return IsDebugPanelEnabled() && !EditorApplication.isPaused;
        }
        
        protected virtual bool IsPoolEnabled => true;

        private bool IsDebugPanelEnabled()
        {
            return Application.isPlaying
                && !serializedObject.isEditingMultipleObjects
                && IsPoolEnabled;
        }

        private void DrawObjectList(IEnumerable<Object> objects)
        {
            EditorGUI.indentLevel++;
            foreach (Object obj in objects)
            {
                EditorGUILayout.ObjectField(GUIContent.none, obj, typeof(Object), true);
            }
            EditorGUI.indentLevel--;
        }
    }

    [CustomEditor(typeof(APrefabPoolAsset<,>), true)]
    [CanEditMultipleObjects]
    public class PrefabPoolAssetEditor : APrefabPoolEditor {}

    [CustomEditor(typeof(APrefabPoolComponent<,>), true)]
    [CanEditMultipleObjects]
    public class PrefabPoolComponentEditor : APrefabPoolEditor
    {
        protected override bool IsPoolEnabled => (serializedObject.targetObject as MonoBehaviour).isActiveAndEnabled;
    }
}
