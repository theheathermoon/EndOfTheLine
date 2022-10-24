using UnityEditor;
using UnityEngine;
using HFPS.Systems;

namespace HFPS.Editors
{
    [CustomEditor(typeof(MonoBehaviour), true)]
    public class ScriptlessDrawer : Editor
    {
        private bool hideScriptField;

        private void OnEnable()
        {
            hideScriptField = target.GetType().GetCustomAttributes(typeof(Scriptless), false).Length > 0;
        }

        public override void OnInspectorGUI()
        {
            if (hideScriptField)
            {
                serializedObject.Update();

                EditorGUI.BeginChangeCheck();

                DrawPropertiesExcluding(serializedObject, "m_Script");

                if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
            }
            else
            {
                base.OnInspectorGUI();
            }
        }
    }
}