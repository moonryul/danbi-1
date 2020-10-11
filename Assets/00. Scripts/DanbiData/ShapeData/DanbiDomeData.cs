using UnityEngine;
namespace Danbi
{
    [System.Serializable]
    public class DanbiDomeData : DanbiBaseShapeData
    {
        [Readonly]
        public float Distance; // 4

        [Readonly]
        public float Height; // 4

        [Readonly]
        public float Radius; // 4                     

        public int stride => 108;

        public DanbiDomeData_struct asStruct => new DanbiDomeData_struct()
        {
            Distance = this.Distance * 0.01f,
            Height = this.Height * 0.01f,
            Radius = this.Radius * 0.01f,
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
        public float Distance; // 4
        public float Height; // 4
        public float Radius; // 4    
        public Matrix4x4 local2World; // 64
        public int indexCount; // 4
        public int indexOffset; // 4                
        public Vector3 specular; // 12
        public Vector3 emission; // 12
    }; // 96
};
