using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using UnityEngine;
using UnityEngine.UIElements;

namespace Danbi
{
#pragma warning disable 3001
#pragma warning disable 3002
    public static class DanbiComputeShaderHelper
    {
        public static ComputeShader FindComputeShader(string fileName)
        {
            foreach (var i in Resources.FindObjectsOfTypeAll<ComputeShader>())
            {
                if (i.name == fileName)
                {
                    return i;
                }
            }
            Debug.LogError($"Failed to find ComputeShaders!");
            return null;
        }

        public static void PrepareRenderTextures((int width, int height) screenResolutions,
                                                 out int samplingCounter,
                                                 ref RenderTexture resRT_lowRes,
                                                 ref RenderTexture convergedRT_highRes)
        {
            // 1. Create LowRes rt
            if (resRT_lowRes.Null())
            {
                resRT_lowRes = new RenderTexture(screenResolutions.width,
                                           screenResolutions.height,
                                           0,
                                           RenderTextureFormat.ARGBFloat,
                                           RenderTextureReadWrite.Linear)
                {
                    enableRandomWrite = true
                };
                resRT_lowRes.Create();
            }
            // 2. Create HighRes rt
            if (convergedRT_highRes.Null())
            {
                convergedRT_highRes = new RenderTexture(screenResolutions.width,
                                            screenResolutions.height,
                                            0,
                                            RenderTextureFormat.ARGBFloat,
                                            RenderTextureReadWrite.Linear)
                {
                    enableRandomWrite = true
                };
                convergedRT_highRes.Create();
            }
            // 3. reset SamplingCounter
            samplingCounter = 0;
        }

        public static void ClearRenderTexture(RenderTexture rt)
        {
            // To clear the target render texture, we have to set this as a main frame buffer.
            // so we swap to the previous RT.
            var prevRT = RenderTexture.active;
            RenderTexture.active = rt;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = prevRT;
        }

        public static void CreateComputeBuffer<T>(ComputeBuffer buffer, List<T> data, int stride)
          where T : struct
        {

            if (!buffer.Null())
            {
                // if there's no data or buffer which doesn't match the given criteria, release it.
                if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride)
                {
                    buffer.Release();
                    buffer = null;
                }

                if (data.Count != 0)
                {
                    // If the buffer has been released or wasn't there to begin with, create it!
                    if (buffer.Null())
                    {
                        buffer = new ComputeBuffer(data.Count, stride);
                    }
                    buffer.SetData(data);
                }
            }
        }

        public static void CreateComputeBuffer<T>(ComputeBuffer buffer, T data, int stride)
          where T : struct
        {
            if (!buffer.Null())
            {
                // if there's no data or buffer which doesn't match the given criteria, release it.
                if (buffer.stride != stride)
                {
                    buffer.Release();
                    buffer = null;
                }

                // If the buffer has been released or wasn't there to begin with, create it!
                if (buffer.Null())
                {
                    buffer = new ComputeBuffer(1, stride);
                }

                buffer.SetData(new List<T> { data });
            }
        }

        public static ComputeBuffer CreateComputeBuffer_Ret<T>(List<T> data, int stride)
          where T : struct
        {
            var res = new ComputeBuffer(data.Count, stride);
            res.SetData(data);
            return res;
        }

        public static ComputeBuffer CreateComputeBuffer_Ret<T>(T data, int stride)
          where T : struct
        {
            var res = new ComputeBuffer(1, stride);
            res.SetData(new List<T> { data });
            return res;
        }

        #region Old Calibration Codes
        public static Matrix4x4 GetOpenCV_KMatrix(float alpha, float beta, float x0, float y0,/* float imgHeight,*/ float near, float far)
        {
            Matrix4x4 PerspK = new Matrix4x4();
            float A = -(near + far);
            float B = near * far;

            PerspK[0, 0] = alpha;
            PerspK[1, 1] = beta;
            PerspK[0, 2] = -x0;
            PerspK[1, 2] = -y0;/*-(imgHeight - y0);*/
            PerspK[2, 2] = A;
            PerspK[2, 3] = B;
            PerspK[3, 2] = -1.0f;

            return PerspK;
        }

