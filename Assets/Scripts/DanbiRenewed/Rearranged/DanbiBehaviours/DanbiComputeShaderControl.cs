using System;
using System.Collections;
using System.Collections.Generic;

using UnityEditor;

using UnityEngine;

using ComputeBuffersDic = System.Collections.Generic.Dictionary<string, UnityEngine.ComputeBuffer>;

namespace Danbi
{
    [System.Serializable]
    public class DanbiComputeShaderControl : MonoBehaviour
    {
        #region Exposed           

        [SerializeField, Header("2 by default for the best performance")]
        int MaxNumOfBounce = 2;

        [SerializeField]
        uint SamplingThreshold = 30u;

        #endregion Exposed

        #region Internal
        [SerializeField]
        ComputeShader rayTracingShader;

        Material AddMaterial_ScreenSampling;

        uint SamplingCounter;

        public RenderTexture resultRT_LowRes { get; set; }

        public RenderTexture convergedResultRT_HiRes { get; set; }

        public ComputeBuffersDic buffersDic { get; } = new ComputeBuffersDic();

        public delegate void OnValueChanged();
        public static OnValueChanged Call_OnValueChanged;

        public delegate void OnShaderParamsUpdated();
        public static OnShaderParamsUpdated Call_OnShaderParamsUpdated;

        #endregion Internal    

        #region Event Functions

        void Awake()
        {
            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogError("This machine doesn't support Compute Shader!", this);
            }
            //rayTracingShader = DanbiComputeShaderHelper.CreateComputeShader("DanbiMain");

            // 1. Initialize the Screen Sampling shader.
            AddMaterial_ScreenSampling = new Material(Shader.Find("Hidden/AddShader"));

            // 2. Bind the delegates.
            Call_OnValueChanged += PrepareMeshesAsComputeBuffer;
            Call_OnShaderParamsUpdated += SetShaderParams;
        }

        void Reset()
        {
            MaxNumOfBounce = 2;
            SamplingThreshold = 30;
        }

        void Start()
        {
            PrepareMeshesAsComputeBuffer();
        }

        void OnDisable()
        {
            Call_OnValueChanged -= PrepareMeshesAsComputeBuffer;
            Call_OnShaderParamsUpdated -= SetShaderParams;
        }

        #endregion Event Functions

        #region Behaviours

        void PrepareMeshesAsComputeBuffer()
        {
            // Rebuild Prerequisites.

            var prewarper = FindObjectsOfType<DanbiPrewarperSetting>();
            foreach (var i in prewarper)
            {
                i.Call_OnMeshRebuild?.Invoke(this);
            }
            // DanbiPrewarperSetting.Call_OnMeshRebuild?.Invoke(this);
        }

        void SetShaderParams()
        {
            rayTracingShader.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
        }

        public void MakePredistortedImage(Texture2D target, (int x, int y) screenResolutions, Camera mainCamRef)
        {
            // 01. Prepare RenderTextures.
            DanbiComputeShaderHelper.PrepareRenderTextures(screenResolutions,
                                                           out SamplingCounter,
                                                           resultRT_LowRes,
                                                           convergedResultRT_HiRes);

            // 02. Prepare the current kernel for connecting Compute Shader.
            int currentKernel = DanbiKernelHelper.CurrentKernelIndex;
            // Set DanbiOpticalData, DanbiShapeTransform as MeshAdditionalData into the compute shader.
            rayTracingShader.SetBuffer(currentKernel, "_MeshAdditionalData", buffersDic["_MeshAdditionalData"]);
            rayTracingShader.SetInt("_MaxBounce", MaxNumOfBounce);
            rayTracingShader.SetBuffer(currentKernel, "_Vertices", buffersDic["_Vertices"]);
            rayTracingShader.SetBuffer(currentKernel, "_Indices", buffersDic["_Indices"]);
            rayTracingShader.SetBuffer(currentKernel, "_Texcoords", buffersDic["_Texcoords"]);


            // 03. Prepare the translation matrices.
            // TODO: How you will notice that shader's using simulator mode?
            CreateProjectionMatrix(screenResolutions, mainCamRef);

            // 04. Textures.
            DanbiComputeShaderHelper.ClearRenderTexture(resultRT_LowRes);
            rayTracingShader.SetTexture(currentKernel, "_Result", resultRT_LowRes);
            rayTracingShader.SetTexture(currentKernel, "_PanoramaImage", target);
        }

