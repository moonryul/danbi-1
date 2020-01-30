using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Color mode stands for which is applied for the mesh object.
/// </summary>
[System.Serializable]
public enum eColorMode {
  NONE = 0,
  TEXTURE = 1,
  VERTEX_COLOR = 2
};

/// <summary>
/// The mesh object for ray tracing. every mesh object for the ray tracing shader must inherit this class.
/// </summary>
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class RTmeshObject : MonoBehaviour {
  public eColorMode ColorMode;
  //public Vector3 Albedo;
  //public Vector3 Specular;
  //public Vector3 Emission;
  //public Vector3 Smoothness;  

  /// <summary>
  /// OnEnable(), all the references of this gameObject is registered into the RTmeshObjectsList
  /// To rebuild every mesh objects!
  /// </summary>
  public virtual void OnEnable() {
    Assert.IsFalse(gameObject.isStatic, "Mesh Objects cannot be static!");
    RTcomputeShaderHelper.RegisterToRTmeshObjectsList(this);
  }

  void Start() {
    var pivotLoc = transform.position;
    Debug.Log($"Object {gameObject.name} : ({pivotLoc.x}, {pivotLoc.y}, {pivotLoc.z})", this);
  }

  /// <summary>
  /// OnDisable(), all the references inside the RTmeshObjectsList is removed.
  /// </summary>
  public virtual void OnDisable() {
    RTcomputeShaderHelper.UnregisterFromRTmeshObjectsList(this);
  }
};
