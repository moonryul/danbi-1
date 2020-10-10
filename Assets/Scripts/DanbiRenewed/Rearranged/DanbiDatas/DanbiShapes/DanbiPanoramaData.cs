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

        public int stride => 160;
        public DanbiPanoramaData_struct asStruct => new DanbiPanoramaData_struct()
        {
            local2World = this.local2World,
            world2Local = this.world2Local,
            high = this.high * 0.01f,
            low = this.low * 0.01f,
            specular = this.specular,
            useTex = this.useTex,
            indexOffset = this.indexOffset,
            indexCount = this.indexCount
        };
    };

    [System.Serializable]
    public struct DanbiPanoramaData_struct
    {
        public float high; // 4
        public float low; // 4
        public Matrix4x4 local2World; // 64
        public Matrix4x4 world2Local; // 64
        public int indexOffset; // 4
        public int indexCount; // 4
        public Vector3 specular; // 12
        public int useTex; // 4
    }; // 92
};
