using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiProjectorControl : MonoBehaviour
    {
        [SerializeField, Readonly]
        EDanbiProjectorMode m_projectionMode;
        public EDanbiProjectorMode projectionMode { get => m_projectionMode; set => m_projectionMode = value; }

        Texture2D m_projectImage;
        public RenderTexture m_projectImageRT;

        Camera thisCam;

        void Awake()
        {

            thisCam = GetComponent<Camera>();
            DanbiUIProjectionImagePanelControl.onProjectionImageUpdate +=

            (Texture2D tex) =>
            {
                m_projectImage = tex;
                m_projectImageRT = new RenderTexture(m_projectImage.width, m_projectImage.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                RenderTexture.active = m_projectImageRT;
                Graphics.Blit(m_projectImage, m_projectImageRT);
            };            
        }

        void OnPreRender()
        {
            if (DanbiManager.instance.simulatorMode != EDanbiSimulatorMode.Project)
            {
                return;
            }

            if (m_projectImage is null)
            {
                return;
            }

            // if (transform.hasChanged)
            // {

            // }
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            switch (DanbiManager.instance.simulatorMode)
            {
                case EDanbiSimulatorMode.Prepare:
                    // Blit the dest with the current activeTexture (Framebuffer[0]).
                    // Graphics.Blit(Camera.main.activeTexture, destination);
                    break;

                case EDanbiSimulatorMode.Render:
                    // bStopRender is already true, but the result isn't saved yet (by button).                    
                    // so we stop updating rendering but keep the screen with the result for preventing performance issue.  
                    // 
                    // Enabled after clicking the image/video generating button?? => this boolean variable is not set anywhere

                    //  public bool renderFinished { get => m_distortedImageRenderStarted; private set => m_distortedImageRenderStarted = value; }
                    //  m_distortedImageRenderStarted is set to true in DanbiMananger.cs when 
                    //  the GenerateImage() method is invoked.
                    //  But renderFinished  should become true when the given pixel samples are created by calling Dispatch iteratively.

                    if  (!DanbiManager.instance.renderStarted )
                    {
                        return;    // it is not ready to render the predistorted image, so just return from OnRenderImage()
                    }

                    // Now it is ready to render the predistorted image
                    
                    if (DanbiManager.instance.renderFinished)
                    {   // The rendering of the predistorted image is finished; just blit the current predistorted image to the
                        // frame buffer. 
                        // Graphics.Blit(ShaderControl.resultRT_LowRes, destination);
                    }
                    else
                    {   // The rendering of the distorted image is not yet finished; continue to render.

                        // Calculate the resolution-wise thread size from the current screen resolution.
                        // and then Dispatch to the shader.
                        var resolution = DanbiManager.instance.screen.screenResolution;
                        var threadGroupXY = (Mathf.CeilToInt(resolution.x * 0.125f), Mathf.CeilToInt(resolution.y * 0.125f));

                        DanbiManager.instance.shaderControl.Dispatch(threadGroupXY, destination);  // this Dispatch is different from the API Dispatch()

                    }
                    break;

                case EDanbiSimulatorMode.Project:
                    // RenderTexture.active = m_projectImageRT;
                    // Graphics.Blit(m_projectImage, m_projectImageRT);
                    // thisCam.targetTexture = m_projectImageRT;
                    // RenderTexture.active = m_projectImageRT;
                    // Graphics.Blit(m_projectImage, m_projectImageRT);
                    // thisCam.targetTexture = m_projectImageRT;

                    // thisCam.Render();

                    // Graphics.Blit(thisCam.targetTexture, destination);
                    // Graphics.Blit(m_projectImage, destination);
                    // Graphics.Blit(m_projectImageRT, destination);
                    // TODO:
                    break;
            }
        }
    };
};
