namespace Danbi
{
    [System.Serializable]
    public struct DanbiPanoramaShapeTransform
    {
        public float Distance; // 4
        public float Height; // 4
        public float Radius; // 4    
        public UnityEngine.Matrix4x4 local2World; // 64        

        public int stride => 84;
    }; // 80
};
