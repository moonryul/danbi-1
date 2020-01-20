

using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PyramidMirrorObject : MonoBehaviour
{

    public PyramidMirror mPyramidMirror;

    public Mesh mPyramidMesh; // mesh for the pyramid
  

    //-PYRAMID MIRROR------------------------------------
    public struct PyramidMirror
    {
        public Matrix4x4 localToWorldMatrix; // the world frame of the pyramid
        public float height;
        public float width;  // the radius of the base of the cone
        public float depth;
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
        public int indices_count;

    };

    [System.Serializable]
    public struct PyramidParam
    {
        public float height;
        //public Vector3 Origin; // Origin is (0,0,0) all the time
        public float width;
        public float depth;
    }



    [SerializeField, Header("Pyramid Parameters"), Space(20)]
    public PyramidParam mPyramidParam =  // use "object initializer syntax" to initialize the structure:https://www.tutorialsteacher.com/csharp/csharp-object-initializer
                                         // See also: https://stackoverflow.com/questions/3661025/why-are-c-sharp-3-0-object-initializer-constructor-parentheses-optional

      new PyramidParam
      {
          height = 1f,  // the length unit is  meter
                        // Origin  = new Vector3(0f,0f,0f),
          width = 1f,
          depth = 1f
      };


    Vector3[] MyCalNormals(Mesh mesh)
    {
        Vector3[] normals = new Vector3[mesh.vertices.Length];

        // Use the list of vertex indices for each triangle to compute the normal to the triangle
        // and use it as the normal to the vertices of the triangle

        for (int i = 0; i < mesh.triangles.Length; ++i)
        {

            if ((i + 1) % 3 == 0)
            {
                int i0 = mesh.triangles[i - 2];    // get the index to the vertex for the (i-1) th item in mesh.triangles

                int i1 = mesh.triangles[i - 1];
                int i2 = mesh.triangles[i];

                Vector3 ab = mesh.vertices[i1] - mesh.vertices[i0];
                Vector3 bc = mesh.vertices[i2] - mesh.vertices[i1];
                Vector3 normal = Vector3.Cross(ab, bc).normalized; // Cross() is defined by the Left-Hand Rule
                normals[i - 2] = normal; normals[i - 1] = normal; normals[i] = normal;
            }
            else
            {
                continue;
            }
        }

        return normals;

    } // MyCalNormals()


    public void BuildPyramidMesh()
    {
        //http://www.wolfpack.pe.kr/853
        MeshFilter meshFilter = this.gameObject.GetComponent<MeshFilter>();
        // The  "gameObject" field of the Pyramid Script object is created when this Script (actually an instance of it)
        // is attached a particular object, which is the Pyramid gameObject in this case.   
        if (meshFilter == null)
        {
            Debug.LogError("MeshFilter not found!");
            // for debugging: stop after printing the debug info just once
#if UNITY_EDITOR
            // Application.Quit() does not work in the editor so
            UnityEditor.EditorApplication.isPlaying = false;
#else
                                      Application.Quit();
#endif
            return;
        }


        mPyramidMesh = meshFilter.sharedMesh;

        if (mPyramidMesh == null)
        {
            meshFilter.sharedMesh = new Mesh();
            mPyramidMesh = meshFilter.sharedMesh;
        }
        mPyramidMesh.Clear();


        // Refer to https://blog.nobel-joergensen.com/2010/12/25/procedural-generated-mesh-in-unity/

        //Create a pyramid: https://www.reddit.com/r/Unity3D/comments/3e1rxy/beginner_question_creating_a_pyramid/
        // To understand the structure of a mesh, Read https://blog.nobel-joergensen.com/2010/12/25/procedural-generated-mesh-in-unity/
        // Also: 
        // Refer to http://ilkinulas.github.io/development/unity/2016/04/30/cube-mesh-in-unity3d.html
        // https://catlikecoding.com/unity/tutorials/procedural-grid/
        //https://answers.unity.com/questions/441722/splitting-up-verticies.html

        // Create a mesh for the Pyramid within the Script rather than importing the predefined pyradmid made by 3D tools e.g. Maya

        // create 5 vertices for the pyramid



        // Refer to https://blog.nobel-joergensen.com/2010/12/25/procedural-generated-mesh-in-unity/

        //Create a pyramid: https://www.reddit.com/r/Unity3D/comments/3e1rxy/beginner_question_creating_a_pyramid/
        // To understand the structure of a mesh, Read https://blog.nobel-joergensen.com/2010/12/25/procedural-generated-mesh-in-unity/
        // Also: 
        // Refer to http://ilkinulas.github.io/development/unity/2016/04/30/cube-mesh-in-unity3d.html
        // https://catlikecoding.com/unity/tutorials/procedural-grid/
        //https://answers.unity.com/questions/441722/splitting-up-verticies.html

        // Create a mesh for the Pyramid within the Script rather than importing the predefined pyradmid made by 3D tools e.g. Maya

        // create 5 vertices for the pyramid


        mPyramidParam.width = mPyramidParam.depth;

        Vector3 p0 = new Vector3(-0.5f * mPyramidParam.width, 0.0f, -0.5f * mPyramidParam.depth); // the left/rear corner of the base
        Vector3 p1 = new Vector3(0.5f * mPyramidParam.width, 0.0f, -0.5f * mPyramidParam.depth); // the right/rear   corner
        Vector3 p2 = new Vector3(0.5f * mPyramidParam.width, 0.0f, 0.5f * mPyramidParam.depth); // the right/front corner
        Vector3 p3 = new Vector3(-0.5f * mPyramidParam.width, 0.0f, 0.5f * mPyramidParam.depth);              // the left/front corner
        Vector3 p4 = new Vector3(0.0f, mPyramidParam.height, 0.0f); // the apex

        mPyramidMesh.vertices = new Vector3[]  // duplicate the vertices so that each triangle has its own normal
                                       // p0 is duplicated 4 times, p2 4 times, p1 and p3 3 times, p4 4 times => 18 vertices 
        {
            p0, p1, p2,                 // the front triangle of the base 
            p0, p2, p3,                 // the rear triangle of the base
           // p0, p1,p4,                  // the front side triangle
            p0, p4, p1,
           // p1, p2, p4,                 // the right side triangle
            p1,p4,p2,
           // p2, p3, p4,                  // the rear side triangle 
            p2,p4,p3,
           // p3, p0, p4                   // the left side triangle
           p3,p4,p0
        };

        mPyramidMesh.triangles = new int[]  // The set of indices for each triangle: the normal of the triangle is determined by the left-hand rule
                                    
        {
            0, 1, 2,
            3, 4, 5,
            6, 7, 8,
            9, 10, 11,
            12, 13, 14,
            15, 16, 17
        };

        mPyramidMesh.RecalculateNormals(); // calculate the normal of each vertex using the face normals. 
                                           // Because no vertices are shared among the adjacent triangles, the normals to the vertcies are not 
                                           // interpolated among the adjacent triangles.


    }  //public void BuildPyramidMesh()

    private void OnEnable()
    {
      
        BuildPyramidMesh();   // => creates mPyramidMesh

        // Set the fields of mPyramidMirrorMesh:
        //     public Matrix4x4 localToWorldMatrix; // the world frame of the pyramid
        //public float height;
        //public float width;  // the radius of the base of the cone
        //public float depth;
        //public Vector3 albedo;
        //public Vector3 specular;
        //public float smoothness;
        //public Vector3 emission;
        //public int indices_count;

        mPyramidMirror.localToWorldMatrix = this.gameObject.transform.localToWorldMatrix;
       
        mPyramidMirror.height = mPyramidParam.height;
        mPyramidMirror.depth= mPyramidParam.depth;
        mPyramidMirror.width = mPyramidParam.width;
        mPyramidMirror.albedo = new Vector3(0.0f, 0.0f, 0.0f);
        mPyramidMirror.specular = new Vector3(1.0f, 1.0f, 1.0f);

        mPyramidMirror.smoothness = 0.97f;
        mPyramidMirror.emission = new Vector3(0.0f, 0.0f, 0.0f);
        mPyramidMirror.indices_count = mPyramidMesh.triangles.Length;

        RayTracingMaster.RegisterPyramidMirrorObject(this);
    }

    private void OnDisable()
    {
        RayTracingMaster.UnregisterPyramidMirrorObject(this);
    }
                   

}  // class PyramidMirrorObject
