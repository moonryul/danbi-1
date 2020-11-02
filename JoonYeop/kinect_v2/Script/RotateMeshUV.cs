using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateMeshUV : MonoBehaviour
{
    // 파노라마 벽 메쉬를 담을 변수
    Mesh mesh;

    // 회전각 -> kinectAction script에서 처리 후 값을 받을 예정
    public float InteractionRotateAngle;

    // 원본
    List<Vector2> NewMeshUVList = new List<Vector2>();
    List<Vector2> OriginMeshUVList = new List<Vector2>();

    private void Awake()
    {
        // 스크린의 메쉬를 얻어옴
        mesh = GetComponent<MeshFilter>().mesh;
        
        
        InteractionRotateAngle = 0;

        
        mesh.GetUVs(0, NewMeshUVList); // 0 -> first uv map
        mesh.GetUVs(0, OriginMeshUVList);


    }
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        // 각 정점을 순환하며 회전하고 싶은만큼 uv의 x좌표를 변경 
        // 스크린면이 4개이기 때문에 정점갯수 = 16
        for(int i = 0; i< OriginMeshUVList.Count; ++i)
        {
            // 새로운 uv의 x좌표 = 기존의 uv의 x좌표 + 회전각
          
            float meshUV_x =  OriginMeshUVList[i].x + (InteractionRotateAngle / 360);
            //if (meshUV_x > 1)
            //{
            //    meshUV_x -= 1;
            //}
            NewMeshUVList[i] = new Vector2(meshUV_x, NewMeshUVList[i].y);
        }
        // 스크린 메시의 새로운 uv를 설정
        mesh.SetUVs(0, NewMeshUVList);
    }
}
