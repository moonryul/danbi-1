using UnityEngine;
namespace Danbi
{

#if UNITY_EDITOR
    using UnityEditor;
    [CustomEditor(typeof(DanbiCameraControl))]
    public class DanbiCameraControlEditor : Editor
    {
        SerializedProperty ThresholdIterativeProp, SafeCounterProp, ThresholdNewtonProp, ScriptDisplayProp, CameraExternalDataProp;

        public override void OnInspectorGUI()
        {
            var src = target as DanbiCameraControl;

            // ThresholdIterativeProp = serializedObject.FindProperty("ThresholdIterative");
            // SafeCounterProp = serializedObject.FindProperty("SafeCounter");
            // ThresholdNewtonProp = serializedObject.FindProperty("ThresholdNewton");
            if (ScriptDisplayProp is null)
            {
                ScriptDisplayProp = serializedObject.FindProperty("m_Script");
            }
            // CameraExternalDataProp = serializedObject.FindProperty("CameraExternalData");

            // 1. Display the script prop.
            GUI.enabled = false;
            EditorGUILayout.PropertyField(ScriptDisplayProp, true);
            GUI.enabled = true;
            // DrawDefaultInspector();

            PushSpace(1);

            GUI.enabled = false;

            DrawOthers(src);

            // DrawPhysicalCamera(src);

            DrawCalibratedProjectorMode(src);

            DrawCalibratedProjectorInternalParameters(src);

            DrawCalibratedProjectorExternalParameters(src);

            GUI.enabled = true;

            serializedObject.ApplyModifiedProperties();
        }

        // void DrawPhysicalCamera(DanbiCameraControl src)
        // {
        //     EditorGUI.BeginChangeCheck();
        //     PushSeparator();
        //     // Toggle if you are useing Physical Camera.    
        //     src.usePhysicalCamera = EditorGUILayout.Toggle("Use Physical Camera?", src.usePhysicalCamera);

        //     PushSpace(2);
        //     // Open the other properties.
        //     if (src.usePhysicalCamera)
        //     {
        //         src.focalLength = EditorGUILayout.FloatField("Focal Length", src.focalLength);
        //         src.sensorSize = EditorGUILayout.Vector2Field("Sensor Size", src.sensorSize);
        //         PushSpace(2);
        //     }

        //     // update the changes in the editor.
        //     if (EditorGUI.EndChangeCheck())
        //     {
        //         var fwdCamRef = Camera.main;
        //         fwdCamRef.usePhysicalProperties = src.usePhysicalCamera;
        //         fwdCamRef.focalLength = src.focalLength;
        //         fwdCamRef.sensorSize = src.sensorSize;
        //     }
        // }

        void DrawCalibratedProjectorMode(DanbiCameraControl src)
        {
            // Toggle if you are only using Projection Calibration.
            EditorGUI.BeginChangeCheck();
            PushSeparator();
            src.useCalibratedProjector = EditorGUILayout.Toggle("Use Camera Calibration?", src.useCalibratedProjector);

            PushSpace(2);
            if (src.useCalibratedProjector)
            {
                src.calibratedProjectorMode = (EDanbiCameraUndistortionMethod)EditorGUILayout.EnumPopup("Calibration Mode", src.calibratedProjectorMode);

                switch (src.calibratedProjectorMode)
                {
                    case EDanbiCameraUndistortionMethod.Direct:
                        //
                        break;

                    case EDanbiCameraUndistortionMethod.Iterative:
                        src.iterativeThreshold = EditorGUILayout.FloatField("Threshold", src.iterativeThreshold);
                        src.iterativeSafetyCounter = EditorGUILayout.FloatField("Safety Counter", src.iterativeSafetyCounter);
                        break;

                    case EDanbiCameraUndistortionMethod.Newton:
                        src.newtonThreshold = EditorGUILayout.FloatField("Threshold", src.newtonThreshold);
                        break;
                }
                PushSpace(2);

                // update the changes on Calibration.
                // if (EditorGUI.EndChangeCheck()) {

                // }
            }
        }

        void DrawCalibratedProjectorInternalParameters(DanbiCameraControl src)
        {
            if (src.cameraInternalData != null)
            {
                EditorGUI.BeginChangeCheck();
                PushSeparator();

                var fwdData = src.cameraInternalData;

                src.radialCoefficient = EditorGUILayout.Vector3Field("Radial Coefficient", new Vector3(fwdData.radialCoefficientX, fwdData.radialCoefficientY, fwdData.radialCoefficientZ));
                src.tangentialCoefficient = EditorGUILayout.Vector2Field("Tangential Coefficient", new Vector2(fwdData.tangentialCoefficientX, fwdData.tangentialCoefficientY));
                src.principalCoefficient = EditorGUILayout.Vector2Field("Principal Coefficient", new Vector2(fwdData.principalPointX, fwdData.principalPointY));
                src.externalFocalLength = EditorGUILayout.Vector2Field("External Focal Length", new Vector2(fwdData.focalLengthX, fwdData.focalLengthY));
                src.skewCoefficient = EditorGUILayout.FloatField("Skew Coefficient", fwdData.skewCoefficient);

                PushSpace(2);
                EditorGUI.EndChangeCheck();
            }
        }

        void DrawCalibratedProjectorExternalParameters(DanbiCameraControl src)
        {
            if (src.cameraExternalData != null)
            {
                EditorGUI.BeginChangeCheck();
                PushSeparator();
                var fwdData = src.cameraExternalData;

                src.projectorPosition = EditorGUILayout.Vector3Field("Projector Position", new Vector3(fwdData.projectorPosition.x, fwdData.projectorPosition.y, fwdData.projectorPosition.z));
                src.xAxis = EditorGUILayout.Vector3Field("X Axis", new Vector3(fwdData.xAxis.x, fwdData.xAxis.y, fwdData.xAxis.z));
                src.yAxis = EditorGUILayout.Vector3Field("Y Axis", new Vector3(fwdData.yAxis.x, fwdData.yAxis.y, fwdData.yAxis.z));
                src.zAxis = EditorGUILayout.Vector3Field("Z Axis", new Vector3(fwdData.zAxis.x, fwdData.zAxis.y, fwdData.zAxis.z));

                PushSpace(2);
                EditorGUI.EndChangeCheck();
            }
        }

        void DrawOthers(DanbiCameraControl src)
        {
            EditorGUI.BeginChangeCheck();

            EditorGUILayout.LabelField("Camera Basic Parameters");
            src.fov = EditorGUILayout.FloatField("Field of View", src.fov);
            src.nearFar = EditorGUILayout.Vector2Field("Near-----Far", src.nearFar);
            src.aspectRatioDivided = EditorGUILayout.FloatField("Divided Aspect Ratio", src.aspectRatioDivided);
            src.aspectRatio = EditorGUILayout.Vector2Field("Aspect Ratio (width, height)", src.aspectRatio);
            PushSpace(2);

            if (EditorGUI.EndChangeCheck())
            {
                var fwdCamRef = Camera.main;

                fwdCamRef.fieldOfView = src.fov;
                fwdCamRef.nearClipPlane = src.nearFar.x;
                fwdCamRef.farClipPlane = src.nearFar.y;
                fwdCamRef.aspect = src.aspectRatioDivided;
            }
        }

        static void PushSpace(int count)
        {
            for (int i = 0; i < count; ++i)
            {
                EditorGUILayout.Space();
            }
        }

        static void PushSeparator()
        {
            EditorGUILayout.LabelField("------------------------------------------------------------------------------------------------------------------------");
        }
    }; // end-of-class.
#endif
};