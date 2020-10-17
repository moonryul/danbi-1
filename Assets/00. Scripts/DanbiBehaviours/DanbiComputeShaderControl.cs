using UnityEngine;

using ComputeBuffersDic = System.Collections.Generic.Dictionary<string, UnityEngine.ComputeBuffer>;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiComputeShaderControl : MonoBehaviour
    {
        [SerializeField, Header("2 by default for the best performance"), Readonly]
        int MaxNumOfBounce = 2;

        [SerializeField, Readonly]
        int SamplingThreshold = 30;

        public ComputeShader rayTracingShader;

        Material addMaterial_ScreenSampling;

        int SamplingCounter;

        DanbiCameraControl CameraControl;
        DanbiCameraControl cameraControl
        {
            get
            {
                CameraControl.NullFinally(() => CameraControl = GetComponent<DanbiCameraControl>());
                return CameraControl;
            }
        }

        public RenderTexture resultRT_LowRes;

        public RenderTexture convergedResultRT_HiRes;

        public ComputeBuffersDic buffersDic { get; } = new ComputeBuffersDic();

        public delegate void OnValueChanged();
        public static OnValueChanged Call_OnSettingChanged;
        readonly System.DateTime seedDateTime = new System.DateTime();

        void Awake()
        {
            // 1. query the hardward it supports the compute shader.
            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogError("This machine doesn't support Compute Shader!", this);
            }

            // 2. Find Compute Shader in case that it's not assigned.
            if (rayTracingShader.Null())
            {
                rayTracingShader = DanbiComputeShaderHelper.FindComputeShader("DanbiMain");
            }

            // 3. Initialize the Screen Sampling shader.
            addMaterial_ScreenSampling = new Material(Shader.Find("Hidden/AddShader"));

            // 4. Bind the delegates.
            Call_OnSettingChanged += PrepareMeshesAsComputeBuffer;
            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdate;

            // 5. Populate kernels index.
            PopulateKernels();
        }

        void Start()
        {
            // 1. start with building meshes as compute buffers.
            PrepareMeshesAsComputeBuffer();
        }

        void OnDisable()
        {
            // 1. unbind the delegates.
            Call_OnSettingChanged -= PrepareMeshesAsComputeBuffer;
            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdate;

            // 2. release all the compute buffers.
            foreach (var it in buffersDic)
            {
                it.Value.Release();
            }
        }

        void Update()
        {
            SetShaderParams();
        }

        void PopulateKernels()
        {
            DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Dome_Reflector_Cube_Panorama,
                rayTracingShader.FindKernel("Dome_Reflector_Cube_Panorama"));
            DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Dome_Reflector_Cylinder_Panorama,
                rayTracingShader.FindKernel("Dome_Reflector_Cylinder_Panorama"));

            // foreach (var k in DanbiKernelHelper.KernalDic)
            // {
            //     Debug.Log($"Kernel key {k.Key} -> {k.Value}", this);
            // }
            // DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Halfsphere_Reflector_Cylinder_Panorama, rayTracingShader.FindKernel("Halfsphere_Reflector_Cylinder_Panorama"));
            // DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Cone_Reflector_Cube_Panorama, rayTracingShader.FindKernel("Cone_Reflector_Cube_Panorama"));
            // DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Cone_Reflector_Cylinder_Panorama, rayTracingShader.FindKernel("Cone_Reflector_Cylinder_Panorama"));
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            PrepareMeshesAsComputeBuffer();
            
            if (control is DanbiUIImageGeneratorParametersPanelControl)
            {
                var imageGeneratorParamPanel = control as DanbiUIImageGeneratorParametersPanelControl;

                MaxNumOfBounce = imageGeneratorParamPanel.maxBoundCount;
                SamplingThreshold = imageGeneratorParamPanel.samplingThreshold;
                return;
            }

            // if (control is DanbiUIVideoGeneratorParametersPanelControl)
            // {
            //     var videoGeneratorParamPanel = control as DanbiUIVideoGeneratorParametersPanelControl;
            //     MaxNumOfBounce = videoGeneratorParamPanel.MaximumBoundCount;
            //     SamplingThreshold = videoGeneratorParamPanel.SamplingThreshold;
            //     return;
            // }
        }
        void PrepareMeshesAsComputeBuffer() => DanbiPrewarperSetting.Call_OnPreparePrerequisites?.Invoke(this);

        void SetShaderParams()
        {
            Random.InitState(seedDateTime.Millisecond);
            rayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
        }

        public void SetBuffersAndRenderTextures(Texture2D panoramaImage, (int x, int y) screenResolutions)
        {
            // 01. Prepare RenderTextures.
            DanbiComputeShaderHelper.PrepareRenderTextures(screenResolutions,
                                                           out SamplingCounter,
                                                           ref resultRT_LowRes,
                                                           ref convergedResultRT_HiRes);

            // 02. Prepare the current kernel for connecting Compute Shader.                    
            int currentKernel = DanbiKernelHelper.CurrentKernelIndex;

            // Set the other parameters.
            rayTracingShader.SetBuffer(currentKernel, "_DomeData", buffersDic["_DomeData"]);
            rayTracingShader.SetBuffer(currentKernel, "_PanoramaData", buffersDic["_PanoramaData"]);
            rayTracingShader.SetInt("_MaxBounce", MaxNumOfBounce);
            rayTracingShader.SetBuffer(currentKernel, "_Vertices", buffersDic["_Vertices"]);
            rayTracingShader.SetBuffer(currentKernel, "_Indices", buffersDic["_Indices"]);
            rayTracingShader.SetBuffer(currentKernel, "_Texcoords", buffersDic["_Texcoords"]);

            // 03. Prepare the translation matrices.
            DanbiCameraControl.Call_OnSetCameraBuffers?.Invoke(screenResolutions, this);

            // 04. Textures.
            DanbiComputeShaderHelper.ClearRenderTexture(resultRT_LowRes);
            rayTracingShader.SetTexture(currentKernel, "_DistortedImage", resultRT_LowRes);
            rayTracingShader.SetTexture(currentKernel, "_PanoramaImage", panoramaImage);

            SamplingCounter = 0;
        }

        // public void MakePredistortedVideo(Texture2D target, (int x, int y) screenResolutions, Camera mainCamRef)
        // {
        //     // TODO: fill the body
        // }

        public void Dispatch((int x, int y) threadGroups, RenderTexture dest)
        {
            // 01. Check the ray tracing shader is valid.
            if (rayTracingShader.Null())
            {
                Debug.LogError("Ray-tracing shader is invalid!", this);
            }

            // 02. Dispatch with the current kernel.
            rayTracingShader.Dispatch(DanbiKernelHelper.CurrentKernelIndex, threadGroups.x, threadGroups.y, 1);

            // 03. Check Screen Sampler and apply it.      
            addMaterial_ScreenSampling.SetFloat("_SampleCount", SamplingCounter);

            // 04. Sample the result into the ConvergedResultRT to improve the aliasing quality.
            Graphics.Blit(resultRT_LowRes, convergedResultRT_HiRes, addMaterial_ScreenSampling);

            // 05. Upscale float precisions to improve the resolution of the result RenderTextue and blit to dest rendertexture.
            Graphics.Blit(convergedResultRT_HiRes, dest);

            // 06. Update the sample counts.
            ++SamplingCounter;
            if (SamplingCounter > SamplingThreshold)
            {
                DanbiControl.Call_OnChangeImageRendered?.Invoke(true);
                SamplingCounter = 0;
            }
        }
    }; // class ending.
}; // namespace Danbi
