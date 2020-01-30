using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(MeshFilter))]
public class NormalVisualizer : Editor {
  Mesh mesh;

  void OnEnable() {
    var mf = target as MeshFilter;
    if (mf != null) {
      mesh = mf.sharedMesh;
    }
  }

  void OnSceneGUI() {
    if (mesh == null) {
      return;
    }

    for (int i = 0; i < mesh.vertexCount; i++) {
      //if (i % 2 == 0) continue;
      Handles.matrix = (target as MeshFilter).transform.localToWorldMatrix;
      Handles.color = Color.yellow;
      Handles.DrawLine(
          mesh.vertices[i],
          mesh.vertices[i] + mesh.normals[i] * 0.3f);
    }
  }
};
#endif
