using UnityEngine;
using Danbi;

#if UNITY_EDITOR
using UnityEditor;
[CustomEditor(typeof(DanbiCameraControl))]
public class DanbiCameraControlEditor : Editor
{
    SerializedProperty ThresholdIterativeProp, SafeCounterProp, ThresholdNewtonProp, ScriptDisplayProp, CameraExternalDataProp;

    void OnEnable()
    {
        ThresholdIterativeProp = serializedObject.FindProperty("ThresholdIterative");
        SafeCounterProp = serializedObject.FindProperty("SafeCounter");
        ThresholdNewtonProp = serializedObject.FindProperty("ThresholdNewton");
        ScriptDisplayProp = serializedObject.FindProperty("m_Script");
        CameraExternalDataProp = serializedObject.FindProperty("CameraExternalData");
    }

    public override void OnInspectorGUI()
    {
        var src = target as DanbiCameraControl;

        // 1. Display the script prop.
        GUI.enabled = false;
        EditorGUILayout.PropertyField(ScriptDisplayProp, true);
        GUI.enabled = true;
        // DrawDefaultInspector();

        PushSpace(1);

        DrawOthers(src);

        DrawPhysicalCamera(src);

        DrawCameraCalibratedProjectionOnly(src);

        DrawCameraCalbiration(src);

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
            var fwdCamRef = src.mainCamRef;
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
        src.useCameraExternalData = EditorGUILayout.Toggle("Use Camera External Data?", src.useCameraExternalData);
        PushSpace(2);

        if (src.useCameraExternalData)
        {
            EditorGUILayout.PropertyField(CameraExternalDataProp, true);

            if (!CameraExternalDataProp.objectReferenceValue.Null())
            {
                var fwdData = src.cameraExternalData;
                src.radialCoefficient = EditorGUILayout.Vector3Field("Radial Coefficient", fwdData.RadialCoefficient);
                src.tangentialCoefficient = EditorGUILayout.Vector2Field("Tangential Coefficient", fwdData.TangentialCoefficient);
                src.principalCoefficient = EditorGUILayout.Vector2Field("Principal Coefficient", fwdData.PrincipalPoint);
                src.externalFocalLength = EditorGUILayout.Vector2Field("External Focal Length", fwdData.FocalLength);
                src.skewCoefficient = EditorGUILayout.FloatField("Skew Coefficient", fwdData.SkewCoefficient);
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
        PushSpace(2);

        if (EditorGUI.EndChangeCheck())
        {
            var fwdCamRef = src.mainCamRef;
            fwdCamRef.fieldOfView = src.fovDirection == 0 ? src.fov.x : src.fov.y;
            fwdCamRef.nearClipPlane = src.nearFar.x;
            fwdCamRef.farClipPlane = src.nearFar.y;
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