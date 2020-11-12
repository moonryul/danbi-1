using Unity.Mathematics;

namespace Danbi
{
    [System.Serializable]
    public class DanbiCameraExternalData
    {
        public float3 projectorPosition;
        public float3 xAxis;
        public float3 yAxis;
        public float3 zAxis;

        public int stride => 4 * 3 * 4; // 48

        public DanbiCameraExternalData_struct asStruct
        {
            get
            {
                return new DanbiCameraExternalData_struct
                {
                    cameraPosition = this.projectorPosition,
                    xAxis = this.xAxis,
                    yAxis = this.yAxis,
                    zAxis = this.zAxis
                };
            }
        }

        public float3x3 UnityToOpenCVMat
        {
            get
            {
                return new float3x3(
                    1, 0, 0,
                    0, 0, -1,
                    0, -1, 0
                );
            }
        }
    };

    /// <summary>
    /// Converted struct from the original class to make a compute buffer.
    /// </summary>
    [System.Serializable]
    public struct DanbiCameraExternalData_struct
    {
        public float3 cameraPosition;
        public float3 xAxis;
        public float3 yAxis;
        public float3 zAxis;
    }; // 48
};