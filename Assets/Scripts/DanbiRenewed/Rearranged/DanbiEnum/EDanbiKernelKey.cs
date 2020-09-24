namespace Danbi
{
    [System.Serializable]
    public enum EDanbiKernelKey : uint
    {
        None = 0x0000,
        /// <summary>
        /// 0x0101
        /// </summary>
        Halfsphere_Reflector_Cube_Panorama = 0x0101,
        /// <summary>
        /// 0x0102
        /// </summary>
        Halfsphere_Reflector_Cylinder_Panorama = 0x0102,
        /// <summary>
        /// 0x0201
        /// </summary>
        Cone_Reflector_Cube_Panorama = 0x0201,
        /// <summary>
        /// 0x0202
        /// </summary>
        Cone_Reflector_Cylinder_Panorama = 0x0202
    };
}; // namespace Danbi