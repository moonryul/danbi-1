using UnityEngine;
using Unity.Mathematics;
namespace Danbi
{
    [System.Serializable]


    public class DanbiCameraInternalData
    {
        public float3 radialCoefficient;
        public float2 tangentialCoefficient;
        public float2 principalPoint;
        public float2 focalLength;

        public int stride => 36;
        public DanbiCameraInternalData_struct asStruct => new DanbiCameraInternalData_struct()
        {
            radialCoefficient = this.radialCoefficient,
            tangentialCoefficient = this.tangentialCoefficient,
            principalPoint = this.principalPoint,
            focalLength = this.focalLength
        };
    };

    [System.Serializable]
    public struct DanbiCameraInternalData_struct
    {
        public float3 radialCoefficient;
        public float2 tangentialCoefficient;
        public float2 principalPoint;
        public float2 focalLength;
    }; // 216
};
