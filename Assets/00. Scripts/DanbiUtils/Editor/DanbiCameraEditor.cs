// using UnityEngine;
// namespace Danbi
// {

// #if UNITY_EDITOR
//     using UnityEditor;
//     [CustomEditor(typeof(DanbiCamera))]
//     public class DanbiCameraEditor : Editor
//     {
//         SerializedProperty ThresholdIterativeProp, SafeCounterProp, ThresholdNewtonProp, ScriptDisplayProp, CameraExternalDataProp;

//         public override void OnInspectorGUI()
//         {
//             var src = target as DanbiCamera;

//             // ThresholdIterativeProp = serializedObject.FindProperty("ThresholdIterative");
//             // SafeCounterProp = serializedObject.FindProperty("SafeCounter");
//             // ThresholdNewtonProp = serializedObject.FindProperty("ThresholdNewton");
//             if (ScriptDisplayProp is null)
//             {
//                 ScriptDisplayProp = serializedObject.FindProperty("m_Script");
//             }
//             // CameraExternalDataProp = serializedObject.FindProperty("CameraExternalData");

//             // 1. Display the script prop.
//             GUI.enabled = false;
//             EditorGUILayout.PropertyField(ScriptDisplayProp, true);
//             GUI.enabled = true;
//             // DrawDefaultInspector();

//             PushSpace(1);

//             GUI.enabled = false;

//             DrawOthers(src);

//             // DrawPhysicalCamera(src);

//             DrawCalibratedProjectorMode(src);

//             DrawCalibratedProjectorInternalParameters(src);

//             DrawCalibratedProjectorExternalParameters(src);

//             GUI.enabled = true;

//             serializedObject.ApplyModifiedProperties();
//         }

//         // void DrawPhysicalCamera(DanbiCamera src)
//         // {
//         //     EditorGUI.BeginChangeCheck();
//         //     PushSeparator();
//         //     // Toggle if you are useing Physical Camera.    
//         //     src.usePhysicalCamera = EditorGUILayout.Toggle("Use Physical Camera?", src.usePhysicalCamera);

//         //     PushSpace(2);
//         //     // Open the other properties.
//         //     if (src.usePhysicalCamera)
//         //     {
//         //         src.focalLength = EditorGUILayout.FloatField("Focal Length", src.focalLength);
//         //         src.sensorSize = EditorGUILayout.Vector2Field("Sensor Size", src.sensorSize);
//         //         PushSpace(2);
//         //     }

//         //     // update the changes in the editor.
//         //     if (EditorGUI.EndChangeCheck())
//         //     {
//         //         var fwdCamRef = Camera.main;
//         //         fwdCamRef.usePhysicalProperties = src.usePhysicalCamera;
//         //         fwdCamRef.focalLength = src.focalLength;
//         //         fwdCamRef.sensorSize = src.sensorSize;
//         //     }
//         // }

//         void DrawCalibratedProjectorMode(DanbiCamera src)
//         {
//             // Toggle if you are only using Projection Calibration.
//             EditorGUI.BeginChangeCheck();
//             PushSeparator();
//             src.m_useCalibratedProjector = EditorGUILayout.Toggle("Use Camera Calibration?", src.m_useCalibratedProjector);

//             PushSpace(2);
//             if (src.m_useCalibratedProjector)
//             {
//                 src.m_lensUndistortMode = (EDanbiLensUndistortMode)EditorGUILayout.EnumPopup("Calibration Mode", src.m_lensUndistortMode);

//                 switch (src.m_lensUndistortMode)
//                 {
//                     case EDanbiLensUndistortMode.Direct:
//                         //
//                         break;

//                     case EDanbiLensUndistortMode.Iterative:
//                         src.m_iterativeThreshold = EditorGUILayout.FloatField("Threshold", src.m_iterativeThreshold);
//                         src.m_iterativeSafetyCounter = EditorGUILayout.FloatField("Safety Counter", src.m_iterativeSafetyCounter);
//                         break;

//                     case EDanbiLensUndistortMode.Newton:
//                         src.m_newtonThreshold = EditorGUILayout.FloatField("Threshold", src.m_newtonThreshold);
//                         break;
//                 }
//                 PushSpace(2);

