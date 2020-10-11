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
        DanbiComputeShaderControl ShaderControl;
        DanbiScreen Screen;

        void Start()
        {
            // 1. bind the listeners
            DanbiControl.Call_OnGenerateImage += Caller_ListenStartRender;
            DanbiControl.Call_OnSaveImage += Caller_ListenSaveImage;
            DanbiControl.Call_OnChangeSimulatorMode += Caller_ListenSimulatorMode;
            DanbiControl.Call_OnChangeImageRendered += Caller_ListenImageRendered;
        }

        void OnDisable()
        {
            DanbiControl.Call_OnGenerateImage -= Caller_ListenStartRender;
            DanbiControl.Call_OnSaveImage -= Caller_ListenSaveImage;
            DanbiControl.Call_OnChangeSimulatorMode += Caller_ListenSimulatorMode;
            DanbiControl.Call_OnChangeImageRendered -= Caller_ListenImageRendered;
        }

        public void PrepareResources(DanbiComputeShaderControl shaderControl,
                                     DanbiScreen screen)
        {
            ShaderControl = shaderControl;
            Screen = screen;
        }

        void Caller_ListenStartRender() => renderStarted = true;
        void Caller_ListenSaveImage() => renderStarted = false;
        void Caller_ListenSimulatorMode(EDanbiSimulatorMode mode) => SimulatorMode = mode;
        void Caller_ListenImageRendered(bool isRendered) => isImageRendered = isRendered;

        // TODO: Rearrange 
        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            if (!renderStarted) return;

            switch (SimulatorMode)
            {
                case EDanbiSimulatorMode.PREPARE:
                    // Blit the dest with the current activeTexture (Framebuffer[0]).
                    Graphics.Blit(Camera.main.activeTexture, destination);
                    break;

                case EDanbiSimulatorMode.CAPTURE:
                    // bStopRender is already true, but the result isn't saved yet (by button).
                    // 
                    // so we stop updating rendering but keep the screen with the result for preventing performance issue.          
                    if (isImageRendered)
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

                default:
                    Debug.LogError($"Other Value {SimulatorMode} isn't used in this context.", this);
                    break;
            }
        }
    };
};
