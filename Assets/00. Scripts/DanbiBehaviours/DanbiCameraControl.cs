using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
        public float iterativeThreshold;
        public float iterativeSafetyCounter;
        public float newtonThreshold;
        public EDanbiFOVDirection fovDirection = EDanbiFOVDirection.Vertical;
        public float fov;
        public Vector2 nearFar = new Vector2(0.01f, 250.0f);
        public Vector2 aspectRatio = new Vector2(16, 9);
        public float aspectRatioDivided;
        public float focalLength;
        public Vector2 sensorSize;
        public Vector3 radialCoefficient;
        public Vector2 tangentialCoefficient;
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
            var buf = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(CameraInternalData.asStruct, CameraInternalData.stride);
            control.buffersDic.Add("_CameraInternalData", buf);
            // if (control.buffersDic.ContainsKey("_CameraInternalData"))
            // {

            // }
            // 1. Create a ComputeBuffer of Camera Internal Data

        }

        void Caller_OnSetCameraBuffers((int width, int height) imageResolution, DanbiComputeShaderControl control)
        {
            control.NullFinally(() => Debug.LogError($"<color=red>ComputeShaderControl is null!</color>"));

            var rayTracingShader = control.rayTracingShader;

            // 1. set the Camera to World Transformation matrix as a buffer into the compute shader.            
            rayTracingShader.SetMatrix("_CameraToWorldMat", mainCam.cameraToWorldMatrix);
            rayTracingShader.SetVector("_CameraViewDirectionInUnitySpace", mainCam.transform.forward);

            // 2. Projection & CameraInverseProjection are differed from the usage of the Camera Calibration.
            if (!useCalibration)
            {
                //https://answers.unity.com/questions/1192139/projection-matrix-in-unity.html 
                // Unity uses the OpenGL convention for the projection matrix. 
                //The required z-flipping is done by the cameras worldToCameraMatrix (V).  
                //So the projection matrix (P) should look the same as in OpenGL. x_clip = P * V * M * v_obj
                rayTracingShader.SetMatrix("_Projection", mainCam.projectionMatrix);
                rayTracingShader.SetMatrix("_CameraInverseProjection", mainCam.projectionMatrix.inverse);
            }
            else
            {
                Debug.Log($"<color=violet>You are using Camera Calibration.</color>", this);

                // .. Construct the projection matrix from the calibration parameters
                //    and the field-of-view of the current main camera.

                float width = imageResolution.width;
                float height = imageResolution.height;
                float near = mainCam.nearClipPlane; // positive
                float far = mainCam.farClipPlane; // positive

                // https://answers.unity.com/questions/1192139/projection-matrix-in-unity.html
                // http://ksimek.github.io/2013/06/03/calibrated_cameras_in_opengl/

                #region comments
                //You've calibrated your camera. You've decomposed it into intrinsic and extrinsic camera matrices.
                //Now you need to use it to render a synthetic scene in OpenGL. 
                //You know the extrinsic matrix corresponds to the modelview matrix
                //and the intrinsic is the projection matrix, but beyond that you're stumped.

                //In reality, glFrustum does two things: first it performs perspective projection, 
                //    and then it converts to normalized device coordinates(NDC). 
                //    The former is a common operation in projective geometry, 
                //    while the latter is OpenGL arcana, an implementation detail.
                // THe main Point: Proj=NDC×Persp

                // the actual projection matrix representation inside the GPU might be different
                //from the representation you use in Unity. 
                //However you don't have to worry about that since Unity handles this automatically. 
                //The only case where it does matter when you directly pass a matrix from your code to a shader.
                //In that case Unity offers the method GL.GetGPUProjectionMatrix which converts 
                //the given projection matrix into the right format used by the GPU.

                //So to sum up how the MVP matrix is composed:

                //M = transform.localToWorld of the object
                //V = camera.worldToCameraMatrix
                //P = GL.GetGPUProjectionMatrix(camera.projectionMatrix)
                //  MVP = P V M
                // NDC(normalized device coordinates) are the coordinates after the perspective divide
                //    which is performed by the GPU. The Projection matrix actually outputs homogenous clipspace coordinates
                //    which are similar to NDC but before the normalization.

                // Specifically, you should pass the pixel coordinates of the left, right, bottom, and top of the window 
                //you used when performing calibration. For example, lets assume you calibrated using a 640×480 image.
                //If you used a pixel coordinate system whose origin is at the top - left, with the y - axis
                //    increasing in the downward direction, you would call glOrtho(0, 640, 480, 0, near, far). 
                //    If you calibrated with an origin at zero and normal leftward / upward x,y axis,
                //    you would call glOrtho(-320, 320, -240, 240, near, far).

                #endregion comments
                // http://www.songho.ca/opengl/gl_projectionmatrix.html         

                var dat = CameraInternalData;
                var openGLNDCMatrix = DanbiComputeShaderHelper.GetOrthoMatOpenGL(0, width, 0, height, near, far);
                var openGLPerspMatrix = DanbiComputeShaderHelper.OpenCV_KMatrixToOpenGLPerspMatrix(dat.focalLengthX,
                                                                                                   dat.focalLengthY,
                                                                                                   dat.principalPointX,
                                                                                                   dat.principalPointY,
                                                                                                   near,
                                                                                                   far,
                                                                                                   width,
                                                                                                   height);

                // var openGLKMatrix;                                                                               
                // var OpenCVToUnity = DanbiComputeShaderHelper.GetOpenCVToUnity();
                //Debug.Log($"OpenGL To Unity Matrix -> \n{OpenGLToUnity}");

                //Matrix4x4 OpenGLToOpenCV = GetOpenGLToOpenCV(CurrentScreenResolutions.y);
                //Debug.Log($"OpenGL to OpenCV Matrix -> \n{OpenGLToOpenCV}");

                Matrix4x4 projMat = openGLNDCMatrix * openGLPerspMatrix; //* OpenCVToUnity; // * OpenGLToUnity;

                rayTracingShader.SetMatrix("_Projection", projMat);
                rayTracingShader.SetMatrix("_CameraInverseProjection", projMat.inverse);

                // rayTracingShader.SetInt("_UseUndistortion", useCalibration ? 1 : 0);
                rayTracingShader.SetBool("_UseUndistortion", useCalibration);
                Debug.Log($"Using Undistortion? {useCalibration}");
                rayTracingShader.SetInt("_UndistortionMethod", (int)undistortionMethod);
                Debug.Log($"Using Undistortion Method -> {(int)undistortionMethod}, ({undistortionMethod})");
                rayTracingShader.SetBuffer(DanbiKernelHelper.CurrentKernelIndex, "_CameraInternalData", control.buffersDic["_CameraInternalData"]);

                // rayTracingShader.SetFloat("_IterativeThreshold", iterativeThreshold);
                // rayTracingShader.SetFloat("_IterativeSafeCounter", iterativeSafetyCounter);
                // rayTracingShader.SetFloat("_NewTonThreshold", newtonThreshold);
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
                    float fovFwd = mainCam.fieldOfView;
                    physicalCameraPanel.Call_OnFOVCalcuated?.Invoke(fovFwd);
                    fov = fovFwd;
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

                if (!useCameraExternalParameters)
                    return;

                // Load Camera Internal Paramters                

                CameraInternalData = internalParamsPanel.internalData;

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
    };
};
