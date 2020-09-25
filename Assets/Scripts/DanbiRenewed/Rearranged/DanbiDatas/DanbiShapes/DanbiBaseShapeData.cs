namespace Danbi
{    
    public class DanbiBaseShapeData
    {
        public UnityEngine.Vector3 specular; // 4 * 3 = 12
        public UnityEngine.Matrix4x4 local2World; // 4 * 4 * 4 = 64
        public int indexOffset; // 4
        public int indexCount; // 4
    }; // 84
};
