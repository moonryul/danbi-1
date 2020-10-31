using System.Collections.Generic;
using System.Collections;

using UnityEngine;

namespace Danbi
{
    [RequireComponent(typeof(DanbiScreen),
                      typeof(DanbiComputeShaderControl),
                      typeof(DanbiCameraControl))]
    public sealed class DanbiControl : MonoBehaviour
    {
        // /// <summary>
        // /// When this is true, then current renderTexture is transferred into the frame buffer.  
        // /// </summary>
        // [Readonly, SerializeField]
        // bool bStopRender = false;

        /// <summary>
        /// When this is true, then the current RenderTexture is used for render.
        /// </summary>    
        [SerializeField, Readonly]
        bool isImageRendered = false; //16.36976 9.2079

        // [Readonly, SerializeField]
        // bool isRenderStarted = false;

        [SerializeField, Readonly]
        Texture2D PanoramaTexture;

        [Readonly, SerializeField, Header("Current State of Simulator."), Space(20)]
        EDanbiSimulatorMode SimulatorMode = EDanbiSimulatorMode.PREPARE;

        EDanbiImageType imageType = EDanbiImageType.png;
        EDanbiVideoType videoType = EDanbiVideoType.mp4;

        DanbiComputeShaderControl ShaderControl;
        DanbiVideoControl VideoControl;

        DanbiScreen Screen;

        [SerializeField, Readonly]
        DanbiProjectorControl Projector;

        // [SerializeField] KinectSensorManager 
        [SerializeField, Readonly]
        string imageFileSavePathAndName;
        string imageFilePath;

        [SerializeField, Readonly]
        string videoFileSavePathAndName;
        string videoFilePath;


        /// <summary>
        /// Called on generating image.
        /// </summary>
        /// <param name="overridingTex">if it's not null, using this instead!</param>
        public delegate void OnGenerateImage(Texture2D overridingTex);
        public static OnGenerateImage Call_OnGenerateImage;

        /// <summary>
        /// Called on saving image.
        /// </summary>
        public delegate void OnSaveImage();
        public static OnSaveImage Call_OnSaveImage;

        /// <summary>
        /// Called on changing simlator mode.
        /// </summary>
        /// <param name="mode"></param>
        public delegate void OnChangeSimulatorMode(EDanbiSimulatorMode mode);
        public static OnChangeSimulatorMode Call_OnChangeSimulatorMode;

        /// <summary>
        /// Called on Changing state for image rendered.
        /// </summary>
        /// <param name="isRendered"></param>
        public delegate void OnImageRendered(bool isRendered);
        public static OnImageRendered Call_OnImageRendered;

        public delegate void OnImageRenderedForVideoFrame(RenderTexture res);
        /// <summary>
        /// => m_distortedRT = converged_resultRT;
        /// </summary>
        public static OnImageRenderedForVideoFrame Call_OnImageRenderedForVideoFrame;

        /// <summary>
        /// 
        /// </summary>
        public delegate void OnGenerateVideo(TMPro.TMP_Text progressDisplay, TMPro.TMP_Text statusDisplay);
        public static OnGenerateVideo Call_OnGenerateVideo;
        /// <summary>
        /// 
        /// </summary>
        public delegate void OnSaveVideo(string ffmpegExecutableLocation);
        public static OnSaveVideo Call_OnSaveVideo;

        // public static void UnityEvent_CreatePredistortedImage()
        //     => Call_OnGenerateImage?.Invoke();

        // public static void UnityEvent_SaveImageAt(string path/* = Not used*/)
        //     => Call_OnSaveImage?.Invoke();

        void Awake()
        {
#if UNITY_EDITOR
            // Turn off unnecessary MeshRenderer settings.
            DanbiDisableMeshFilterProps.DisableAllUnnecessaryMeshRendererProps();
#endif
            // 1. Acquire resources.
            Screen = GetComponent<DanbiScreen>();
            ShaderControl = GetComponent<DanbiComputeShaderControl>();
            VideoControl = GetComponent<DanbiVideoControl>();
            Projector = transform.parent.GetComponentInChildren<DanbiProjectorControl>();

            // 2. bind the delegates.      
            Call_OnGenerateImage += Caller_OnGenerateImage;
            Call_OnSaveImage += Caller_OnSaveImage;

            // Bind OnChangeSimulatorMode
            Call_OnChangeSimulatorMode +=
                (EDanbiSimulatorMode mode) => SimulatorMode = mode;
            // Bind OnImageRendered
            Call_OnImageRendered +=
                (bool isRendered) => isImageRendered = isRendered;

            Call_OnGenerateVideo += Caller_OnGenerateVideo;
            Call_OnSaveVideo += Caller_OnSaveVideo;

            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdate;
        }

