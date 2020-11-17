using System.Collections.Generic;
using System.Collections;

using UnityEngine;

namespace Danbi
{
    public sealed class DanbiManager : SingletonAsComponent<DanbiManager>
    {
        [SerializeField, Readonly, Space(5)]
        EDanbiSimulatorMode m_simulatorMode = EDanbiSimulatorMode.Prepare;

        public EDanbiSimulatorMode prevSimulatorMode { get; set; }
        public EDanbiSimulatorMode simulatorMode { get => m_simulatorMode; set => m_simulatorMode = value; }

        /// <summary>
        /// Enabled after clicking the image/video generating button
        /// </summary>
        [SerializeField, Readonly]
        public bool m_distortedImageRenderStarted = false;
        public bool m_distortedImageRenderFinished = false; // **MOON**
        public bool renderStarted { get => m_distortedImageRenderStarted; private set => m_distortedImageRenderStarted = value; }
        public bool renderFinished { get => m_distortedImageRenderFinished; private set => m_distortedImageRenderFinished = value; }

        [SerializeField, Readonly]
        Camera m_projectorCamera;

        public GameObject videoDisplay;

        [SerializeField, Readonly]
        DanbiCameraControl m_cameraControl;
        public DanbiCameraControl cameraControl => m_cameraControl;


        [SerializeField, Readonly]
        DanbiComputeShaderControl m_shaderControl;
        public DanbiComputeShaderControl shaderControl => m_shaderControl;

        [SerializeField, Readonly]
        DanbiImageWriter m_imageWriter;

        // [SerializeField, Readonly]
        // DanbiVideoControl m_videoControl;
        // public DanbiVideoControl videoControl => m_videoControl;

        [SerializeField, Readonly]
        DanbiOpencvVideoWriter m_videoControl;

        [SerializeField, Readonly]
        DanbiScreen m_screen;
        public DanbiScreen screen => m_screen;

        [SerializeField, Readonly]
        DanbiProjectorControl m_projectorControl;

        void Awake()
        {
#if UNITY_EDITOR
            // Turn off unnecessary MeshRenderer settings.
            DanbiDisableMeshFilterProps.DisableAllUnnecessaryMeshRendererProps();
#endif
            // 1. Acquire resources.
            m_screen = FindObjectOfType<DanbiScreen>();
            m_shaderControl = FindObjectOfType<DanbiComputeShaderControl>();
            m_imageWriter = FindObjectOfType<DanbiImageWriter>();
            // m_videoControl = FindObjectOfType<DanbiVideoControl>();
            m_videoControl = FindObjectOfType<DanbiOpencvVideoWriter>();

            m_projectorControl = FindObjectOfType<DanbiProjectorControl>();

            m_cameraControl = FindObjectOfType<DanbiCameraControl>();

            m_projectorCamera = m_projectorControl.GetComponent<Camera>();
            videoDisplay = GameObject.Find("Video Display");
        }
        /// <summary>
        /// Set Resources for rendering.
        /// </summary>
        /// <param name="videoInputTex">if it's null, then panoramaTex from the UI Panel is used.</param>
        public void GenerateImage(TMPro.TMP_Text statusDisplay, Texture2D videoInputTex = null)
        {
            // statusDisplay.NullFinally(() => DanbiUtils.LogErr("no status display for generating image detected!"));
            
            List<Texture2D> usedTexList = new List<Texture2D>();

            if (videoInputTex != null)
            {
                usedTexList.Add(videoInputTex); // length -> 1
            }
            else
            {
                usedTexList.AddRange(m_imageWriter.tex); // length depends on the count of texture selection on UI panel.
            }            

            // 1. Set all the buffers and textures needed for  the compute shader.            
            m_shaderControl.SetBuffersAndRenderTextures(usedTexList, (m_screen.screenResolution.x, m_screen.screenResolution.y));

            // 2. change the states from PREPARE to CAPTURE           
            m_simulatorMode = EDanbiSimulatorMode.Render;
            m_distortedImageRenderStarted = true;
        }

        public void SaveImage()
        {
            DanbiFileSys.SaveImage(m_simulatorMode,
                                   m_imageWriter.imageType,
                                   m_shaderControl.convergedResultRT_HiRes,
                                   m_imageWriter.imageSavePathAndName,
                                   m_imageWriter.imageSavePathOnly,
                                   (m_screen.screenResolution.x, m_screen.screenResolution.y));

            m_simulatorMode = EDanbiSimulatorMode.Prepare;
            m_distortedImageRenderStarted = false;
        }

        public void GenerateVideo(TMPro.TMP_Text progressDisplay, TMPro.TMP_Text statusDisplay)
        {
            progressDisplay.NullFinally(() => DanbiUtils.LogErr("no process display for generating video detected!"));
            statusDisplay.NullFinally(() => DanbiUtils.LogErr("no status display for generating video detected!"));

            m_simulatorMode = EDanbiSimulatorMode.Render;
            StartCoroutine(m_videoControl.MakeVideo(progressDisplay, statusDisplay));
            // m_videoControl.MakeVideo(progressDisplay, statusDisplay);
        }
    };
};
