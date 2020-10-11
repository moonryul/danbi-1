using UnityEngine;

namespace Danbi
{
    [System.Serializable]
    public class DanbiPanoramaData : DanbiBaseShapeData
    {
        [Readonly]
        public float high; // 4

        [Readonly]
        public float low; // 4

        public int stride => 168;
        public DanbiPanoramaData_struct asStruct => new DanbiPanoramaData_struct()
        {
            high = this.high * 0.01f,
            low = this.low * 0.01f,
            local2World = this.local2World,
            world2Local = this.world2Local,
            indexCount = this.indexCount,
            indexOffset = this.indexOffset,
            specular = this.specular,
            emission = this.emission
        };
    };

    [System.Serializable]
    public struct DanbiPanoramaData_struct
    {
        public float high; // 4
        public float low; // 4
        public Matrix4x4 local2World; // 64
        public Matrix4x4 world2Local; // 64
        public int indexCount; // 4
        public int indexOffset; // 4
        public Vector3 specular; // 12
        public Vector3 emission; // 12
    }; // 92
};
