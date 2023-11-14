using Gilzoide.PrefabPool.Internal;
using UnityEditor;
using UnityEngine;

namespace Gilzoide.PrefabPool.Editor
{
    [CanEditMultipleObjects]
    public abstract class APrefabPoolEditor : UnityEditor.Editor
    {
        public override bool HasPreviewGUI()
        {
            return Application.isPlaying && IsEnabled;
        }

        public override void OnPreviewGUI(Rect r, GUIStyle background)
        {
            using (new EditorGUI.DisabledScope(true))
            {
                var pool = serializedObject.targetObject as IPrefabPool;
                r.height = EditorGUI.GetPropertyHeight(SerializedPropertyType.Integer, GUIContent.none);
                EditorGUI.IntField(r, "Inactive Count", pool.CountInactive);
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.IntField(r, "Active Count", pool.CountActive);
                r.y += r.height + EditorGUIUtility.standardVerticalSpacing;
                EditorGUI.IntField(r, "Total Count", pool.CountAll);
            }
        }

        public override bool RequiresConstantRepaint()
        {
            return HasPreviewGUI();
        }
        
        protected virtual bool IsEnabled => true;
    }

    [CustomEditor(typeof(APrefabPoolAsset<,>), true)]
    public class PrefabPoolAssetEditor : APrefabPoolEditor {}

    [CustomEditor(typeof(APrefabPoolComponent<,>), true)]
    public class PrefabPoolComponentEditor : APrefabPoolEditor
    {
        protected override bool IsEnabled => (serializedObject.targetObject as MonoBehaviour).isActiveAndEnabled;
    }
}
