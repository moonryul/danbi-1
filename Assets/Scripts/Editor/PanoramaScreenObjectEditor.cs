using System.Collections;
using System.Collections.Generic;

using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(PanoramaScreenObject))]
public class PanoramaScreenobjectEditor : Editor {
  SerializedProperty ScriptDisplayProp;
  void OnEnable() {
    ScriptDisplayProp = serializedObject.FindProperty("m_Script");
  }

  public override void OnInspectorGUI() {
    var src = target as PanoramaScreenObject;

    // 1. Display the script prop.
    GUI.enabled = false;
    EditorGUILayout.PropertyField(ScriptDisplayProp, true);
    GUI.enabled = true;
  }

  static void PushSpace(int count) {
    for (int i = 0; i < count; ++i) {
      EditorGUILayout.Space();
    }
  }

  static void PushSeparator() {
    EditorGUILayout.LabelField("------------------------------------------------------------------------------------------------------------------------");
  }
};
#endif
