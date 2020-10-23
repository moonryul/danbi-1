using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiProjector : MonoBehaviour
    {
        EDanbiSimulatorMode SimulatorMode = EDanbiSimulatorMode.PREPARE;
        bool bPredistortedImageReady;
        RenderTexture ResultRenderTex;
        RenderTexture ConvergedRenderTexForNewImage;
        ComputeShader RTShader;
        (int x, int y) CurrentScreenResolutions;

        Material AddMaterial_WholeSizeScreenSampling;
        uint CurrentSamplingCountForRendering;
        uint MaxSamplingCountForRendering;

        void Start()
        {
            Debug.Log("displays connected: " + Display.displays.Length);
            // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
            // Check if additional displays are available and activate each.

            for (int i = 1; i < Display.displays.Length; i++)
            {
                // Activated display is used automatically on build standalone.
                // and it detects the multi-displays automatically.
                Display.displays[i].Activate();
            }
        }

        public void Setup(ref EDanbiSimulatorMode mode,
                          ref bool _bPredistortedImageReady,
                          RenderTexture resRT,
                          RenderTexture convergedRT,
                          ComputeShader rtShader,
                          (int x, int y) currentScreenResolutions,
                          Material addMaterial_WholeSizeScreenSampling,
                          ref uint samplingCountForRendering,
                          uint maxSamplingCountForRendering)
        {
            SimulatorMode = mode;
            bPredistortedImageReady = _bPredistortedImageReady;
            ResultRenderTex = resRT;
            ConvergedRenderTexForNewImage = convergedRT;
            RTShader = rtShader;
            CurrentScreenResolutions = currentScreenResolutions;
            AddMaterial_WholeSizeScreenSampling = addMaterial_WholeSizeScreenSampling;
            CurrentSamplingCountForRendering = samplingCountForRendering;
            MaxSamplingCountForRendering = maxSamplingCountForRendering;
        }

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // OnRenderImage() is called every frame when this script is attached to the camera
            // SimulatorMode is PREPARE when this script is started() and becomes CAPTURE when OnInitCreateDistortedImage() is called
            // when the user presses its button. 

            if (SimulatorMode == EDanbiSimulatorMode.PREPARE) { return; }
            //  This check is  needed, because OnRenderImage() is
            //  should not be executed before  OnInitCreateDistortedImage(), which sets SimulatorMode to Capture

            if (SimulatorMode == EDanbiSimulatorMode.CAPTURE)
            {
                if (bPredistortedImageReady)  // //  bPredistortedImageReady = true when an enough number of rendering has been performed on the same image.
                                              // becomes false when when  OnInitCreateDistortedImage() is called again.
                                              // When the distorted image is ready, the frame buffer is not updated, 
                                              // but the same content is transferred to the framebuffer
                                              //  to make the screen alive
                {
                    //Debug.Log("current sample not incremented =" + CurrentSamplingCountForRendering);
                    //Debug.Log("no dispatch of compute shader = blit of the current _coverged to framebuffer");

                    // Ignore the target Texture of the camera in order to blit to the null target (which is
                    // the frame buffer
                    //the destination (frame buffer= null) has a resolution of Screen.width x Screen.height
                    //Graphics.Blit(ConvergedRenderTexForNewImage, null as RenderTexture);
                    Graphics.Blit(ConvergedRenderTexForNewImage, destination);
                }
                else
                {   // continue to render onto the frame buffer  until an enough number of rendering is done
                    // and thus bPredistortedImageReady is true.
                    //Debug.Log("current sample=" + _currentSample);

                    //Next, we dispatch the shader. This means that we are telling the GPU to get busy 
                    //    with a number of thread groups executing our shader code.Each thread group consists of a number of threads
                    //    which is set in the shader itself.The size and number of thread groups can be specified in up to three dimensions, 
                    //    which makes it easy to apply compute shaders to problems of either dimensionality. 
                    //    In our case, we want to spawn one thread per pixel of the render target.
                    //    The default thread group size as defined in the Unity compute shader template is [numthreads(8, 8, 1)],
                    //    so we'll stick to that and spawn **one thread group per 8×8 pixels**. 
                    //    Finally, we write our result to the screen using Graphics.Blit.
                    int threadGroupsX = Mathf.CeilToInt(CurrentScreenResolutions.x * 0.125f); // same as (/ 8).
                    int threadGroupsY = Mathf.CeilToInt(CurrentScreenResolutions.y * 0.125f);

                    //Different mKernelToUse is used depending on the task, that is, on the value
                    // of _CaptureOrProjectOrView

                    RTShader.Dispatch(Danbi.DanbiKernelDict.CurrentKernelIndex, threadGroupsX, threadGroupsY, 1);
                    // This dispatch of the compute shader will set _Target TWTexure2D

                    if (AddMaterial_WholeSizeScreenSampling == null)
                    {
                        AddMaterial_WholeSizeScreenSampling = new Material(Shader.Find("Hidden/AddShader"));
                    }

                    AddMaterial_WholeSizeScreenSampling.SetFloat("_Sample", CurrentSamplingCountForRendering);

                    // TODO: Upscale To 4K and downscale to 1k.
                    //_Target is the RWTexture2D created by the compute shader
                    // note that _cameraMain.targetTexture = _convergedForCreateImage by OnPreRender(); =>
                    // not used right now.

                    // Blit (source, dest, material) sets dest as the render target, and source as _MainTex property
                    // on the material and draws a full-screen quad.
                    //If  dest == null, the screen backbuffer is used as
                    // the blit destination, EXCEPT if the Camera.main has a non-null targetTexture;
                    // If the Camera.main has a non-null targetTexture, it will be the target even if 
                    // dest == null.

                    Graphics.Blit(ResultRenderTex, ConvergedRenderTexForNewImage, AddMaterial_WholeSizeScreenSampling);

                    // to improve the resolution of the result image, We need to use Converged Render Texture (upscaled in float precision).
                    //Graphics.Blit(ConvergedRenderTexForNewImage, null as RenderTexture);
                    Graphics.Blit(ConvergedRenderTexForNewImage, destination);

                    // Ignore the target Texture of the camera in order to blit to the null target which it is the framebuffer.
                    ++CurrentSamplingCountForRendering;
                    //  bPredistortedImageReady = true when an enough number of rendering has been performed on the same image.

                    if (CurrentSamplingCountForRendering > MaxSamplingCountForRendering)
                    {
                        Debug.Log($"Ready to display or store the distorted image!", this);
                        bPredistortedImageReady = true;
                        CurrentSamplingCountForRendering = 0;
                    }

                    // Each cycle of rendering, a new location within every pixel area is sampled 
                    // for the purpose of  anti-aliasing.
                } // else of if (mPauseNewRendering)
            } // if  (SimulatorMode == EDanbiSimulatorMode.CAPTURE)
        }  //void Start()
    };  // class DanbiProjector
};       // nameSpace Danbi
