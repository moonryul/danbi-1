using UnityEngine;
using Danbi;

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

        DrawPhysicalCamera(src);

        DrawCameraCalibratedProjectionOnly(src);

        DrawCameraCalbiration(src);
        GUI.enabled = true;

        serializedObject.ApplyModifiedProperties();
    }

    void DrawPhysicalCamera(DanbiCameraControl src)
    {
        EditorGUI.BeginChangeCheck();
        PushSeparator();
        // Toggle if you are useing Physical Camera.    
        src.usePhysicalCamera = EditorGUILayout.Toggle("Use Physical Camera?", src.usePhysicalCamera);

        PushSpace(2);
        // Open the other properties.
        if (src.usePhysicalCamera)
        {
            src.focalLength = EditorGUILayout.FloatField("Focal Length", src.focalLength);
            src.sensorSize = EditorGUILayout.Vector2Field("Sensor Size", src.sensorSize);
            PushSpace(2);
        }

        // update the changes in the editor.
        if (EditorGUI.EndChangeCheck())
        {
            var fwdCamRef = Camera.main;
            fwdCamRef.usePhysicalProperties = src.usePhysicalCamera;
            fwdCamRef.focalLength = src.focalLength;
            fwdCamRef.sensorSize = src.sensorSize;
        }
    }

    void DrawCameraCalibratedProjectionOnly(DanbiCameraControl src)
    {
        // Toggle if you are only using Projection Calibration.
        EditorGUI.BeginChangeCheck();
        PushSeparator();
        src.useCalibration = EditorGUILayout.Toggle("Use Camera Calibration?", src.useCalibration);

        PushSpace(2);
        if (src.useCalibration)
        {
            src.undistortionMethod = (EDanbiCameraUndistortionMethod)EditorGUILayout.EnumPopup("Calibration Mode", src.undistortionMethod);

            switch (src.undistortionMethod)
            {
                case EDanbiCameraUndistortionMethod.Direct:
                    //
                    break;

                case EDanbiCameraUndistortionMethod.Iterative:
                    src.iterativeThreshold = EditorGUILayout.IntField("Threshold", src.iterativeThreshold);
                    src.iterativeSafetyCounter = EditorGUILayout.IntField("Safety Counter", src.iterativeSafetyCounter);
                    break;

                case EDanbiCameraUndistortionMethod.Newton:
                    src.newtonThreshold = EditorGUILayout.IntField("Threshold", src.newtonThreshold);
                    break;
            }
            PushSpace(2);

            // update the changes on Calibration.
            // if (EditorGUI.EndChangeCheck()) {

            // }
        }
    }

    void DrawCameraCalbiration(DanbiCameraControl src)
    {
        EditorGUI.BeginChangeCheck();
        PushSeparator();
        src.useCameraExternalParameters = EditorGUILayout.Toggle("Use Camera External Data?", src.useCameraExternalParameters);
        PushSpace(2);

        if (src.useCameraExternalParameters)
        {
            // // EditorGUILayout.ObjectField(CameraExternalDataProp, new GUIContent("External Data!"));
            // EditorGUILayout.PropertyField(CameraExternalDataProp, new GUIContent("External Data"));
            //!CameraExternalDataProp.objectReferenceValue.Null() && 
            if (!(src.CameraExternalData is null))
            {
                var fwdData = src.CameraExternalData;

                src.radialCoefficient = EditorGUILayout.Vector3Field("Radial Coefficient", new Vector3(fwdData.radialCoefficientX, fwdData.radialCoefficientY, fwdData.radialCoefficientZ));
                src.tangentialCoefficient = EditorGUILayout.Vector3Field("Tangential Coefficient", new Vector3(fwdData.tangentialCoefficientX, fwdData.tangentialCoefficientY, fwdData.tangentialCoefficientZ));
                src.principalCoefficient = EditorGUILayout.Vector2Field("Principal Coefficient", new Vector2(fwdData.principalPointX, fwdData.principalPointY));
                src.externalFocalLength = EditorGUILayout.Vector2Field("External Focal Length", new Vector2(fwdData.focalLengthX, fwdData.focalLengthY));
                src.skewCoefficient = EditorGUILayout.FloatField("Skew Coefficient", fwdData.skewCoefficient);
                PushSpace(2);
            }
        }

        // // update the changes on Calibration.
        // if (EditorGUI.EndChangeCheck()) {
        // }
    }

    void DrawOthers(DanbiCameraControl src)
    {
        EditorGUI.BeginChangeCheck();

        EditorGUILayout.LabelField("Camera Basic Parameters");
        src.fovDirection = (EDanbiFOVDirection)EditorGUILayout.EnumPopup("FOV Direction", src.fovDirection);
        src.fov = EditorGUILayout.Vector2Field("Field of View ", src.fov);
        src.nearFar = EditorGUILayout.Vector2Field("Near-----Far", src.nearFar);
        src.aspectRatioDivided = EditorGUILayout.FloatField("Divided Aspect Ratio", src.aspectRatioDivided);
        src.aspectRatio = EditorGUILayout.Vector2Field("Aspect Ratio (width, height)", src.aspectRatio);
        PushSpace(2);

        if (EditorGUI.EndChangeCheck())
        {
            var fwdCamRef = Camera.main;

            fwdCamRef.fieldOfView = src.fovDirection == 0 ? src.fov.x : src.fov.y;
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