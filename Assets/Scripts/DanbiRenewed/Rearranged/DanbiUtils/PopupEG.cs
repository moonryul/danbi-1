using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
public class PopupEG : EditorWindow {
  public string[] options = new string[] { "Cube", "Sphere", "Plane" };
  public int idx = 0;

  [MenuItem("Examples/PopupEG", false, priority: 1)]
  static void Init() {
    var wnd = EditorWindow.GetWindow<PopupEG>();
    wnd.Show();
  }

  void OnGUI() {
    idx = EditorGUILayout.Popup(selectedIndex: idx, displayedOptions: options);
    if (GUILayout.Button("Create")) {
      OpenPopup();
    }
  }

  void OpenPopup() {
    switch (idx) {
      case 0:
      var cubeGo = GameObject.CreatePrimitive(PrimitiveType.Cube);
      cubeGo.transform.position = default;
      break;

      case 1:
      var sphereGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      sphereGo.transform.position = default;
      break;

      case 2:
      var planeGo = GameObject.CreatePrimitive(PrimitiveType.Plane);
      planeGo.transform.position = default;
      break;

      default:
      Debug.LogError("Unrecognized Option!", this);
      break;
    }
  }
};
#endif
