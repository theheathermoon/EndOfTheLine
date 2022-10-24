using UnityEditor;
using HFPS.Player;

namespace HFPS.Editors
{
    [CustomEditor(typeof(WeaponController)), CanEditMultipleObjects]
    public class WeaponControllerEditor : Editor
    {
        SerializedProperty m_bulletModelSettings;
        SerializedProperty m_shellEjectSettings;

        WeaponController controller;

        void OnEnable()
        {
            controller = target as WeaponController;
            m_bulletModelSettings = serializedObject.FindProperty("bulletModelSettings");
            m_shellEjectSettings = serializedObject.FindProperty("shellEjectSettings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            DrawPropertiesExcluding(serializedObject, "bulletModelSettings", "shellEjectSettings");

            if (controller.bulletType == WeaponController.BulletType.Bullet)
            {
                EditorGUILayout.PropertyField(m_bulletModelSettings, true);
            }

            if (controller.bulletSettings.ejectShells)
            {
                EditorGUILayout.PropertyField(m_shellEjectSettings, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}