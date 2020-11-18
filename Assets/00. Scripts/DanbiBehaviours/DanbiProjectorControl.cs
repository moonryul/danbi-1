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

        void Awake()
        {
            DanbiUIProjectionImagePanelControl.onProjectionImageUpdate +=
            (Texture2D tex) =>
            {
                m_projectImage = tex;
                m_projectImageRT = new RenderTexture(m_projectImage.width, m_projectImage.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                RenderTexture.active = m_projectImageRT;
                Graphics.Blit(m_projectImage, m_projectImageRT);
            };
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!DanbiManager.instance.renderStarted)
            {
                // it is not ready to render the predistorted image, so just return from OnRenderImage()
                return;
            }

            // Now it is ready to render the predistorted image
            if (DanbiManager.instance.renderFinished)
            {
                // The rendering of the predistorted image is finished; just blit the current predistorted image to the
                // frame buffer. 
                Graphics.Blit(DanbiManager.instance.shaderControl.convergedResultRT_HiRes, destination);
            }
            else
            {
                // The rendering of the distorted image is not yet finished; continue to render.
                // Calculate the resolution-wise thread size from the current screen resolution.
                // and then Dispatch to the shader.
                var resolution = DanbiManager.instance.screen.screenResolution;
                var threadGroupXY = (Mathf.CeilToInt(resolution.x * 0.125f), Mathf.CeilToInt(resolution.y * 0.125f));
                DanbiManager.instance.shaderControl.Dispatch(threadGroupXY, destination);  // this Dispatch is different from the API Dispatch()                        
            }
        }
    };
};
