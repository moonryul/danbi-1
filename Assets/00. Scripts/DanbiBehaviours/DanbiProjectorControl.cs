using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiProjectorControl : MonoBehaviour
    {
        bool renderStarted = false;
        EDanbiSimulatorMode SimulatorMode = EDanbiSimulatorMode.PREPARE;
        bool isImageRendered;
        bool isVideoRendering;
        DanbiComputeShaderControl ShaderControl;
        DanbiScreen Screen;

        void Start()
        {
            // 1. bind the listeners
            DanbiControl.Call_OnGenerateImage += Caller_ListenStartRenderImage;
            DanbiControl.Call_OnGenerateVideo += Caller_ListenStartRenderVideo;
            DanbiControl.Call_OnSaveImage += Caller_ListenSaveImage;
            DanbiControl.Call_OnChangeSimulatorMode += Caller_ListenSimulatorMode;
            DanbiControl.Call_OnImageRendered += Caller_ListenImageRendered;
        }

        void OnDisable()
        {
            // 1. unbind the listeners.
            DanbiControl.Call_OnGenerateImage -= Caller_ListenStartRenderImage;
            DanbiControl.Call_OnSaveImage -= Caller_ListenSaveImage;
            DanbiControl.Call_OnChangeSimulatorMode += Caller_ListenSimulatorMode;
            DanbiControl.Call_OnImageRendered -= Caller_ListenImageRendered;
        }

        public void PrepareResources(DanbiComputeShaderControl shaderControl,
                                     DanbiScreen screen)
        {
            ShaderControl = shaderControl;
            Screen = screen;
        }

        void Caller_ListenStartRenderImage(Texture2D overridingTex) => renderStarted = true;
        void Caller_ListenStartRenderVideo() => isVideoRendering = true;
        void Caller_ListenSaveImage() => renderStarted = false;
        void Caller_ListenSimulatorMode(EDanbiSimulatorMode mode) => SimulatorMode = mode;
        void Caller_ListenImageRendered(bool isRendered) => isImageRendered = isRendered;

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
                    if (!renderStarted)
                    {
                        // converge to highres 해야함
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
            }
        }
    };
};