//                 // update the changes on Calibration.
//                 // if (EditorGUI.EndChangeCheck()) {

//                 // }
//             }
//         }

//         void DrawCalibratedProjectorInternalParameters(DanbiCamera src)
//         {
//             if (src.m_cameraInternalData != null)
//             {
//                 EditorGUI.BeginChangeCheck();
//                 PushSeparator();

//                 var fwdData = src.m_cameraInternalData;

//                 src.m_radialCoefficient = EditorGUILayout.Vector3Field("Radial Coefficient", new Vector3(fwdData.radialCoefficientX, fwdData.radialCoefficientY, fwdData.radialCoefficientZ));
//                 src.m_tangentialCoefficient = EditorGUILayout.Vector2Field("Tangential Coefficient", new Vector2(fwdData.tangentialCoefficientX, fwdData.tangentialCoefficientY));
//                 src.m_principalCoefficient = EditorGUILayout.Vector2Field("Principal Coefficient", new Vector2(fwdData.principalPointX, fwdData.principalPointY));
//                 src.m_externalFocalLength = EditorGUILayout.Vector2Field("External Focal Length", new Vector2(fwdData.focalLengthX, fwdData.focalLengthY));
//                 src.m_skewCoefficient = EditorGUILayout.FloatField("Skew Coefficient", fwdData.skewCoefficient);

//                 PushSpace(2);
//                 EditorGUI.EndChangeCheck();
//             }
//         }

//         void DrawCalibratedProjectorExternalParameters(DanbiCamera src)
//         {
//             if (src.m_cameraExternalData != null)
//             {
//                 EditorGUI.BeginChangeCheck();
//                 PushSeparator();
//                 var fwdData = src.m_cameraExternalData;

//                 src.m_projectorPosition = EditorGUILayout.Vector3Field("Projector Position", new Vector3(fwdData.projectorPosition.x, fwdData.projectorPosition.y, fwdData.projectorPosition.z));
//                 src.m_xAxis = EditorGUILayout.Vector3Field("X Axis", new Vector3(fwdData.xAxis.x, fwdData.xAxis.y, fwdData.xAxis.z));
//                 src.m_yAxis = EditorGUILayout.Vector3Field("Y Axis", new Vector3(fwdData.yAxis.x, fwdData.yAxis.y, fwdData.yAxis.z));
//                 src.m_zAxis = EditorGUILayout.Vector3Field("Z Axis", new Vector3(fwdData.zAxis.x, fwdData.zAxis.y, fwdData.zAxis.z));

//                 PushSpace(2);
//                 EditorGUI.EndChangeCheck();
//             }
//         }

//         void DrawOthers(DanbiCamera src)
//         {
//             EditorGUI.BeginChangeCheck();

//             EditorGUILayout.LabelField("Camera Basic Parameters");
//             src.m_fov = EditorGUILayout.FloatField("Field of View", src.m_fov);
//             src.m_nearFar = EditorGUILayout.Vector2Field("Near-----Far", src.m_nearFar);
//             src.m_aspectRatioDivided = EditorGUILayout.FloatField("Divided Aspect Ratio", src.m_aspectRatioDivided);
//             src.m_aspectRatio = EditorGUILayout.Vector2Field("Aspect Ratio (width, height)", src.m_aspectRatio);
//             PushSpace(2);

//             if (EditorGUI.EndChangeCheck())
//             {
//                 var fwdCamRef = Camera.main;

//                 fwdCamRef.fieldOfView = src.m_fov;
//                 fwdCamRef.nearClipPlane = src.m_nearFar.x;
//                 fwdCamRef.farClipPlane = src.m_nearFar.y;
//                 fwdCamRef.aspect = src.m_aspectRatioDivided;
//             }
//         }

//         static void PushSpace(int count)
//         {
//             for (int i = 0; i < count; ++i)
//             {
//                 EditorGUILayout.Space();
//             }
//         }

//         static void PushSeparator()
//         {
//             EditorGUILayout.LabelField("------------------------------------------------------------------------------------------------------------------------");
//         }
//     }; // end-of-class.
// #endif
// };