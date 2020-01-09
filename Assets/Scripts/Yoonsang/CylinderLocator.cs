using UnityEngine;

public class CylinderLocator : MonoBehaviour {
  public Transform Actual;
  Mesh ThisMesh;

  void Start() {
    ThisMesh = GetComponent<MeshFilter>().sharedMesh;
    var mesh = ThisMesh.vertices;
  }
}