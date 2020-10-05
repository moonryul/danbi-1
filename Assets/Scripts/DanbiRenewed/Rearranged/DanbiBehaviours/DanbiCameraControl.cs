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
        public DanbiCameraExternalData CameraExternalData = new DanbiCameraExternalData();
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
                 = CameraExternalData;
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

                if (!usePhysicalCamera)
                {
                    aspectRatio = new Vector2(screenPanel.aspectRatioWidth, screenPanel.aspectRatioHeight);
                    Camera.main.aspect = aspectRatioDivided = aspectRatio.x / aspectRatio.y;

                    fovDirection = screenPanel.fovDirection;
                    fov.x = fov.y = screenPanel.fov;                    
                    switch (fovDirection)
                    {
                        case EDanbiFOVDirection.Horizontal:
                            Camera.main.fieldOfView = fov.x;
                            // Camera.HorizontalToVerticalFieldOfView(fov.x, aspectRatioDivided);
                            break;

                        case EDanbiFOVDirection.Vertical:
                        Camera.main.fieldOfView = fov.y;
                            break;
                    }
                }
            }

            if (control is DanbiUIProjectorPhysicalCameraPanelControl)
            {
                var physicalCameraPanel = control as DanbiUIProjectorPhysicalCameraPanelControl;

                Camera.main.usePhysicalProperties = usePhysicalCamera = physicalCameraPanel.isToggled;
                if (usePhysicalCamera)
                {
                    Camera.main.focalLength = focalLength = physicalCameraPanel.focalLength;
                    sensorSize.x = physicalCameraPanel.sensorSize.width;
                    sensorSize.y = physicalCameraPanel.sensorSize.height;
                    Camera.main.sensorSize = new Vector2(sensorSize.x, sensorSize.y);

                    fovDirection = physicalCameraPanel.fovDirection;
                    fov.x = physicalCameraPanel.fov.horizontal;
                    fov.y = physicalCameraPanel.fov.vertical;

                    Camera.main.fieldOfView = fovDirection == EDanbiFOVDirection.Horizontal ? fov.x : fov.y;
                    //     fovDirection == EDanbiFOVDirection.Horizontal 
                    //     ? fov.x : Camera.VerticalToHorizontalFieldOfView(fov.y, aspectRatioDivided);                    
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

            if (control is DanbiUIProjectorExternalParametersPanelControl)
            {
                var externalParameterPanel = control as DanbiUIProjectorExternalParametersPanelControl;

                useCameraExternalParameters = externalParameterPanel.useExternalParameters;

                if (useCameraExternalParameters)
                {
                    if (!string.IsNullOrEmpty(externalParameterPanel.loadPath))
                    {
                        CameraExternalData = externalParameterPanel.externalData;
                    }

                    radialCoefficient.x = externalParameterPanel.externalData.radialCoefficientX;
                    radialCoefficient.y = externalParameterPanel.externalData.radialCoefficientY;
                    radialCoefficient.z = externalParameterPanel.externalData.radialCoefficientZ;

                    tangentialCoefficient.x = externalParameterPanel.externalData.tangentialCoefficientX;
                    tangentialCoefficient.y = externalParameterPanel.externalData.tangentialCoefficientY;

                    principalCoefficient.x = externalParameterPanel.externalData.principalPointX;
                    principalCoefficient.y = externalParameterPanel.externalData.principalPointY;

                    externalFocalLength.x = externalParameterPanel.externalData.focalLengthX;
                    externalFocalLength.y = externalParameterPanel.externalData.focalLengthY;

                    skewCoefficient = externalParameterPanel.externalData.skewCoefficient;
                }
            }
        }
    };
};
