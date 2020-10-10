namespace Danbi
{
    [System.Serializable]
    public class DanbiHalfsphereData : DanbiBaseShapeData
    {
        [Readonly]
        public float Distance; // 4

        [Readonly]
        public float Height; // 4

        [Readonly]
        public float Radius; // 4                     

        public int stride => 164;

        public DanbiHalfsphereData_struct asStruct => new DanbiHalfsphereData_struct()
        {
            Distance = this.Distance * 0.01f,
            Height = this.Height * 0.01f,
            Radius = this.Radius * 0.01f,
            local2World = this.local2World,
            world2Local = this.world2Local,
            indexCount = this.indexCount,
            indexOffset = this.indexOffset,
            specular = this.specular,
            useTex = this.useTex
        };
    };

    [System.Serializable]
    public struct DanbiHalfsphereData_struct
    {
        public float Distance; // 4
        public float Height; // 4
        public float Radius; // 4    
        public UnityEngine.Matrix4x4 local2World; // 64
        public UnityEngine.Matrix4x4 world2Local; // 64
        public int indexCount; // 4
        public int indexOffset; // 4                
        public UnityEngine.Vector3 specular; // 12
        public int useTex; // 4
    }; // 96
};
