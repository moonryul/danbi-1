namespace Danbi
{
    public class DanbiCyclinderData : DanbiBaseShapeData
    {
        public float Distance;
        public float Height;
        public float Radius;

        public DanbiCyclinderData_struct asStruct => new DanbiCyclinderData_struct()
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
    public struct DanbiCyclinderData_struct
    {
        public float Distance; // 4
        public float Height; // 4
        public float Radius; // 4    
        public UnityEngine.Matrix4x4 local2World; // 64
        public int indexCount; // 4
        public int indexOffset; // 4                
    };
};