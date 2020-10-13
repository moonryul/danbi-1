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
        public EDanbiCameraUndistortionMethod undistortionMethod = EDanbiCameraUndistortionMethod.Direct;
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
        Camera MainCam;
        Camera mainCam
        {
            get
            {
                MainCam.NullFinally(() => MainCam = Camera.main);
                return MainCam;
            }
        }

        public delegate void OnSetCameraBuffers((int width, int height) imageResolution, DanbiComputeShaderControl control);
        public static OnSetCameraBuffers Call_OnSetCameraBuffers;

        void Awake()
        {
            // 1. bind the delegates
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdate;
            Call_OnSetCameraBuffers += Caller_OnSetCameraBuffers;
            DanbiPrewarperSetting.Call_OnPreparePrerequisites += Caller_ListenOnPreparePrerequisites;
        }

        void OnDisable()
        {
            // 1. unbind the delegates
            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdate;
            Call_OnSetCameraBuffers -= Caller_OnSetCameraBuffers;
            DanbiPrewarperSetting.Call_OnPreparePrerequisites -= Caller_ListenOnPreparePrerequisites;
        }

        void Caller_ListenOnPreparePrerequisites(DanbiComputeShaderControl control)
        {
            // 1. Create a ComputeBuffer of Camera Internal Data
            control.buffersDic.Add("_CameraInternalData",
                DanbiComputeShaderHelper.CreateComputeBuffer_Ret(CameraInternalData.asStruct, CameraInternalData.stride));
        }

        void Caller_OnSetCameraBuffers((int width, int height) imageResolution, DanbiComputeShaderControl control)
        {
            control.NullFinally(() =>
            {
                Debug.LogError($"<color=red>ComputeShaderControl is null!</color>");
                return;
            });

            var rayTracingShader = control.rayTracingShader;

            // 1. set the Camera to World Transformation matrix as a buffer into the compute shader.
            rayTracingShader.SetMatrix("_CameraToWorldMat", mainCam.cameraToWorldMatrix);

            // 2. Projection & CameraInverseProjection are differed from the usage of the Camera Calibration.
            if (!useCalibration)
            {
                rayTracingShader.SetMatrix("_Projection", mainCam.projectionMatrix);
                rayTracingShader.SetMatrix("_CameraInverseProjection", mainCam.projectionMatrix.inverse);
            }
            else
            {
                Debug.Log($"<color=violet>You are using Camera Calibration.</color>", this);
                // float left = 0.0f;
                // float right = imageResolution.width;
                // float bottom = 0.0f;
                // float top = imageResolution.height;
                // float near = Camera.main.nearClipPlane;
                // float far = Camera.main.farClipPlane;

                // .. Construct the projection matrix from the calibration parameters
                //    and the field-of-view of the current main camera.
                //    test1
                float left = 0.0f;
                float right = imageResolution.width; // MOON: change it to Projector Width
                float top = 0.0f;
                // MOON: change it to Projector Height // y axis goes downward.
                float bottom = imageResolution.height;


                // test2
                //float left = -ProjectedCamParams.PrincipalPoint.x;
                //float right = CurrentScreenResolutions.x - ProjectedCamParams.PrincipalPoint.x;

                //float top = ProjectedCamParams.PrincipalPoint.y;
                //float bottom = ProjectedCamParams.PrincipalPoint.y - CurrentScreenResolutions.y;

                // y axis goes downward.
                float near = -mainCam.nearClipPlane;
                float far = -mainCam.farClipPlane;

                var openGLNDCMatrix = DanbiComputeShaderHelper.GetOrthoMatOpenGLGPU(left, right, bottom, top, near, far);

                var dat = CameraInternalData;
                var openGLKMatrix = DanbiComputeShaderHelper.GetOpenGL_KMatrix(dat.focalLengthX,
                                                                                 dat.focalLengthY,
                                                                                 dat.principalPointX,
                                                                                 dat.principalPointY,
                                                                                 near, far);
                //Matrix4x4 OpenGLToUnity = GetOpenGLToUnity();
                //Debug.Log($"OpenGL To Unity Matrix -> \n{OpenGLToUnity}");

                //Matrix4x4 OpenGLToOpenCV = GetOpenGLToOpenCV(CurrentScreenResolutions.y);
                //Debug.Log($"OpenGL to OpenCV Matrix -> \n{OpenGLToOpenCV}");

                Matrix4x4 projMat = openGLNDCMatrix /** OpenGLToOpenCV*/ * openGLKMatrix; // * OpenGLToUnity;

                rayTracingShader.SetMatrix("_Projection", projMat);
                rayTracingShader.SetMatrix("_CameraInverseProjection", projMat.inverse);
                // TODO: Need to decide how we choose the way how we un-distort.

                rayTracingShader.SetBuffer(DanbiKernelHelper.CurrentKernelIndex, "_CameraInternalData", control.buffersDic["_CameraInternalData"]);
                rayTracingShader.SetInt("_UndistortionMethod", (int)undistortionMethod);
                rayTracingShader.SetInt("_ThresholdIterative", iterativeThreshold);
                rayTracingShader.SetInt("_IterativeSafeCounter", iterativeSafetyCounter);
                rayTracingShader.SetInt("_ThresholdNewTonIterative", newtonThreshold);
            }
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            // 1. Update Screen props
            if (control is DanbiUIProjectorScreenPanelControl)
            {
                var screenPanel = control as DanbiUIProjectorScreenPanelControl;

                // update aspect ratio
                aspectRatio = new Vector2(screenPanel.aspectRatioWidth, screenPanel.aspectRatioHeight);
                mainCam.aspect = aspectRatioDivided = aspectRatio.x / aspectRatio.y;
                // Screen Resolution is updated in DanbiScreen.                
            }
            
            // 2. Update physical camera props
            if (control is DanbiUIProjectorPhysicalCameraPanelControl)
            {
                var physicalCameraPanel = control as DanbiUIProjectorPhysicalCameraPanelControl;

                mainCam.usePhysicalProperties = usePhysicalCamera = physicalCameraPanel.isToggled;

                if (usePhysicalCamera)
                {
                    mainCam.focalLength = focalLength = physicalCameraPanel.focalLength;
                    sensorSize.x = physicalCameraPanel.sensorSize.width;
                    sensorSize.y = physicalCameraPanel.sensorSize.height;
                    mainCam.sensorSize = new Vector2(sensorSize.x, sensorSize.y);
                    // Update the fov display
                    physicalCameraPanel.Call_OnFOVCalcuated?.Invoke(mainCam.fieldOfView);
                }
            }

            // 3. Update calibration props
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

            // 4. Update internal parameter props
            if (control is DanbiUIProjectorInternalParametersPanelControl)
            {
                var internalParamsPanel = control as DanbiUIProjectorInternalParametersPanelControl;

                useCameraExternalParameters = internalParamsPanel.useInternalParameters;

                if (useCameraExternalParameters)
                {
                    if (!string.IsNullOrEmpty(internalParamsPanel.loadPath))
                    {
                        CameraInternalData = internalParamsPanel.internalData;
                    }

                    radialCoefficient.x = internalParamsPanel.internalData.radialCoefficientX;
                    radialCoefficient.y = internalParamsPanel.internalData.radialCoefficientY;
                    radialCoefficient.z = internalParamsPanel.internalData.radialCoefficientZ;

                    tangentialCoefficient.x = internalParamsPanel.internalData.tangentialCoefficientX;
                    tangentialCoefficient.y = internalParamsPanel.internalData.tangentialCoefficientY;

                    principalCoefficient.x = internalParamsPanel.internalData.principalPointX;
                    principalCoefficient.y = internalParamsPanel.internalData.principalPointY;

                    externalFocalLength.x = internalParamsPanel.internalData.focalLengthX;
                    externalFocalLength.y = internalParamsPanel.internalData.focalLengthY;

                    skewCoefficient = internalParamsPanel.internalData.skewCoefficient;
                }
            }
        }
    };
};
