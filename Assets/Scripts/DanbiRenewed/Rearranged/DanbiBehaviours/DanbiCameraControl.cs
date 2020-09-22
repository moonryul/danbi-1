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

        public bool usePhysicalCamera { get; set; }

        public bool useCalibration { get; set; }

        public bool useCameraExternalData { get; set; }

        public bool useCameraExternalDataScriptableObject { get; set; }

        public EDanbiCalibrationMode undistortMode { get; set; }

        public float thresholdIterative { get; set; }

        public int safetyCounter { get; set; }

        public float thresholdNewton { get; set; }

        public float fov { get; set; } = 39.0f;

        public Vector2 nearFar { get; set; } = new Vector2(0.01f, 250.0f);

        public float focalLength { get; set; }

        public Vector2 sensorSize { get; set; }

        public Vector3 radialCoefficient { get; set; }

        public Vector2 tangentialCoefficient { get; set; }

        public Vector2 principalCoefficient { get; set; }

        public Vector2 externalFocalLength { get; set; }

        public float skewCoefficient { get; set; }

        void Awake()
        {
            CameraExternalData = ScriptableObject.CreateInstance<DanbiCameraExternalData>();
            // 1. Transfer CamAdditionalData to the PrewarperSetting 
            // to Rebuild the object (stride, CamAdditionalData for ComputeShader).      
            FindObjectOfType<DanbiPrewarperSetting>().camAdditionalData
                 = CameraExternalData;

        }

        void Update()
        {
            if (MainCamRef.Null())
            {
                MainCamRef = Camera.main;
            }
            MainCamRef.fieldOfView = fov;
            MainCamRef.nearClipPlane = nearFar.x;
            MainCamRef.farClipPlane = nearFar.y;
            MainCamRef.usePhysicalProperties = usePhysicalCamera;
            MainCamRef.focalLength = focalLength;
            MainCamRef.sensorSize = sensorSize;
        }
    };
};
