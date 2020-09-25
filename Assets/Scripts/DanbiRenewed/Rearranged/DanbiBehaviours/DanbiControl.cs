using System.Collections.Generic;

using UnityEngine;

namespace Danbi
{
    [RequireComponent(typeof(DanbiScreen),
                      typeof(DanbiComputeShaderControl),
                      typeof(DanbiCameraControl))]
    public sealed class DanbiControl : MonoBehaviour
    {
        #region Exposed
        /// <summary>
        /// When this is true, then current renderTexture is transferred into the frame buffer.  
        /// </summary>
        [Readonly, SerializeField]
        bool bStopRender = false;

        /// <summary>
        /// When this is true, then the current RenderTexture is used for render.
        /// </summary>    
        [Readonly, SerializeField]
        bool bDistortionReady = false;

        /// <summary>
        /// All render actions which requires performance is stopped.
        /// </summary>
        [Readonly, SerializeField]
        bool bCaptureFinished = false;

        [SerializeField, Readonly]
        Texture2D TargetPanoramaTex;

        [Readonly, SerializeField, Header("Current State of Simulator."), Space(20)]
        EDanbiSimulatorMode SimulatorMode = EDanbiSimulatorMode.PREPARE;

        #endregion Exposed

        #region Internal

        /// <summary>
        /// Everything about Shader goes here.
        /// </summary>
        DanbiComputeShaderControl ShaderControl;

        /// <summary>
        /// Result Screen Info.
        /// </summary>
        DanbiScreen Screen;
        //public Texture2D targetPanoramaTex => TargetPanoramaTex;

        /// <summary>
        /// used to raytracing to create an pre-distorted image and to project the distorted image onto the scene
        /// </summary>
        Camera MainCameraCache;

        #endregion Internal

        #region Delegates

        public delegate void OnGenerateImage();
        public static OnGenerateImage Call_OnGenerateImage;

        public delegate void OnGenerateImageFinished();
        public static OnGenerateImageFinished Call_OnGenerateImageFinished;

        public delegate void OnSaveImage();
        public static OnSaveImage Call_OnSaveImage;

        public delegate void OnGenerateVideo();
        public static OnGenerateVideo Call_OnGenerateVideo;

        public delegate void OnGenerateVideoFinished();
        public static OnGenerateVideoFinished Call_OnGenerateVideoFinished;

        public delegate void OnSaveVideo();
        public static OnSaveVideo Call_OnSaveVideo;

        public static void UnityEvent_CreatePredistortedImage() => Call_OnGenerateImage?.Invoke();

        public static void UnityEvent_OnRenderFinished() => Call_OnGenerateImageFinished?.Invoke();

        public static void UnityEvent_SaveImageAt(string path/* = Not used*/) => Call_OnSaveImage?.Invoke();

        #endregion Delegates

        void OnReset()
        {
            bStopRender = false;
            bDistortionReady = false;
            bCaptureFinished = false;
            SimulatorMode = EDanbiSimulatorMode.PREPARE;
        }

        void OnValidate()
        {
            /**/
        }

        void Start()
        {
            // 1. Acquire resources.
            Screen = GetComponent<DanbiScreen>();
            MainCameraCache = Camera.main;
            ShaderControl = GetComponent<DanbiComputeShaderControl>();

            DanbiImage.ScreenResolutions = Screen.screenResolution;
            DanbiDisableMeshFilterProps.DisableAllUnnecessaryMeshRendererProps();
            // 2. bind the call backs.      
            DanbiControl.Call_OnGenerateImage += Caller_OnGenerateImage;
            DanbiControl.Call_OnGenerateImageFinished += Caller_OnGenerateImageFinished;
            DanbiControl.Call_OnSaveImage += Caller_OnSaveImage;

            DanbiControl.Call_OnGenerateVideo += Caller_OnGenerateVideo;
            DanbiControl.Call_OnGenerateVideoFinished += Caller_OnGenerateVideoFinished;
            DanbiControl.Call_OnSaveVideo += Caller_OnSaveVideo;

            DanbiUISync.Call_OnPanelUpdate += OnPanelUpdate;
        }

