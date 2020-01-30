using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class GetMeshInfo : MonoBehaviour {

    public string objectName;
 
  private void OnEnable() {

        Debug.Log(" object=" + objectName);

        var mesh = this.gameObject.GetComponent<MeshFilter>().sharedMesh;

        //Debug.Log( (cnt++)  + "th mesh:");
        //for (int i = 0; i < mesh.vertices.Length; i++)
        //{
        //    Debug.Log(i + "th vertex=" + mesh.vertices[i].ToString("F6"));

        //}
        // Ways to get other components (sibling components) of the gameObject to which 
        // this component is attached:
        // this.GetComponent<T>, where this is a component class
        // this.gameObject.GetComponent<T> does the same thing

        
        int[] indices = mesh.GetIndices(0); // mesh.Triangles() is a special  case of this method
                                            // when the mesh topology is triangle;
                                            // indices will contain a multiple of three indices
                                            // our mesh is actually a triangular mesh.

        // show the local coordinates of the triangles
        for (int i = 0; i < indices.Length; i += 3)
        {   // a triangle v0,v1,v2 

            Debug.Log("triangle vertex (local) =(" + mesh.vertices[indices[i]].ToString("F6")
                      + "," + mesh.vertices[indices[i + 1]].ToString("F6")
                      + "," + mesh.vertices[indices[i + 2]].ToString("F6") + ")");



        }
       
  }

  private void OnDisable() {
  
  }
}