using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
public class BeginEndVerticalEG : EditorWindow {
  [MenuItem("Examples/Begin-End Vertical usage")]
  public static void Init() {
    var wnd = EditorWindow.GetWindow<BeginEndVerticalEG>();
    wnd.Show();
  }

  void OnGUI() {
    Rect rect = EditorGUILayout.BeginVertical("Button");
    if (GUI.Button(rect, GUIContent.none)) {
      Debug.Log("Go here!");
    }

    GUILayout.Label("I''m inside the button");
    GUILayout.Label("So am I");
    EditorGUILayout.EndVertical();
  }
};
#endif
