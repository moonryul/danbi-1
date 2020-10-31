using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiProjectorControl : MonoBehaviour
    {
        /// <summary>
        /// Enabled after clicking the image/video generating button
        /// </summary>
        bool m_renderFinished = false;
        EDanbiSimulatorMode SimulatorMode = EDanbiSimulatorMode.PREPARE;
        bool isImageRendered;
        bool isVideoRendering;
        /// <summary>
        /// Used for Dispatch()
        /// </summary>
        DanbiComputeShaderControl m_computeShaderControl;
        /// <summary>
        /// Used for getting screen resolutions
        /// </summary>
        DanbiScreen m_screen;

        void Start()
        {
            // 1. bind the listeners
            DanbiControl.Call_OnGenerateImage += Caller_StartRenderImage;
            DanbiControl.Call_OnGenerateVideo += Caller_StartRenderVideo;
            DanbiControl.Call_OnSaveImage += Caller_SaveImage;
            DanbiControl.Call_OnChangeSimulatorMode += Caller_SimulatorMode;
            DanbiControl.Call_OnImageRendered += Caller_ImageRendered;
        }

        void OnDisable()
        {
            // 1. unbind the listeners.
            DanbiControl.Call_OnGenerateImage -= Caller_StartRenderImage;
            DanbiControl.Call_OnSaveImage -= Caller_SaveImage;
            DanbiControl.Call_OnChangeSimulatorMode += Caller_SimulatorMode;
            DanbiControl.Call_OnImageRendered -= Caller_ImageRendered;
        }

        public void PrepareResources(DanbiComputeShaderControl shaderControl,
                                     DanbiScreen screen)
        {
            m_computeShaderControl = shaderControl;
            m_screen = screen;
        }

        void Caller_StartRenderImage(Texture2D overridingTex) => m_renderFinished = true;
        void Caller_StartRenderVideo(TMPro.TMP_Text progressDisplay, TMPro.TMP_Text statusDisplay) => isVideoRendering = true;
        void Caller_SaveImage() => m_renderFinished = false;
        void Caller_SimulatorMode(EDanbiSimulatorMode mode) => SimulatorMode = mode;
        void Caller_ImageRendered(bool isRendered) => isImageRendered = isRendered;

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            switch (SimulatorMode)
            {
                case EDanbiSimulatorMode.PREPARE:
                    // Blit the dest with the current activeTexture (Framebuffer[0]).
                    Graphics.Blit(Camera.main.activeTexture, destination);
                    break;

                case EDanbiSimulatorMode.CAPTURE:
                    // bStopRender is already true, but the result isn't saved yet (by button).                    
                    // so we stop updating rendering but keep the screen with the result for preventing performance issue.  
                    // 
                    // Enabled after clicking the image/video generating button
                    if (!m_renderFinished)
                    {
                        // TODO: converge to highres 해야함
                        // Graphics.Blit(ShaderControl.resultRT_LowRes, destination);
                    }
                    else
                    {
                        // 1. Calculate the resolution-wise thread size from the current screen resolution.
                        //    and Dispatch.
                        m_computeShaderControl.Dispatch((Mathf.CeilToInt(m_screen.screenResolution.x * 0.125f), Mathf.CeilToInt(m_screen.screenResolution.y * 0.125f)),
                                               destination);
                    }
                    break;
            }
        }
    };
};
