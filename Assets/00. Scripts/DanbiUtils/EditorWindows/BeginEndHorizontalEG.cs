using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
public class BeginEndHorizontalEG : EditorWindow {
  [MenuItem("Examples/Begin-End Horizontal usage")]
  static void Init() {
    var window = EditorWindow.GetWindow<BeginEndHorizontalEG>();
    window.Show();
  }

  void OnGUI() {
    Rect rect = EditorGUILayout.BeginHorizontal("Button");
    if (GUI.Button(rect, GUIContent.none)) {
      Debug.Log("Go here");
    }

    GUILayout.Label("I'm inside the button");
    GUILayout.Label("So am I");
    EditorGUILayout.EndHorizontal();
  }
};
#endif