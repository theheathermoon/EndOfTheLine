using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GeneratorEditor : MonoBehaviour
{
    [CustomEditor(typeof(Generator))]
    class DecalMeshHelperEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if(!Application.isPlaying)
            {
                GUILayout.Space(20);
                if (GUILayout.Button("Generate", GUILayout.Height(50)))
                    GameObject.FindObjectOfType<Generator>().RunProgram();
            }
        }
    }
}
