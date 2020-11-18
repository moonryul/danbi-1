using UnityEngine;
namespace Danbi
{
    [System.Serializable]
    public class DanbiDomeData : DanbiBaseShapeData
    {
        [Readonly]
        public float distance; // 4

        [Readonly]
        public float usedHeight; // 4        

        [Readonly]
        public float radius; // 4                     

        public int stride => 108;

        public DanbiDomeData_struct asStruct => new DanbiDomeData_struct()
        {
            distance = this.distance * 0.01f,
            usedHeight = this.usedHeight * 0.01f,
            radius = this.radius * 0.01f,
            local2World = this.local2World,
            indexCount = this.indexCount,
            indexOffset = this.indexOffset,
            specular = this.specular,
            emission = this.emission
        };
    };

    [System.Serializable]
    public struct DanbiDomeData_struct
    {
        public float distance; // 4
        public float usedHeight; // 4
        public float radius; // 4    
        public Matrix4x4 local2World; // 64
        public int indexCount; // 4
        public int indexOffset; // 4                
        public Vector3 specular; // 12
        public Vector3 emission; // 12
    }; // 96
};
