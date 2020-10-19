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
        public static Matrix4x4 OpenCV_KMatrixToOpenGLPerspMatrix(float alpha, float beta, float x0, float y0, float near, float far, float width, float height)
        {
            #region comments
            //Our 3x3 intrinsic camera matrix K needs two modifications before it's ready to use in OpenGL.
            //    First, for proper clipping, the (3,3) element of K must be -1. OpenGL's camera looks down the negative z - axis, 
            //    so if K33 is positive, vertices in front of the camera will have a negative w coordinate after projection. 
            //    In principle, this is okay, but because of how OpenGL performs clipping, all of these points will be clipped.

            // If K33 isn't -1, your intrinsic and extrinsic matrices need some modifications. 
            //    Getting the camera decomposition right isn't trivial,
            //    so I'll refer the reader to my earlier article on camera decomposition,
            //    which will walk you through the steps.
            //    Part of the result will be the negation of the third column of the intrinsic matrix, 
            //    so you'll see those elements negated below.



            //   K= \alpha 0  u_0 
            //      \beta 0  v_0
            //       0    0    1  


            //     u0, v0 are the image principle point ,  with f being the focal length and 
            //     being scale factors relating pixels to distance.
            //     Multiplying a point  
            //     by this matrix and dividing by resulting z-coordinate then gives the point projected into the image.
            //The OpenGL parameters are quite different.  Generally the projection is set using the glFrustum command,
            //    which takes the left, right, top, bottom, near and far clip plane locations as parameters
            //    and maps these into "normalized device coordinates" which range from[-1, 1].
            //    The normalized device coordinates are then transformed by the current viewport, 
            //    which maps them onto the final image plane.Because of the differences,
            //    obtaining an OpenGL projection matrix which matches a given set of intrinsic parameters 
            //   is somewhat complicated.


            // construct a projection matrix, this is identical to the 
            // projection matrix computed for the intrinsicx, except an
            // additional row is inserted to map the z-coordinate to
            // OpenGL. 

            //https://answers.unity.com/questions/1359718/what-do-the-values-in-the-matrix4x4-for-cameraproj.html
            // Set an off-center projection, where perspective's vanishing
            // point is not necessarily in the center of the screen.
            //
            // left/right/top/bottom define near plane size, i.e.
            // how offset are corners of camera's near plane.
            // Tweak the values and you can see camera's frustum change.
            //https://stackoverflow.com/questions/2286529/why-does-sign-matter-in-opengl-projection-matrix
            //https://docs.microsoft.com/en-us/windows/win32/opengl/glfrustum?redirectedfrom=MSDN
            //
            //        -The intersection of the optical axis with the image place is called principal point or
            //image center.
            //(note: the principal point is not always the "actual" center of the image)

            //Less commonly, we may wish to translate the 2D normalized device coordinates by
            //[cx, cy]. This can be modeled in the projection matrix as   in p. 95 in Foundations of Computer Graphics
            // In a shifted camera, we translate the normalized device coordinates and
            // keep the[−1..1] region in these shifted coordinates, as shown in Fig. 10.7 in the above book.
            // The [shifted] 3D frustum is defined by specifying an image rectangle on the near
            // plane as in Fig. 10.9 of the book.
            #endregion

            var PerspK = new Matrix4x4();
            float A = near + far;
            float B = near * far;

            float centerX = width / 2;
            float centerY = height / 2;
            float y0InBottomLeft = height - y0; // y0 : Top left image space.

            PerspK[0, 0] = alpha;
            PerspK[1, 1] = beta;
            PerspK[0, 2] = -x0;
            PerspK[1, 2] = -y0InBottomLeft;
            PerspK[2, 2] = A;
            PerspK[2, 3] = B;
            PerspK[3, 2] = -1.0f;

            // Notice that element (3, 2) of the projection matrix is -1.
            // as the camera looks in the negative z direction.
            // which is the opposite of the convention used by Hartley and Zisserman.
            return PerspK;
        }

        public static Matrix4x4 GetOrthoMatOpenGL(float left, float right, float bottom, float top, float near, float far)
        {
            #region comments
            //Our 3x3 intrinsic camera matrix K needs two modifications before it's ready to use in OpenGL.
            //    First, for proper clipping, the (3,3) element of K must be -1. OpenGL's camera looks down the negative z - axis, 
            //    so if K33 is positive, vertices in front of the camera will have a negative w coordinate after projection. 
            //    In principle, this is okay, but because of how OpenGL performs clipping, all of these points will be clipped.

            // If K33 isn't -1, your intrinsic and extrinsic matrices need some modifications. 
            //    Getting the camera decomposition right isn't trivial,
            //    so I'll refer the reader to my earlier article on camera decomposition,
            //    which will walk you through the steps.
            //    Part of the result will be the negation of the third column of the intrinsic matrix, 
            //    so you'll see those elements negated below.



            //   K= \alpha 0  u_0 
            //      \beta 0  v_0
            //       0    0    1  


            //     u0, v0 are the image principle point ,  with f being the focal length and 
            //     being scale factors relating pixels to distance.
            //     Multiplying a point  
            //     by this matrix and dividing by resulting z-coordinate then gives the point projected into the image.
            //The OpenGL parameters are quite different.  Generally the projection is set using the glFrustum command,
            //    which takes the left, right, top, bottom, near and far clip plane locations as parameters
            //    and maps these into "normalized device coordinates" which range from[-1, 1].
            //    The normalized device coordinates are then transformed by the current viewport, 
            //    which maps them onto the final image plane.Because of the differences,
            //    obtaining an OpenGL projection matrix which matches a given set of intrinsic parameters 
            //   is somewhat complicated.


            // construct a projection matrix, this is identical to the 
            // projection matrix computed for the intrinsicx, except an
            // additional row is inserted to map the z-coordinate to
            // OpenGL. 

            //https://answers.unity.com/questions/1359718/what-do-the-values-in-the-matrix4x4-for-cameraproj.html
            // Set an off-center projection, where perspective's vanishing
            // point is not necessarily in the center of the screen.
            //
            // left/right/top/bottom define near plane size, i.e.
            // how offset are corners of camera's near plane.
            // Tweak the values and you can see camera's frustum change.
            //https://stackoverflow.com/questions/2286529/why-does-sign-matter-in-opengl-projection-matrix
            //https://docs.microsoft.com/en-us/windows/win32/opengl/glfrustum?redirectedfrom=MSDN
            //
            //        -The intersection of the optical axis with the image place is called principal point or
            //image center.
            //(note: the principal point is not always the "actual" center of the image)

            //Less commonly, we may wish to translate the 2D normalized device coordinates by
            //[cx, cy]. This can be modeled in the projection matrix as   in p. 95 in Foundations of Computer Graphics
            // In a shifted camera, we translate the normalized device coordinates and
            // keep the[−1..1] region in these shifted coordinates, as shown in Fig. 10.7 in the above book.
            // The [shifted] 3D frustum is defined by specifying an image rectangle on the near
            // plane as in Fig. 10.9 of the book.
            #endregion
            float m00 = 2.0f / (right - left);
            float m11 = 2.0f / (top - bottom);
            float m22 = -2.0f / (far - near);

            float tx = -(left + right) / (right - left);
            float ty = -(bottom + top) / (top - bottom);
            float tz = -(near + far) / (far - near);

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
    };
};

