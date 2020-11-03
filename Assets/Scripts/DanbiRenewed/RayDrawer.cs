using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]

//[ExecuteInEditMode] => Use OnValidate()
public class RayDrawer : MonoBehaviour     // This script will be attached the roomCube gameObject
{

    //What is serialization? https://docs.unity3d.com/Manual/script-Serialization.html?_ga=2.2554163.263825091.1603270151-1561115542.1585633305

    //In Unity, by default the private variables aren't serialized 
    //unless you put the [SerializeField] on the front of them.
    //The public variables are automatically serialized
    //unless you put the[NonSerialized] attribute in the front of them.
    // More info here: https://docs.unity3d.com/Manual/script-Serialization.html

    //When you view or change the value of a GameObject’s component field in the Inspector window, 
    //Unity serializes this data and then displays it in the Inspector  window.
    //==> So, serialization is the first that happens, and the showing in the Inspector view is the
    // result.


    List<GameObject> lrGameObjectList = new List<GameObject>();

    [System.NonSerialized]
    public LineRenderer[,] lineRenderers = new LineRenderer[4, 2];

   
    [SerializeField, Header("The Height of the Room Wall:"), Space(20)]
    protected float heightOfWall = 2.6f; // 260cm

    [SerializeField, Header("The Position of the Topmost Ray:"), Space(20)]
    float topMostRayPosition = 0.0f; // cm
    [SerializeField, Header("The Position of the BottomMost Ray:"), Space(20)]
    float bottomMostRayPosition = -2.13f; // 213cm

    public HemisphereMirrorObject hemisphereMirrorObject;  // referred to in HemisphereMirrorObject.cs

    PanoramaScreenObject panoramaScreenObject;
  

    //https://stackoverflow.com/questions/47207315/how-to-move-a-line-renderer-as-its-game-object-moves
    void CreateLine(int i, int j)
    {
        // A new gameObject is added to the scene
        // this.gameObject is "RoomCube". To draw four lines in it, add thour empty gameObjects
        // to it as children, and attach LineRenderer to each child gameObject.

        GameObject lrGameObject = new GameObject($"lr{i}{j}");
        // This script is attached  to fulCubeRoom gameObject; so the gameObject created becomes a child of the fullCube.

        this.lrGameObjectList.Add(lrGameObject);

        LineRenderer lr = lrGameObject.AddComponent<LineRenderer>();
        this.lineRenderers[i, j] = lr;

        lr.transform.SetParent(this.gameObject.transform, false);
        // just to be sure reset position and rotation as well
        lr.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);

        lr.material = new Material(Shader.Find("Yoonsang/ColoredLine"));

        // Attributes of the line Renderer component is added
        lr.positionCount = 2;
        // 너비 설정.
        lr.startWidth = 0.02f; // 2cm
        lr.endWidth = 0.02f;   // 2cm
                               //lr.useWorldSpace = true;      // false=> lines are defined relative to the gameObject to which the

