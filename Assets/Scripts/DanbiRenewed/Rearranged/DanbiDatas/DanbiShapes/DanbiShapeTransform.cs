namespace Danbi
{
    [System.Serializable]
    public struct DanbiShapeTransform
    {
        public float Distance; // 4
        public float Height; // 4
        public float Radius; // 4    
        public UnityEngine.Matrix4x4 local2World; // 64
        public int indexCount; // 4
        public int indexOffset; // 4
        public float high; // 4
        public float low; // 4
        public int stride => 92;
    }; // 80
};
