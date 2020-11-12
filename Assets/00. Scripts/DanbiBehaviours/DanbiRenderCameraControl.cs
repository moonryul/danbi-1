using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;

namespace Danbi
{
    public class DanbiRenderCameraControl : MonoBehaviour
    {
        void Start()
        {                        
            if (DanbiManager.instance.cameraControl != null)
            {
                var camControl = DanbiManager.instance.cameraControl;
                if (!camControl.useCalibratedProjector)
                {
                    // use the default graphics camera
                    this.gameObject.transform.eulerAngles = new Vector3(90, 0, 0);
                    this.gameObject.transform.position = new Vector3(0, 2.123f, 0);

                    Debug.Log($"camera position={  this.gameObject.transform.position.y}");
                    Debug.Log($"localToWorldMatrix =\n{ this.gameObject.transform.localToWorldMatrix}, " +
                       $"\nQuaternion Mat4x4: { this.gameObject.transform.rotation} ");


                }
                else
                {
                    float4x4 ViewTransform_OpenCV = new float4x4(new float4(camControl.cameraExternalData.xAxis, 0),
                                                                 new float4(camControl.cameraExternalData.yAxis, 0),
                                                                 new float4(camControl.cameraExternalData.zAxis, 0),
                                                                 new float4(camControl.cameraExternalData.projectorPosition, 0));
                    Debug.Log($"ViewTransform =\n{ ViewTransform_OpenCV }");

                    #region comments
                    //https://en.wikibooks.org/wiki/Cg_Programming/Vector_and_Matrix_Operations
                    //GLSL:

                    //    mat3 m(column0, column1, column2);
                    //    m[0]; // returs the first column
                    //HLSL:

                    //    float3x3 m = float3x3(row0, row1, row2); // sets rows of matrix n
                    //    m[0]; // Returns first row.

                    // But In Unity, float3x3 behaves similarly as Matrix4x4 by 
                    // constructing matrix from column vectors:

                    //https://github.com/Unity-Technologies/Unity.Mathematics/blob/master/src/Unity.Mathematics/math_unity_conversion.cs


                    //Handedness and matrices and quaternion:
                    //https://stackoverflow.com/questions/1274936/flipping-a-quaternion-from-right-to-left-handed-coordinates/39519536#39519536
                    // By Paul de Bois:

                    //https://en.wikipedia.org/wiki/Matrix_similarity
                    // Change of basis => a simpler form of the same transformation
                    // In the changed frame (OpenCV) , y' = Sx'; In the original frame, y = Tx, where vectors x and y, and the unknown transform
                    // T are in the original basis (Unity). To write T in terms of simpler matrix S:
                    // y' = Sx' => y' = Py, x' = Px by change of basis P.
                    // Py = SPx => y = P^{-1}SP x = Tx => T = P^{-1}SP


                    //Conversion from quaternion to 3x3 matrix does not involve handedness of any sort. It is purely "solve for the matrix M such that Mv = qv" (assuming you're using column vectors).
                    //See euclideanspace.com/maths/geometry/rotations/conversions/… for the derivation. – Paul Du Bois Aug 29 '17 at 18:57


                    //quaternions don't have handedness (*). Handedness (or what I'll call "axis conventions") is a property 
                    //  that humans apply;  it's how we map our concepts of "forward, right, up" to the X, Y, Z axes.

                    //These things are true:

                    //            (1)    Pure - rotation matrices(orthogonal, determinant 1, etc) can be converted to a unit quaternion and back, 
                    //                    recovering the original matrix.
                    //            (2)          Matrices that are not pure rotations(ones that have determinant -1, 
                    // for example matrices that flip a single axis)
                    //                    are also called "improper rotations", and cannot be converted to a unit quaternion and back. 
                    //                    Your mat_to_quat() routine may not blow up, but it won't give you the right answer 
                    //                    (in the sense that quat_to_mat(mat_to_quat(M)) == M).
                    //            (3) A change-of - basis that swaps handedness has determinant - 1.It is an improper rotation: equivalent to a rotation(maybe identity) 
                    //                    composed with a mirroring about the origin.

                    //         Now, To change the basis of a quaternion, say from ROS(right - handed) to Unity(left-handed), we can use the method of .

                    //              mat3x3 ros_to_unity = /* construct this by hand */;
                    //                mat3x3 unity_to_ros = ros_to_unity.inverse();
                    //                quat q_ros = ...;
                    //                mat3x3 m_unity = ros_to_unity * mat3x3(q_ros) * unity_to_ros;
                    //                quat q_unity = mat_to_quat(m_unity);
                    //                Lines 1 - 4 are simply the method of https://stackoverflow.com/a/39519079/194921: 
                    //"How do you perform a change-of-basis on a matrix?"

                    //             Line 5 is interesting.We know mat_to_quat() only works on pure-rotation matrices.
                    //How do we know that m_unity is a pure rotation? It's certainly conceivable that it's not,
                    //    because unity_to_ros and ros_to_unity both have determinant -1(as a result of the handedness switch).

                    //             The hand-wavy answer is that the handedness is switching twice, so the result has no handedness switch.
                    //             The deeper answer has to do with the fact that similarity transformations preserve certain aspects of the operator,
                    //                    but I don't have enough math to make the proof.


                    //                The problem you ask about arises even if the two coordinate systems are same - handed; 
                    //                it turns out that handedness flips don't make the problem significantly harder.
                    //                    Here is how to do it in general. To change the basis of a quaternion,
                    //                    say from ROS (right-handed, Z up) to Unity (left-handed, Y up):

                    //mat3x3 ros_to_unity = /* construct this by hand by mapping input axes to output axes */;
                    //                mat3x3 unity_to_ros = ros_to_unity.inverse();
                    //                quat q_ros = ...;
                    //                mat3x3 m_unity = ros_to_unity * mat3x3(q_ros) * unity_to_ros;
                    //                quat q_unity = mat_to_quat(m_unity);
                    //                Lines 1 - 4 are simply the method of https://stackoverflow.com/a/39519079/194921: "How do you perform a change-of-basis on a matrix?"

                    //Line 5 is interesting; not all matrices convert to quats, but if ros_to_unity is correct, then this conversion will succeed.


                    //Pure - rotation matrices(orthogonal, determinant 1, etc) can be converted to a unit quaternion and back, 
                    //                    recovering the original matrix.
                    //Matrices that are not pure rotations(ones that have determinant -1, for example matrices that flip a single axis) 
                    //                    are also called "improper rotations", and cannot be converted to a unit quaternion and back. 
                    //                    Your mat_to_quat() routine may not blow up, but it won't give you the right answer 
                    //                    (in the sense that quat_to_mat(mat_to_quat(M)) == M).
                    //A change-of - basis that swaps handedness has determinant - 1.It is an improper rotation:
                    //                equivalent to a rotation(maybe identity) composed with a mirroring about the origin.


                    //Camera external parameters: http://ksimek.github.io/2012/08/22/extrinsic/

                    //Let C be a column vector describing the location of the camera-center in world coordinates, 
                    //    and let Rc be the rotation matrix describing the camera's orientation 
                    //    with respect to the world coordinate axes. 
                    //    The transformation matrix that describes the camera's pose is then[Rc | C]
                    // R =   worldToCameraRotation;  Rc = tranpose(R)

                    // t =  -R^{T} * C, where C is the camera position in the world
                    // t =  CameraExternalParameters.translation;
                    // R^{T} * t = = -C
                    //float4 cameraOrigionOpenCVWorld = - math.mul(openCVWorldToCameraMat,
                    //                                             new float4(CameraExternalParameters.translation, 1));
                    #endregion comments

                    float3x3 ViewTransform_Rot_OpenCV = new float3x3(ViewTransform_OpenCV.c0.xyz, ViewTransform_OpenCV.c1.xyz, ViewTransform_OpenCV.c2.xyz);
                    float3 ViewTransform_Trans_OpenCV = ViewTransform_OpenCV.c3.xyz;
                    float3x3 CameraTransformation_Rot_OpenCV = math.transpose(ViewTransform_Rot_OpenCV);
                    float4x4 CameraTransformation_OpenCV = new float4x4(CameraTransformation_Rot_OpenCV, -math.mul(CameraTransformation_Rot_OpenCV, ViewTransform_Trans_OpenCV));

                    Debug.Log($"CameraTransformation_OpenCV (obtained by transpose) =\n{ CameraTransformation_OpenCV }");


                    float4x4 CameraTransform_OpenCV = math.inverse(ViewTransform_OpenCV);
                    Debug.Log($" CameraTransform_OpenCV (obtained by inverse)=\n{  CameraTransform_OpenCV }");

                    // https://stackoverflow.com/questions/1263072/changing-a-matrix-from-right-handed-to-left-handed-coordinate-system


                    // UnityToOpenMat is a change of basis matrix, a swap of axes, with a determinmant -1, which is
                    // improper rotation, and so a well-defined quaternion does not exist for it.

                    float4 column0 = new float4(camControl.cameraExternalData.UnityToOpenCVMat.c0, 0);
                    float4 column1 = new float4(camControl.cameraExternalData.UnityToOpenCVMat.c1, 0);
                    float4 column2 = new float4(camControl.cameraExternalData.UnityToOpenCVMat.c2, 0);
                    float4 column3 = new float4(0, 0, 0, 1);


                    float4x4 UnityToOpenCV = new float4x4(column0, column1, column2, column3);

                    float3x3 UnityToOpenCV_Rot = new float3x3(column0.xyz, column1.xyz, column2.xyz);
                    float3x3 OpenCVToUnity_Rot = math.transpose(UnityToOpenCV_Rot);
                    float3 UnityToOpenCV_Trans = column3.xyz;

                    float4x4 OpenCVToUnity = new float4x4(OpenCVToUnity_Rot, -math.mul(OpenCVToUnity_Rot, UnityToOpenCV_Trans));

                    Debug.Log($" UnityToOpenCV inverse = \n {math.inverse(UnityToOpenCV)} ");

                    Debug.Log($" UnityToOpenCV transpose  = \n {OpenCVToUnity}");

                    // Camera Transformation in Unity Frame is defined in terms of the camera transformation in 
                    // the new auxiliary frame, which is the OpenCV frame relative to which the camera transformation
                    // is already determined. 


                    // Change of basis => a simpler form of the same transformation
                    // In the changed frame (OpenCV) , y' = Sx'; In the original frame, y = Tx, where vectors x and y, and the unknown transform
                    // T are in the original basis (Unity). To write T in terms of simpler matrix S:
                    // y' = Sx' => y' = Py, x' = Px by change of basis P.
                    // Py = SPx => y = P^{-1}SP x = Tx => T = P^{-1}SP

                    // P = UnityToOpenCV

                    // CameraTransform_AuxFrame_Unity  = inverse(UnityToOpenCV) * CameraTransform_OpenCV * UnityToOpenCV

                    // O^{t} = W{t} O , where o^{t} is the camera frame relative to the original (Unity) frame
                    // The camera transformation is specified relative to an auxilary frame a^{t}, a^{t}= w^{t}A
                    // o^{t} = a^{t}A^{-1}: Transform o^{t} by M relative to a^{t} (OpenCV frame):
                    // o^{t} => a^{t} M A^{-1} O = w^{t}AMA^{-1}O. => Camera transform from O to AMA^{-1}O,

                    // A = UnityToOpenCV, M =   CameraTransform_OpenCV

                    float4x4 MatForObjectFrame = new float4x4(
                                                new float4(1, 0, 0, 0),
                                                new float4(0, 0, 1, 0),
                                                new float4(0, -1, 0, 0),
                                                new float4(0, 0, 0, 1));

                    float4x4 CameraTransform_Unity = math.mul(
                                                         math.mul(
                                                            math.mul(
                                                                UnityToOpenCV,
                                                                CameraTransform_OpenCV
                                                             ),
                                                          OpenCVToUnity //math.inverse(UnityToOpenCV)
                                                          ),

                                                         MatForObjectFrame
                                                         );



                    // Debug.Log($"Determinimant of OpenCVToUnity\n{( (Matrix4x4)OpenCVToUnity).determinant}");


                    Matrix4x4 CameraTransform_Unity_Mat4x4 = (Matrix4x4)CameraTransform_Unity;
                    Debug.Log($"Determinimant of CameraTransform_Unity_Mat4x4=\n{CameraTransform_Unity_Mat4x4.determinant}");


                    // Camera.main.gameObject.transform.position = GetPosition(CameraTransform_Unity_Mat4x4); 

                    Camera.main.gameObject.transform.position = new Vector3(0, 2.123f, 0);

                    Debug.Log($"Quaternion = CameraTransform_Unity_Mat4x4.rotation=  \n {CameraTransform_Unity_Mat4x4.rotation}");
                    Debug.Log($"QuaternionFromMatrix(MatForUnityCameraFrameMat4x4)\n{DanbiComputeShaderHelper.QuaternionFromMatrix(CameraTransform_Unity_Mat4x4)}");


                    Camera.main.gameObject.transform.rotation = DanbiComputeShaderHelper.GetRotation(CameraTransform_Unity_Mat4x4);
                    // Camera.main.gameObject.transform.rotation = MatForUnityCameraFrameMat4x4.rotation;
                    // quaternion quat = new quaternion(MatForUnityCameraFrame);

                    // Camera.main.gameObject.transform.rotation = quat;                       

                    //https://answers.unity.com/questions/402280/how-to-decompose-a-trs-matrix.html?_ga=2.218542876.407438402.1604700797-1561115542.1585633305


                    Debug.Log($"CameraTransform_Unity_Mat4x4= \n {CameraTransform_Unity_Mat4x4}");


                    Debug.Log($"localToWorldMatrix =\n{ Camera.main.gameObject.transform.localToWorldMatrix}, " +
                        $"\nQuaternion Mat4x4: { Camera.main.gameObject.transform.rotation}, " +

                        // $"\nquaternion quat =\n{quat})" +
                        $"\neulerAngles ={ Camera.main.gameObject.transform.eulerAngles}");
                }

            }
        }

    };
};
