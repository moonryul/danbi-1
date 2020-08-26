using UnityEngine;
using Danbi;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(DanbiCameraControl))]
public class CanbiCameraControlEditor : Editor {

  public SerializedProperty ThresholdIterativeProp, SafeCounterProp, ThresholdNewtonProp, ScriptDisplayProp;

  // MonoScript scriptPicker;  

  void OnEnable() {
    ThresholdIterativeProp = serializedObject.FindProperty("ThresholdIterative");
    SafeCounterProp = serializedObject.FindProperty("SafeCounter");
    ThresholdNewtonProp = serializedObject.FindProperty("ThresholdNewton");
    ScriptDisplayProp = serializedObject.FindProperty("m_Script");
    //scriptPicker = MonoScript.FromMonoBehaviour(target as DanbiCameraControl);

  }

  public override void OnInspectorGUI() {
    var src = target as DanbiCameraControl;    

    // -> Draw "scriptPicker" field.
    //EditorGUI.BeginDisabledGroup(true);
    //EditorGUILayout.TextField("Script", GUILayout.Width(40), GUILayout.Height(20));
    //scriptPicker = EditorGUILayout.ObjectField(scriptPicker, typeof(MonoScript), false, GUILayout.Width(200), GUILayout.Height(20)) as MonoScript;
    //EditorGUI.EndDisabledGroup();    

    // 1. Display the script prop.
    GUI.enabled = false;    
    EditorGUILayout.PropertyField(ScriptDisplayProp, true);
    GUI.enabled = true;

    PushSpace(1);

    // 2. Toggle if you are using Projection Calibration.
    EditorGUI.BeginChangeCheck();
    src.useCalibration = EditorGUILayout.Toggle("Use Camera Calibration?", src.useCalibration);
    EditorGUI.EndChangeCheck();

    if (src.useCalibration) {
      EditorGUI.BeginChangeCheck();
      src.undistortMode = (EDanbiCalibrationMode)EditorGUILayout.EnumPopup("Calibration Mode", src.undistortMode);

      switch (src.undistortMode) {
        case EDanbiCalibrationMode.Direct:
          //
          break;

        case EDanbiCalibrationMode.Iterative:
          src.thresholdIterative = EditorGUILayout.FloatField("Threshold", src.thresholdIterative);
          src.safetyCounter = EditorGUILayout.IntField("Safety Counter", src.safetyCounter);          
          break;

        case EDanbiCalibrationMode.Newton:
          src.thresholdNewton = EditorGUILayout.FloatField("Threshold", src.thresholdNewton);          
          break;
      }
      EditorGUI.EndChangeCheck();
    }

    // 3. Toggle if you are useing Physical Camera.
    EditorGUI.BeginChangeCheck();
    src.usePhysicalCamera = EditorGUILayout.Toggle("Use Physical Camera?", src.usePhysicalCamera);
    EditorGUI.EndChangeCheck();

    if (src.usePhysicalCamera) {
      EditorGUI.BeginChangeCheck();
      src.focalLength = EditorGUILayout.FloatField("Focal Length", src.focalLength);
      src.sensorSize = EditorGUILayout.Vector2Field("Sensor Size", src.sensorSize);
      EditorGUI.EndChangeCheck();
    }

    PushSpace(2);

    EditorGUI.BeginChangeCheck();
    src.fov = EditorGUILayout.FloatField("Field of View", src.fov);
    src.nearFar = EditorGUILayout.Vector2Field("Near | Far", src.nearFar);
    EditorGUI.EndChangeCheck();
    //DrawDefaultInspector();
  }

  static void PushSpace(int count) {
    for (int i = 0; i < count; ++i) {
      EditorGUILayout.Space();
    }
  }
}; // end-of-class.
#endif