namespace Danbi
{
#pragma warning disable 3001
    [System.Serializable]
    public class DanbiBaseShapeData
    {
        [Readonly]
        public UnityEngine.Vector3 specular; // 12
        [Readonly]
        public UnityEngine.Vector3 emission; // 12        
        [Readonly]
        public UnityEngine.Matrix4x4 local2World; // 64
        [Readonly]
        public UnityEngine.Matrix4x4 world2Local; // 64
        [Readonly]
        public int indexOffset; // 4
        [Readonly]
        public int indexCount; // 4
    };
};
