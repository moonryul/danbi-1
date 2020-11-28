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
    public class DanbiCamera : MonoBehaviour
    {
        [SerializeField]
        DanbiCameraInternalData m_cameraInternalData = new DanbiCameraInternalData();

        [SerializeField, Space(20)]
        DanbiCameraExternalData m_cameraExternalData = new DanbiCameraExternalData();

        [SerializeField, Readonly]
        bool m_useCalibratedProjector = false;

        [SerializeField, Readonly]
        EDanbiLensUndistortMode m_lensUndistortMode = EDanbiLensUndistortMode.Direct;

        [SerializeField, Readonly]
        float m_fov;


        [SerializeField, Readonly]
        float m_projectorHeight;

        [SerializeField]
        Matrix4x4 m_calibratedProjectionMatrixGL;

        [SerializeField]
        Vector3 m_unityCamPos;

        [SerializeField]
        Matrix4x4 m_cameraTransform_Unity_Mat4x4;

        readonly int m_errorVal = -9999;

        void Awake()
        {
            #region bind calibration delegates
            DanbiUIProjectorCalibratedPanel.onUseCalibratedCamera +=
                (bool use) =>
                {
                    m_useCalibratedProjector = use;
                    SetRenderingCameraTransformation();
                };

            DanbiUIProjectorCalibratedPanel.onLensDistortModeChange +=
                (EDanbiLensUndistortMode mode) => m_lensUndistortMode = mode;
            #endregion bind calibration delegates

            #region bind projector info delegates
            DanbiUIProjectorInfoPanel.onFovUpdate +=
                (float fov) =>
                {
                    m_fov = fov;
                    Camera.main.fieldOfView = m_fov;
                };

            DanbiUIProjectorInfoPanel.onProjectorHeightUpdate +=
                (float projectorHeight) =>
                {
                    m_projectorHeight = projectorHeight;
                };
            #endregion bind projector info delegates

            #region bind internal parameters delegates            
            DanbiUIProjectorInternalParametersPanel.onRadialCoefficientXUpdate +=
                (float x) =>
                {
                    m_cameraInternalData.radialCoefficient.x = x;
                    // set main camera by calibrate camera usage.
                    PrepareInternalParametersToMatrix();
                };

            DanbiUIProjectorInternalParametersPanel.onRadialCoefficientYUpdate +=
                (float y) =>
                {
                    m_cameraInternalData.radialCoefficient.y = y;
                    // set main camera by calibrate camera usage.
                    PrepareInternalParametersToMatrix();
                };

            DanbiUIProjectorInternalParametersPanel.onRadialCoefficientZUpdate +=
                (float z) =>
                {
                    m_cameraInternalData.radialCoefficient.z = z;
                    // set main camera by calibrate camera usage.
                    PrepareInternalParametersToMatrix();
                };

            DanbiUIProjectorInternalParametersPanel.onTangentialCoefficientXUpdate +=
                (float x) =>
                {
                    m_cameraInternalData.tangentialCoefficient.x = x;
                    // set main camera by calibrate camera usage.
                    PrepareInternalParametersToMatrix();
                };

            DanbiUIProjectorInternalParametersPanel.onTangentialCoefficientYUpdate +=
                (float y) =>
                {
                    m_cameraInternalData.tangentialCoefficient.y = y;
                    // set main camera by calibrate camera usage.
                    PrepareInternalParametersToMatrix();
                };

            DanbiUIProjectorInternalParametersPanel.onPrincipalPointXUpdate +=
                (float x) =>
                {
                    m_cameraInternalData.principalPoint.x = x;
                    // set main camera by calibrate camera usage.
                    PrepareInternalParametersToMatrix();
                };

            DanbiUIProjectorInternalParametersPanel.onPrincipalPointYUpdate +=
                (float y) =>
                {
                    m_cameraInternalData.principalPoint.y = y;
                    // set main camera by calibrate camera usage.
                    PrepareInternalParametersToMatrix();
                };

            DanbiUIProjectorInternalParametersPanel.onFocalLengthXUpdate +=
                (float x) =>
                {
                    m_cameraInternalData.focalLength.x = x;
                    // set main camera by calibrate camera usage.
                    PrepareInternalParametersToMatrix();
                };

            DanbiUIProjectorInternalParametersPanel.onFocalLengthYUpdate +=
                (float y) =>
                {
                    m_cameraInternalData.focalLength.y = y;
                    // set main camera by calibrate camera usage.
                    PrepareInternalParametersToMatrix();
                };

            #endregion bind internal parameters delegates

            #region bind external parameters delegates
            DanbiUIProjectorExternalParametersPanel.onProjectorPositionXUpdate +=
                (float x) =>
                {
                    m_cameraExternalData.projectorPosition.x = x;
                    PrepareExternalParametersToMatrix();
                };

            DanbiUIProjectorExternalParametersPanel.onProjectorPositionYUpdate +=
                (float y) =>
                {
                    m_cameraExternalData.projectorPosition.y = y;
                    PrepareExternalParametersToMatrix();
                };

            DanbiUIProjectorExternalParametersPanel.onProjectorPositionZUpdate +=
                (float z) =>
                {
                    m_cameraExternalData.projectorPosition.z = z;
                    PrepareExternalParametersToMatrix();
                };

            DanbiUIProjectorExternalParametersPanel.onProjectorXAxisXUpdate +=
                (float x) =>
                {
                    m_cameraExternalData.xAxis.x = x;
                    PrepareExternalParametersToMatrix();
                };

            DanbiUIProjectorExternalParametersPanel.onProjectorXAxisYUpdate +=
                (float y) =>
                {
                    m_cameraExternalData.xAxis.y = y;
                    PrepareExternalParametersToMatrix();
                };

            DanbiUIProjectorExternalParametersPanel.onProjectorXAxisZUpdate +=
                (float z) =>
                {
                    m_cameraExternalData.xAxis.z = z;
                    PrepareExternalParametersToMatrix();
                };

            DanbiUIProjectorExternalParametersPanel.onProjectorYAxisXUpdate +=
                (float x) =>
                {
                    m_cameraExternalData.yAxis.x = x;
                    PrepareExternalParametersToMatrix();
                };

            DanbiUIProjectorExternalParametersPanel.onProjectorYAxisYUpdate +=
                (float y) =>
                {
                    m_cameraExternalData.yAxis.y = y;
                    PrepareExternalParametersToMatrix();
                };

            DanbiUIProjectorExternalParametersPanel.onProjectorYAxisZUpdate +=
                (float z) =>
                {
                    m_cameraExternalData.yAxis.z = z;
                    PrepareExternalParametersToMatrix();
                };

            DanbiUIProjectorExternalParametersPanel.onProjectorZAxisXUpdate +=
                (float x) =>
                {
                    m_cameraExternalData.zAxis.x = x;
                    PrepareExternalParametersToMatrix();
                };

            DanbiUIProjectorExternalParametersPanel.onProjectorZAxisYUpdate +=
                (float y) =>
                {
                    m_cameraExternalData.zAxis.y = y;
                    PrepareExternalParametersToMatrix();
                };

            DanbiUIProjectorExternalParametersPanel.onProjectorZAxisZUpdate +=
                (float z) =>
                {
                    m_cameraExternalData.zAxis.z = z;
                    PrepareExternalParametersToMatrix();
                };

            #endregion bind external parameters delegates
        }

        void Update()
        {
            DanbiManager.instance.shaderControl.danbiShader.SetInt("_LensUndistortMode", (int)m_lensUndistortMode);
            DanbiManager.instance.shaderControl.danbiShader.SetBool("_UseCalibratedCamera", m_useCalibratedProjector);
        }

        public void CreateCameraBuffers(DanbiComputeShader shaderControl)
        {
            shaderControl.bufferDict.AddBuffer_NoDuplicate("_CameraInternalData", DanbiComputeShaderHelper.CreateComputeBuffer_Ret(m_cameraInternalData.asStruct, m_cameraInternalData.stride));
        }

        // TODO:
        // bool CheckAllInternalValuesValid()
        // {
        //     // return (DanbiManager.instance.screen.screenResolution.x != m_errorVal &&
        //     //         DanbiManager.instance.screen.screenResolution.y != m_errorVal &&
        //     //         m_cameraInternalData.focalLength.x != m_errorVal &&

        // }

        // bool CheckAllExternalValuesValid()
        // {

        // }

        void PrepareInternalParametersToMatrix()
        {            
            // 3. Create the Projection Matrix
            float width = DanbiManager.instance.screen.screenResolution.x; // width = 3840 =  Projector Width
            float height = DanbiManager.instance.screen.screenResolution.y; // height = 2160 = Projector Height

            float left = 0;
            float right = width;
            float bottom = 0;
            float top = height;

            float near = Camera.main.nearClipPlane;      // near: positive
            float far = Camera.main.farClipPlane;        // far: positive

            // float aspectRatio = width / height;

            float scaleFactorX = m_cameraInternalData.focalLength.x;
            float scaleFactorY = m_cameraInternalData.focalLength.y;

            float cx = m_cameraInternalData.principalPoint.x;
            float cy = m_cameraInternalData.principalPoint.y;

            // http://ksimek.github.io/2013/06/03/calibrated_cameras_in_opengl/
            //we can think of the perspective transformation as converting 
            // a trapezoidal-prism - shaped viewing volume
            //    into a rectangular - prism - shaped viewing volume,
            //    which glOrtho() scales and translates into the 2x2x2 cube in Normalized Device Coordinates.

            Matrix4x4 NDCMatrix_OpenGL = DanbiComputeShaderHelper.GetOrthoMat(left, right, bottom, top, near, far);

            //  // refer to to   //http://ksimek.github.io/2012/08/14/decompose/  to  understand the following code
            Matrix4x4 KMatrixFromOpenCVToOpenGL = DanbiComputeShaderHelper.OpenCVKMatrixToOpenGLKMatrix(scaleFactorX, scaleFactorY, cx, cy, near, far);

            // our discussion of 2D coordinate conventions have referred to the coordinates used during calibration.
            // If your application uses a different 2D coordinate convention,
            //  you'll need to transform K using 2D translation and reflection.

            //  For example, consider a camera matrix that was calibrated with the origin in the top-left 
            //  and the y - axis pointing downward, but you prefer a bottom-left origin with the y-axis pointing upward.
            //  To convert, you'll first negate the image y-coordinate and then translate upward by the image height, h. 
            //  The resulting intrinsic matrix K' is given by:

            // K' = [ 1 0 0; 0 1 h; 0 0 1] *  [ 1 0 0; 0 -1 0; 0 0 1] * K

            Vector4 column0 = new Vector4(1f, 0f, 0f, 0f);
            Vector4 column1 = new Vector4(0f, -1f, 0f, 0f);
            Vector4 column2 = new Vector4(0f, 0f, 1f, 0f);
            Vector4 column3 = new Vector4(0f, height, 0f, 1f);

            Matrix4x4 OpenCVCameraToOpenGLCamera = new Matrix4x4(column0, column1, column2, column3);

            m_calibratedProjectionMatrixGL = NDCMatrix_OpenGL * OpenCVCameraToOpenGLCamera * KMatrixFromOpenCVToOpenGL;
            SetRenderingCameraTransformation();
        }

        void PrepareExternalParametersToMatrix()
        {
            // 1. Create the Camera Transform                
            float4x4 ViewTransform_OpenCV = new float4x4(new float4(m_cameraExternalData.xAxis, 0),
                                                         new float4(m_cameraExternalData.yAxis, 0),
                                                         new float4(m_cameraExternalData.zAxis, 0),
                                                         new float4(m_cameraExternalData.projectorPosition * 0.001f, 1)
                                                         );
            // Debug.Log($"ViewTransform =\n{  ViewTransform_OpenCV }");

            float3x3 ViewTransform_Rot_OpenCV = new float3x3(
                ViewTransform_OpenCV.c0.xyz, ViewTransform_OpenCV.c1.xyz, ViewTransform_OpenCV.c2.xyz);

            float3 ViewTransform_Trans_OpenCV = ViewTransform_OpenCV.c3.xyz;


            float3x3 CameraTransformation_Rot_OpenCV = math.transpose(ViewTransform_Rot_OpenCV);


            // float4x4 CameraTransformation_OpenCV = new float4x4(CameraTransformation_Rot_OpenCV,
            //                                         -math.mul(CameraTransformation_Rot_OpenCV, ViewTransform_Trans_OpenCV));

            float4x4 CameraTransformation_OpenCV = new float4x4(new float4(CameraTransformation_Rot_OpenCV.c0, 0.0f),
                                                                new float4(CameraTransformation_Rot_OpenCV.c1, 0.0f),
                                                                new float4(CameraTransformation_Rot_OpenCV.c2, 0.0f),
                                                                new float4(-math.mul(CameraTransformation_Rot_OpenCV, ViewTransform_Trans_OpenCV), 1.0f));

            // Debug.Log($"CameraTransformation_OpenCV (obtained by transpose) =\n{ CameraTransformation_OpenCV }");


            // float4x4 CameraTransform_OpenCV = math.inverse(ViewTransform_OpenCV);
            // Debug.Log($" CameraTransform_OpenCV (obtained by inverse)=\n{  CameraTransform_OpenCV }");

            // https://stackoverflow.com/questions/1263072/changing-a-matrix-from-right-handed-to-left-handed-coordinate-system

            // UnityToOpenMat is a change of basis matrix, a swap of axes, with a determinmant -1, which is
            // improper rotation, and so a well-defined quaternion does not exist for it.

            float4 externalData_column0 = new float4(DanbiCameraExternalData.UnityToOpenCVMat.c0, 0);
            float4 externalData_column1 = new float4(DanbiCameraExternalData.UnityToOpenCVMat.c1, 0);
            float4 externalData_column2 = new float4(DanbiCameraExternalData.UnityToOpenCVMat.c2, 0);
            float4 externalData_column3 = new float4(0, 0, 0, 1);


            float4x4 UnityToOpenCV = new float4x4(externalData_column0, externalData_column1, externalData_column2, externalData_column3);

            float3x3 UnityToOpenCV_Rot = new float3x3(UnityToOpenCV.c0.xyz,
                                                      UnityToOpenCV.c1.xyz,
                                                      UnityToOpenCV.c2.xyz);

            float3x3 OpenCVToUnity_Rot = math.transpose(UnityToOpenCV_Rot);

            float3 UnityToOpenCV_Trans = UnityToOpenCV.c3.xyz;

            float4x4 OpenCVToUnity = new float4x4(OpenCVToUnity_Rot, -math.mul(OpenCVToUnity_Rot, UnityToOpenCV_Trans));

            // Debug.Log($" UnityToOpenCV inverse = \n {math.inverse(UnityToOpenCV)} ");

            // Debug.Log($" UnityToOpenCV transpose  = \n {OpenCVToUnity}");

            float4x4 MatForObjectFrame = new float4x4(
                                        new float4(1, 0, 0, 0),
                                        new float4(0, 0, 1, 0),
                                        new float4(0, -1, 0, 0),
                                        new float4(0, 0, 0, 1));


            float4x4 CameraTransform_Unity = math.mul(
                                                 math.mul(
                                                     math.mul(UnityToOpenCV,
                                                              CameraTransformation_OpenCV
                                                        ),
                                                     OpenCVToUnity),
                                                 MatForObjectFrame
                                                 );

            m_cameraTransform_Unity_Mat4x4 = CameraTransform_Unity;
            // Debug.Log($"Determinimant of m_cameraTransform_Unity_Mat4x4=\n{m_cameraTransform_Unity_Mat4x4.determinant}");

            // 2. Set the Camera.main.transform.


            // Debug.Log($"Quaternion = m_cameraTransform_Unity_Mat4x4.rotation=  \n {m_cameraTransform_Unity_Mat4x4.rotation}");
            // Debug.Log($"QuaternionFromMatrix(MatForUnityCameraFrameMat4x4)\n{DanbiComputeShaderHelper.QuaternionFromMatrix(m_cameraTransform_Unity_Mat4x4)}");

            // Camera.main.gameObject.transform.position = new Vector3(0.0f, m_cameraExternalData.projectorPosition.z * 0.001f, 0.0f); // m_cameraHeight -> cm
            m_unityCamPos = DanbiComputeShaderHelper.GetPosition(m_cameraTransform_Unity_Mat4x4);
            m_unityCamPos.x = 0.0f;
            m_unityCamPos.z = 0.0f;
            SetRenderingCameraTransformation();
        }

        void SetRenderingCameraTransformation()
        {
            if (!m_useCalibratedProjector)
            {
                // Normal Camera -> Use Normal Transformation from UI Panel                
                Camera.main.gameObject.transform.eulerAngles = new Vector3(90, 0, 0);
                if (m_projectorHeight != default)
                {
                    Camera.main.gameObject.transform.position = new Vector3(0, m_projectorHeight * 0.01f, 0); // m_cameraHeight -> cm
                }
            }
            else
            {
                // Calibrated Camera -> Use Calibrated Transformation / Projection by calculation.
                if (m_cameraTransform_Unity_Mat4x4 != default)
                {
                    Camera.main.gameObject.transform.rotation = DanbiComputeShaderHelper.GetRotation(m_cameraTransform_Unity_Mat4x4);
                }

                if (m_unityCamPos != default)
                {
                    Camera.main.gameObject.transform.position = m_unityCamPos;
                }
            }
        }

        /// <summary>
        /// The following function sets the buffers related to the projector which plays the role of the
        /// camera for rendering the scene.
        /// called on SetBuffersAndRenderTextures()
        /// </summary>
        /// <param name="width"></param>
        /// <param name="imageResolution"></param>
        /// <param name="shaderControl"></param>
        public void SetCameraParametersToComputeShader(DanbiComputeShader shaderControl)
        {
            shaderControl.NullFinally(() => Debug.LogError($"<color=red>ComputeShaderControl is null!</color>"));

            var rayTracingShader = shaderControl.danbiShader;

            // TODO: Logic error
            this.CreateCameraBuffers(DanbiManager.instance.shaderControl);
            rayTracingShader.SetBuffer(DanbiKernelHelper.CurrentKernelIndex, "_CameraInternalData", shaderControl.bufferDict.GetBuffer("_CameraInternalData"));
            // rayTracingShader.SetInt("_LensUndistortMode", (int)m_lensUndistortMode);
            // rayTracingShader.SetBool("_UseCalibratedCamera", m_useCalibratedProjector);

            // 2. Set Camera Transform  the compute shader variables.
            rayTracingShader.SetMatrix("_CameraToWorldMat", Camera.main.cameraToWorldMatrix);
            rayTracingShader.SetVector("_CameraViewDirectionInUnitySpace", Camera.main.transform.forward);

            // 3. Set Projection Matrix  the compute shader variables. 
            // Camera.main.projectionMatrix and its inverse varied from the calibrated camera setting.            
            if (!m_useCalibratedProjector)
            {
                rayTracingShader.SetMatrix("_Projection", Camera.main.projectionMatrix);
                rayTracingShader.SetMatrix("_CameraInverseProjection", Camera.main.projectionMatrix.inverse);
            }
            else
            {
                rayTracingShader.SetMatrix("_Projection", m_calibratedProjectionMatrixGL);
                rayTracingShader.SetMatrix("_CameraInverseProjection", m_calibratedProjectionMatrixGL.inverse);
            }

            #region test unused

            // if (m_useCalibratedProjector)
            // {
            //     // 1. Create Camera Transform
            //     // Camera.main.gameObject.transform.eulerAngles = new Vector3(90, 0, 0);
            //     // Camera.main.gameObject.transform.position = new Vector3(0, m_cameraHeight * 0.01f, 0); // m_cameraHeight -> cm

            //     // Debug.Log($"camera position={  Camera.main.gameObject.transform.position.y}");
            //     // Debug.Log($"localToWorldMatrix =\n{ Camera.main.gameObject.transform.localToWorldMatrix}, " +
            //     //     $"\nQuaternion Mat4x4: { Camera.main.gameObject.transform.rotation} ");
            // }
            // else // m_useCalibratedProjector == true
            // {
            //     if (m_lensUndistortMode == EDanbiLensUndistortMode.NotUsing)
            //     {
            //         // 1. Create Camera Transform
            //         // Camera.main.gameObject.transform.eulerAngles = new Vector3(90, 0, 0);
            //         // Camera.main.gameObject.transform.position = new Vector3(0, m_cameraHeight * 0.01f, 0); // m_cameraHeight -> cm

            //         // Debug.Log($"camera position={  Camera.main.gameObject.transform.position.y}");
            //         // Debug.Log($"localToWorldMatrix =\n{ Camera.main.gameObject.transform.localToWorldMatrix}, " +
            //         //     $"\nQuaternion Mat4x4: { Camera.main.gameObject.transform.rotation} ");

            //         // 3. Set Projection Matrix  the compute shader variables.
            //         rayTracingShader.SetMatrix("_Projection", Camera.main.projectionMatrix);
            //         rayTracingShader.SetMatrix("_CameraInverseProjection", Camera.main.projectionMatrix.inverse);
            //     }
            //     else
            //     {
            //         #region detach logic test                    
            //         // // 1. Create the Camera Transform                
            //         // float4x4 ViewTransform_OpenCV = new float4x4(new float4(m_cameraExternalData.xAxis, 0),
            //         //                                              new float4(m_cameraExternalData.yAxis, 0),
            //         //                                              new float4(m_cameraExternalData.zAxis, 0),
            //         //                                              new float4(m_cameraExternalData.projectorPosition * 0.001f, 1)
            //         //                                              );
            //         // // Debug.Log($"ViewTransform =\n{  ViewTransform_OpenCV }");

            //         // float3x3 ViewTransform_Rot_OpenCV = new float3x3(
            //         //     ViewTransform_OpenCV.c0.xyz, ViewTransform_OpenCV.c1.xyz, ViewTransform_OpenCV.c2.xyz);

            //         // float3 ViewTransform_Trans_OpenCV = ViewTransform_OpenCV.c3.xyz;


            //         // float3x3 CameraTransformation_Rot_OpenCV = math.transpose(ViewTransform_Rot_OpenCV);


            //         // // float4x4 CameraTransformation_OpenCV = new float4x4(CameraTransformation_Rot_OpenCV,
            //         // //                                         -math.mul(CameraTransformation_Rot_OpenCV, ViewTransform_Trans_OpenCV));

            //         // float4x4 CameraTransformation_OpenCV = new float4x4(new float4(CameraTransformation_Rot_OpenCV.c0, 0.0f),
            //         //                                                     new float4(CameraTransformation_Rot_OpenCV.c1, 0.0f),
            //         //                                                     new float4(CameraTransformation_Rot_OpenCV.c2, 0.0f),
            //         //                                                     new float4(-math.mul(CameraTransformation_Rot_OpenCV, ViewTransform_Trans_OpenCV), 1.0f));

            //         // // Debug.Log($"CameraTransformation_OpenCV (obtained by transpose) =\n{ CameraTransformation_OpenCV }");


            //         // float4x4 CameraTransform_OpenCV = math.inverse(ViewTransform_OpenCV);
            //         // // Debug.Log($" CameraTransform_OpenCV (obtained by inverse)=\n{  CameraTransform_OpenCV }");

            //         // // https://stackoverflow.com/questions/1263072/changing-a-matrix-from-right-handed-to-left-handed-coordinate-system

            //         // // UnityToOpenMat is a change of basis matrix, a swap of axes, with a determinmant -1, which is
            //         // // improper rotation, and so a well-defined quaternion does not exist for it.

            //         // float4 externalData_column0 = new float4(DanbiCameraExternalData.UnityToOpenCVMat.c0, 0);
            //         // float4 externalData_column1 = new float4(DanbiCameraExternalData.UnityToOpenCVMat.c1, 0);
            //         // float4 externalData_column2 = new float4(DanbiCameraExternalData.UnityToOpenCVMat.c2, 0);
            //         // float4 externalData_column3 = new float4(0, 0, 0, 1);


            //         // float4x4 UnityToOpenCV = new float4x4(externalData_column0, externalData_column1, externalData_column2, externalData_column3);

            //         // float3x3 UnityToOpenCV_Rot = new float3x3(UnityToOpenCV.c0.xyz,
            //         //                                           UnityToOpenCV.c1.xyz,
            //         //                                           UnityToOpenCV.c2.xyz);

            //         // float3x3 OpenCVToUnity_Rot = math.transpose(UnityToOpenCV_Rot);

            //         // float3 UnityToOpenCV_Trans = UnityToOpenCV.c3.xyz;

            //         // float4x4 OpenCVToUnity = new float4x4(OpenCVToUnity_Rot, -math.mul(OpenCVToUnity_Rot, UnityToOpenCV_Trans));

            //         // // Debug.Log($" UnityToOpenCV inverse = \n {math.inverse(UnityToOpenCV)} ");

            //         // // Debug.Log($" UnityToOpenCV transpose  = \n {OpenCVToUnity}");

            //         // float4x4 MatForObjectFrame = new float4x4(
            //         //                             new float4(1, 0, 0, 0),
            //         //                             new float4(0, 0, 1, 0),
            //         //                             new float4(0, -1, 0, 0),
            //         //                             new float4(0, 0, 0, 1));


            //         // float4x4 CameraTransform_Unity = math.mul(
            //         //                                      math.mul(
            //         //                                          math.mul(UnityToOpenCV,
            //         //                                                   CameraTransformation_OpenCV
            //         //                                             ),
            //         //                                          OpenCVToUnity),
            //         //                                      MatForObjectFrame
            //         //                                      );

            //         // Matrix4x4 m_cameraTransform_Unity_Mat4x4 = CameraTransform_Unity;
            //         // // Debug.Log($"Determinimant of m_cameraTransform_Unity_Mat4x4=\n{m_cameraTransform_Unity_Mat4x4.determinant}");

            //         // // 2. Set the Camera.main.transform.


            //         // // Debug.Log($"Quaternion = m_cameraTransform_Unity_Mat4x4.rotation=  \n {m_cameraTransform_Unity_Mat4x4.rotation}");
            //         // // Debug.Log($"QuaternionFromMatrix(MatForUnityCameraFrameMat4x4)\n{DanbiComputeShaderHelper.QuaternionFromMatrix(m_cameraTransform_Unity_Mat4x4)}");

            //         // // Camera.main.gameObject.transform.position = new Vector3(0.0f, m_cameraExternalData.projectorPosition.z * 0.001f, 0.0f); // m_cameraHeight -> cm
            //         // Vector3 m_unityCamPos = DanbiComputeShaderHelper.GetPosition(m_cameraTransform_Unity_Mat4x4);
            //         // m_unityCamPos.x = 0.0f;
            //         // m_unityCamPos.z = 0.0f;

            //         // Camera.main.gameObject.transform.position = m_unityCamPos;
            //         // Camera.main.gameObject.transform.rotation = DanbiComputeShaderHelper.GetRotation(m_cameraTransform_Unity_Mat4x4);

            //         // // Debug.Log($"m_cameraTransform_Unity_Mat4x4= \n {m_cameraTransform_Unity_Mat4x4}");

            //         // // Debug.Log($"localToWorldMatrix =\n{ Camera.main.gameObject.transform.localToWorldMatrix}, " +
            //         // //     $"\nQuaternion Mat4x4: { Camera.main.gameObject.transform.rotation}, " +
            //         // //     $"\neulerAngles ={ Camera.main.gameObject.transform.eulerAngles}");

            //         // // Debug.Log($"m_cameraTransform_Unity_Mat4x4= \n {m_cameraTransform_Unity_Mat4x4}");


            //         // // Debug.Log($"localToWorldMatrix =\n{ Camera.main.gameObject.transform.localToWorldMatrix}, " +
            //         // //     $"\nQuaternion Mat4x4: { Camera.main.gameObject.transform.rotation}, " +
            //         // //     $"\neulerAngles ={ Camera.main.gameObject.transform.eulerAngles}");

            //         // // 3. Set the transforms related to the camera to the compute shader variables. 
            //         // rayTracingShader.SetMatrix("_CameraToWorldMat", Camera.main.cameraToWorldMatrix);
            //         // Vector4 cameraDirection = new Vector4(Camera.main.transform.forward.x, Camera.main.transform.forward.y,
            //         //                                       Camera.main.transform.forward.z, 0f);
            //         // rayTracingShader.SetVector("_CameraViewDirectionInUnitySpace", Camera.main.transform.forward);

            //         // // 4. Create the Projection Matrix
            //         // float width = DanbiManager.instance.screen.screenResolution.x; // width = 3840 =  Projector Width
            //         // float height = DanbiManager.instance.screen.screenResolution.y; // height = 2160 = Projector Height

            //         // float left = 0;
            //         // float right = width;
            //         // float bottom = 0;
            //         // float top = height;

            //         // float near = Camera.main.nearClipPlane;      // near: positive
            //         // float far = Camera.main.farClipPlane;        // far: positive

            //         // float aspectRatio = width / height;

            //         // float scaleFactorX = m_cameraInternalData.focalLengthX;
            //         // float scaleFactorY = m_cameraInternalData.focalLengthY;

            //         // float cx = m_cameraInternalData.principalPointX;
            //         // float cy = m_cameraInternalData.principalPointY;

            //         // // http://ksimek.github.io/2013/06/03/calibrated_cameras_in_opengl/
            //         // //we can think of the perspective transformation as converting 
            //         // // a trapezoidal-prism - shaped viewing volume
            //         // //    into a rectangular - prism - shaped viewing volume,
            //         // //    which glOrtho() scales and translates into the 2x2x2 cube in Normalized Device Coordinates.

            //         // Matrix4x4 NDCMatrix_OpenGL = DanbiComputeShaderHelper.GetOrthoMat(left, right, bottom, top, near, far);

            //         // //  // refer to to   //http://ksimek.github.io/2012/08/14/decompose/  to  understand the following code
            //         // Matrix4x4 KMatrixFromOpenCVToOpenGL =
            //         //     DanbiComputeShaderHelper.OpenCVKMatrixToOpenGLKMatrix(scaleFactorX, scaleFactorY, cx, cy, near, far);

            //         // // our discussion of 2D coordinate conventions have referred to the coordinates used during calibration.
            //         // // If your application uses a different 2D coordinate convention,
            //         // //  you'll need to transform K using 2D translation and reflection.

            //         // //  For example, consider a camera matrix that was calibrated with the origin in the top-left 
            //         // //  and the y - axis pointing downward, but you prefer a bottom-left origin with the y-axis pointing upward.
            //         // //  To convert, you'll first negate the image y-coordinate and then translate upward by the image height, h. 
            //         // //  The resulting intrinsic matrix K' is given by:

            //         // // K' = [ 1 0 0; 0 1 h; 0 0 1] *  [ 1 0 0; 0 -1 0; 0 0 1] * K


            //         // Vector4 column0 = new Vector4(1f, 0f, 0f, 0f);
            //         // Vector4 column1 = new Vector4(0f, -1f, 0f, 0f);
            //         // Vector4 column2 = new Vector4(0f, 0f, 1f, 0f);
            //         // Vector4 column3 = new Vector4(0f, height, 0f, 1f);

            //         // Matrix4x4 OpenCVCameraToOpenGLCamera = new Matrix4x4(column0, column1, column2, column3);

            //         // Matrix4x4 projectionMatrixGL1 = NDCMatrix_OpenGL * OpenCVCameraToOpenGLCamera * KMatrixFromOpenCVToOpenGL;

            //         // Debug.Log($"Reconstructed projection matrix, method 1:\n{projectionMatrixGL1}");

            //         // 5. Set the projection matrix to the compute shader variables.

            //         #endregion detach logic test

            //         // rayTracingShader.SetMatrix("_Projection", projectionMatrixGL1);
            //         rayTracingShader.SetMatrix("_Projection", Camera.main.projectionMatrix);
            //         rayTracingShader.SetMatrix("_CameraInverseProjection", Camera.main.projectionMatrix.inverse);

            //         #region dbg
            //         // float4x4 tmpCameraToWorld = Camera.main.cameraToWorldMatrix;
            //         // Debug.Log($"Camera World To Mat");
            //         // Debug.Log($"c0 : {tmpCameraToWorld.c0.x}, {tmpCameraToWorld.c0.y}, {tmpCameraToWorld.c0.z}, {tmpCameraToWorld.c0.w}");
            //         // Debug.Log($"c1 : {tmpCameraToWorld.c1.x}, {tmpCameraToWorld.c1.y}, {tmpCameraToWorld.c1.z}, {tmpCameraToWorld.c1.w}");
            //         // Debug.Log($"c2 : {tmpCameraToWorld.c2.x}, {tmpCameraToWorld.c2.y}, {tmpCameraToWorld.c2.z}, {tmpCameraToWorld.c2.w}");
            //         // Debug.Log($"c3 : {tmpCameraToWorld.c3.x}, {tmpCameraToWorld.c3.y}, {tmpCameraToWorld.c3.z}, {tmpCameraToWorld.c3.w}");

            //         // float4x4 tmpCameraInverseProjection = projectionMatrixGL1.inverse;
            //         // Debug.Log($"Camera Inverse Projection");
            //         // Debug.Log($"c0 : {tmpCameraInverseProjection.c0.x}, {tmpCameraInverseProjection.c0.y}, {tmpCameraInverseProjection.c0.z}, {tmpCameraInverseProjection.c0.w}");
            //         // Debug.Log($"c1 : {tmpCameraInverseProjection.c1.x}, {tmpCameraInverseProjection.c1.y}, {tmpCameraInverseProjection.c1.z}, {tmpCameraInverseProjection.c1.w}");
            //         // Debug.Log($"c2 : {tmpCameraInverseProjection.c2.x}, {tmpCameraInverseProjection.c2.y}, {tmpCameraInverseProjection.c2.z}, {tmpCameraInverseProjection.c2.w}");
            //         // Debug.Log($"c3 : {tmpCameraInverseProjection.c3.x}, {tmpCameraInverseProjection.c3.y}, {tmpCameraInverseProjection.c3.z}, {tmpCameraInverseProjection.c3.w}");
            //         #endregion dbg
            //     }

            #endregion test unused        
        } // SetCameraParameters()
    };   // class DanbiCameraControl
};   //   namespace Danbi
