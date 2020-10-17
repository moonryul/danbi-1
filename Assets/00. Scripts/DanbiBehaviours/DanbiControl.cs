using System.Collections.Generic;

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

        EDanbiImageType ImageType = EDanbiImageType.png;

        DanbiComputeShaderControl ShaderControl;

        DanbiScreen Screen;

        [SerializeField, Readonly]
        DanbiProjectorControl Projector;

        // [SerializeField] KinectSensorManager 
        [SerializeField, Readonly]
        string fileSavePathAndName;
        string filePath;


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
        public delegate void OnChangeImageRendered(bool isRendered);
        public static OnChangeImageRendered Call_OnChangeImageRendered;

        /// <summary>
        /// 
        /// </summary>
        public delegate void OnGenerateVideo();
        public static OnGenerateVideo Call_OnGenerateVideo;
        /// <summary>
        /// 
        /// </summary>
        public delegate void OnSaveVideo();
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
            Projector = transform.parent.GetComponentInChildren<DanbiProjectorControl>();

            // 2. bind the delegates.      
            Call_OnGenerateImage += Caller_OnGenerateImage;
            Call_OnSaveImage += Caller_OnSaveImage;

            Call_OnChangeSimulatorMode +=
                (EDanbiSimulatorMode mode) => SimulatorMode = mode;
            Call_OnChangeImageRendered +=
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
                Debug.Log($"Texture is loaded!", this);
                PanoramaTexture = texturePanel.loadedTex;
            }

            // if (control is DanbiUIVideoGeneratorParametersPanelControl)
            // {
            //     //var videoPanel = control as DanbiUIVideoGeneratorParametersPanelControl;
            //     // TargetPanoramaTex = videoPanel;                
            // }

            if (control is DanbiUIImageGeneratorFilePathPanelControl)
            {
                var fileSavePanel = control as DanbiUIImageGeneratorFilePathPanelControl;
                // Debug.Log($"Previous File save path is loaded!", this);
                fileSavePathAndName = fileSavePanel.fileSavePathAndName;
                filePath = fileSavePanel.filePath;
                ImageType = fileSavePanel.imageType;
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
                ShaderControl.SetBuffersAndRenderTextures(usedTex, (2560, 1440));
            }

            // 2. change the states
            Call_OnChangeImageRendered?.Invoke(false);
            Call_OnChangeSimulatorMode?.Invoke(EDanbiSimulatorMode.CAPTURE);
            Projector.PrepareResources(ShaderControl, Screen);
        }

        void Caller_OnSaveImage()
        {
            Call_OnChangeImageRendered?.Invoke(true);
            DanbiFileSys.SaveImage(SimulatorMode,
                                   ImageType,
                                   ShaderControl.convergedResultRT_HiRes,
                                   fileSavePathAndName,
                                   filePath,
                                   (Screen.screenResolution.x, Screen.screenResolution.y));
            Call_OnChangeSimulatorMode?.Invoke(EDanbiSimulatorMode.PREPARE);
            // TODO:
        }

        void Caller_OnGenerateVideo()
        {
            bStopRender = false;
            bDistortionReady = false;
            SimulatorMode = EDanbiSimulatorMode.CAPTURE;
        }

        void Caller_OnSaveVideo()
        {
            bStopRender = true;
            bDistortionReady = true;
            SimulatorMode = EDanbiSimulatorMode.PREPARE;
        }
    };
};
