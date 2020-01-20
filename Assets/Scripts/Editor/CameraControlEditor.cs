using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(CameraControl))]
public class CameraControlEditor : Editor {
  string[] DisplayOption = new string[] {
    "Free Camera", "Vertical Camera", "Y-Rotating Camera"
  };

  int[] DisplayOptionToInt = new int[] {
    0, 1, 2
  };

  public override void OnInspectorGUI() {
    base.OnInspectorGUI();

    var fwd = target as CameraControl;
    int status = (int)fwd.MovementMode;
    string contents = default;
    EditorStyles.helpBox.fontSize = 14;
    EditorStyles.helpBox.fontStyle = FontStyle.Bold;
    switch (status) {
      // Free Camera.
      case 0:
      contents = "<Free Camera>\n\n" +
        "Movement : W / A / S / D\n" +
        "Move Upwards : E / Move Downwards : Q\n" +
        "Fast Move : Shift / Slow : Caps Lock\n";
      EditorGUILayout.LabelField("");
      EditorGUILayout.LabelField("");
      EditorGUILayout.HelpBox(contents, MessageType.Info);
      EditorGUILayout.LabelField("");
      EditorGUILayout.LabelField("");
      break;
      // Vertical Camera.
      case 1:
      contents = "<Vertical Camera>\n\n" +
        "Move Upwards : W\n" +
        "Move Downwards : S\n" +
        "Fast Move : Shift / Slow : Caps Lock\n";
      EditorGUILayout.LabelField("");
      EditorGUILayout.LabelField("");
      EditorGUILayout.HelpBox(contents, MessageType.Info);
      EditorGUILayout.LabelField("");
      EditorGUILayout.LabelField("");
      break;
      // Y-Rotating Camera.      
      case 2:
      contents = "<Y-Rotating Camera>\n\n" +
      "Rotate to Left : A\n" +
      "Rotate to Right : D\n" +
      "Fast Move : Shift / Slow : Caps Lock\n";
      EditorGUILayout.LabelField("");
      EditorGUILayout.LabelField("");
      EditorGUILayout.HelpBox(contents, MessageType.Info);
      EditorGUILayout.LabelField("");
      EditorGUILayout.LabelField("");
      break;
    }
  }
};
#endif