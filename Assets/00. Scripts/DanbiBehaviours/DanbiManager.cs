using System.Collections.Generic;
using System.Collections;

using UnityEngine;

namespace Danbi
{
    public sealed class DanbiManager : SingletonAsComponent<DanbiManager>
    {
        [SerializeField, Readonly, Space(5)]
        EDanbiSimulatorMode m_simulatorMode = EDanbiSimulatorMode.Prepare;

        public EDanbiSimulatorMode simulatorMode { get => m_simulatorMode; set => m_simulatorMode = value; }

        /// <summary>
        /// Enabled after clicking the image/video generating button
        /// </summary>
        [SerializeField, Readonly]
        bool m_renderFinished;
        public bool renderFinished { get => m_renderFinished; private set => m_renderFinished = value; }

        [SerializeField, Readonly]
        DanbiComputeShaderControl m_shaderControl;
        public DanbiComputeShaderControl shaderControl => m_shaderControl;

        [SerializeField, Readonly]
        DanbiImageControl m_imageControl;
        public DanbiImageControl imageControl => m_imageControl;

        // [SerializeField, Readonly]
        // DanbiVideoControl m_videoControl;
        // public DanbiVideoControl videoControl => m_videoControl;

        [SerializeField, Readonly]
        DanbiOpencvVideoWriter m_videoControl;
        public DanbiOpencvVideoWriter videoControl => m_videoControl;

        [SerializeField, Readonly]
        DanbiScreen m_screen;
        public DanbiScreen screen => m_screen;

        [SerializeField, Readonly]
        DanbiProjectorControl m_projectorControl;
        public DanbiProjectorControl projectorControl => m_projectorControl;

        /// <summary>
        /// Called on generating image.
        /// </summary>
        /// <param name="overridingTex">if it's not null, using this instead!</param>
        public delegate void OnGenerateImage(Texture2D overridingTex = default(Texture2D));
        public OnGenerateImage onGenerateImage;

        /// <summary>
        /// Called on saving image.
        /// </summary>
        public delegate void OnSaveImage();
        public OnSaveImage onSaveImage;

        /// <summary>
        /// 
        /// </summary>
        public delegate void OnGenerateVideo(TMPro.TMP_Text progressDisplay, TMPro.TMP_Text statusDisplay);
        public OnGenerateVideo onGenerateVideo;

        void Awake()
        {
#if UNITY_EDITOR
            // Turn off unnecessary MeshRenderer settings.
            DanbiDisableMeshFilterProps.DisableAllUnnecessaryMeshRendererProps();
#endif
            // 1. Acquire resources.
            m_screen = FindObjectOfType<DanbiScreen>();
            m_shaderControl = FindObjectOfType<DanbiComputeShaderControl>();
            m_imageControl = FindObjectOfType<DanbiImageControl>();
            // m_videoControl = FindObjectOfType<DanbiVideoControl>();
            m_videoControl = FindObjectOfType<DanbiOpencvVideoWriter>();
            m_projectorControl = FindObjectOfType<DanbiProjectorControl>();

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

        void SetResourcesToShader(Texture2D overridingTex = default)
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
            DanbiFileSys.SaveImage(m_simulatorMode,
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

            m_simulatorMode = EDanbiSimulatorMode.Render;
            StartCoroutine(m_videoControl.MakeVideo(progressDisplay, statusDisplay));
            // m_videoControl.StartMakingVideo(progressDisplay, statusDisplay);
        }
    };
};
