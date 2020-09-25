using UnityEngine;

namespace Danbi
{
    [System.Serializable]
    public class DanbiPanoramaData : DanbiBaseShapeData
    {        
        public float high; // 4
        public float low; // 4

        public int stride => 92;
        public DanbiPanoramaData_struct asStruct => new DanbiPanoramaData_struct()
        {
            local2World = this.local2World,
            high = this.high,
            low = this.low,
            specular = this.specular,
            indexOffset = this.indexOffset,
            indexCount = this.indexCount
        };
    };

    [System.Serializable]
    public struct DanbiPanoramaData_struct
    {
        public Matrix4x4 local2World;
        public float high;
        public float low;
        public Vector3 specular;
        public int indexOffset;
        public int indexCount;
    }; // 80
};
