using UnityEngine;

namespace Danbi
{
    public class DanbiPanoramaData
    {
        public Matrix4x4 localToWorldMatrix;
        public float high;
        public float low;
        public Vector3 specular;
        public int indices_offset;
        public int indices_count;
    };

    [System.Serializable]
    public struct DanbiPanoramaData_struct
    {
        public Matrix4x4 localToWorldMatrix;
        public float high;
        public float low;
        public Vector3 specular;
        public int indices_offset;
        public int indices_count;
    }; // 80
};
