namespace Danbi
{
    [System.Serializable]
    public class DanbiHalfsphereData : DanbiBaseShapeData
    {
        public float Distance; // 4
        public float Height; // 4
        public float Radius; // 4                     
        public int stride => 96;
        public DanbiHalfsphereData_struct asStruct => new DanbiHalfsphereData_struct()
        {
            Distance = this.Distance,
            Height = this.Height,
            Radius = this.Radius,
            local2World = this.local2World,
            indexCount = this.indexCount,
            indexOffset = this.indexOffset
        };
    };

    [System.Serializable]
    public struct DanbiHalfsphereData_struct
    {
        public float Distance; // 4
        public float Height; // 4
        public float Radius; // 4    
        public UnityEngine.Matrix4x4 local2World; // 64
        public int indexCount; // 4
        public int indexOffset; // 4                
    };
};
