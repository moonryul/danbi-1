using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrintUV : MonoBehaviour
{
    MeshFilter meshFilter;

    Mesh CylinderMesh;

    // cylinder의 원점은 (0,0,0) 이라고 가정한다 
    float radius;
    float height;
    // Start is called before the first frame update
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        CylinderMesh = meshFilter.mesh;

        // 기본 단위 (m)
        radius = 1.5f;
        height = 2.28f;

        List<Vector2> uvs = new List<Vector2>();
        List<Vector3> vertices = new List<Vector3>();
        meshFilter.mesh.GetVertices(vertices);
        meshFilter.mesh.GetUVs(0, uvs);

        Vector2[] newUV = new Vector2[meshFilter.mesh.vertexCount];
        int index = 0;
        foreach(Vector3 _vec in CylinderMesh.vertices)
        {
            newUV[index] = GetUVonCyliner(_vec);
            index++;
        }
        CylinderMesh.uv = newUV;
        
        //for(int i = 0; i< meshFilter.mesh.vertexCount; ++i )
        //{
        //    newUV[i]
        //}
        //int index = 0;

        //Debug.Log("VertexCount " + meshFilter.mesh.vertexCount);
        //foreach (Vector3 _vertex in vertices)
        //{
        //    Debug.Log("VertexIndex : " + _vertex);
        //    Debug.Log("VertexPosition : " + vertices[index]);
        //    Debug.Log("UV : " + uvs[index]);
        //    Debug.Log("\n");

        //    index++;
        //}

    }

    Vector2 GetUVonCyliner(Vector3 _vertexPos)
    {
        Vector2 newUV;
        float degree;

       
        if (_vertexPos.z > 0)  // z > 0 이면 해당 좌표는 0~180도 사이의 각
        {
            degree = Mathf.Acos(_vertexPos.x / radius) * Mathf.Rad2Deg; 
        }
        else  // z < 0  이면 해당 좌표는 180~360도 사이의 각
        {
            degree = 360 - Mathf.Acos(_vertexPos.x / radius) * Mathf.Rad2Deg;
        }
       
        newUV.x = degree / 360;
        // 원점이 0,0,0임을 가정
        newUV.y = _vertexPos.y / height;

        return newUV;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