        void OnDisable()
        {
            // Dispose buffer resources.
            DanbiControl.Call_OnGenerateImage -= Caller_OnGenerateImage;
            DanbiControl.Call_OnGenerateImageFinished -= Caller_OnGenerateImageFinished;
            DanbiControl.Call_OnSaveImage -= Caller_OnSaveImage;
            DanbiControl.Call_OnGenerateVideo -= Caller_OnGenerateVideo;
            DanbiControl.Call_OnGenerateVideoFinished -= Caller_OnGenerateVideoFinished;
            DanbiControl.Call_OnSaveVideo -= Caller_OnSaveVideo;
            DanbiUISync.Call_OnPanelUpdate -= OnPanelUpdate;
        }

        void OnPanelUpdate(DanbiUIPanelControl control)
        {
            if (control is DanbiUIImageGeneratorParametersPanelControl)
            {
                var imagePanel = control as DanbiUIImageGeneratorParametersPanelControl;
                TargetPanoramaTex = imagePanel.targetTex;
            }

            if (control is DanbiUIVideoGeneratorParametersPanelControl)
            {
                var videoPanel = control as DanbiUIVideoGeneratorParametersPanelControl;
                // TargetPanoramaTex = videoPanel;
            }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            switch (SimulatorMode)
            {
                case EDanbiSimulatorMode.PREPARE:
                    Graphics.Blit(Camera.main.activeTexture, destination);
                    break;

                case EDanbiSimulatorMode.CAPTURE:
                    // bStopRender is already true, but the result isn't saved yet (by button).
                    // 
                    // so we stop updating rendering but keep the screen with the result for preventing performance issue.          
                    if (bDistortionReady)
                    {
                        Graphics.Blit(ShaderControl.resultRT_LowRes, destination);
                    }
                    else
                    {
                        // 1. Calculate the resolution-wise thread size from the current screen resolution.
                        //    and Dispatch.
                        ShaderControl.Dispatch((Mathf.CeilToInt(Screen.screenResolution.x * 0.125f), Mathf.CeilToInt(Screen.screenResolution.y * 0.125f)),
                                               destination);
                    }
                    break;

                default:
                    Debug.LogError($"Other Value {SimulatorMode} isn't used in this context.", this);
                    break;
            }
        }

        #region Binded Caller    
        void Caller_OnGenerateImage()
        {
            bStopRender = false;
            // var ui = DanbiUIControl.instance.PanelControlDic[DanbiUIPanelKey.ImageGeneratorParameters];
            // var tex = (ui as DanbiUIImageGeneratorParametersPanelControl).targetTex;
            ShaderControl.MakePredistortedImage(null,
                                                (Screen.screenResolution.x, Screen.screenResolution.y),
                                                MainCameraCache);
            SimulatorMode = EDanbiSimulatorMode.CAPTURE;
        }

        void Caller_OnGenerateImageFinished()
        {
            bDistortionReady = true;
            SimulatorMode = EDanbiSimulatorMode.PREPARE;
            // TODO:
        }

        void Caller_OnSaveImage()
        {
            bStopRender = true;
            bDistortionReady = true;
            SimulatorMode = EDanbiSimulatorMode.PREPARE;
            // TODO:
        }

        void Caller_OnGenerateVideo()
        {
            bStopRender = false;
            SimulatorMode = EDanbiSimulatorMode.CAPTURE;
        }

        void Caller_OnGenerateVideoFinished()
        {
            bDistortionReady = true;
            SimulatorMode = EDanbiSimulatorMode.PREPARE;
        }

        void Caller_OnSaveVideo()
        {
            bStopRender = true;
            bDistortionReady = true;
            SimulatorMode = EDanbiSimulatorMode.PREPARE;
        }
        #endregion Binded Caller
    };
};
