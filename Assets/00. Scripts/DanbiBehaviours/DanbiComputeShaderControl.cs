using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using ComputeBuffersDict = System.Collections.Generic.Dictionary<string, UnityEngine.ComputeBuffer>;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiComputeShaderControl : MonoBehaviour
    {
        [SerializeField, Readonly, Space(5)]
        int MaxNumOfBounce = 2;

        [SerializeField, Readonly]
        int m_SamplingThreshold = 30;

        [SerializeField]
        ComputeShader m_danbiShader;
        public ComputeShader danbiShader => m_danbiShader;

        [SerializeField, Readonly]
        int m_isPanoramaTex;

        [SerializeField, Readonly]
        Vector4 m_centerOfPanoramaMesh;

        Material m_addMaterial_ScreenSampling;

        int m_SamplingCounter;

        [SerializeField]
        RenderTexture m_resultRT_LowRes;
        public RenderTexture resultRT_LowRes => m_resultRT_LowRes;

        [SerializeField]
        RenderTexture m_convergedRT_HiRes;
        public RenderTexture convergedResultRT_HiRes => m_convergedRT_HiRes;

        public ComputeBuffersDict buffersDict { get; } = new ComputeBuffersDict();
        readonly System.DateTime seedDateTime = new System.DateTime();

        public delegate void OnSampleFinished(RenderTexture sampledRenderTex);
        public static event OnSampleFinished onSampleFinished;


        // ComputeBuffer dbg_centerOfPanoBuf;
        // Vector4 dbg_centerOfPanoArr = new Vector4(0.0f, 0.0f, 0.0f, 0.0f);

        // ComputeBuffer dbg_rayLengthBuf;
        // Vector3 dbg_rayLengthArr = new Vector3();

        // ComputeBuffer dbg_hitInfoBuf;
        // Vector4 dbg_hitInfoArr = new Vector4();

        void Awake()
        {
            // query the hardward it supports the compute shader.
            if (!SystemInfo.supportsComputeShaders)
            {
                Debug.LogError("This machine doesn't support Compute Shader!", this);
            }

            // Initialize the Screen Sampling shader.
            m_addMaterial_ScreenSampling = new Material(Shader.Find("Hidden/AddShader"));

            // Bind the delegates.
            DanbiUISync.onPanelUpdated += OnPanelUpdate;
            DanbiPanoramaScreenTexMapper.onCenterPosOfMeshUpdate_Panorama +=
                (Vector3 newCenterOfPanoramaMesh) =>
                {
                    m_centerOfPanoramaMesh = new Vector4(newCenterOfPanoramaMesh.x,
                                                         newCenterOfPanoramaMesh.y,
                                                         newCenterOfPanoramaMesh.z,
                                                         0.0f);
                };

            // Populate kernels index.
            PopulateKernels();

            // DanbiDbg.PrepareDbgBuffers();
            // SetData is performed automatically when the buffer is created.
            // dbg_centerOfPanoBuf = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(dbg_centerOfPanoArr, 16);
            // dbg_rayLengthBuf = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(dbg_rayLengthArr, 12);
            // dbg_hitInfoBuf = DanbiComputeShaderHelper.CreateComputeBuffer_Ret(dbg_hitInfoArr, 16);
        }

        void Start()
        {
            // 1. start with building meshes as compute buffers.
            PrepareMeshesAsComputeBuffer();
        }

        void Update()
        {
            SetShaderParams();

            // dbg_centerOfPanoBuf.GetData(arr);
            // foreach (var i in arr)
            // {
            //     Debug.Log($"{i.x}, {i.y}, {i.z}");
            // }
            // if (Input.GetKeyDown(KeyCode.D))
            // {
            //     // var arr = new Vector3[1];
            //     // dbg_rayLengthBuf.GetData(arr);
            //     var arr = new Vector4[1];
            //     dbg_hitInfoBuf.GetData(arr);
            //     foreach (var i in arr)
            //     {
            //         Debug.Log($"{i.x}, {i.y}, {i.z}");
            //     }
            // }
        }

        void PopulateKernels()
        {
            DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Dome_Reflector_Cube_Panorama,
                danbiShader.FindKernel("Dome_Reflector_Cube_Panorama"));
            // DanbiKernelHelper.AddKernalIndexWithKey(EDanbiKernelKey.Dome_Reflector_Cylinder_Panorama,
            //     rayTracingShader.FindKernel("Dome_Reflector_Cylinder_Panorama"));

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

            if (control is DanbiUIImageGeneratorTexturePanelControl)
            {
                var texControl = control as DanbiUIImageGeneratorTexturePanelControl;
                m_isPanoramaTex = (int)texControl.textureType;
                // Debug.Log($"Using panorama tex : {m_isPanoramaTex}");
            }

            if (control is DanbiUIImageGeneratorParametersPanelControl)
            {
                var imageGeneratorParamPanel = control as DanbiUIImageGeneratorParametersPanelControl;

                MaxNumOfBounce = imageGeneratorParamPanel.maxBoundCount;
                m_SamplingThreshold = imageGeneratorParamPanel.samplingThreshold;
                return;
            }

            if (control is DanbiUIVideoGeneratorVideoPanelControl)
            {
                var vidControl = control as DanbiUIVideoGeneratorVideoPanelControl;
                m_isPanoramaTex = (int)vidControl.vidType;
            }

            if (control is DanbiUIVideoGeneratorParametersPanelControl)
            {
                var videoGeneratorParamPanel = control as DanbiUIVideoGeneratorParametersPanelControl;

                MaxNumOfBounce = videoGeneratorParamPanel.maxBoundCount;
                m_SamplingThreshold = videoGeneratorParamPanel.samplingThreshold;
                return;
            }
        }
        public void PrepareMeshesAsComputeBuffer()
        {
            DanbiPrewarperSetting.onPrepareShaderData?.Invoke(this);
        }

        void SetShaderParams()
        {
            Random.InitState(seedDateTime.Millisecond);
            danbiShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
            danbiShader.SetInt("_isPanoramaTex", m_isPanoramaTex);
            danbiShader.SetInt("_MaxBounce", MaxNumOfBounce);
            // 03. Prepare the translation matrices.
            if (Camera.main.transform.hasChanged)
            {
                DanbiCameraControl.onSetCameraBuffers?.Invoke((DanbiManager.instance.screen.screenResolution.x, DanbiManager.instance.screen.screenResolution.y), this);
            }
        }

        public void SetBuffersAndRenderTextures(Texture2D panoramaImage, (int x, int y) screenResolutions)
        {
            // 01. Prepare RenderTextures.
            DanbiComputeShaderHelper.PrepareRenderTextures(screenResolutions,
                                                           out m_SamplingCounter,
                                                           ref m_resultRT_LowRes,
                                                           ref m_convergedRT_HiRes);

            // 02. Prepare the current kernel for connecting Compute Shader.                    
            int currentKernel = DanbiKernelHelper.CurrentKernelIndex;

            // danbiShader.SetBuffer(currentKernel, "dbg_centerOfPano", dbg_centerOfPanoBuf);
            // danbiShader.SetBuffer(currentKernel, "dbg_rayLengthBuf", dbg_rayLengthBuf);
            // danbiShader.SetBuffer(currentKernel, "dbg_hitInfoBuf", dbg_hitInfoBuf);

            // Set the other parameters as buffer into the ray tracing compute shader.
            danbiShader.SetBuffer(currentKernel, "_DomeData", buffersDict["_DomeData"]);
            danbiShader.SetBuffer(currentKernel, "_PanoramaData", buffersDict["_PanoramaData"]);
            danbiShader.SetBuffer(currentKernel, "_Vertices", buffersDict["_Vertices"]);
            danbiShader.SetBuffer(currentKernel, "_Indices", buffersDict["_Indices"]);
            danbiShader.SetBuffer(currentKernel, "_Texcoords", buffersDict["_Texcoords"]);

            // 04. Textures.
            // DanbiComputeShaderHelper.ClearRenderTexture(resultRT_LowRes);
            danbiShader.SetTexture(currentKernel, "_DistortedImage", resultRT_LowRes);

            // Panorama image params.
            DanbiCameraControl.onSetCameraBuffers?.Invoke((DanbiManager.instance.screen.screenResolution.x, DanbiManager.instance.screen.screenResolution.y), this);

            // danbiShader.SetBuffer(currentKernel, "dbg_centerOfPanoBuf", dbg_centerOfPanoBuf);

            danbiShader.SetTexture(currentKernel, "_PanoramaImage", panoramaImage);

            // rayTracingShader.SetBuffer(currentKernel, "_Dbg_direct", DanbiDbg.Dbg   Buf_direct);

            m_SamplingCounter = 0;
        }

        public void Dispatch((int x, int y) threadGroups, RenderTexture dest)
        {
            // Dispatch with the current kernel.
            danbiShader.Dispatch(DanbiKernelHelper.CurrentKernelIndex, threadGroups.x, threadGroups.y, 1);

            // Check Screen Sampler and apply it.      
            m_addMaterial_ScreenSampling.SetFloat("_SampleCount", m_SamplingCounter);

            // Sample the result into the ConvergedResultRT to improve the aliasing quality.
            Graphics.Blit(resultRT_LowRes, convergedResultRT_HiRes, m_addMaterial_ScreenSampling);
            // Upscale float precisions to improve the resolution of the result RenderTextue and blit to dest rendertexture.
            Graphics.Blit(convergedResultRT_HiRes, dest);

            // Update the sample counts.
            if (m_SamplingCounter++ > m_SamplingThreshold)
            {
                m_SamplingCounter = 0;
                // TODO: Only called for video.                
                onSampleFinished?.Invoke(convergedResultRT_HiRes);
            }
        }
    };
};