        public static Matrix4x4 GetOrthoMatOpenGLGPU_old(float left, float right, float bottom, float top, float near, float far)
        {
            float m00 = 2.0f / (right - left);
            float m11 = 2.0f / (top - bottom);
            float m22 = -2.0f / (far - near);

            float tx = -(left + right) / (right - left);
            float ty = -(bottom + top) / (top - bottom);
            //float tz = -(near + far) / (far - near);
            float tz = (near + far) / (far - near);

            Matrix4x4 Ortho = new Matrix4x4();   // member fields are init to zero
            Ortho[0, 0] = m00;
            Ortho[1, 1] = m11;
            Ortho[2, 2] = m22;
            Ortho[0, 3] = tx;
            Ortho[1, 3] = ty;
            Ortho[2, 3] = tz;
            Ortho[3, 3] = 1.0f;

            return Ortho;
        }

        public static Matrix4x4 GetOpenCVToUnity()
        {
            var frameTransform = new Matrix4x4();
            frameTransform[0, 0] = 1.0f;
            frameTransform[1, 1] = -1.0f;
            frameTransform[2, 2] = 1.0f;
            frameTransform[3, 3] = 1.0f;
            return frameTransform;
        }
        #endregion

        #region New Calibration Codes
        public static Matrix4x4 GetOpenGL_KMatrix(float alpha, float beta, float x0, float y0,/* float imgHeight,*/ float near, float far)
        {
            var PerspK = new Matrix4x4();

            float A = -(near + far);
            float B = near * far;

            PerspK[0, 0] = alpha;
            PerspK[1, 1] = beta;
            PerspK[0, 2] = -x0;
            PerspK[1, 2] = -y0;/*-(imgHeight - y0);*/
            PerspK[2, 2] = A;
            PerspK[2, 3] = B;
            PerspK[3, 2] = -1.0f;

            return PerspK;
        }

        // Based On the Foundation of 3D Computer Graphics (book)
        public static Matrix4x4 GetOpenGLToUnity()
        {
            var FrameTransform = new Matrix4x4();   // member fields are init to zero

            FrameTransform[0, 0] = 1.0f;
            FrameTransform[1, 1] = 1.0f;
            FrameTransform[2, 2] = -1.0f;
            FrameTransform[3, 3] = 1.0f;

            return FrameTransform;
        }

        // Based On the Foundation of 3D Computer Graphics (book)
        public static Matrix4x4 GetOpenGLToOpenCV(float ScreenHeight)
        {
            var FrameTransform = new Matrix4x4();   // member fields are init to zero

            FrameTransform[0, 0] = 1.0f;
            FrameTransform[1, 1] = -1.0f;
            FrameTransform[1, 3] = ScreenHeight;
            FrameTransform[2, 2] = 1.0f;
            FrameTransform[3, 3] = 1.0f;

            return FrameTransform;
        }

        // Based On the Foundation of 3D Computer Graphics (book)
        public static Matrix4x4 GetOrthoMatOpenGLGPU(float left, float right, float bottom, float top, float near, float far)
        {
            float m00 = 2.0f / (right - left);
            float m11 = 2.0f / (top - bottom);
            float m22 = -2.0f / (far - near);

            float tx = -(left + right) / (right - left);
            float ty = -(bottom + top) / (top - bottom);
            //float tz = -(near + far) / (far - near);
            float tz = (near + far) / (far - near);

            var Ortho = new Matrix4x4();

            Ortho[0, 0] = m00;
            Ortho[1, 1] = m11;
            Ortho[2, 2] = m22;
            Ortho[0, 3] = tx;
            Ortho[1, 3] = ty;
            Ortho[2, 3] = tz;
            Ortho[3, 3] = 1.0f;

            return Ortho;
        }
        #endregion New Calibration Codes
    };
};

