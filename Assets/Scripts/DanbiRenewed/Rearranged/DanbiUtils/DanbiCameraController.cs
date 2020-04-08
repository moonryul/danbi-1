using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
/// <summary>
/// 
/// </summary>
public class DanbiCameraController : EditorWindow {
  Camera[] Cams;

  [MenuItem("Danbi/Open CameraController")]
  public static void OpenWindow() {
    var windowInstance = GetWindow<DanbiCameraController>();
    windowInstance.Show();
    // Set the title name of current window.
    windowInstance.titleContent = new GUIContent("Danbi Camera Controller!");
  }

  void OnGUI() {

    EditorGUILayout.Space();
    EditorGUILayout.Space();
    EditorGUILayout.LabelField("          Danbi Camera Controller");
    EditorGUILayout.Space();
    EditorGUILayout.Space();

    if (GUILayout.Button("Get All Camera References")) {
      Cams = FindObjectsOfType<Camera>();
      foreach (var cam in Cams) {
        Debug.Log($"{cam.name} is selected! number of cameras : {Cams.Length}.");
      }
    }

    EditorGUILayout.Space();
    foreach (var cam in Cams) {
      cam.usePhysicalProperties = EditorGUILayout.BeginToggleGroup("Is Using Physical Camera?", cam.usePhysicalProperties);
      EditorGUILayout.Space();

      cam.focalLength = float.Parse(EditorGUILayout.TextField("Current Focal Length", cam.focalLength.ToString()));
      EditorGUILayout.LabelField("(default = 13,1)");
      EditorGUILayout.Space();

      float x, y;
      x = float.Parse(EditorGUILayout.TextField("Current Sensor Size X", cam.sensorSize.x.ToString()));
      EditorGUILayout.LabelField("(default = 16,36976)");
      EditorGUILayout.Space();

      y = float.Parse(EditorGUILayout.TextField("Current Sensor Size Y", cam.sensorSize.y.ToString()));
      EditorGUILayout.LabelField("(default = 10,02311)");
      EditorGUILayout.Space();

      cam.sensorSize = new UnityEngine.Vector2(x, y);
      cam.gateFit = Camera.GateFitMode.None;
      EditorGUILayout.LabelField($"Current GateFit Mode is {cam.gateFit.ToString()}");
      EditorGUILayout.EndToggleGroup();
    }
  }
};
#endif