        void OnDisable()
        {
            // 1. unbind the delegates.
            Call_OnGenerateImage -= Caller_OnGenerateImage;
            Call_OnSaveImage -= Caller_OnSaveImage;

            Call_OnGenerateVideo -= Caller_OnGenerateVideo;
            Call_OnSaveVideo -= Caller_OnSaveVideo;

            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdate;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIImageGeneratorTexturePanelControl)
            {
                var texturePanel = control as DanbiUIImageGeneratorTexturePanelControl;
                // Debug.Log($"Texture is loaded!", this);
                PanoramaTexture = texturePanel.loadedTex;
            }

            // if (control is DanbiUIVideoGeneratorParametersPanelControl)
            // {
            //     //var videoPanel = control as DanbiUIVideoGeneratorParametersPanelControl;
            //     // TargetPanoramaTex = videoPanel;                
            // }

            // Update image file paths
            if (control is DanbiUIImageGeneratorFilePathPanelControl)
            {
                var fileSavePanel = control as DanbiUIImageGeneratorFilePathPanelControl;
                imageFileSavePathAndName = fileSavePanel.fileSavePathAndName;
                imageFilePath = fileSavePanel.filePath;
                imageType = fileSavePanel.imageType;
            }


            // Update video file paths
            if (control is DanbiUIVideoGeneratorFileSavePathPanelControl)
            {
                var fileSavePanel = control as DanbiUIVideoGeneratorFileSavePathPanelControl;
                videoFileSavePathAndName = fileSavePanel.fileSavePathAndName;
                videoFilePath = fileSavePanel.filePath;
                videoType = fileSavePanel.videoExt;
            }

            DanbiComputeShaderControl.Call_OnSettingChanged?.Invoke();
        }

        void Caller_OnGenerateImage(Texture2D overridingTex)
        {
            var usedTex = overridingTex.Null() ? PanoramaTexture : overridingTex;
            // 1. prepare prerequisites
            if (Screen.screenResolution.x != 0.0f && Screen.screenResolution.y != 0.0f)
            {
                ShaderControl.SetBuffersAndRenderTextures(usedTex, (Screen.screenResolution.x, Screen.screenResolution.y));
            }
            else
            {
                // TODO: User must decide the screen resolution.
                // notice to UI
                ShaderControl.SetBuffersAndRenderTextures(usedTex, (2560, 1440));
            }

            // 2. change the states from PREPARE to CAPTURE           
            Call_OnChangeSimulatorMode?.Invoke(EDanbiSimulatorMode.CAPTURE);
            Projector.PrepareResources(ShaderControl, Screen);
        }

        void Caller_OnSaveImage()
        {
            Call_OnImageRendered?.Invoke(true);
            DanbiFileSys.SaveImage(SimulatorMode,
                                   imageType,
                                   ShaderControl.m_convergedResultRT_HiRes,
                                   imageFileSavePathAndName,
                                   imageFilePath,
                                   (Screen.screenResolution.x, Screen.screenResolution.y));
            Call_OnChangeSimulatorMode?.Invoke(EDanbiSimulatorMode.PREPARE);
            // TODO:
        }

        void Caller_OnGenerateVideo(TMPro.TMP_Text progressDisplay, TMPro.TMP_Text statusDisplay)
        {
            // bStopRender = false;
            // bDistortionReady = false;

            SimulatorMode = EDanbiSimulatorMode.CAPTURE;
            StartCoroutine(VideoControl.StartProcessVideo(progressDisplay, statusDisplay));
        }

        void Caller_OnSaveVideo(string ffmpegExecutableLocation)
        {
            // bStopRender = true;
            // bDistortionReady = true;
            SimulatorMode = EDanbiSimulatorMode.PREPARE;

        }
    };
};
