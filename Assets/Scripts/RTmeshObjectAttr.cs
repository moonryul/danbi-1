public struct RTmeshObjectAttr {       // Byte Offsets.
  public UnityEngine.Matrix4x4 Local2WorldMatrix;  // 4 * 16 = 64
  public int IndicesOffset;            // 4
  public int IndicesCount;             // 4
  public int colorMode;                // 4
  public UnityEngine.Vector3 Albedo;     // 12
  public UnityEngine.Vector3 Specular;   // 12
  public UnityEngine.Vector3 Emission;   // 12
  public UnityEngine.Vector3 Smoothness; // 12
  public static readonly int SizeOfAttr = 124;
};                                     // Required Byte Offsets -> 124.
