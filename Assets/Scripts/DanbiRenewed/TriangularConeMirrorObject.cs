using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class TriangularConeMirrorObject : MonoBehaviour {

  public string objectName;
  public int mirrorType;

    // MeshOpticalProperty struct is defined in RayTracingObject.cs file
    // outside of the class defined in that file

    public MeshOpticalProperty mMeshOpticalProperty = new MeshOpticalProperty()
    {
        albedo = new Vector3(0.0f, 0.0f, 0.0f),
        specular = new Vector3(1.0f, 1.0f, 1.0f),
        smoothness = 1.0f,
        emission = new Vector3(0.0f, 0.0f, 0.0f)
    };



    private void OnEnable() {
    RayTracingMaster.RegisterTriangularConeMirror(this);
  }

  private void OnDisable() {
    RayTracingMaster.UnregisterTriangularConeMirror(this);
  }
}