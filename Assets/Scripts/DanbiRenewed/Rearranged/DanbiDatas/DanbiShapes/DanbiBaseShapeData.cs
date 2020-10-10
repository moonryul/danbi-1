namespace Danbi
{
    public class DanbiBaseShapeData
    {
        [Readonly]
        public UnityEngine.Vector3 specular; // 4 * 3 = 12
        [Readonly]
        public UnityEngine.Vector3 emission; // 4 * 3 = 12

        [Readonly]
        public UnityEngine.Matrix4x4 local2World; // 4 * 4 * 4 = 64

        [Readonly]
        public UnityEngine.Matrix4x4 world2Local;

        [Readonly]
        public int indexOffset; // 4

        [Readonly]
        public int indexCount; // 4
    }; // 84
};
