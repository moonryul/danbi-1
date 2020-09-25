namespace Danbi
{
    [System.Serializable]
    public enum EDanbiKernelKey : int
    {
        None = 0x0000,        
        Halfsphere_Reflector_Cube_Panorama = 0x0101,        
        Halfsphere_Reflector_Cylinder_Panorama = 0x0110,        
        Cone_Reflector_Cube_Panorama = 0x1001,    
        Cone_Reflector_Cylinder_Panorama = 0x1010,
        Cylinder_Reflector_Cube_Panorama = 0x10001,
        Cylinder_Reflector_Cylinder_Panorama = 0x10010,
        Pyramid_Reflector_Cube_Panorama = 0x100001,
        Pyramid_Reflector_Cylinder_Panorama = 0x100010,
    };
}; // namespace Danbi