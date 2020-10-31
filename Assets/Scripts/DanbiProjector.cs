using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Danbi
{
    public class DanbiProjector : MonoBehaviour
    {

        // for debug

        public ComputeBuffer m_SampleCountBuffer { get; protected set; } // null reference
        public float[] m_BufferArray;

        // The following variables are set  in RayTracingMaster.cs

        public EDanbiRenderingState RenderingState;

        public RenderTexture ResultRenderTex;
        public RenderTexture ConvergedRenderTexForNewImage;
        public ComputeShader RTShader;
        public Vector2Int CurrentScreenResolutions;

        Material AddMaterial_WholeSizeScreenSampling;
        public uint CurrentSamplingCountForRendering;
        public uint MaxSamplingCountForRendering;

        void Start()
        {

            // Check for shader model 4.5 or better support
            //if (SystemInfo.graphicsShaderLevel >= 45)
            Debug.LogFormat("Woohoo, decent shaders supported!: {0}", SystemInfo.graphicsShaderLevel);

            //Output the current screen window width in the console
            Debug.LogFormat("Screen Width : {0}, Screen Height: {1}",  Screen.width, Screen.height);

            //ComputeBuffer class creates & fills them from script code, 
            // and use them in compute shaders or regular shaders.
            // In regular graphics shaders the compute buffer support requires minimum shader model 4.5.
            //But with shader model 5.0 and d3d11 you can now do more or less the same in regular vertex fragment shaders. 
            //https://forum.unity.com/threads/how-to-pass-a-structured-buffer-in-to-fragment-shader.862216/
            //https://forum.unity.com/threads/write-in-a-custom-buffer-in-a-regular-shader-non-post-process.515357/
            //http://blog.deferredreality.com/write-to-custom-data-buffers-from-shaders/

            //https://forum.unity.com/threads/rwstructuredbuffer-in-vertex-shader.406592/
           
            //RWStructuredBuffer is supported for all types of shaders in hardware with minimum D3D_FEATURE_LEVEL_11_1.
            //(minimum AMD Radeon HD 8000 or NVIDIA GeForce GTX 900 or Intel HD Graphics 5000 / 4x00 and Windows 8).
            //For hardware with D3D_FEATURE_LEVEL_11_0 is only supported for pixel and compute shaders.

            //So, now it explains, why my sample code works on the newest GPUs and not on NVIDIA GTX 660.
            //I have to upgrade graphics card.  GeForce Mine is GTX750

            int bufferSize = 1;
            m_SampleCountBuffer = new ComputeBuffer(bufferSize, sizeof(float));

            m_BufferArray = new float[1];

            m_SampleCountBuffer.SetData(m_BufferArray); // buffer is R or RW
            Graphics.ClearRandomWriteTargets();
            Graphics.SetRandomWriteTarget(1, m_SampleCountBuffer, true);

            //SetRandomWriteTarget(int index, ComputeBuffer uav, [Internal.DefaultValue("false")] bool preserveCounterValue);
            //
            // Parameters:
            //   index:
            //     Index of the random write target in the shader.
            //
            //   uav:
            //     RenderTexture to set as write target.

            //public static void SetRandomWriteTarget(int index, RenderTexture uav)


            if (AddMaterial_WholeSizeScreenSampling == null)
            {
                AddMaterial_WholeSizeScreenSampling = new Material(Shader.Find("Hidden/AddShader"));
            }
            AddMaterial_WholeSizeScreenSampling.SetBuffer("_DebugBuffer", m_SampleCountBuffer);


           // Debug.Log("displays connected: " + Display.displays.Length);
            // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
            // Check if additional displays are available and activate each.

            for (int i = 1; i < Display.displays.Length; i++)
            {
                // Activated display is used automatically on build standalone.
                // and it detects the multi-displays automatically.
                Display.displays[i].Activate();
            }
        }

        //public void Setup(//EDanbiSimulatorMode mode,
        //                  bool _bPredistortedImageInProgress,
        //                  RenderTexture resRT,
        //                  RenderTexture convergedRT,
        //                  ComputeShader rtShader,
        //                  (int x, int y) currentScreenResolutions,
        //                  Material addMaterial_WholeSizeScreenSampling,
        //                  uint samplingCountForRendering,
        //                  uint maxSamplingCountForRendering)
        //{
        //    //SimulatorMode = mode;
        //    bPredistortedImageInProgress = _bPredistortedImageInProgress;
        //    ResultRenderTex = resRT;
        //    ConvergedRenderTexForNewImage = convergedRT;
        //    RTShader = rtShader;
        //    CurrentScreenResolutions = currentScreenResolutions;
        //    AddMaterial_WholeSizeScreenSampling = addMaterial_WholeSizeScreenSampling;
        //    CurrentSamplingCountForRendering = samplingCountForRendering;
        //    MaxSamplingCountForRendering = maxSamplingCountForRendering;
        //}


        protected void SetShaderFrameParameters()
        {
            //if (SimulatorMode == EDanbiSimulatorMode.PREPARE) { return; }

            var pixelOffset = new Vector2(Random.value, Random.value);
            RTShader.SetVector("_PixelOffset",new Vector4( pixelOffset.x, pixelOffset.y,0,0) );

            //Debug.Log("_PixelOffset =" + pixelOffset);
            float seed = Random.value;
            RTShader.SetFloat("_Seed", seed);
            //Debug.Log("_Seed =" + seed);
        }   //SetShaderFrameParameters()

        void OnRenderImage(RenderTexture source, RenderTexture destination)
        {
            // OnRenderImage() is called every frame when this script is attached to the camera.
            // OnRenderImage is called after all rendering is complete to render image for postprocessing.
            // destination is the currently active renderTexture, which is null, that is the framebuffer
            // by default. The current renderTexture is saved in  RenderTexture.active.

            // Each cycle of rendering, a new location within every pixel area is sampled 
            // for the purpose of  anti-aliasing:

            //Debug.LogFormat("RenderTexture.sRGB= {0}", destination.sRGB);

            if (RTShader == null)    // the compute shader is not set in RayTracingMaster script
            {
                return;
            }

         

            if (RenderingState == EDanbiRenderingState.Completed)
            // //  RenderingState is completed when an enough number of rendering has been performed on the same image.
            // ( It becomes true during the incremental rendering of the image. )
            // While completed,  the frame buffer is not updated, 
            // but the same content is transferred to the framebuffer
            //  to make the screen alive
            {
                //Debug.Log("current sample not incremented =" + CurrentSamplingCountForRendering);
                //Debug.Log("no dispatch of compute shader = blit of the current _coverged to framebuffer");

                // the destination (frame buffer= null) has a low resolution compared to   ConvergedRenderTexForNewImage.
               // Debug.Log($"The current image is not updated but simply blit to the framebuffer", this);
               // Graphics.Blit(ConvergedRenderTexForNewImage, destination);  // what happens when we do not this.
                return;
            }   //  if (RenderingState == EDanbiRenderingState.Completed)

            if (RenderingState == EDanbiRenderingState.InProgress)
            {   // continue to render onto the converging renderTexture   until an enough number of rendering is done

                //Debug.Log("current sample=" + _currentSample);

                SetShaderFrameParameters();

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

                RTShader.Dispatch(Danbi.DanbiKernelDict.CurrentKernelIndex, threadGroupsX, threadGroupsY, 1);
                // This dispatch of the compute shader will produce the target texture ResultRenderTexture

                //_Sample refers to the current sampling of each pixel. It is used for progressive accumulation of the color
                // for each pixel.

                // debug
                //m_BufferArray[0] = (float)CurrentSamplingCountForRendering;

                AddMaterial_WholeSizeScreenSampling.SetFloat("_SampleCount", (float)CurrentSamplingCountForRendering);

                // TODO: Upscale To 4K and downscale to 1k.
                //_Target is the RWTexture2D created by the compute shader
                // note that _cameraMain.targetTexture = _convergedForCreateImage

                // Blit (source, dest, material) sets dest as the render target, and source as _MainTex property
                // on the material and draws a full-screen quad.
                //If  dest == null, the screen backbuffer is used as
                // the blit destination, EXCEPT if the Camera.main has a non-null targetTexture;
                // If the Camera.main has a non-null targetTexture, it will be the target even if 
                // dest == null.

                m_SampleCountBuffer.SetData(m_BufferArray); // buffer is R or RW
                Graphics.ClearRandomWriteTargets();
                //This function clears any "random write" targets that were previously set with SetRandomWriteTarget.

                Graphics.SetRandomWriteTarget(1, m_SampleCountBuffer, true);

                //Accumulation
                //    For some reason or another, Unity wouldn’t give me an HDR texture as destination in OnRenderImage.
                //    The format for me was R8G8B8A8_Typeless, so the precision would quickly be too low 
                //    for adding up more than a few samples.To overcome this,
                //    let’s add private RenderTexture _converged to our C# script. 
                //   This will be our buffer to accumulate the results with high precision, before displaying it on screen. 
                Graphics.Blit(ResultRenderTex,
                              ConvergedRenderTexForNewImage,
                              AddMaterial_WholeSizeScreenSampling);


                //debug
                m_SampleCountBuffer.GetData(m_BufferArray); // buffer is R or RW
               // Debug.LogFormat(" SampleCountReadFromShader = {0}", m_BufferArray[0]);

                // to improve the resolution of the result image, We need to use Converged Render Texture (upscaled in float precision).
                //Graphics.Blit(ConvergedRenderTexForNewImage, null as RenderTexture);
                Graphics.Blit(ConvergedRenderTexForNewImage, destination);

                //public static void Blit(Texture source, RenderTexture dest, Material mat, int pass = -1);
                //	The destination RenderTexture. Set this to null to blit directly to screen. 
                //Blit sets dest as the render target, sets source _MainTex property on the material, 
                //and draws a full-screen quad.
                //If you are using the Built-in Render Pipeline, when dest is null,
                //Unity uses the screen backbuffer as the blit destination. 
                //However, if the main camera is set to render to a RenderTexture 
                //(that is, if Camera.main has a non-null targetTexture property), 
                //the blit uses the render target of the main camera as destination.
                //To ensure that the blit actually writes to the screen backbuffer,
                //make sure to set /Camera.main.targetTexture/ to null before calling Blit.
                // Ignore the target Texture of the camera in order to blit to the null target which it is the framebuffer.


                ++CurrentSamplingCountForRendering;
                //  bPredistortedImageInProgress = true when an enough number of rendering has been performed on the same image.

                if (CurrentSamplingCountForRendering > MaxSamplingCountForRendering)
                {
                    Debug.Log($"The current image is fully constructed (The incremental rendering finished)", this);
                    RenderingState = EDanbiRenderingState.Completed;
                    //CurrentSamplingCountForRendering = 0;
                }
                else
                {
                    //Debug.LogFormat("The current image is being incrementally constructed with  sampling step={0}",
                    //                 CurrentSamplingCountForRendering );
                }

                return;

            } //  if (RenderingState == EDanbiRenderingState.InProgress)  

        }  //void  OnRenderImage()
    }  // class DanbiProjector
}    // nameSpace Danbi
