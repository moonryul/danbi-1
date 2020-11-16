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
        public DanbiCameraInternalData cameraInternalData;

        [HideInInspector]
        public DanbiCameraExternalData cameraExternalData;

        public bool useCalibratedProjector = false;

        #region Calibrated Projector Mode

        public EDanbiCameraUndistortionMethod calibratedProjectorMode = EDanbiCameraUndistortionMethod.Direct;
        public float iterativeThreshold;
        public float iterativeSafetyCounter;
        public float newtonThreshold;

        #endregion Calibrated Projector Mode

        #region Camera Info
        public float fov;
        public Vector2 nearFar = new Vector2(0.01f, 250.0f);
        public Vector2 aspectRatio = new Vector2(16, 9);
        public float aspectRatioDivided;

        #endregion Camera Info

        #region Internal Projector Parameters

        public Vector3 radialCoefficient;
        public Vector2 tangentialCoefficient;
        public Vector2 principalCoefficient;
        public Vector2 externalFocalLength;
        public float skewCoefficient;

        #endregion Internal Projector Parameters

        #region External Projector Parameters

        public Vector3 projectorPosition;
        public Vector3 xAxis;
        public Vector3 yAxis;
        public Vector3 zAxis;

        #endregion External Projector Parameters

        Camera MainCam;
        Camera mainCam
        {
            get
            {
                MainCam.NullFinally(() => MainCam = Camera.main);
                return MainCam;
            }
        }

        public delegate void OnSetCameraInternalParameters((int width, int height) imageResolution, DanbiComputeShaderControl control);
        public static OnSetCameraInternalParameters onSetCameraInternalParameters;

        public delegate void OnSetCameraExternalParameters((int width, int height) imageResolution, DanbiComputeShaderControl control);
        public static OnSetCameraExternalParameters onSetCameraExternalParameters;

        void Awake()
        {
            // 1. bind the delegates
            DanbiUISync.onPanelUpdate += OnPanelUpdate;
            onSetCameraInternalParameters += setCameraParameters;
            onSetCameraExternalParameters += SetCameraExternalBuffers;
            DanbiPrewarperSetting.onPrepareShaderData += prepareCameraData;
        }

        void OnDisable()
        {
            // 1. unbind the delegates
            onSetCameraInternalParameters -= setCameraParameters;
            onSetCameraExternalParameters -= SetCameraExternalBuffers;
            DanbiPrewarperSetting.onPrepareShaderData -= prepareCameraData;
        }

        void prepareCameraData(DanbiComputeShaderControl control)
        {
            control.buffersDict.Add("_CameraInternalData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret(cameraInternalData.asStruct, cameraInternalData.stride));
            control.buffersDict.Add("_CameraExternalData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret(cameraExternalData.asStruct, cameraExternalData.stride));
        }

        void setCameraParameters((int width, int height) imageResolution, DanbiComputeShaderControl control)
        {
            control.NullFinally(() => Debug.LogError($"<color=red>ComputeShaderControl is null!</color>"));
            var rayTracingShader = control.danbiShader;

            // 1. set the Camera to World Transformation matrix as a buffer into the compute shader.            
            rayTracingShader.SetMatrix("_CameraToWorldMat", mainCam.cameraToWorldMatrix);
            rayTracingShader.SetVector("_CameraViewDirectionInUnitySpace", mainCam.transform.forward);
            rayTracingShader.SetBool("_UseCalibratedCamera", useCalibratedProjector);

            // rayTracingShader.SetMatrix("_CameraToWorld", mainCam.cameraToWorldMatrix);
            // Vector4 cameraDirection = new Vector4(mainCam.transform.forward.x, mainCam.transform.forward.y,
            //                                 mainCam.transform.forward.z, 0f);
            // rayTracingShader.SetVector(" _CameraViewDirectionInUnitySpace", mainCam.transform.forward);
            // rayTracingShader.SetVector("_CameraForwardDirection", cameraDirection);
            // Vector4.

            //Debug
            // Debug.Log("_CameraForwardDirection DebugLog=" + mainCam.transform.forward.x + "," +
            //                  mainCam.transform.forward.y + "," + mainCam.transform.forward.z);
            rayTracingShader.SetMatrix("_Projection", mainCam.projectionMatrix);
            rayTracingShader.SetMatrix("_CameraInverseProjection", mainCam.projectionMatrix.inverse);
            // 2. Projection & CameraInverseProjection are differed from the usage of the Camera Calibration.
            if (!useCalibratedProjector)
            {
                // rayTracingShader.SetMatrix("_Projection", mainCam.projectionMatrix);
                // rayTracingShader.SetMatrix("_CameraInverseProjection", mainCam.projectionMatrix.inverse);
                //https://answers.unity.com/questions/1192139/projection-matrix-in-unity.html 
                // Unity uses the OpenGL convention for the projection matrix. 
                //The required z-flipping is done by the cameras worldToCameraMatrix (V).  
                //So the projection matrix (P) should look the same as in OpenGL. x_clip = P * V * M * v_obj
                // rayTracingShader.SetMatrix("_Projection", mainCam.projectionMatrix);
                // rayTracingShader.SetMatrix("_CameraInverseProjection", mainCam.projectionMatrix.inverse);
            }
            else
            {
                // .. Construct the projection matrix from the calibration parameters
                //    and the field-of-view of the current main camera.

                // float width = imageResolution.width;
                // float height = imageResolution.height;
                // float near = mainCam.nearClipPlane; // positive
                // float far = mainCam.farClipPlane; // positive

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

                #region comments
                // .. Construct the projection matrix from the calibration parameters
                //    and the field-of-view of the current main camera.        
                // Here we reconstruct the GL projection matrix from the assumed calibrated parameters
                // of the OPENGL camera.

                //This discussion of camera-scaling shows that there are an infinite number of pinhole cameras 
                //that produce the same image. The intrinsic matrix is only concerned with the relationship 
                //    between camera coordinates and image coordinates, so the absolute camera dimensions are irrelevant. 
                //    Using pixel units for focal length and principal point offset allows us to represent
                //    the relative dimensions of the camera,
                //namely, the film's position relative to its size in pixels.
                //Another way to say this is that the intrinsic camera transformation is invariant to uniform scaling
                //of the camera geometry. By representing dimensions in pixel units, 
                //we naturally capture this invariance.


                //https://answers.unity.com/questions/1192139/projection-matrix-in-unity.html
                // http://ksimek.github.io/2013/06/03/calibrated_cameras_in_opengl/
                //http://ksimek.github.io/2012/08/14/decompose/

                //http://ksimek.github.io/2013/08/13/intrinsic/



                //You've calibrated your camera. You've decomposed it into intrinsic and extrinsic camera matrices.
                //Now you need to use it to render a synthetic scene in OpenGL. 
                //You know the extrinsic matrix corresponds to the modelview matrix
                //and the intrinsic is the projection matrix, but beyond that you're stumped.

                //In reality, glFrustum does two things: first it performs perspective projection, 
                //    and then it converts to normalized device coordinates(NDC). 
                //    The former is a common operation in projective geometry, 
                //    while the latter is OpenGL arcana, an implementation detail.

                // THe main Point: Proj = NDC × Persp

                // the actual projection matrix representation inside the GPU might be different
                //from the representation you use in Unity. 
                //However you don't have to worry about that since Unity handles this automatically. 
                //The only case where it does matter when you directly pass a matrix from your code to a shader.
                //In that case Unity offers the method GL.GetGPUProjectionMatrix which converts 
                //the given projection matrix into the right format used by the GPU.

                //So to sum up how the MVP matrix is composed:
                //https://m.blog.naver.com/PostView.nhn?blogId=techshare&logNo=221362240987&proxyReferer=https:%2F%2Fwww.google.com%2F

                //M = transform.localToWorld of the object
                //V = camera.worldToCameraMatrix
                //P = GL.GetGPUProjectionMatrix(camera.projectionMatrix)  // camera.projectionMatrix follows OpenGL
                //  MVP = P V M                                           // GL.GetGPUProjectionMatrix() follows DX11
                // NDC(normalized device coordinates) are the coordinates after the perspective divide
                //    which is performed by the GPU. The Projection matrix actually outputs homogenous clipspace coordinates
                //    which are similar to NDC but before the normalization.

                // Specifically, you should pass the pixel coordinates of the left, right, bottom, and top of the window 
                // you used when performing calibration. For example, lets assume you calibrated using a 640×480 image.
                // If you used a pixel coordinate system whose origin is at the top - left, with the y - axis
                //    increasing in the downward direction, you would call glOrtho(0, 640, 480, 0, near, far). 
                //    If you calibrated with an origin at zero and normal leftward / upward x,y axis,
                //    you would call glOrtho(-320, 320, -240, 240, near, far).

                //http://www.songho.ca/opengl/gl_projectionmatrix.html

                //If you used a pixel coordinate system whose origin is at the top-left (OpenCV), 
                // with the y-axis increasing in the  downward direction, call:
                // Matrix4x4 openGLNDCMatrix = GetOrthoMatOpenGL(0, width, 0, height, near, far); 


                //Camera Calibration (Very Good with a good picture): 
                //https://docs.opencv.org/2.4/modules/calib3d/doc/camera_calibration_and_3d_reconstruction.html

                //(cx, cy) is a principal point that is usually at the image center; It is measured with resepct to the
                // top left corner of the iamge space:  x' = x_e/Z-e, y'=y_e/z_e; u= fx * x' + c_x; v= fy * y' + c_y;
                // x' = 0 when x_e =0; y'=0 when y_e =0; 

                // The left-top corner of the 'image" is away from the principal point
                // (which the z-axis of the camera intersects) by   (c_x, c_y).


                //    Radial Coefficient  -0.00987701 0.22019886 - 0.56139517
                //    Tangential Coefficient  -0.00093723 - 0.00275611
                //    Principal Point 1922.94259  1089.44916
                //    Focal Length    3242.25507  3240.55697


                //     R(Rotation Matrix) = [[-9.99984648e-01, -5.54101900e-03, 3.07308875e-06],
                //     [5.54067185e-03, -9.99927907e-01, -1.06527964e-02],
                //     [6.21002143e-05, -1.06526158e-02, 9.99943257e-01]]
                //     T(Translation Vector) = [611.53383621, 407.5504153, 2118.60708594]


                // 1) 코드
                //fovx, fovy, focalLength, principalPoint, aspectRatio = cv2.calibrationMatrixValues(proj_int, proj_shape, 16.4, 10.2)

                //proj_int = 프로젝터 내부 파라미터 명시된 매트릭스(캘리브레이션으로 구한 매트릭스)
                //proj_shape = 프로젝터 해상도 = (proj_height, proj_width) = (2160, 3840)
                //16.4[mm] = 프로젝터 sensor width
                //10.2[mm] = 프로젝터 Sensor height

                //2) 결과

                //(3840, 2160) 으로 변경하여 코드 실행시켰더니

                //fovx: 61.266343684968184
                //fovy: 36.863725892803764
                //focalLength: 13.847131021228204
                //principalPoint: (8.21256731975761, 5.144621018523849)
                //aspectRatio: 0.9994762599607733
                //이렇게 나옵니다.

                //유림 학생 Yurim, [08.11.20 12:23]
                //(cx, cy) = (1922.94259, 1089.44916)[pixel] 단위를[mm] 단위로 변경하면(8.268, 5.120) 이 나오는데 얼추 비슷한 것 같습니다.

                //cx * (16.4 / 3840) = 8.268
                //cy * (10.2 / 2160) = 5.120


                #endregion comments

                float width = (float)DanbiManager.instance.screen.screenResolution.x; // width = 3840 =  Projector Width
                float height = (float)DanbiManager.instance.screen.screenResolution.y; // height = 2160 = Projector Height

                float left = 0;
                float right = width;
                float bottom = 0;
                float top = height;


                float near = mainCam.nearClipPlane;      // near: positive
                float far = mainCam.farClipPlane;        // far: positive

                float aspectRatio = width / height;
                // float scaleFactorX = 1 / (aspectRatio * Mathf.Tan(fieldOfView));
                // float scaleFactorY = 1 / Mathf.Tan(fieldOfView);

                float scaleFactorX = cameraInternalData.focalLengthX;
                float scaleFactorY = cameraInternalData.focalLengthY;


                // MainCamera.fieldOfView = 2 * Mathf.Atan(height / (2 * scaleFactorY)) * 180/  Mathf.PI;

                float cx = cameraInternalData.principalPointX;
                float cy = cameraInternalData.principalPointY;

                // float cx = width / 2;
                // float cy = height /2;

                // Method 1: the most rigourous
                //Matrix4x4 NDCMatrixOpenGL1 = GetOrthoMat(left, right, top, bottom, near, far);

                Matrix4x4 NDCMatrix_OpenGL = DanbiComputeShaderHelper.GetOrthoMat(left, right, bottom, top, near, far);
                //Matrix4x4 openGLPerspMatrix1 = OpenCV_KMatrixToOpenGLPerspMatrix(CameraInternalParameters.FocalLength.x, CameraInternalParameters.FocalLength.y,
                //                                              CameraInternalParameters.PrincipalPoint.x, CameraInternalParameters.PrincipalPoint.y,
                //                                              near, far);

                // refer to to   //http://ksimek.github.io/2012/08/14/decompose/ 
                // understand the following code
                Matrix4x4 KMatrixFromOpenCVToOpenGL = DanbiComputeShaderHelper.OpenCVKMatrixToOpenGLKMatrix(scaleFactorX, scaleFactorY, cx, cy, near, far);


                //we can think of the perspective transformation as converting 
                // a trapezoidal-prism - shaped viewing volume
                //    into a rectangular - prism - shaped viewing volume,
                //    which glOrtho() scales and translates into the 2x2x2 cube in Normalized Device Coordinates.

                // Invert the direction of y axis and translate by height along the inverted direction.

                //                Until now, our discussion of 2D coordinate conventions have referred to the coordinates used during calibration.
                //                    If your application uses a different 2D coordinate convention,
                //                    you'll need to transform K using 2D translation and reflection.

                //               For example, consider a camera matrix that was calibrated with the origin in the top-left 
                //                    and the y - axis pointing downward, but you prefer a bottom-left origin with the y-axis pointing upward.
                //                    To convert, you'll first negate the image y-coordinate and then translate upward by the image height, h. 
                //                    The resulting intrinsic matrix K' is given by:

                // K' = [ 1 0 0; 0 1 h; 0 0 1] *  [ 1 0 0; 0 -1 0; 0 0 1] * K

                // http://ksimek.github.io/2013/06/03/calibrated_cameras_in_opengl/

                Vector4 column0 = new Vector4(1f, 0f, 0f, 0f);
                Vector4 column1 = new Vector4(0f, -1f, 0f, 0f);
                Vector4 column2 = new Vector4(0f, 0f, 1f, 0f);
                Vector4 column3 = new Vector4(0f, height, 0f, 1f);

                Matrix4x4 OpenCVCameraToOpenGLCamera = new Matrix4x4(column0, column1, column2, column3);

                Matrix4x4 projectionMatrixGL1 = NDCMatrix_OpenGL * OpenCVCameraToOpenGLCamera * KMatrixFromOpenCVToOpenGL;


                // Matrix4x4 projectionMatrixGL1 = NDCMatrixOpenGL * openGLPerspMatrix1;
                Debug.Log($"Reconstructed projection matrix, method 1:\n{projectionMatrixGL1}");

                #region comments
                // MainCamera.projectionMatrix = projectionMatrixGL; 

                // Debug.Log($"NDCMatrixOpenCV=\n {NDCMatrixOpenCV}");

                // Debug.Log($"OpenCVtoOpenGL=\n{OpenCVtoOpenGL}");


                //// Method 2: shitfing orthogonal with the central projection.

                //left = CameraInternalParameters.PrincipalPoint.x;
                //right = width - left;

                //top = CameraInternalParameters.PrincipalPoint.y;
                //bottom = -(height - top);

                //Matrix4x4 NDCMatrixOpenGL2 = GetOrthoMat(left, right, top, bottom, near, far);

                //Matrix4x4 NDCMatrixOpenCV2 = GetOrthoMat(left, right, bottom, top, near, far);
                //Matrix4x4 openGLPerspMatrix2 = OpenCVKMatrixToOpenGLKMatrix(CameraInternalParameters.FocalLength.x, CameraInternalParameters.FocalLength.y,
                //                                              0.0f, 0.0f,
                //                                              near, far);



                ////we can think of the perspective transformation as converting 
                //// a trapezoidal-prism - shaped viewing volume
                ////    into a rectangular - prism - shaped viewing volume,
                ////    which glOrtho() scales and translates into the 2x2x2 cube in Normalized Device Coordinates.

                //// Invert the direction of y axis and translate by height along the inverted direction.

                //column0 = new Vector4(1f, 0f, 0f, 0f);
                //column1 = new Vector4(0f, -1f, 0f, 0f);
                //column2 = new Vector4(0f, 0f, 1f, 0f);
                //column3 = new Vector4(0f, height, 0f, 1f);

                //Matrix4x4 OpenCVtoOpenGL2 = new Matrix4x4(column0, column1, column2, column3);

                //// Matrix4x4 projectionMatrixGL1 = NDCMatrixOpenCV * OpenCVtoOpenGL * openGLPerspMatrix;


                //Matrix4x4 projectionMatrixGL2 = NDCMatrixOpenGL2 * openGLPerspMatrix2;

                //Debug.Log($"projection matrix, method 2:\n{projectionMatrixGL2}");


                ////Debug.Log($"NDC  Matrix: Using GLOrtho directly=\n {NDCMatrixOpenGL}");

                ////Matrix4x4 NDCMatrixOpenGL2 = NDCMatrixOpenCV * OpenCVtoOpenGL;
                ////Debug.Log($"NDC  Matrix:Frame Transform Approach=\n{NDCMatrixOpenGL2}");
                #endregion comments

                rayTracingShader.SetMatrix("_Projection", projectionMatrixGL1);
                rayTracingShader.SetMatrix("_CameraInverseProjection", projectionMatrixGL1.inverse);

                rayTracingShader.SetInt("_UndistortionMethod", (int)calibratedProjectorMode);
                // prepareCameraData(control);
                rayTracingShader.SetBuffer(DanbiKernelHelper.CurrentKernelIndex, "_CameraInternalData", control.buffersDict["_CameraInternalData"]);
                // rayTracingShader.SetBuffer(DanbiKernelHelper.CurrentKernelIndex, "_CameraExternalData", control.buffersDict["_CameraExternalData"]);

                // rayTracingShader.SetFloat("_IterativeThreshold", iterativeThreshold);
                // rayTracingShader.SetFloat("_IterativeSafeCounter", iterativeSafetyCounter);
                // rayTracingShader.SetFloat("_NewTonThreshold", newtonThreshold);

                #region predecesor
                // http://www.songho.ca/opengl/gl_projectionmatrix.html         

                // var dat = m_cameraInternalData;
                // var openGLNDCMatrix = DanbiComputeShaderHelper.GetOrthoMatOpenGL(0, width, 0, height, near, far);
                // var openGLPerspMatrix = DanbiComputeShaderHelper.OpenCV_KMatrixToOpenGLPerspMatrix(dat.focalLengthX,
                //                                                                                    dat.focalLengthY,
                //                                                                                    dat.principalPointX,
                //                                                                                    dat.principalPointY,
                //                                                                                    near,
                //                                                                                    far,
                //                                                                                    width,
                //                                                                                    height);

                // var openGLKMatrix;                                                                               
                // var OpenCVToUnity = DanbiComputeShaderHelper.GetOpenCVToUnity();
                //Debug.Log($"OpenGL To Unity Matrix -> \n{OpenGLToUnity}");

                //Matrix4x4 OpenGLToOpenCV = GetOpenGLToOpenCV(CurrentScreenResolutions.y);
                //Debug.Log($"OpenGL to OpenCV Matrix -> \n{OpenGLToOpenCV}");

                //Matrix4x4 projMat = openGLNDCMatrix * openGLPerspMatrix; //* OpenCVToUnity; // * OpenGLToUnity;



                // rayTracingShader.SetMatrix("_Projection", projMat);
                // rayTracingShader.SetMatrix("_CameraInverseProjection", projMat.inverse);

                // rayTracingShader.SetInt("_UseUndistortion", useCalibration ? 1 : 0);

                // Debug.Log($"Using Undistortion? {useCalibration}");
                // rayTracingShader.SetInt("_UndistortionMethod", (int)undistortionMethod);
                // Debug.Log($"Using Undistortion Method -> {(int)undistortionMethod}, ({undistortionMethod})");
                // rayTracingShader.SetBuffer(DanbiKernelHelper.CurrentKernelIndex, "_CameraInternalData", control.buffersDict["_CameraInternalData"]);

                // rayTracingShader.SetFloat("_IterativeThreshold", iterativeThreshold);
                // rayTracingShader.SetFloat("_IterativeSafeCounter", iterativeSafetyCounter);
                // rayTracingShader.SetFloat("_NewTonThreshold", newtonThreshold);
                #endregion
            }
        }

        void SetCameraExternalBuffers((int width, int height) imageResolution, DanbiComputeShaderControl control)
        {

        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            // 1. Update Screen props
            if (control is DanbiUIProjectorInfoPanelControl)
            {
                var screenPanel = control as DanbiUIProjectorInfoPanelControl;

                // update aspect ratio
                aspectRatio = new Vector2(screenPanel.aspectRatioWidth, screenPanel.aspectRatioHeight);
                mainCam.aspect = aspectRatioDivided = aspectRatio.x / aspectRatio.y;
                mainCam.fieldOfView = fov = screenPanel.fov;

                // Screen Resolution is updated in DanbiScreen.                
            }

            // 2. Update physical camera props
            // if (control is DanbiUIProjectorPhysicalCameraPanelControl)
            // {
            //     var physicalCameraPanel = control as DanbiUIProjectorPhysicalCameraPanelControl;

            //     mainCam.usePhysicalProperties = usePhysicalCamera = physicalCameraPanel.isToggled;

            //     if (usePhysicalCamera)
            //     {
            //         mainCam.focalLength = focalLength = physicalCameraPanel.focalLength;
            //         sensorSize.x = physicalCameraPanel.sensorSize.width;
            //         sensorSize.y = physicalCameraPanel.sensorSize.height;
            //         mainCam.sensorSize = new Vector2(sensorSize.x, sensorSize.y);

            //         // Update the fov display
            //         float fovFwd = mainCam.fieldOfView;
            //         physicalCameraPanel.onFOVCalc?.Invoke(fovFwd);
            //         fov = fovFwd;
            //     }
            // }

            // 3. Update calibration props
            if (control is DanbiUIProjectorCalibratedPanelControl)
            {
                var calibrationPanel = control as DanbiUIProjectorCalibratedPanelControl;

                useCalibratedProjector = calibrationPanel.useCalbiratedCamera;

                if (useCalibratedProjector)
                {
                    calibratedProjectorMode = calibrationPanel.lensDistortionMode;
                    newtonThreshold = calibrationPanel.newtonThreshold;
                    iterativeThreshold = calibrationPanel.iterativeThreshold;
                    iterativeSafetyCounter = calibrationPanel.iterativeSafetyCounter;
                }
            }

            // 4. Update internal parameter props
            if (control is DanbiUIProjectorInternalParametersPanelControl)
            {
                if (!useCalibratedProjector)
                {
                    return;
                }

                var internalParamsPanel = control as DanbiUIProjectorInternalParametersPanelControl;
                cameraInternalData = new DanbiCameraInternalData();
                // Load Camera Internal Paramters                
                cameraInternalData = internalParamsPanel.internalData;

                radialCoefficient.x = cameraInternalData.radialCoefficientX;
                radialCoefficient.y = cameraInternalData.radialCoefficientY;
                radialCoefficient.z = cameraInternalData.radialCoefficientZ;

                tangentialCoefficient.x = cameraInternalData.tangentialCoefficientX;
                tangentialCoefficient.y = cameraInternalData.tangentialCoefficientY;

                principalCoefficient.x = cameraInternalData.principalPointX;
                principalCoefficient.y = cameraInternalData.principalPointY;

                externalFocalLength.x = cameraInternalData.focalLengthX;
                externalFocalLength.y = cameraInternalData.focalLengthY;

                skewCoefficient = cameraInternalData.skewCoefficient;
            }

            // 5. Update external parameter props
            if (control is DanbiUIProjectorExternalParametersPanelControl)
            {
                if (!useCalibratedProjector)
                {
                    return;
                }

                var externalParamsPanel = control as DanbiUIProjectorExternalParametersPanelControl;
                cameraExternalData = new DanbiCameraExternalData();
                // Load Projector External Parameters
                cameraExternalData = externalParamsPanel.externalData;

                projectorPosition.x = cameraExternalData.projectorPosition.x;
                projectorPosition.y = cameraExternalData.projectorPosition.y;
                projectorPosition.z = cameraExternalData.projectorPosition.z;

                xAxis.x = cameraExternalData.xAxis.x;
                xAxis.y = cameraExternalData.xAxis.y;
                xAxis.z = cameraExternalData.xAxis.z;

                yAxis.x = cameraExternalData.yAxis.x;
                yAxis.y = cameraExternalData.yAxis.y;
                yAxis.z = cameraExternalData.yAxis.z;

                zAxis.x = cameraExternalData.zAxis.x;
                zAxis.y = cameraExternalData.zAxis.y;
                zAxis.z = cameraExternalData.zAxis.z;

            }
        }
    };
};
