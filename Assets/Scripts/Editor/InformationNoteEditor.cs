using UnityEngine;

#if UNITY_EDITOR  
using UnityEditor;
[CustomEditor(typeof(InformationNote))]
public class InformationNoteEditor : Editor {
  /// <summary>
  /// 
  /// </summary>
  string ButtonText = "Start typing.";
  /// <summary>
  /// 
  /// </summary>
  int Status = 5;
  /// <summary>
  /// 
  /// </summary>
  string[] DisplayOptions = new string[] {
    "Line Label", "Box Text", "Box Info", "Box Warning", "Box Error"
  };
  /// <summary>
  /// 
  /// </summary>
  int[] FinalDisplayOption = new int[] {
    0, 1, 2, 3, 4
  };

  public override void OnInspectorGUI() {
    base.OnInspectorGUI();
    var inst = target as InformationNote;
    if (!inst.IsReady) {
      switch (Status) {
        case 0:
        if (EditorGUILayout.Toggle(inst.IsReady)) {
          inst.SwitchToggle();
        }
        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField(inst.TextInfo);
        EditorGUILayout.LabelField("");
        break;

        case 1: // Small info box.
        if (GUILayout.Button(ButtonText)) {
          inst.SwitchToggle();
        }
        ButtonText = "INFO";
        EditorGUILayout.LabelField("");
        EditorGUILayout.HelpBox(inst.TextInfo, MessageType.None);
        EditorGUILayout.LabelField("");
        break;

        case 2:
        goto default; // same as default.      

        case 3: // Warning box.
        if (GUILayout.Button(ButtonText)) {
          inst.SwitchToggle();
        }
        ButtonText = "ALERT";
        EditorGUILayout.LabelField("");
        EditorGUILayout.HelpBox(inst.TextInfo, MessageType.Warning);
        EditorGUILayout.LabelField("");
        break;

        case 4: // Error box.
        if (GUILayout.Button(ButtonText)) {
          inst.SwitchToggle();
        }
        ButtonText = "ERROR";
        EditorGUILayout.LabelField("");
        EditorGUILayout.HelpBox(inst.TextInfo, MessageType.Error);
        EditorGUILayout.LabelField("");
        break;

        default:
        if (GUILayout.Button(ButtonText)) {
          inst.SwitchToggle();
        }
        ButtonText = "README";
        EditorGUILayout.LabelField("");
        EditorGUILayout.HelpBox(inst.TextInfo, MessageType.Info);
        EditorGUILayout.LabelField("");
        break;
      }
    } else {
      // Visualization of final text in the inspector.
      ButtonText = "LOCK";
      // Display [ LOCK ] Button and switch if it is pressed.
      if (GUILayout.Button(ButtonText)) {
        inst.SwitchToggle();
      }

      // [ Input Text ]
      inst.TextInfo = EditorGUILayout.TextArea(inst.TextInfo);

      // Selection.
      Status = EditorGUILayout.IntPopup("Text Type:", Status, DisplayOptions, FinalDisplayOption);

      // Warning.
      EditorGUILayout.HelpBox("Press LOCK at the top when finish. ", MessageType.Warning);
    }
  }
};
#endif
