using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Unity.Mathematics;

namespace Danbi
{
    public class DanbiCameraControl : MonoBehaviour
    {
        [HideInInspector]
        public DanbiCameraInternalData m_cameraInternalData;

        [HideInInspector]
        public DanbiCameraExternalData m_cameraExternalData;

        public bool m_useCalibratedProjector = false;

        #region Calibrated Projector Mode

        public EDanbiLensUndistortMode m_lensUndistortMode = EDanbiLensUndistortMode.Direct;
        public float m_iterativeThreshold;
        public float m_iterativeSafetyCounter;
        public float m_newtonThreshold;

        #endregion Calibrated Projector Mode

        #region Camera Info
        public float m_fov;
        public Vector2 m_nearFar = new Vector2(0.01f, 250.0f);
        public Vector2 m_aspectRatio = new Vector2(16, 9);
        public float m_aspectRatioDivided;

        #endregion Camera Info

        #region Internal Projector Parameters

        public Vector3 m_radialCoefficient;
        public Vector2 m_tangentialCoefficient;
        public Vector2 m_principalCoefficient;
        public Vector2 m_externalFocalLength;
        public float m_skewCoefficient;

        #endregion Internal Projector Parameters

        #region External Projector Parameters
        // for inspector only.
        public Vector3 m_projectorPosition;
        public Vector3 m_xAxis;
        public Vector3 m_yAxis;
        public Vector3 m_zAxis;

        #endregion External Projector Parameters

        Camera m_mainCam;
        private float m_cameraHeight;

        Camera mainCam
        {
            get
            {
                m_mainCam.NullFinally(() => m_mainCam = Camera.main);
                return m_mainCam;
            }
        }

        void Awake()
        {
            // 1. bind the delegates
            DanbiUISync.onPanelUpdate += OnPanelUpdate;
            // DanbiPrewarperSetting.onPrepareShaderData += prepareCameraParameters;
        }


        void Start()
        {
            m_cameraInternalData = new DanbiCameraInternalData();
            m_cameraExternalData = new DanbiCameraExternalData();
        }

