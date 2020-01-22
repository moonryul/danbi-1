using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RayTracingMaster : MonoBehaviour {
  public ComputeShader RayTracingShader;
  public Texture SkyboxTexture;
  public Texture _RoomTexture;
  public Light DirectionalLight;

  [Header("Spheres")]
  public int SphereSeed;
  public Vector2 SphereRadius = new Vector2(3.0f, 8.0f);
  public uint SpheresMax = 100;
  public float SpherePlacementRadius = 100.0f;

  Camera _camera;
  float _lastFieldOfView;
  RenderTexture _target;
  RenderTexture _converged;
  Material _addMaterial;
  uint _currentSample = 0;
  ComputeBuffer _sphereBuffer;
  List<Transform> _transformsToWatch = new List<Transform>();
  static bool _meshObjectsNeedRebuilding = false;
  static bool _pyramidMeshObjectsNeedRebuilding = false;

  static List<RayTracingObject> _rayTracingObjects = new List<RayTracingObject>(); 
    
  static List<MeshObject> _meshObjects = new List<MeshObject>();

  MeshObjectRW[] _meshObjectArray;

  static List<Vector3> _vertices = new List<Vector3>();
  static List<int> _indices = new List<int>();
  static List<Vector2> _texcoords = new List<Vector2>();

 
  ComputeBuffer _meshObjectBuffer;
  ComputeBuffer _meshObjectBufferRW;

    ComputeBuffer _vertexBuffer;
  ComputeBuffer _indexBuffer;
  ComputeBuffer _texcoordsBuffer;

    ComputeBuffer _vertexBufferRW;
    // added by Moon Jung
    static List<PyramidMirrorObject.PyramidMirror> _pyramidMirrors
                             = new List<PyramidMirrorObject.PyramidMirror>();
  static List<PyramidMirrorObject> _pyramidMirrorObjects
                             = new List<PyramidMirrorObject>();
  
  static List<Vector3> _pyramidMeshVertices = new List<Vector3>();
  static List<int> _pyramidMeshIndices = new List<int>();
  ComputeBuffer _pyramidMirrorBuffer;
  ComputeBuffer _pyramidMeshVertexBuffer;
  ComputeBuffer _pyramidMeshIndexBuffer;

    // for debugging

    public ComputeBuffer mRayDirectionBuffer;
    public ComputeBuffer mIntersectionBuffer;
    public ComputeBuffer mAccumRayEnergyBuffer;
    public ComputeBuffer mEmissionBuffer;
    public ComputeBuffer mSpecularBuffer;
    //
    // ComputeBuffer(int count, int stride, ComputeBufferType type);
    // 
    Vector3[] mVertexArray;
    Vector4[] mRayDirectionArray;
    Vector4[] mIntersectionArray, mAccumRayEnergyArray, mEmissionArray, mSpecularArray;
        //
    struct MeshObject {
    public Matrix4x4 localToWorldMatrix;
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
    public int indices_offset;
    public int indices_count;
  }
    struct MeshObjectRW
    {
        public Matrix4x4 localToWorldMatrix;
        public Vector3 albedo;
        public Vector3 specular;
        public float smoothness;
        public Vector3 emission;
    }



    struct Sphere {
    public Vector3 position;
    public float radius;
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
  }

  void Awake() {
    _camera = this.GetComponent<Camera>();

    _transformsToWatch.Add(transform);
    _transformsToWatch.Add(DirectionalLight.transform);

    
  }

  void OnEnable() {
    _currentSample = 0;
   // SetUpScene();      commented out by Moon Jung, because this creates spheres
  }

  void OnDisable() {
    _sphereBuffer?.Release();
    _meshObjectBuffer?.Release();
    _vertexBuffer?.Release();
    _indexBuffer?.Release();
  }

  void Update() {
    if (Input.GetKeyDown(KeyCode.F12)) {
      ScreenCapture.CaptureScreenshot(Time.time + "-" + _currentSample + ".png");
    }

    if (_camera.fieldOfView != _lastFieldOfView) {
      _currentSample = 0;
      _lastFieldOfView = _camera.fieldOfView;
    }

    foreach (var t in _transformsToWatch) {
      if (t.hasChanged) {
        _currentSample = 0;
        t.hasChanged = false;
      }
    }
  }

  public static void RegisterObject(RayTracingObject obj) {
    _rayTracingObjects.Add(obj);
    _meshObjectsNeedRebuilding = true;
  }
  public static void UnregisterObject(RayTracingObject obj) {
    _rayTracingObjects.Remove(obj);
    _meshObjectsNeedRebuilding = true;
  }

  public static void RegisterPyramidMirrorObject(PyramidMirrorObject obj) {

    _pyramidMirrorObjects.Add(obj);
    _pyramidMeshObjectsNeedRebuilding = true;
  }
  public static void UnregisterPyramidMirrorObject(PyramidMirrorObject obj) {
    _pyramidMirrorObjects.Remove(obj);
    _pyramidMeshObjectsNeedRebuilding = true;
  }

  void SetUpScene() {
    Random.InitState(SphereSeed);
    var spheres = new List<Sphere>();

    // Add a number of random spheres
    for (int i = 0; i < SpheresMax; i++) {
      var sphere = new Sphere();

      // Radius and radius
      sphere.radius = SphereRadius.x + Random.value * (SphereRadius.y - SphereRadius.x);
      var randomPos = Random.insideUnitCircle * SpherePlacementRadius;
      sphere.position = new Vector3(randomPos.x, sphere.radius, randomPos.y);

      // Reject spheres that are intersecting others
      foreach (var other in spheres) {
        float minDist = sphere.radius + other.radius;
        if (Vector3.SqrMagnitude(sphere.position - other.position) < minDist * minDist) {
          goto SkipSphere;
        }
      }

      // Albedo and specular color
      var color = Random.ColorHSV();
      float chance = Random.value;
      if (chance < 0.8f) {
        bool metal = chance < 0.4f;
        sphere.albedo = metal ? Vector4.zero : new Vector4(color.r, color.g, color.b);
        sphere.specular = metal ? new Vector4(color.r, color.g, color.b) : new Vector4(0.04f, 0.04f, 0.04f);
        sphere.smoothness = Random.value;
      }
      else {
        var emission = Random.ColorHSV(0, 1, 0, 1, 3.0f, 8.0f);
        sphere.emission = new Vector3(emission.r, emission.g, emission.b);
      }

      // Add the sphere to the list
      spheres.Add(sphere);

    SkipSphere:
      continue;
    }

    // Assign to compute buffer
    if (_sphereBuffer != null) {
      _sphereBuffer.Release();
    }

    if (spheres.Count > 0) {
      _sphereBuffer = new ComputeBuffer(spheres.Count, 56);
      _sphereBuffer.SetData(spheres);
    }
    }   //void SetUpScene()

  void RebuildMeshObjectBuffers() {
    if (!_meshObjectsNeedRebuilding) {
      return;
    }

    _meshObjectsNeedRebuilding = false;
    _currentSample = 0;

    // Clear all lists
    _meshObjects.Clear();
    _vertices.Clear();
    _indices.Clear();
    _texcoords.Clear();

        // Loop over all objects and gather their data
        int cnt = 0;

        foreach (var obj in _rayTracingObjects)
        {

            var mesh = obj.GetComponent<MeshFilter>().sharedMesh;

            Debug.Log( (cnt++)  + "th mesh:");
            for (int i = 0; i < mesh.vertices.Length; i++)
            {
                Debug.Log(i + "th vertex=" + mesh.vertices[i].ToString("F6"));

            }
            // Ways to get other components (sibling components) of the gameObject to which 
            // this component is attached:
            // this.GetComponent<T>, where this is a component class
            // this.gameObject.GetComponent<T> does the same thing

            // Add vertex data
            // get the current number of vertices in the vertex list
            int firstVertex = _vertices.Count;
            _vertices.AddRange(mesh.vertices);

            // Add index data - if the vertex buffer wasn't empty before, the
            // indices need to be offset
            int firstIndex = _indices.Count; // the current count of _indices  list
            int[] indices = mesh.GetIndices(0);
            _indices.AddRange(indices.Select(index => index + firstVertex));

            // Add Texcoords data.
            _texcoords.AddRange(mesh.uv);

            // Add the object itself
            _meshObjects.Add(new MeshObject()
            {
                localToWorldMatrix = obj.transform.localToWorldMatrix,
                albedo = obj.mMeshOpticalProperty.albedo,

                specular = obj.mMeshOpticalProperty.specular,
                smoothness = obj.mMeshOpticalProperty.smoothness,
                emission = obj.mMeshOpticalProperty.emission,

                indices_offset = firstIndex,
                indices_count = indices.Length
            });// foreach (RayTracingObject obj in _rayTracingObjects)

            //    struct MeshObject
            //{
            //    public Matrix4x4 localToWorldMatrix;
            //    public Vector3 albedo;
            //    public Vector3 specular;
            //    public float smoothness;
            //    public Vector3 emission;
            //    public int indices_offset;
            //    public int indices_count;
            //}

            int meshObjStride = 16 * sizeof(float) + sizeof(float)
                                + 3 * 3 * sizeof(float) + 2 * sizeof(int);

            // create a computebuffer and set the data to it

            CreateComputeBuffer(ref _meshObjectBuffer, _meshObjects, meshObjStride);

            CreateComputeBuffer(ref _vertexBuffer, _vertices, 12);
            CreateComputeBuffer(ref _indexBuffer, _indices, 4);
            CreateComputeBuffer(ref _texcoordsBuffer, _texcoords, 8);


            //// for debugging
            //_meshObjectArray = new MeshObjectRW[_meshObjects.Count];

            //int meshObjRWStride = 16 * sizeof(float) + sizeof(float)
            //               + 3 * 3 * sizeof(float);


            //_meshObjectBufferRW = new ComputeBuffer(_meshObjects.Count, meshObjRWStride);


            ////ComputeBufferType.Default: In HLSL shaders, this maps to StructuredBuffer<T> or RWStructuredBuffer<T>.

            _vertexBufferRW = new ComputeBuffer(_vertices.Count, 3 * sizeof(float), ComputeBufferType.Default);
            mIntersectionBuffer = new ComputeBuffer(Screen.width * Screen.height, 4 * sizeof(float), ComputeBufferType.Default);
            mRayDirectionBuffer = new ComputeBuffer(Screen.width * Screen.height, 4 * sizeof(float), ComputeBufferType.Default);
            mIntersectionBuffer = new ComputeBuffer(Screen.width * Screen.height, 4 * sizeof(float), ComputeBufferType.Default);
            mAccumRayEnergyBuffer = new ComputeBuffer(Screen.width * Screen.height, 4 * sizeof(float), ComputeBufferType.Default);
            mEmissionBuffer = new ComputeBuffer(Screen.width * Screen.height, 4 * sizeof(float), ComputeBufferType.Default);
            mSpecularBuffer = new ComputeBuffer(Screen.width * Screen.height, 4 * sizeof(float), ComputeBufferType.Default);


            mVertexArray = new Vector3[_vertices.Count];
            mRayDirectionArray = new Vector4[Screen.width * Screen.height];
            mIntersectionArray = new Vector4[Screen.width * Screen.height];
            mAccumRayEnergyArray = new Vector4[Screen.width * Screen.height];
            mEmissionArray = new Vector4[Screen.width * Screen.height];
            mSpecularArray = new Vector4[Screen.width * Screen.height];



            ////The static Array.Clear() method "sets a range of elements in the Array to zero, to false, or to Nothing, depending on the element type".If you want to clear your entire array, you could use this method an provide it 0 as start index and myArray.Length as length:
            //// Array.Clear(mUVMapArray, 0, mUVMapArray.Length);


            //_meshObjectBufferRW.SetData(_meshObjectArray);

            _vertexBufferRW.SetData(mVertexArray);

            mRayDirectionBuffer.SetData(mRayDirectionArray);

            mIntersectionBuffer.SetData(mIntersectionArray);

            mAccumRayEnergyBuffer.SetData(mAccumRayEnergyArray);
            mEmissionBuffer.SetData(mEmissionArray);
            mSpecularBuffer.SetData(mSpecularArray);

        } //    foreach (var obj in _rayTracingObjects)

    }   // RebuildMeshObjectBuffers()


    // Build the vertices and the indices of the mesh for the mirror object within the script
    void RebuildMirrorObjectBuffers() {

    if (!_pyramidMeshObjectsNeedRebuilding) {
      return;
    }

    _pyramidMeshObjectsNeedRebuilding = false;
    _currentSample = 0;

    // Clear all lists
    _pyramidMirrorObjects.Clear();
    _pyramidMeshVertices.Clear();
    _pyramidMeshIndices.Clear();

    // Loop over all objects and gather their data
    //foreach (RayTracingObject obj in _rayTracingObjects)
    var pyramidMirrorObj = _pyramidMirrorObjects[0];
    //  public PyramidMirrorMesh mPyramidMirrorMesh;
    //  public Mesh mPyramidMesh; // mesh for the pyramid
    //   public Matrix4x4 mPyramidLocalToWorldMatrix;


    //{
    // Mesh mesh = obj.GetComponent<MeshFilter>().sharedMesh;
    // Ways to get other components (sibling components) of the gameObject to which 
    // this component is attached:
    // this.GetComponent<T>, where this is a component class
    // this.gameObject.GetComponent<T> does the same thing

    // Add vertex data
    // int firstVertex = _vertices.Count;
    _pyramidMeshVertices.AddRange(pyramidMirrorObj.mPyramidMesh.vertices);

    // Add index data - if the vertex buffer wasn't empty before, the
    // indices need to be offset
    //int firstIndex = _indices.Count;
    //var indices = mesh.GetIndices(0);
    _pyramidMeshIndices.AddRange(pyramidMirrorObj.mPyramidMesh.triangles);

    // Add the object itself
    // _meshObjects.Add(new MeshObject()
    //{
    //   localToWorldMatrix = obj.transform.localToWorldMatrix,
    //  indices_offset = firstIndex,
    //  indices_count = indices.Length
    //});

    _pyramidMirrors.Add(pyramidMirrorObj.mPyramidMirror);

    // }   // foreach (RayTracingObject obj in _rayTracingObjects)

    //int stride  = sizeof(Vector3) + sizeof(Vector3) + sizeof(Vector2)
    //public Matrix4x4 localToWorldMatrix; // the world frame of the pyramid
    //public float height;
    //public float width;  // the radius of the base of the cone
    //public float depth;
    //public Vector3 albedo;
    //public Vector3 specular;
    //public float smoothness;
    //public Vector3 emission;
    //public int indices_count;
    // stride = sizeof(Matrix4x4) + 4 * sizeof(float) + 3 * sizeof(Vector3) + sizeof(int)

    int pyramidMirrorStride = 16 * sizeof(float) + 4 * sizeof(float) + sizeof(int)
                 + 3 * 3 * sizeof(float);
    int vector3Stride = 3 * sizeof(float); //==12
    int intStride = sizeof(int); // ==4

    CreateComputeBuffer(ref _pyramidMirrorBuffer, _pyramidMirrors, pyramidMirrorStride);
    CreateComputeBuffer(ref _pyramidMeshVertexBuffer, _pyramidMeshVertices, vector3Stride);
    CreateComputeBuffer(ref _pyramidMeshIndexBuffer, _pyramidMeshIndices, intStride);
  }   // RebuildMirrorObjectBuffers()



  private static void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride)
     where T : struct {
    // Do we already have a compute buffer?
    if (buffer != null) {
      // If no data or buffer doesn't match the given criteria, release it
      if (data.Count == 0 || buffer.count != data.Count || buffer.stride != stride) {
        buffer.Release();
        buffer = null;
      }
    }

    if (data.Count != 0) {
      // If the buffer has been released or wasn't there to
      // begin with, create it
      if (buffer == null) {

        buffer = new ComputeBuffer(data.Count, stride);
      }

      // Set data on the buffer
      buffer.SetData(data);
    }
  }   //CreateComputeBuffer

  void SetComputeBuffer(string name, ComputeBuffer buffer) {
    if (buffer != null) {
      RayTracingShader.SetBuffer(0, name, buffer);
    }
  }

  void SetShaderParameters() {
    RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
    RayTracingShader.SetTexture(0, "_RoomTexture", _RoomTexture);
    RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);


        // "forward" in OpenGL is "-z".In Unity forward is "+z".Most hand - rules you might know from math are inverted in Unity
        //    .For example the cross product usually uses the right hand rule c = a x b where a is thumb, b is index finger and c is the middle
        //    finger.In Unity you would use the same logic, but with the left hand.

        //    However this does not affect the projection matrix as Unity uses the OpenGL convention for the projection matrix.
        //    The required z - flipping is done by the cameras worldToCameraMatrix.
        //    So the projection matrix should look the same as in OpenGL.


    RayTracingShader.SetMatrix("_Projection", _camera.projectionMatrix);

    RayTracingShader.SetMatrix("_CameraInverseProjection", _camera.projectionMatrix.inverse);
    RayTracingShader.SetVector("_PixelOffset", new Vector2(Random.value, Random.value));
    RayTracingShader.SetFloat("_Seed", Random.value);

    var l = DirectionalLight.transform.forward;
    RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

     // Added by Moon Jung, 2020/1/21
    RayTracingShader.SetFloat("_FOV", Mathf.Deg2Rad * _camera.fieldOfView);

        //SetComputeBuffer("_Spheres", _sphereBuffer);   commented out by Moon Jung
    SetComputeBuffer("_MeshObjects", _meshObjectBuffer);

    SetComputeBuffer("_Vertices", _vertexBuffer);
    SetComputeBuffer("_Indices", _indexBuffer);
    SetComputeBuffer("_UVs", _texcoordsBuffer);


        //#region debugging
        //RayTracingShader.SetBuffer(0, "_MeshObjectBufferRW", _meshObjectBufferRW);

        RayTracingShader.SetBuffer(0, "_VertexBufferRW", _vertexBufferRW);
        RayTracingShader.SetBuffer(0, "_RayDirectionBuffer", mRayDirectionBuffer);

        RayTracingShader.SetBuffer(0, "_IntersectionBuffer", mIntersectionBuffer);
        RayTracingShader.SetBuffer(0,"_AccumRayEnergyBuffer", mAccumRayEnergyBuffer);
        RayTracingShader.SetBuffer(0, "_EmissionBuffer", mEmissionBuffer);
        RayTracingShader.SetBuffer(0, "_SpecularBuffer", mSpecularBuffer);


    }   //SetShaderParameters()

    void InitRenderTexture() {

    if (_target == null || _target.width != Screen.width || _target.height != Screen.height) {
      // Release render texture if we already have one
      if (_target != null) {
        _target.Release();
        _converged.Release();
      }

      // Get a render target for Ray Tracing
      _target = new RenderTexture(Screen.width, Screen.height, 0,
          RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      _target.enableRandomWrite = true;
      _target.Create();

      _converged = new RenderTexture(Screen.width, Screen.height, 0,
          RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
      _converged.enableRandomWrite = true;
      _converged.Create();

      // Reset sampling
      _currentSample = 0;
    }
  }

  void Render(RenderTexture destination) {
    // Make sure we have a current render target
    InitRenderTexture();

    // Set the target and dispatch the compute shader
    RayTracingShader.SetTexture(0, "Result", _target);
    int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
    int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);

    RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);



        // for debugging: print the buffer

        //_meshObjectBufferRW.GetData(_meshObjectArray);

        //for (int i = 0; i < _meshObjects.Count; i++)
        //{
        //    Debug.Log(i + "th mesh:" + "albedo=" + _meshObjectArray[i].albedo);
        //    Debug.Log(i + "th mesh:" + "specular=" + _meshObjectArray[i].specular);
        //    Debug.Log(i + "th mesh:" + "emission=" + _meshObjectArray[i].emission);
        //}

        _vertexBufferRW.GetData(mVertexArray);

        for (int i = 0; i < _vertices.Count; i++)
        {
            Debug.Log("vertex of meshes:" + mVertexArray[i].ToString("F6"));

        }

        mRayDirectionBuffer.GetData(mRayDirectionArray);
        mIntersectionBuffer.GetData(mIntersectionArray);

        mAccumRayEnergyBuffer.GetData(mAccumRayEnergyArray);
        mEmissionBuffer.GetData(mEmissionArray);
        mSpecularBuffer.GetData(mSpecularArray);

        for (int y = 0; y < Screen.height; y += 10)
            for (int x = 0; x < Screen.width; x += 10)
            {
                int idx = y * Screen.width + x;

                Vector4 myRayDir = mRayDirectionArray[idx];
                Vector4 intersection = mIntersectionArray[idx];
                Vector4 accumRayEnergy = mAccumRayEnergyArray[idx];
                Vector4 emission = mEmissionArray[idx];
                Vector4 specular = mSpecularArray[idx];


                // for debugging
                //_IntersectionBuffer[id.y * width + id.x] = float4(posInCamera, 0);
                //_RayDirectionBuffer[id.y * width + id.x] = float4(posInScreenSpace, 0);

                //_EmissionBuffer[id.y * width + id.x] = float4(myPosInCamera, 0);
                //_SpecularBuffer[id.y * width + id.x] = float4(myPosInScreenSpace, 0);

                Debug.Log("(" + x + "," + y + "):" + "incoming ray direction=" + myRayDir.ToString("F6"));
                Debug.Log("(" + x + "," + y + "):" + "hit point=" + intersection.ToString("F6"));                     

               
                Debug.Log("(" + x + "," + y + "):" + "attenudated ray energy=" + accumRayEnergy.ToString("F6"));
                Debug.Log("(" + x + "," + y + "):" + "emission color=" + emission.ToString("F6"));
                Debug.Log("(" + x + "," + y + "):" + "reflected direction=" + specular.ToString("F6"));
            }

     

        // Blit the result texture to the screen
        if (_addMaterial == null) {
      _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
    }

    _addMaterial.SetFloat("_Sample", _currentSample);
    // TODO: Upscale To 4K and downscale to 1k.
    Graphics.Blit(_target, _converged, _addMaterial);
    Graphics.Blit(_converged, destination);
    _currentSample++;
  }  // Render()

  void OnRenderImage(RenderTexture source, RenderTexture destination) {
   // RebuildMirrorObjectBuffers();        // commented out by Moon Jung

    
    RebuildMeshObjectBuffers();

    SetShaderParameters();
    Render(destination);


#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        UnityEditor.EditorApplication.isPlaying = false;
        //UnityEditor.EditorApplication.Exit(0);
#else
                   Application.Quit();
#endif


    }//OnRenderImage()

}  //RayTracingMaster
