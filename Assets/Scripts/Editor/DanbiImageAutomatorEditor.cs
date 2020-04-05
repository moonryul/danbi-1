using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(DanbiImageAutomator))]
public class DanbiImageAutomatorEditor : Editor {

  SerializedProperty DistanceRangeProp;

  void OnEnable() {
    DistanceRangeProp = serializedObject.FindProperty("Distance");
  }
  public override void OnInspectorGUI() {
    //base.OnInspectorGUI();
    serializedObject.Update();
    EditorGUILayout.PropertyField(DistanceRangeProp);
    serializedObject.ApplyModifiedProperties();

    if (GUILayout.Button("Add Automation Items!")) {
      var inst = target as RayTracingMaster;

    }

    if (GUILayout.Button("Create Images!")) {
      var inst = target as RayTracingMaster;

    }

  }

};
#endif