        public void MakePredistortedVideo(Texture2D target, (int x, int y) screenResolutions, Camera mainCamRef)
        {
            // TODO:
        }

        void CreateProjectionMatrix((int width, int height) screenResolutions, Camera mainCamRef)
        {
            var cameraControlRef = GetComponent<DanbiCameraControl>();

            bool useCameraProjection = cameraControlRef?.useCalibration ?? false;

            rayTracingShader.SetMatrix("_CameraToWorldMat", mainCamRef.cameraToWorldMatrix);

            if (!useCameraProjection)
            {
                rayTracingShader.SetMatrix("_Projection", mainCamRef.projectionMatrix);
                rayTracingShader.SetMatrix("_CameraInverseProjection", mainCamRef.projectionMatrix.inverse);
            }
            else
            {
                float left = 0.0f;
                float right = screenResolutions.width;
                float bottom = 0.0f;
                float top = screenResolutions.height;
                float near = mainCamRef.nearClipPlane;
                float far = mainCamRef.farClipPlane;

                var openGL_NDC_KMat = DanbiComputeShaderHelper.GetOpenGL_KMatrix(left, right, bottom, top, near, far);

                DanbiCameraExternalData cameraExternalData = cameraControlRef.cameraExternalData;
                var openCV_NDC_KMat = DanbiComputeShaderHelper.GetOpenCV_KMatrix(cameraExternalData.FocalLength.x,
                                                                                 cameraExternalData.FocalLength.y,
                                                                                 cameraExternalData.PrincipalPoint.x,
                                                                                 cameraExternalData.PrincipalPoint.y,
                                                                                 near, far);
                var projMat = openGL_NDC_KMat * openCV_NDC_KMat;
                rayTracingShader.SetMatrix("_Projection", projMat);
                rayTracingShader.SetMatrix("_CameraInverseProjection", projMat.inverse);
                // TODO: Need to decide how we choose the way how we un-distort.

                rayTracingShader.SetBuffer(DanbiKernelHelper.CurrentKernelIndex, "_DanbiCameraExternalData", buffersDic["_DanbiCameraExternalData"]);
                //RTShader.SetVector("_ThresholdIterative", new Vector2())
                //RTShader.SetInt("_IterativeSafeCounter", );
                //RTShader.SetVector("_ThresholdNewTonIterative", );
            }
        }

        public void Dispatch((int x, int y) threadGroups, RenderTexture dest)
        {
            SetShaderParams();
            // 01. Check the ray tracing shader is valid.
            if (rayTracingShader.Null())
            {
                Debug.LogError("Ray-tracing shader is invalid!", this);
            }

            // 02. Dispatch with the current kernel.
            rayTracingShader.Dispatch(DanbiKernelHelper.CurrentKernelIndex, threadGroups.x, threadGroups.y, 1);
            // 03. Check Screen Sampler and apply it.      
            AddMaterial_ScreenSampling.SetFloat("_Sample", SamplingCounter);
            // 04. Sample the result into the ConvergedResultRT to improve the aliasing quality.
            Graphics.Blit(resultRT_LowRes, convergedResultRT_HiRes, AddMaterial_ScreenSampling);
            // 05. To improve the resolution of the result RenderTextue, we upscale it in float precision.
            Graphics.Blit(convergedResultRT_HiRes, dest);
            // 06. Update the sample counts.
            ++SamplingCounter;
            if (SamplingCounter > SamplingThreshold)
            {
                DanbiControl.Call_OnGenerateImageFinished?.Invoke();
                SamplingCounter = 0;
            }
        }
        #endregion Behaviours

    }; // class ending.
}; // namespace Danbi
