using System.Collections.Generic;
using System.Collections;

using UnityEngine;

namespace Danbi
{
    public sealed class DanbiManager : MonoBehaviour
    {
        [Readonly, SerializeField, Header("Current State of Simulator."), Space(5)]
        EDanbiSimulatorMode SimulatorMode = EDanbiSimulatorMode.Prepare;

        public EDanbiSimulatorMode simulatorMode { get => SimulatorMode; set => SimulatorMode = value; }

        /// <summary>
        /// Enabled after clicking the image/video generating button
        /// </summary>
        [SerializeField, Readonly]
        bool RenderFinished;
        public bool renderFinished { get => RenderFinished; private set => RenderFinished = value; }

        DanbiComputeShaderControl m_shaderControl;
        DanbiImageControl m_imageControl;
        DanbiVideoControl m_videoControl;
        DanbiScreen m_screen;

        /// <summary>
        /// Called on generating image.
        /// </summary>
        /// <param name="overridingTex">if it's not null, using this instead!</param>
        public delegate void OnGenerateImage(Texture2D overridingTex);
        public static OnGenerateImage onGenerateImage;

        /// <summary>
        /// Called on saving image.
        /// </summary>
        public delegate void OnSaveImage();
        public static OnSaveImage onSaveImage;

        /// <summary>
        /// 
        /// </summary>
        public delegate void OnGenerateVideo(TMPro.TMP_Text progressDisplay, TMPro.TMP_Text statusDisplay);
        public static OnGenerateVideo onGenerateVideo;

        void Awake()
        {
#if UNITY_EDITOR
            // Turn off unnecessary MeshRenderer settings.
            DanbiDisableMeshFilterProps.DisableAllUnnecessaryMeshRendererProps();
#endif
            // 1. Acquire resources.
            m_screen = GetComponent<DanbiScreen>();
            m_shaderControl = GetComponent<DanbiComputeShaderControl>();
            m_imageControl = GetComponent<DanbiImageControl>();
            m_videoControl = GetComponent<DanbiVideoControl>();

            // 2. bind the delegates.      
            onGenerateImage += SetResourcesToShader;
            onSaveImage += SaveImage;
            onGenerateVideo += StartGenerateVideo;
        }

        void OnDisable()
        {
            // 1. unbind the delegates.
            onGenerateImage -= SetResourcesToShader;
            onSaveImage -= SaveImage;
            onGenerateVideo -= StartGenerateVideo;
        }

        void SetResourcesToShader(Texture2D overridingTex)
        {
            var usedTex = overridingTex ?? m_imageControl.panoramaTex;
            // 1. prepare prerequisites
            if (m_screen.screenResolution.x != 0.0f && m_screen.screenResolution.y != 0.0f)
            {
                m_shaderControl.SetBuffersAndRenderTextures(usedTex, (m_screen.screenResolution.x, m_screen.screenResolution.y));
            }
            else
            {
                // TODO: User must decide the screen resolution.
                // notice to UI
                m_shaderControl.SetBuffersAndRenderTextures(usedTex, (2560, 1440));
            }

            // 2. change the states from PREPARE to CAPTURE           
            simulatorMode = EDanbiSimulatorMode.Render;
            renderFinished = true;
        }

        void SaveImage()
        {
            DanbiFileSys.SaveImage(SimulatorMode,
                                   m_imageControl.imageType,
                                   m_shaderControl.convergedResultRT_HiRes,
                                   m_imageControl.imageSavePathAndName,
                                   m_imageControl.imageSavePathOnly,
                                   (m_screen.screenResolution.x, m_screen.screenResolution.y));

            simulatorMode = EDanbiSimulatorMode.Prepare;
            renderFinished = false;
        }

        void StartGenerateVideo(TMPro.TMP_Text progressDisplay, TMPro.TMP_Text statusDisplay)
        {
            // bStopRender = false;
            // bDistortionReady = false;

            SimulatorMode = EDanbiSimulatorMode.Render;
            m_videoControl.StartMakingVideo(progressDisplay, statusDisplay);
        }
    };
};
