using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;

namespace Danbi
{
    public class DanbiCameraControl : MonoBehaviour
    {
        [HideInInspector]
        public DanbiCameraInternalData CameraInternalData = new DanbiCameraInternalData();
        public bool usePhysicalCamera = false;
        public bool useCalibration = false;
        public bool useCameraExternalParameters = false;
        public EDanbiCameraUndistortionMethod undistortionMethod;
        public int iterativeThreshold;
        public int iterativeSafetyCounter;
        public int newtonThreshold;
        public EDanbiFOVDirection fovDirection = EDanbiFOVDirection.Vertical;
        public Vector2 fov = new Vector2(39.0f, 39.0f);
        public Vector2 nearFar = new Vector2(0.01f, 250.0f);
        public Vector2 aspectRatio = new Vector2(16, 9);
        public float aspectRatioDivided;
        public float focalLength;
        public Vector2 sensorSize;
        public Vector3 radialCoefficient;
        public Vector3 tangentialCoefficient;
        public Vector2 principalCoefficient;
        public Vector2 externalFocalLength;
        public float skewCoefficient;

        void Awake()
        {
            // 1. Transfer CamAdditionalData to the PrewarperSetting 
            // to Rebuild the object (stride, CamAdditionalData for ComputeShader).      
            FindObjectOfType<DanbiPrewarperSetting>().camAdditionalData
                 = CameraInternalData;
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdate;
        }

        void Update()
        {
            // if (fovDirection == EDanbiFOVDirection.Horizontal)
            // {
            //     Camera.main.fieldOfView = fov.x;
            // }
            // else
            // {
            //     Camera.main.fieldOfView = fov.y;
            // }

            // Camera.main.nearClipPlane = Mathf.Max(0.01f, nearFar.x);
            // Camera.main.farClipPlane = Mathf.Min(1000.0f, nearFar.y);
            // Camera.main.usePhysicalProperties = usePhysicalCamera;
            // Camera.main.focalLength = Mathf.Max(30.0f, focalLength);
            // Camera.main.sensorSize = sensorSize;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIProjectorScreenPanelControl)
            {
                var screenPanel = control as DanbiUIProjectorScreenPanelControl;

                // update aspect ratio
                aspectRatio = new Vector2(screenPanel.aspectRatioWidth, screenPanel.aspectRatioHeight);
                Camera.main.aspect = aspectRatioDivided = aspectRatio.x / aspectRatio.y;
                // Screen Resolution is updated in DanbiScreen.                
            }

            if (control is DanbiUIProjectorPhysicalCameraPanelControl)
            {
                var physicalCameraPanel = control as DanbiUIProjectorPhysicalCameraPanelControl;

                Camera.main.usePhysicalProperties = usePhysicalCamera = physicalCameraPanel.isToggled;

                if (usePhysicalCamera)
                {
                    var mainCam = Camera.main;
                    mainCam.focalLength = focalLength = physicalCameraPanel.focalLength;
                    sensorSize.x = physicalCameraPanel.sensorSize.width;
                    sensorSize.y = physicalCameraPanel.sensorSize.height;
                    mainCam.sensorSize = new Vector2(sensorSize.x, sensorSize.y);

                    // fovDirection = physicalCameraPanel.fovDirection;
                    // fov.x = physicalCameraPanel.fov.horizontal;
                    // fov.y = physicalCameraPanel.fov.vertical;

                    // Camera.main.fieldOfView = fovDirection == EDanbiFOVDirection.Horizontal ? fov.x : fov.y;
                    // Camera.main.fieldOfView = Camera.HorizontalToVerticalFieldOfView(fov.y, aspectRatioDivided);
                        // fovDirection == EDanbiFOVDirection.Horizontal 
                        // ? fov.x : Camera.VerticalToHorizontalFieldOfView(fov.y, aspectRatioDivided);
                }
            }

            if (control is DanbiUIProjectorCalibrationPanelControl)
            {
                var calibrationPanel = control as DanbiUIProjectorCalibrationPanelControl;

                useCalibration = calibrationPanel.useCalbiratedCamera;

                if (useCalibration)
                {
                    undistortionMethod = calibrationPanel.undistortionMethod;
                    newtonThreshold = calibrationPanel.newtonThreshold;
                    iterativeThreshold = calibrationPanel.iterativeThreshold;
                    iterativeSafetyCounter = calibrationPanel.iterativeSafetyCounter;
                }
            }

            if (control is DanbiUIProjectorInternalParametersPanelControl)
            {
                var externalParameterPanel = control as DanbiUIProjectorInternalParametersPanelControl;

                useCameraExternalParameters = externalParameterPanel.useInternalParameters;

                if (useCameraExternalParameters)
                {
                    if (!string.IsNullOrEmpty(externalParameterPanel.loadPath))
                    {
                        CameraInternalData = externalParameterPanel.internalData;
                    }

                    radialCoefficient.x = externalParameterPanel.internalData.radialCoefficientX;
                    radialCoefficient.y = externalParameterPanel.internalData.radialCoefficientY;
                    radialCoefficient.z = externalParameterPanel.internalData.radialCoefficientZ;

                    tangentialCoefficient.x = externalParameterPanel.internalData.tangentialCoefficientX;
                    tangentialCoefficient.y = externalParameterPanel.internalData.tangentialCoefficientY;

                    principalCoefficient.x = externalParameterPanel.internalData.principalPointX;
                    principalCoefficient.y = externalParameterPanel.internalData.principalPointY;

                    externalFocalLength.x = externalParameterPanel.internalData.focalLengthX;
                    externalFocalLength.y = externalParameterPanel.internalData.focalLengthY;

                    skewCoefficient = externalParameterPanel.internalData.skewCoefficient;
                }
            }
        }
    };
};
