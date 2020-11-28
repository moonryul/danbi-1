using System.Collections.Generic;
using System.Collections;

using UnityEngine;

namespace Danbi
{
    public sealed class DanbiManager : SingletonAsComponent<DanbiManager>
    {
        /// <summary>
        /// Enabled after clicking the image/video generating button
        /// </summary>
        [SerializeField, Readonly]
        public bool m_distortedImageRenderStarted = false;
        public bool m_distortedImageRenderFinished = false; // **MOON**

        public bool renderStarted { get => m_distortedImageRenderStarted; set => m_distortedImageRenderStarted = value; }
        public bool renderFinished { get => m_distortedImageRenderFinished; set => m_distortedImageRenderFinished = value; }

        public GameObject m_videoDisplay;

        [SerializeField, Readonly]
        DanbiCamera m_cameraControl;
        public DanbiCamera cameraControl => m_cameraControl;


        [SerializeField, Readonly]
        DanbiComputeShader m_shaderControl;
        public DanbiComputeShader shaderControl => m_shaderControl;

        [SerializeField, Readonly]
        DanbiImageWriter m_imageWriter;

        // [SerializeField, Readonly]
        // DanbiVideoControl m_videoControl;
        // public DanbiVideoControl videoControl => m_videoControl;

        [SerializeField, Readonly]
        DanbiOpencvVideoWriter m_videoWriter;

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
            m_shaderControl = FindObjectOfType<DanbiComputeShader>();
            m_imageWriter = FindObjectOfType<DanbiImageWriter>();
            m_videoWriter = FindObjectOfType<DanbiOpencvVideoWriter>();
            m_projectorControl = FindObjectOfType<DanbiProjectorControl>();
            m_cameraControl = FindObjectOfType<DanbiCamera>();

            m_videoDisplay = GameObject.Find("Video Display");
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
            m_distortedImageRenderStarted = true;
            m_distortedImageRenderFinished = false;
        }

        public void SaveImage()
        {
            DanbiFileSys.SaveImage(m_imageWriter.imageExt,
                                   m_shaderControl.convergedResultRT_HiRes,
                                   m_imageWriter.imageSavePathAndName,
                                   m_imageWriter.imageSavePathOnly,
                                   (m_screen.screenResolution.x, m_screen.screenResolution.y));
        }

        public void GenerateVideo(TMPro.TMP_Text progressDisplay, TMPro.TMP_Text statusDisplay)
        {
            progressDisplay.NullFinally(() => DanbiUtils.LogErr("no process display for generating video detected!"));
            statusDisplay.NullFinally(() => DanbiUtils.LogErr("no status display for generating video detected!"));

            m_distortedImageRenderStarted = true;
            m_distortedImageRenderFinished = false;
            StartCoroutine(m_videoWriter.MakeVideo(progressDisplay, statusDisplay));
            // m_videoControl.MakeVideo(progressDisplay, statusDisplay);
        }
    };
};
