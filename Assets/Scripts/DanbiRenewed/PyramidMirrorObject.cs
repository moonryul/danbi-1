

using UnityEngine;


[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]
public class PyramidMirrorObject : MonoBehaviour
{

    [System.Serializable]
    public struct MeshOpticalProperty
    {
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
    };

    public int mMirrorType;

   
    public MeshOpticalProperty mMeshOpticalProperty = new MeshOpticalProperty()
    {
        albedo = new Vector3(0.0f, 0.0f, 0.0f),
        specular = new Vector3(1.0f, 1.0f, 1.0f),
        smoothness = 1.0f,
        emission = new Vector3(0.0f, 0.0f, 0.0f)
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




    private void OnEnable()
    {
      
        RayTracingMaster.RegisterPyramidMirror(this);
    }

    private void OnDisable()
    {
        RayTracingMaster.UnregisterPyramidMirror(this);
    }
                   

}  // class PyramidMirrorObject