        lr.useWorldSpace = false; // line positions are relative to the gameObject to which 
                                  // the lineRenderer component is attached. 



    }  // CreateLine


    void GetHitPointAndDirectionAtMirror(int i, float usedHeight,
                        out Vector3 hitPosInWorld, out Vector3 incidentVector)
    {


        float h, y, r, x, z;

        Matrix4x4 hemisphereTransform = this.hemisphereMirrorObject.gameObject.transform.localToWorldMatrix;

        Debug.Log($"hemisphereTransform in GetHitPointAndDIrectionAtMirror=\n{hemisphereTransform}");

        r = this.hemisphereMirrorObject.hemisphereParam.sphereRadius;

        Debug.Log($"hemisphere radius  in GetHitPointAndDIrectionAtMirror=\n{r}");
        h = usedHeight; 
        y = (r - h);  // z = the distance of the bottom of the semi-hemisphere from the origin of the hemisphere

        //Matrix4x4 camTrans = Camera.main.transform.localToWorldMatrix;
        // The gameObject to which this script (RayDrawer.cs) is attached is the roomCube.

        //Matrix4x4 roomCubeTransform  = this.gameObject.transform.localToWorldMatrix;
        //Debug.Log($"roation={MainCamera.transform.rotation.eulerAngles}");
        //Debug.Log($"forward={MainCamera.transform.forward}");
        //Debug.Log($"right={MainCamera.transform.right}");


        Vector4 intersectionPoint4;

        if (i == 0)
        {
            // the ray to the left on the xy plane of the hemisphere 

            // x^2 + y^2 = r^2
            // y = (r-h)
            // x^2 + r^2 - 2rh + h^2 = r^2
            x = -Mathf.Sqrt(2 * r * h - h * h );

            //x = -Mathf.Sqrt(r * r - (r - h) * (r - h));  // within the hemisphere
            z = 0.0f;
            intersectionPoint4 = hemisphereTransform * new Vector4(x, y, z, 1.0f); // incidentVector in gloabl frame

            hitPosInWorld = intersectionPoint4; // implicit conversion from Vector4 to Vector3
            incidentVector = hitPosInWorld - Camera.main.transform.position;


            Debug.Log($"i ={i}, incidentVector={incidentVector.ToString("F4")}," +
                $" hitPosInWorld={hitPosInWorld.ToString("F4")}");
        }
        else if (i == 1)
        {
            // the ray to the right on the xy plane of the hemisphere

            //x = Mathf.Sqrt(r * r - (r - h) * (r - h));
            x = Mathf.Sqrt(2 * r * h - h * h);
            z = 0.0f;
            intersectionPoint4 = hemisphereTransform * new Vector4(x, y, z, 1.0f);


            hitPosInWorld = intersectionPoint4;

            incidentVector = hitPosInWorld - Camera.main.transform.position;


            Debug.Log($"i ={i}, incidentVector={incidentVector.ToString("F4")}," +
                $" hitPosInWorld={hitPosInWorld.ToString("F4")}");

        }
        else if (i == 2)
        {

            // the ray to the front on the zy plane  of the hemisphere

            // z = -Mathf.Sqrt(r * r - (r - h) * (r - h));
            z = -Mathf.Sqrt(2 * r * h  - h * h);
            x = 0;
            intersectionPoint4 = hemisphereTransform * new Vector4(x, y, z, 1.0f);
            hitPosInWorld = intersectionPoint4;
            incidentVector = hitPosInWorld - Camera.main.transform.position;


            Debug.Log($"i ={i}, incidentVector={incidentVector.ToString("F4")}, " +
                $"hitPosInWorld={hitPosInWorld.ToString("F4")}");
        }
        else // (i == 3)
        {  // the ray to the back on the zy plane    of the hemisphere

            //z = Mathf.Sqrt(r * r - (r - h) * (r - h));
            z = Mathf.Sqrt(2 * r * h - h * h);
            x = 0;
            intersectionPoint4 = hemisphereTransform * new Vector4(x, y, z, 1.0f);
            hitPosInWorld = intersectionPoint4;
            incidentVector = hitPosInWorld - Camera.main.transform.position;


            Debug.Log($"i ={i}, incidentVector={incidentVector.ToString("F4")}, " +
                $"hitPosInWorld={hitPosInWorld.ToString("F4")}");



        }
        return;

    } //GetIntersectionAtMirrorBottom

    Vector3 GetReflectionDir(GameObject hemisphereObj, Vector3 hitPosInWorld, Vector3 incidentVector)
    {

        Vector3 reflectionDir;
        Vector3 normal;
        Vector3 sphereOrigin = hemisphereObj.transform.position;

        normal = (hitPosInWorld - sphereOrigin).normalized;

        Vector3 reflectionVector = incidentVector - 2 * Vector3.Dot(incidentVector, normal) * normal;

        Debug.Log($"reflectionVector in GetReflectionDir() ={reflectionVector},hitPosInWorld={ hitPosInWorld}, incidentVector={ incidentVector}");

        reflectionDir = reflectionVector.normalized;

        return reflectionDir;

    }    //GetReflectionDir

    Vector3 GetIntersectionWithPlane(int i, Vector3 hitPosInWorld, Vector3 reflectionDir)
    {


        //this.gameObject is the "roomCube" gameObject to which this script is attached.
        Matrix4x4 roomTrans = this.gameObject.transform.localToWorldMatrix;

        Vector4 n4;
        Vector4 p04;

        if (i == 0)
        {
            // the ray to the left on the xy plane of the cubeRoom    => get the intersection with 
            // the line with the left side plane.
            n4 = roomTrans * (new Vector4(1, 0, 0, 0));   // relative to the roomCube frame.
            p04 = roomTrans * (new Vector4(-1.6f, 0, -1.6f, 1));    // on the ground (in the worldspac)

        }
        else if (i == 1)
        {  // the ray to the right on the xy plane
            n4 = roomTrans * new Vector4(-1, 0, 0, 0);
            p04 = roomTrans * new Vector4(1.6f, 0, -1.6f, 1);


        }
        else if (i == 2)
        { // the ray to the front on the zy plane
            n4 = roomTrans * new Vector4(0, 0, 1, 0);
            p04 = roomTrans * new Vector4(1.6f, 0, -1.6f, 1);

        }
        else
        {  // the ray to the back on the zy plane
            n4 = roomTrans * new Vector4(0, 0, -1, 0);
            p04 = roomTrans * new Vector4(1.6f, 0, 1.6f, 1);

        }
        

        //  //d= ( p_{0}  - l_{0} )\dot {n} \over {l}\dot {n}.
        Vector3 p0 = new Vector3(p04.x, p04.y, p04.z);
        Vector3 n = new Vector3(n4.x, n4.y, n4.z);

        float ln = Vector3.Dot(reflectionDir, n);
        float t;


        if (ln == 0.0f)
        {
            Debug.Log($"ray ={i} in Ray Drawer:Ray direction {reflectionDir}  is parallel to plane {n} and cannot intersect it=> Go and Stop Manually");

           // Utils.StopPlaying();
            
        }

        t = Vector3.Dot((p0 - hitPosInWorld), n) / ln;

        return (hitPosInWorld + t * reflectionDir);


    }   //GetIntersectionWithPlane

    public void OnDrawRays()
    {
        if (this.lrGameObjectList.Count == 0)
        {
            CreateRays();        
        }

        UpdateRays();
    }

    private void OnEnable()
    {
       // Debug.Log("OnEnable() is called before Play button? NO");
    }
    private void Awake()
    {
      
    
    }

    private void Start()
    {
        this.hemisphereMirrorObject = this.gameObject.transform.GetChild(2).GetComponent<HemisphereMirrorObject>();
             
        Assert.AreNotEqual(this.hemisphereMirrorObject, null, "m_HemisphereMirrorObject should not be null");
          

    }

    private  void CreateRays()
    {

        for (int i = 0; i < 4; ++i)
            for (int j = 0; j < 2; j++)  // j=0: bottommost ray; j=1: topmost ray
            {
                // i=0: the ray to the left on the xy plane, i=1: the ray to the right on the xy plane
                // i=2: the ray to the front on the zy plane, i=3: the ray to the back on the zy plane

                CreateLine(i, j);


            }

    }  // CreateRays()

    public void UpdateRays()
    {


        // The bottommost rays             

        for (int i = 0; i < 2; ++i)
            
        {   // i=0: the ray to the left on the xy plane, i=1: the ray to the right on the xy plane
            // i=2: the ray to the front on the zy plane, i=3: the ray to the back on the zy plane

            int j = 0;

            LineRenderer lrij = this.lineRenderers[i, j];

           float depthFromTopOfMirror = this.hemisphereMirrorObject.hemisphereParam.usedHeight;

            Vector3 hitPosInWorld, incidentVector;

            GetHitPointAndDirectionAtMirror(i, depthFromTopOfMirror,
                                               out hitPosInWorld, out incidentVector);

            Vector3 reflectionDir = GetReflectionDir(this.hemisphereMirrorObject.gameObject, hitPosInWorld, incidentVector);

           // Debug.Log($" ray-{i}: relfectionDir ={reflectionDir} in UpdateRays with hisPosInWorld={hitPosInWorld}, incidentVector={incidentVector}");

            if ( reflectionDir == new Vector3(0.0f, 0.0f, 0.0f))
            {
                Debug.Log($" relfectionDir is zero with hisPosInWorld={hitPosInWorld}, incidentVector={incidentVector}");
            }
            Vector3 endPosInWorld = GetIntersectionWithPlane(i, hitPosInWorld, reflectionDir);

            //Debug.Log($"i={i}{j}: HemiMirrorCenter={ m_HemisphereMirrorObject.gameObject.transform.position.ToString("F4")},");
            //Debug.Log($"i={i}{j}:HitPosInWorld={hitPosInWorld.ToString("F4")},");
            //Debug.Log($"i={i}{j}:IncidentVector={incidentVector.ToString("F4")},");
            //Debug.Log($"i={i}{j}:ReflectionDir={reflectionDir.ToString("F4")}, endPosInWorld={endPosInWorld.ToString("F4")}");

                                                 

            Vector3 startPos = hitPosInWorld;
            Vector3 endPos = endPosInWorld;

           

            // lr.useWorldSpace = false; // line positions are relative to the gameObject to which 
            // the lineRenderer component is attached. 

            Vector4 startPos4 = startPos; 
            Matrix4x4 worldToLocalij  = lrij.gameObject.transform.worldToLocalMatrix;

            Vector4 startPosLocal4 = worldToLocalij * startPos4;
            Vector3 startPosLocal = startPosLocal4;


            Vector4 endPos4 = endPos;
                       
            Vector4 endPosLocal4 = worldToLocalij * endPos4;
            Vector3 endPosLocal = endPosLocal4;

            //lr.widthCurve = new AnimationCurve(
            //     new Keyframe(0, 0.4f) // 0.4 at time 0
            //     , new Keyframe(0.999f - PercentHead, 0.4f)  // neck of arrow
            //     , new Keyframe(1 - PercentHead, 1f)  // max width of arrow head
            //     , new Keyframe(1, 0f));  // tip of arrow
            //lr.SetPositions(new Vector3[] {
            //      startPosLocal
            //      , Vector3.Lerp(startPosLocal, endPosLocal, 0.999f - PercentHead)
            //      , Vector3.Lerp(startPosLocal, endPosLocal, 1 - PercentHead)
            //      , endPosLocal });

            lrij.SetPosition(0, startPosLocal);
            lrij.SetPosition(1, endPosLocal);

            // How to draw arrows: https://answers.unity.com/questions/1100566/making-a-arrow-instead-of-linerenderer.html

            bottomMostRayPosition = endPosInWorld.y - Camera.main.transform.position.y;


            // The topmost rays
            j= 1;
            lrij = lineRenderers[i, j];

            // i=0: the ray to the left on the xy plane, i=1: the ray to the right on the xy plane
            // i=2: the ray to the front on the zy plane, i=3: the ray to the back on the zy plane


            Vector3 prevHitPosInWorld;
            Vector3 prevEndPosInWorld;
            Vector3 prevReflectionDir, prevIncidentVector;


            prevHitPosInWorld = hitPosInWorld;
            prevEndPosInWorld = endPosInWorld;
            prevReflectionDir = reflectionDir;
            prevIncidentVector = incidentVector;

            while (endPosInWorld.y < heightOfWall)
            {
                prevHitPosInWorld = hitPosInWorld;
                prevEndPosInWorld = endPosInWorld;
                prevReflectionDir = reflectionDir;
                prevIncidentVector = incidentVector;

                depthFromTopOfMirror -= 0.005f; // 0.5cm
                GetHitPointAndDirectionAtMirror(i, depthFromTopOfMirror,
                                              out hitPosInWorld, out incidentVector);


                reflectionDir = GetReflectionDir(this.hemisphereMirrorObject.gameObject, hitPosInWorld, incidentVector);
                endPosInWorld = GetIntersectionWithPlane(i, hitPosInWorld, reflectionDir);

            }

            //// create the topmost ray line
            //Debug.Log("************************************The topmost ray");

            //Debug.Log($"i={i}{j}:HemiMirrorCenter={ m_HemisphereMirrorObject.gameObject.transform.position.ToString("F4")}");
            //Debug.Log($"i={i}{j}:PrevHitPosInWorld={prevHitPosInWorld.ToString("F4")}");
            //Debug.Log($"i={i}{j}:PrevIncidentVector={prevIncidentVector.ToString("F4")}");
            //Debug.Log($"i={i}{j}:PrevReflectionDir={prevReflectionDir.ToString("F4")} ");
            //Debug.Log($"i={i}{j}:PrevendPosInWorld={prevEndPosInWorld.ToString("F4")}");

            //Debug.Log($"i={i}{j}:HitPosInWorld={hitPosInWorld.ToString("F4")}");
            //Debug.Log($"i={i}{j}:IncidentVector={incidentVector.ToString("F4")}");
            //Debug.Log($"i={i}{j}:ReflectionDir={reflectionDir.ToString("F4")} ");
            //Debug.Log($"i={i}{j}:endPosInWorld={endPosInWorld.ToString("F4")}");

          


            // Transform the start and end position of the line into the frame
            // of the gameObject to which the lineRenderer is attached.
            startPos = prevHitPosInWorld;
            endPos = prevEndPosInWorld;

            topMostRayPosition = endPos.y - Camera.main.transform.position.y;

       

            startPos4 = startPos;
            startPosLocal4 = lrij.gameObject.transform.worldToLocalMatrix * startPos4;
            startPosLocal = startPosLocal4;


            endPos4 = endPos;
            endPosLocal4 =lrij.gameObject.transform.worldToLocalMatrix * endPos4;
            endPosLocal = endPosLocal4;

            lrij.SetPosition(0, startPosLocal);
            lrij.SetPosition(1, endPosLocal);


        }   //for (int i = 0; i < 4; ++i)


        // Set the two variables of m_PanoramaScreenObject component

        this.panoramaScreenObject = this.gameObject.transform.GetChild(3).GetComponent<PanoramaScreenObject>();
        Assert.AreNotEqual(this.panoramaScreenObject, null, "m_PanoramaScreenObject should not be null");

        this.panoramaScreenObject.topMostRayPosition = topMostRayPosition;
        this.panoramaScreenObject.bottomMostRayPosition = bottomMostRayPosition;


    }  // UpdateRays()

    //https://forum.unity.com/threads/onvalidate-gets-called-at-startup-before-properties-in-other-components-have-deserialized.452658/

    //https://forum.unity.com/threads/how-does-onvalidate-work.616372/
    //This function is called when the script is loaded or a value is changed in the inspector (Called in the editor only).

    //Private variables aren't serialized. That is exactly what's causing this.
    // In editor they're fetched and assigned anyway (because OnValidate is called),
    //but their references are lost as soon as assembly reloads.
    // And in build, .OnValidate is not called ever.

    //You can use this to ensure that when you modify data in an editor, that data stays within a certain range.
    //It's highly advisable to use Awake to initialize your private variables instead of OnValidate. OnValidate is for Editor time to check the values when someone change them in the inspector. Nothing else.
    //It won't initialize for you in the build if Unity didn't serialize the field by itself.

    void OnValidate()
    {
        Debug.Log("OnValidate in RayDrawer: Nothing to check");

    } // OnValidate()

    void  OnApplicationQuit()
    {  // destory the gameObjects for the lineRenderers
        Debug.Log("OnApplicationQuit() is called in RayDrawer");
        for (int i = 0; i < lrGameObjectList.Count; i++)
        {
            GameObject go = lrGameObjectList[i];
            Object.Destroy(go);

        }
    }
};