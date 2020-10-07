using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    // [RequireComponent(UnityEngine.Camera)]
    public class DanbiProjector : MonoBehaviour
    {
        public RenderTexture renderTex;
        public bool renderStarted = false;
        public EDanbiSimulatorMode SimulatorMode = EDanbiSimulatorMode.PREPARE;
        public bool bDistortionReady;

        public DanbiComputeShaderControl ShaderControl;

        public DanbiScreen Screen;

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
    };
};
