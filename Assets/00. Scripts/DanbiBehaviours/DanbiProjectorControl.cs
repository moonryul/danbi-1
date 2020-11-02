using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
#pragma warning disable 3001
    public class DanbiProjectorControl : MonoBehaviour
    {
        [SerializeField, Readonly]
        EDanbiProjectionMode m_projectionMode;
        public EDanbiProjectionMode projectionMode { get => m_projectionMode; set => m_projectionMode = value; }
        /// <summary>
        /// 
        /// </summary>
        DanbiManager m_danbiManager;
        /// <summary>
        /// Used for Dispatch()
        /// </summary>
        DanbiComputeShaderControl m_computeShaderControl;
        /// <summary>
        /// Used for getting screen resolutions
        /// </summary>
        DanbiScreen m_screen;

        void Awake()
        {
            m_danbiManager = GetComponent<DanbiManager>();
            m_computeShaderControl = GetComponent<DanbiComputeShaderControl>();
            m_screen = GetComponent<DanbiScreen>();
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            switch (m_danbiManager.simulatorMode)
            {
                case EDanbiSimulatorMode.Prepare:
                    // Blit the dest with the current activeTexture (Framebuffer[0]).
                    Graphics.Blit(Camera.main.activeTexture, destination);
                    break;

                case EDanbiSimulatorMode.Render:
                    // bStopRender is already true, but the result isn't saved yet (by button).                    
                    // so we stop updating rendering but keep the screen with the result for preventing performance issue.  
                    // 
                    // Enabled after clicking the image/video generating button
                    if (!m_danbiManager.renderFinished)
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

                case EDanbiSimulatorMode.Project:
                    
                    break;
            }
        }
    };
};