        /// <summary>
        /// The following function sets the buffers related to camera
        /// </summary>
        /// <param name="width"></param>
        /// <param name="imageResolution"></param>
        /// <param name="shaderControl"></param>
        public void SetCameraParameters((int width, int height) imageResolution, DanbiComputeShaderControl shaderControl)
        {
            shaderControl.NullFinally(() => Debug.LogError($"<color=red>ComputeShaderControl is null!</color>"));
            var rayTracingShader = shaderControl.danbiShader;

            if (!m_useCalibratedProjector)
            {
                // 1. Create Camera Transform
                Camera.main.gameObject.transform.eulerAngles = new Vector3(90, 0, 0);
                Camera.main.gameObject.transform.position = new Vector3(0, m_cameraHeight, 0);

                Debug.Log($"camera position={  Camera.main.gameObject.transform.position.y}");
                Debug.Log($"localToWorldMatrix =\n{ Camera.main.gameObject.transform.localToWorldMatrix}, " +
                   $"\nQuaternion Mat4x4: { Camera.main.gameObject.transform.rotation} ");

                // 2. Create Projection Matrix
                rayTracingShader.SetMatrix("_Projection", Camera.main.projectionMatrix);
                rayTracingShader.SetMatrix("_CameraInverseProjection", Camera.main.projectionMatrix.inverse);
                rayTracingShader.SetMatrix("_CameraToWorldMat", Camera.main.cameraToWorldMatrix);
                rayTracingShader.SetVector("_CameraViewDirectionInUnitySpace", Camera.main.transform.forward);
                rayTracingShader.SetBool("_UseCalibratedCamera", false);
            }
            else // m_useCalibratedProjector == true
            {
                rayTracingShader.SetBool("_UseCalibratedCamera", true);
                rayTracingShader.SetInt("_LensUndistortMode", (int)m_lensUndistortMode);
                shaderControl.buffersDict.Add("_CameraInternalData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret(m_cameraInternalData.asStruct, m_cameraInternalData.stride));
                rayTracingShader.SetBuffer(DanbiKernelHelper.CurrentKernelIndex, "_CameraInternalData", shaderControl.buffersDict["_CameraInternalData"]);

                // 1. Create Camera Transform
                float4x4 ViewTransform_OpenCV = new float4x4(new float4(m_cameraExternalData.xAxis, 0),
                                                             new float4(m_cameraExternalData.yAxis, 0),
                                                             new float4(m_cameraExternalData.zAxis, 0),
                                                             new float4(m_cameraExternalData.projectorPosition, 1)
                                                             );

                Debug.Log($"ViewTransform =\n{  ViewTransform_OpenCV }");
                float3x3 ViewTransform_Rot_OpenCV = new float3x3(
                    ViewTransform_OpenCV.c0.xyz, ViewTransform_OpenCV.c1.xyz, ViewTransform_OpenCV.c2.xyz);

                float3 ViewTransform_Trans_OpenCV = ViewTransform_OpenCV.c3.xyz;

                float3x3 CameraTransformation_Rot_OpenCV = math.transpose(ViewTransform_Rot_OpenCV);

                float4x4 CameraTransformation_OpenCV = new float4x4(CameraTransformation_Rot_OpenCV,
                                                        -math.mul(CameraTransformation_Rot_OpenCV, ViewTransform_Trans_OpenCV));

                Debug.Log($"CameraTransformation_OpenCV (obtained by transpose) =\n{ CameraTransformation_OpenCV }");


                float4x4 CameraTransform_OpenCV = math.inverse(ViewTransform_OpenCV);
                Debug.Log($" CameraTransform_OpenCV (obtained by inverse)=\n{  CameraTransform_OpenCV }");

                // https://stackoverflow.com/questions/1263072/changing-a-matrix-from-right-handed-to-left-handed-coordinate-system

                // UnityToOpenMat is a change of basis matrix, a swap of axes, with a determinmant -1, which is
                // improper rotation, and so a well-defined quaternion does not exist for it.

                float4 externalData_column0 = new float4(DanbiCameraExternalData.UnityToOpenCVMat.c0, 0);
                float4 externalData_column1 = new float4(DanbiCameraExternalData.UnityToOpenCVMat.c1, 0);
                float4 externalData_column2 = new float4(DanbiCameraExternalData.UnityToOpenCVMat.c2, 0);
                float4 externalData_column3 = new float4(0, 0, 0, 1);


                float4x4 UnityToOpenCV = new float4x4(externalData_column0, externalData_column1, externalData_column2, externalData_column3);

                float3x3 UnityToOpenCV_Rot = new float3x3(externalData_column0.xyz, externalData_column1.xyz, externalData_column2.xyz);
                float3x3 OpenCVToUnity_Rot = math.transpose(UnityToOpenCV_Rot);
                float3 UnityToOpenCV_Trans = externalData_column3.xyz;

                float4x4 OpenCVToUnity = new float4x4(OpenCVToUnity_Rot, -math.mul(OpenCVToUnity_Rot, UnityToOpenCV_Trans));

                Debug.Log($" UnityToOpenCV inverse = \n {math.inverse(UnityToOpenCV)} ");

                Debug.Log($" UnityToOpenCV transpose  = \n {OpenCVToUnity}");

                float4x4 MatForObjectFrame = new float4x4(
                                            new float4(1, 0, 0, 0),
                                            new float4(0, 0, 1, 0),
                                            new float4(0, -1, 0, 0),
                                            new float4(0, 0, 0, 1));

                float4x4 CameraTransform_Unity = math.mul(
                                                     math.mul(
                                                        math.mul(
                                                            UnityToOpenCV,
                                                            CameraTransform_OpenCV
                                                         ),
                                                      OpenCVToUnity //math.inverse(UnityToOpenCV)
                                                      ),

                                                     MatForObjectFrame
                                                     );

                Matrix4x4 CameraTransform_Unity_Mat4x4 = (Matrix4x4)CameraTransform_Unity;
                Debug.Log($"Determinimant of CameraTransform_Unity_Mat4x4=\n{CameraTransform_Unity_Mat4x4.determinant}");


                // Camera.main.gameObject.transform.position = GetPosition(CameraTransform_Unity_Mat4x4);                     

                Camera.main.gameObject.transform.position = new Vector3(0.0f, m_cameraHeight, 0.0f);

                Debug.Log($"Quaternion = CameraTransform_Unity_Mat4x4.rotation=  \n {CameraTransform_Unity_Mat4x4.rotation}");
                Debug.Log($"QuaternionFromMatrix(MatForUnityCameraFrameMat4x4)\n{DanbiComputeShaderHelper.QuaternionFromMatrix(CameraTransform_Unity_Mat4x4)}");

                Camera.main.gameObject.transform.rotation = DanbiComputeShaderHelper.GetRotation(CameraTransform_Unity_Mat4x4);

                Debug.Log($"CameraTransform_Unity_Mat4x4= \n {CameraTransform_Unity_Mat4x4}");


                Debug.Log($"localToWorldMatrix =\n{ Camera.main.gameObject.transform.localToWorldMatrix}, " +
                    $"\nQuaternion Mat4x4: { Camera.main.gameObject.transform.rotation}, " +
                    $"\neulerAngles ={ Camera.main.gameObject.transform.eulerAngles}");


                rayTracingShader.SetMatrix("_CameraToWorld", Camera.main.cameraToWorldMatrix);
                Vector4 cameraDirection = new Vector4(Camera.main.transform.forward.x, Camera.main.transform.forward.y,
                                                      Camera.main.transform.forward.z, 0f);
                rayTracingShader.SetVector("_CameraViewDirectionInUnitySpace", Camera.main.transform.forward);

                // 2. Create Projection Matrix

                float width = (float)DanbiManager.instance.screen.screenResolution.x; // width = 3840 =  Projector Width
                float height = (float)DanbiManager.instance.screen.screenResolution.y; // height = 2160 = Projector Height

                float left = 0;
                float right = width;
                float bottom = 0;
                float top = height;

                float near = Camera.main.nearClipPlane;      // near: positive
                float far = Camera.main.farClipPlane;        // far: positive

                float aspectRatio = width / height;

                float scaleFactorX = m_cameraInternalData.focalLengthX;
                float scaleFactorY = m_cameraInternalData.focalLengthY;

                float cx = m_cameraInternalData.principalPointX;
                float cy = m_cameraInternalData.principalPointY;

                Matrix4x4 NDCMatrix_OpenGL = DanbiComputeShaderHelper.GetOrthoMat(left, right, bottom, top, near, far);

                // understand the following code
                Matrix4x4 KMatrixFromOpenCVToOpenGL = DanbiComputeShaderHelper.OpenCVKMatrixToOpenGLKMatrix(scaleFactorX, scaleFactorY, cx, cy, near, far);

                Vector4 column0 = new Vector4(1f, 0f, 0f, 0f);
                Vector4 column1 = new Vector4(0f, -1f, 0f, 0f);
                Vector4 column2 = new Vector4(0f, 0f, 1f, 0f);
                Vector4 column3 = new Vector4(0f, height, 0f, 1f);

                Matrix4x4 OpenCVCameraToOpenGLCamera = new Matrix4x4(column0, column1, column2, column3);

                Matrix4x4 projectionMatrixGL1 = NDCMatrix_OpenGL * OpenCVCameraToOpenGLCamera * KMatrixFromOpenCVToOpenGL;

                Debug.Log($"Reconstructed projection matrix, method 1:\n{projectionMatrixGL1}");

                rayTracingShader.SetMatrix("_Projection", projectionMatrixGL1);
                rayTracingShader.SetMatrix("_CameraInverseProjection", projectionMatrixGL1.inverse);
            } // if (_UseCalibratedCamera)
        } // SetCameraParameters()

        void SetCameraExternalBuffers((int width, int height) imageResolution, DanbiComputeShaderControl control)
        {

        }

        /// <summary>
        /// class -> DanbiCameraControl
        /// </summary>
        /// <param name="control"></param>
        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            // ie. if projector X coords are written,
            // then control is DanbiUIProjectorExternalParametersPanelControl and it invokes this.OnPanelUpdate().            

            // 1. Update Screen props
            if (control is DanbiUIProjectorInfoPanelControl)
            {
                var screenPanel = control as DanbiUIProjectorInfoPanelControl;

                // update aspect ratio
                m_aspectRatio = new Vector2(screenPanel.aspectRatioWidth, screenPanel.aspectRatioHeight);
                Camera.main.aspect = m_aspectRatioDivided = m_aspectRatio.x / m_aspectRatio.y;
                Camera.main.fieldOfView = m_fov = screenPanel.fov;

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

                m_useCalibratedProjector = calibrationPanel.useCalibratedCamera;

                if (m_useCalibratedProjector)
                {
                    m_lensUndistortMode = calibrationPanel.lensUndistortMode;
                    m_newtonThreshold = calibrationPanel.newtonThreshold;
                    m_iterativeThreshold = calibrationPanel.iterativeThreshold;
                    m_iterativeSafetyCounter = calibrationPanel.iterativeSafetyCounter;
                }
            }

            // 4. Update internal parameter props
            if (control is DanbiUIProjectorInternalParametersPanelControl)
            {
                if (!m_useCalibratedProjector)
                {
                    return;
                }

                var internalParamsPanel = control as DanbiUIProjectorInternalParametersPanelControl;

                // Load Camera Internal Paramters            
                m_cameraInternalData = internalParamsPanel.internalData;

                m_radialCoefficient.x = m_cameraInternalData.radialCoefficientX;
                m_radialCoefficient.y = m_cameraInternalData.radialCoefficientY;
                m_radialCoefficient.z = m_cameraInternalData.radialCoefficientZ;

                m_tangentialCoefficient.x = m_cameraInternalData.tangentialCoefficientX;
                m_tangentialCoefficient.y = m_cameraInternalData.tangentialCoefficientY;

                m_principalCoefficient.x = m_cameraInternalData.principalPointX;
                m_principalCoefficient.y = m_cameraInternalData.principalPointY;

                m_externalFocalLength.x = m_cameraInternalData.focalLengthX;
                m_externalFocalLength.y = m_cameraInternalData.focalLengthY;

                m_skewCoefficient = m_cameraInternalData.skewCoefficient;
            }

            // 5. Update external parameter props
            if (control is DanbiUIProjectorExternalParametersPanelControl)
            {
                if (!m_useCalibratedProjector)
                {
                    return;
                }

                // TODO: 
                // the Projector Position, projector Axes should be determined before calling 
                // OnPanelUpdate() event handler.
                // var externalParamsPanel = control as DanbiUIProjectorExternalParametersPanelControl;
                var externalParamsPanel = (DanbiUIProjectorExternalParametersPanelControl)control;

                // Load Projector External Parameters
                m_cameraExternalData = externalParamsPanel.externalData;

                // the followings are used only for the inspector
                m_projectorPosition.x = m_cameraExternalData.projectorPosition.x;
                m_projectorPosition.y = m_cameraExternalData.projectorPosition.y;
                m_projectorPosition.z = m_cameraExternalData.projectorPosition.z;

                m_xAxis.x = m_cameraExternalData.xAxis.x;
                m_xAxis.y = m_cameraExternalData.xAxis.y;
                m_xAxis.z = m_cameraExternalData.xAxis.z;

                m_yAxis.x = m_cameraExternalData.yAxis.x;
                m_yAxis.y = m_cameraExternalData.yAxis.y;
                m_yAxis.z = m_cameraExternalData.yAxis.z;

                m_zAxis.x = m_cameraExternalData.zAxis.x;
                m_zAxis.y = m_cameraExternalData.zAxis.y;
                m_zAxis.z = m_cameraExternalData.zAxis.z;
            }

            if (control is DanbiUIProjectorInfoPanelControl)
            {
                var infoControl = control as DanbiUIProjectorInfoPanelControl;
                m_cameraHeight = infoControl.projectorHeight;
            }
        }
    };
};
