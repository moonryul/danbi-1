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

        [SerializeField, Header("2 by default for the best performance"), Readonly]
        int MaxNumOfBounce = 2;

        [SerializeField, Readonly]
        int SamplingThreshold = 30;

        #endregion Exposed

        #region Internal
        [SerializeField]
        ComputeShader rayTracingShader;

        Material AddMaterial_ScreenSampling;

        int SamplingCounter;

        public RenderTexture resultRT_LowRes;

        public RenderTexture convergedResultRT_HiRes;

        public ComputeBuffersDic buffersDic { get; } = new ComputeBuffersDic();

        public delegate void OnValueChanged();
        public static OnValueChanged Call_OnValueChanged;

        public delegate void OnShaderParamsUpdated();
        public static OnShaderParamsUpdated Call_OnShaderParamsUpdated;

        #endregion Internal    

        #region Event Functions

        void Awake()
        {
            // 1. query the hardward it supports the compute shader.
            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogError("This machine doesn't support Compute Shader!", this);
            }

            // 2. Find Compute Shader if it's not assigned.
            if (rayTracingShader.Null())
            {
                rayTracingShader = DanbiComputeShaderHelper.FindComputeShader("DanbiMain");
            }

            // 3. Initialize the Screen Sampling shader.
            AddMaterial_ScreenSampling = new Material(Shader.Find("Hidden/AddShader"));

            // 4. Bind the delegates.
            Call_OnValueChanged += PrepareMeshesAsComputeBuffer;
            Call_OnShaderParamsUpdated += SetShaderParams;
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdate;

            // 5. Populate kernels index.
            PopulateKernels();
        }

        void PopulateKernels()
        {
            DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Halfsphere_Reflector_Cube_Panorama, rayTracingShader.FindKernel("Halfsphere_Reflector_Cube_Panorama"));
            // DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Halfsphere_Reflector_Cylinder_Panorama, rayTracingShader.FindKernel("Halfsphere_Reflector_Cylinder_Panorama"));
            // DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Cone_Reflector_Cube_Panorama, rayTracingShader.FindKernel("Cone_Reflector_Cube_Panorama"));
            // DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Cone_Reflector_Cylinder_Panorama, rayTracingShader.FindKernel("Cone_Reflector_Cylinder_Panorama"));
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIImageGeneratorParametersPanelControl)
            {
                var imageGeneratorParamPanel = control as DanbiUIImageGeneratorParametersPanelControl;
                MaxNumOfBounce = imageGeneratorParamPanel.maxBoundCount;
                SamplingThreshold = imageGeneratorParamPanel.samplingThreshold;
                return;
            }

            if (control is DanbiUIVideoGeneratorParametersPanelControl)
            {
                var videoGeneratorParamPanel = control as DanbiUIVideoGeneratorParametersPanelControl;
                MaxNumOfBounce = videoGeneratorParamPanel.MaximumBoundCount;
                SamplingThreshold = videoGeneratorParamPanel.SamplingThreshold;
                return;
            }
        }

        void Start()
        {
            PrepareMeshesAsComputeBuffer();
        }

        void OnDisable()
        {
            Call_OnValueChanged -= PrepareMeshesAsComputeBuffer;
            Call_OnShaderParamsUpdated -= SetShaderParams;
            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdate;
        }

        #endregion Event Functions

        #region Behaviours

        void PrepareMeshesAsComputeBuffer()
        {
            // Rebuild Prerequisites for the each prewarper settings.
            var prewarper = FindObjectsOfType<DanbiPrewarperSetting>();
            foreach (var i in prewarper)
            {
                i.Call_OnMeshRebuild?.Invoke(this);
            }
        }

        void SetShaderParams()
        {
            rayTracingShader.SetVector("_PixelOffset", new Vector2(UnityEngine.Random.value, UnityEngine.Random.value));
        }

        public void MakePredistortedImage(Texture2D target,
                                          (int x, int y) screenResolutions,
                                          Camera mainCamRef)
        {
            // 01. Prepare RenderTextures.
            DanbiComputeShaderHelper.PrepareRenderTextures(screenResolutions,
                                                           out SamplingCounter,
                                                           ref resultRT_LowRes,
                                                           ref convergedResultRT_HiRes);

            // 02. Prepare the current kernel for connecting Compute Shader.                    
            int currentKernel = DanbiKernelHelper.CurrentKernelIndex;

            // Set the other parameters.
            rayTracingShader.SetBuffer(currentKernel, "_HalfsphereData", buffersDic["_HalfsphereData"]);
            rayTracingShader.SetBuffer(currentKernel, "_PanoramaData", buffersDic["_PanoramaData"]);
            rayTracingShader.SetInt("_MaxBounce", MaxNumOfBounce);
            rayTracingShader.SetBuffer(currentKernel, "_Vertices", buffersDic["_Vertices"]);
            rayTracingShader.SetBuffer(currentKernel, "_Indices", buffersDic["_Indices"]);
            rayTracingShader.SetBuffer(currentKernel, "_Texcoords", buffersDic["_Texcoords"]);

            // 03. Prepare the translation matrices.
            CreateProjectionMatrix(screenResolutions, mainCamRef);

            // 04. Textures.
            // DanbiComputeShaderHelper.ClearRenderTexture(resultRT_LowRes);            
            rayTracingShader.SetTexture(currentKernel, "_DistortedImage", resultRT_LowRes);
            rayTracingShader.SetTexture(currentKernel, "_PanoramaImage", target);
        }

        public void MakePredistortedVideo(Texture2D target, (int x, int y) screenResolutions, Camera mainCamRef)
        {
            // TODO: fill the body
        }

        void CreateProjectionMatrix((int width, int height) screenResolutions, Camera mainCamRef)
        {
            var cameraControlRef = GetComponent<DanbiCameraControl>();

            bool useCalibratedCamera = cameraControlRef?.useCalibration ?? false;

            rayTracingShader.SetMatrix("_CameraToWorldMat", mainCamRef.cameraToWorldMatrix);

            if (!useCalibratedCamera)
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

                var cameraExternalData = cameraControlRef.CameraInternalData;
                var openCV_NDC_KMat = DanbiComputeShaderHelper.GetOpenCV_KMatrix(cameraExternalData.focalLengthX,
                                                                                 cameraExternalData.focalLengthY,
                                                                                 cameraExternalData.principalPointX,
                                                                                 cameraExternalData.principalPointY,
                                                                                 near, far);
                var projMat = openGL_NDC_KMat * openCV_NDC_KMat;
                rayTracingShader.SetMatrix("_Projection", projMat);
                rayTracingShader.SetMatrix("_CameraInverseProjection", projMat.inverse);
                // TODO: Need to decide how we choose the way how we un-distort.

                // rayTracingShader.SetBuffer(DanbiKernelHelper.CurrentKernelIndex, "_DanbiCameraExternalData", buffersDic["_DanbiCameraExternalData"]);
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
                // TODO;
                // DanbiControl.Call_OnGenerateImageFinished?.Invoke();
                SamplingCounter = 0;
            }
        }
        #endregion Behaviours

    }; // class ending.
}; // namespace Danbi
