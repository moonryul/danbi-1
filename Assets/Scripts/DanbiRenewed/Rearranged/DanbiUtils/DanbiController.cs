using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
/// <summary>
/// 
/// </summary>
public class DanbiController : EditorWindow {
  Camera[] Cams;

  [MenuItem("Danbi/Open Controller")]
  public static void OpenWindow() {
    var windowInstance = GetWindow<DanbiController>();
    windowInstance.Show();
    // Set the title name of current window.
    windowInstance.titleContent = new GUIContent("Danbi Controller!");
  }

  void OnGUI() {
    EditorGUI.BeginChangeCheck();
    EditorGUILayout.Space();
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("          Danbi Controller");
    EditorGUILayout.Space();
    EditorGUILayout.Space();

    if (GUILayout.Button("Get All Camera References")) {
      Cams = FindObjectsOfType<Camera>();
      foreach (var cam in Cams) {
        Debug.Log($"{cam.name} is selected! number of cameras : {Cams.Length}.");
      }
    }

    EditorGUILayout.Space();
    if (ReferenceEquals(Cams, null)) {
      return;
    }

    foreach (var cam in Cams) {
      cam.usePhysicalProperties = EditorGUILayout.BeginToggleGroup("Is Using Physical Camera?", cam.usePhysicalProperties);
      EditorGUILayout.Space();

      cam.focalLength = float.Parse(EditorGUILayout.TextField("Current Focal Length", cam.focalLength.ToString()));
      EditorGUILayout.LabelField("Max Focal Length = 13,1 (height to Camera : 2.04513m)");
      EditorGUILayout.LabelField("Min Focal Length = 11,1 (height to Camera : 2.04896m)");
      EditorGUILayout.Space();

      EditorGUILayout.TextField("Current Field Of View", cam.fieldOfView.ToString());
      EditorGUILayout.LabelField("Calculated by Physical Camera Props");
      EditorGUILayout.Space();

      float x, y;
      x = float.Parse(EditorGUILayout.TextField("Current Sensor Size X", cam.sensorSize.x.ToString()));
      EditorGUILayout.LabelField("(16:10 일때 = 16,36976)");
      EditorGUILayout.Space();

      y = float.Parse(EditorGUILayout.TextField("Current Sensor Size Y", cam.sensorSize.y.ToString()));
      EditorGUILayout.LabelField("(16:10 일떄 = 10,02311)");
      EditorGUILayout.LabelField("(16:9 일때 = 9,020799)");
      EditorGUILayout.Space();

      EditorGUILayout.LabelField($"Current GateFit Mode is {cam.gateFit.ToString()}");
      EditorGUILayout.EndToggleGroup();

      if (EditorGUI.EndChangeCheck()) {
        cam.sensorSize = new UnityEngine.Vector2(x, y);
        cam.gateFit = Camera.GateFitMode.None;

        if (cam.focalLength == 11.1f) {
          cam.transform.position = new Vector3(0.0f, 2.04896f, 0.0f);
        }
        else if (cam.focalLength == 13.1f) {
          cam.transform.position = new Vector3(0.0f, 2.04513f, 0.0f);
        }
      }
    }
  }
};
#endif
