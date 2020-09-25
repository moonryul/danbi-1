using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;

namespace Danbi
{
    public class DanbiCameraControl : MonoBehaviour
    {

        [SerializeField, HideInInspector]
        DanbiCameraExternalData CameraExternalData;
        [SerializeField, Readonly]
        Camera MainCamRef;
        public Camera mainCamRef
        {
            get
            {
                if (MainCamRef.Null())
                {
                    MainCamRef = Camera.main;
                }
                return MainCamRef;
            }
        }
        public DanbiCameraExternalData cameraExternalData => CameraExternalData;
        public bool usePhysicalCamera;
        public bool useCalibration;
        public bool useCameraExternalData;
        public bool useCameraExternalDataScriptableObject;
        public EDanbiCameraUndistortionMethod undistortionMethod;
        public int iterativeThreshold;
        public int iterativeSafetyCounter;
        public int newtonThreshold;
        public EDanbiFOVDirection fovDirection = 0;
        public Vector2 fov = new Vector2(39.0f, 39.0f);
        public Vector2 nearFar = new Vector2(0.01f, 250.0f);
        public float focalLength;
        public Vector2 sensorSize;
        public Vector3 radialCoefficient;
        public Vector2 tangentialCoefficient;
        public Vector2 principalCoefficient;
        public Vector2 externalFocalLength;
        public float skewCoefficient;

        void Awake()
        {
            CameraExternalData = ScriptableObject.CreateInstance<DanbiCameraExternalData>();
            // 1. Transfer CamAdditionalData to the PrewarperSetting 
            // to Rebuild the object (stride, CamAdditionalData for ComputeShader).      
            FindObjectOfType<DanbiPrewarperSetting>().camAdditionalData
                 = CameraExternalData;
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdate;
        }

        void Update()
        {
            if (MainCamRef.Null())
            {
                MainCamRef = Camera.main;
            }
            if (fovDirection == EDanbiFOVDirection.Horizontal)
            {
                MainCamRef.fieldOfView = fov.x;
            }
            else
            {
                MainCamRef.fieldOfView = fov.y;
            }
            MainCamRef.nearClipPlane = nearFar.x;
            MainCamRef.farClipPlane = nearFar.y;
            MainCamRef.usePhysicalProperties = usePhysicalCamera;
            MainCamRef.focalLength = focalLength;
            MainCamRef.sensorSize = sensorSize;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIProjectorPhysicalCameraPanelControl)
            {
                var physicalCameraPanel = control as DanbiUIProjectorPhysicalCameraPanelControl;

                usePhysicalCamera = physicalCameraPanel.isToggled;
                focalLength = physicalCameraPanel.focalLength;
                sensorSize.x = physicalCameraPanel.sensorSize.width;
                sensorSize.y = physicalCameraPanel.sensorSize.height;
                fov.x = physicalCameraPanel.fov.horizontal;
                fov.y = physicalCameraPanel.fov.vertical;
                usePhysicalCamera = physicalCameraPanel.isToggled;
                fovDirection = physicalCameraPanel.fovDirection;
            }

            if (control is DanbiUIProjectorCalibrationPanelControl)
            {
                var calibrationPanel = control as DanbiUIProjectorCalibrationPanelControl;

                useCalibration = calibrationPanel.useCalbiratedCamera;
                undistortionMethod = calibrationPanel.undistortionMethod;
                newtonThreshold = calibrationPanel.newtonThreshold;
                iterativeThreshold = calibrationPanel.iterativeThreshold;
                iterativeSafetyCounter = calibrationPanel.iterativeSafetyCounter;
            }
        }
    };
};
