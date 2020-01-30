//https://bitbucket.org/Daerst/gpu-ray-tracing-in-unity/src/master/

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// A gameObject = a bag of components; a prefab = a bag of gameObjects

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

  RenderTexture _mainScreenRT; // added by Moon Jung

  Material _addMaterial;
  uint _currentSample = 0;
  ComputeBuffer _sphereBuffer;
  List<Transform> _transformsToWatch = new List<Transform>();


  static bool _meshObjectsNeedRebuilding = false;
  static bool _triangularConeMirrorNeedRebuilding = false;
  static bool _pyramidMeshObjectsNeedRebuilding = false;


  static List<RayTracingObject> _rayTracingObjects = new List<RayTracingObject>();

  static List<MeshObject> _meshObjects = new List<MeshObject>();


  // added by Moon Jung
  static List<PyramidMirror> _pyramidMirrors
                           = new List<PyramidMirror>();
  static List<PyramidMirrorObject> _pyramidMirrorObjects
                             = new List<PyramidMirrorObject>();


  static List<TriangularConeMirrorObject> _triangularConeMirrorObjects = new List<TriangularConeMirrorObject>();
  static List<TriangularConeMirror>
      _triangularConeMirrors = new List<TriangularConeMirror>();

  static List<Vector3> _vertices = new List<Vector3>();
  static List<int> _indices = new List<int>();
  static List<Vector2> _texcoords = new List<Vector2>();


  ComputeBuffer _meshObjectBuffer;

  ComputeBuffer _vertexBuffer;
  ComputeBuffer _indexBuffer;
  ComputeBuffer _texcoordsBuffer;

  ComputeBuffer _vertexBufferRW;



  ComputeBuffer _pyramidMirrorBuffer;

  static List<Vector3> _triangularConeMirrorVertices = new List<Vector3>();
  static List<int> _triangularConeMirrorIndices = new List<int>();
  ComputeBuffer _triangularConeMirrorBuffer;
  ComputeBuffer _triangularConeMirrorVertexBuffer;
  ComputeBuffer _triangularConeMirrorIndexBuffer;


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

  public int _maxNumOfBounce = 8;


  public int _mirrorType = 1; // default = cone mirror
                              /* 
                                  Pyramid = 0,
                                  Cone = 1,
                                  Sphere = 2,
                                  Paraboloid= 3

                                      // we will also use the elliptic versions of these types,
                                      // e. elliptic cone, ellipsoid, elliptic paraboloid
                              */

  //-PYRAMID MIRROR------------------------------------
  public struct PyramidMirror {
    public Matrix4x4 localToWorldMatrix; // the world frame of the pyramid
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
    public float height;
    public float width;  // the radius of the base of the cone
    public float depth;

  };

  public struct TriangularConeMirror {
    public Matrix4x4 localToWorldMatrix;
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
    public int indices_offset;
    public int indices_count;
  }


  public struct ConeMirror {
    Vector3 position; // the world frame of the cone
    Vector3 albedo;
    Vector3 specular;
    float smoothness;
    Vector3 emission;
    float height;
    float radius;  // the radius of the base of the cone

  };


  struct MeshObject {
    public Matrix4x4 localToWorldMatrix;
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
    public int indices_offset;
    public int indices_count;
  }

  struct MeshObjectRW {
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

  RTRayDirectionValidator Validator;

  void Awake() {
    _camera = this.GetComponent<Camera>();

    _transformsToWatch.Add(transform);
    _transformsToWatch.Add(DirectionalLight.transform);
    Validator = GetComponent<RTRayDirectionValidator>();

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

    mIntersectionBuffer?.Release();
    mAccumRayEnergyBuffer?.Release();
    mEmissionBuffer?.Release();
    mSpecularBuffer?.Release();

  }

  void Update() {
    // if (Input.GetKeyDown(KeyCode.F12)) {
    if (Input.GetKeyDown(KeyCode.Space)) {
      ScreenCapture.CaptureScreenshot(Time.time + "-" + _currentSample + ".png");
      Debug.Log("Screen Captured");

    }


    if (Input.GetKeyDown(KeyCode.Escape)) {
#if UNITY_EDITOR
      // Application.Quit() does not work in the editor so
      UnityEditor.EditorApplication.isPlaying = false;
      //UnityEditor.EditorApplication.Exit(0);
#else
                           Application.Quit();
#endif
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


  public static void RegisterTriangularConeMirror(TriangularConeMirrorObject obj) {
    _triangularConeMirrorObjects.Add(obj);
    _triangularConeMirrorNeedRebuilding = true;


  }
  public static void UnregisterTriangularConeMirror(TriangularConeMirrorObject obj) {
    _triangularConeMirrorObjects.Remove(obj);
    _triangularConeMirrorNeedRebuilding = true;

  }


  public static void RegisterPyramidMirror(PyramidMirrorObject obj) {

    _pyramidMirrorObjects.Add(obj);
    _pyramidMeshObjectsNeedRebuilding = true;
  }
  public static void UnregisterPyramidMirror(PyramidMirrorObject obj) {
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

  void RebuildMeshObjectBuffer() {
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


    foreach (var obj in _rayTracingObjects) {

      //string objectName = obj.objectName;
      //Debug.Log("mesh object=" + objectName);

      var mesh = obj.GetComponent<MeshFilter>().sharedMesh;

      //Debug.Log( (cnt++)  + "th mesh:");
      //for (int i = 0; i < mesh.vertices.Length; i++)
      //{
      //    Debug.Log(i + "th vertex=" + mesh.vertices[i].ToString("F6"));

      //}
      // Ways to get other components (sibling components) of the gameObject to which 
      // this component is attached:
      // this.GetComponent<T>, where this is a component class
      // this.gameObject.GetComponent<T> does the same thing

      // Add vertex data
      // get the current number of vertices in the vertex list
      int firstVertex = _vertices.Count;  // The number of vertices so far created; will be used
                                          // as the index of the first vertex of the newly created mesh
      _vertices.AddRange(mesh.vertices);

      // Add index data - if the vertex buffer wasn't empty before, the
      // indices need to be offset
      int firstIndex = _indices.Count; // the current count of _indices  list; will be used
                                       // as the index offset in _indices for the newly created mesh
      int[] indices = mesh.GetIndices(0); // mesh.Triangles() is a special  case of this method
                                          // when the mesh topology is triangle;
                                          // indices will contain a multiple of three indices
                                          // our mesh is actually a triangular mesh.

      // show the local coordinates of the triangles
      //for (int i = 0; i < indices.Length; i += 3) {   // a triangle v0,v1,v2 

      //  Debug.Log("triangle vertex (local) =(" + mesh.vertices[indices[i]].ToString("F6")
      //            + "," + mesh.vertices[indices[i + 1]].ToString("F6")
      //            + "," + mesh.vertices[indices[i + 2]].ToString("F6") + ")");
      //}

      // Change the order of the vertex index in indices
      //for (int i = 0; i < indices.Length; i+=3)
      //{   // a triangle v0,v1,v2 => v2, v1, v0
      //    int intermediate = indices[i];   // indices[i+1] does not change
      //    indices[i] = indices[i + 2];
      //    indices[i + 2] = intermediate;

      //}
      _indices.AddRange(indices.Select(index => index + firstVertex));


      // Add Texcoords data.
      _texcoords.AddRange(mesh.uv);

      // Add the object itself
      _meshObjects.Add(new MeshObject() {
        localToWorldMatrix = obj.transform.localToWorldMatrix,
        albedo = obj.mMeshOpticalProperty.albedo,

        specular = obj.mMeshOpticalProperty.specular,
        smoothness = obj.mMeshOpticalProperty.smoothness,
        emission = obj.mMeshOpticalProperty.emission,

        indices_offset = firstIndex,
        indices_count = indices.Length // set the index count of the mesh of the current obj
      }
      );

    }// foreach (RayTracingObject obj in _rayTracingObjects)



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



  }   // RebuildMeshObjectBuffer()


  void RebuildTriangularConeMirrorBuffer() {
    if (!_triangularConeMirrorNeedRebuilding) {
      return;
    }

    _triangularConeMirrorNeedRebuilding = false;

    // Clear all lists
    _triangularConeMirrors.Clear();
    _triangularConeMirrorVertices.Clear();
    _triangularConeMirrorIndices.Clear();


    var obj = _triangularConeMirrorObjects[0];

    string objectName = obj.objectName;
    _mirrorType = obj.mirrorType;

    Debug.Log("mirror object=" + objectName);

    var mesh = obj.GetComponent<MeshFilter>().sharedMesh;

    //Debug.Log((cnt++) + "th mesh:");
    for (int i = 0; i < mesh.vertices.Length; i++) {
      Debug.Log(i + "th vertex=" + mesh.vertices[i].ToString("F6"));

    }
    // Ways to get other components (sibling components) of the gameObject to which 
    // this component is attached:
    // this.GetComponent<T>, where this is a component class
    // this.gameObject.GetComponent<T> does the same thing

    // Add vertex data
    // get the current number of vertices in the vertex list
    int firstVertexIndex = _triangularConeMirrorVertices.Count;  // The number of vertices so far created; will be used
                                                                 // as the index of the first vertex of the newly created mesh
    _triangularConeMirrorVertices.AddRange(mesh.vertices);

    // Add index data - if the vertex buffer wasn't empty before, the
    // indices need to be offset
    int countOfCurrentIndices = _triangularConeMirrorIndices.Count; // the current count of _indices  list; will be used
                                                                    // as the index offset in _indices for the newly created mesh
    int[] indices = mesh.GetIndices(0); // mesh.Triangles() is a special  case of this method
                                        // when the mesh topology is triangle;
                                        // indices will contain a multiple of three indices
                                        // our mesh is actually a triangular mesh.

    // show the local coordinates of the triangles
    for (int i = 0; i < indices.Length; i += 3) {   // a triangle v0,v1,v2 
      Debug.Log("triangular Mirror: triangle indices (local) =(" + mesh.vertices[indices[i]].ToString("F6")
                + "," + mesh.vertices[indices[i + 1]].ToString("F6")
                + "," + mesh.vertices[indices[i + 2]].ToString("F6") + ")");
    }

    // Change the order of the vertex index in indices
    for (int i = 0; i < indices.Length; i += 3) {   // a triangle v0,v1,v2 => v2, v1, v0
      int intermediate = indices[i];   // indices[i+1] does not change
      indices[i] = indices[i + 2];
      indices[i + 2] = intermediate;
    }
    //}
    _triangularConeMirrorIndices.AddRange(indices.Select(index => index + firstVertexIndex));


    // Add Texcoords data.
    //_texcoords.AddRange(mesh.uv);

    // Add the object itself
    _triangularConeMirrors.Add(new TriangularConeMirror() {
      localToWorldMatrix = obj.transform.localToWorldMatrix,
      albedo = obj.mMeshOpticalProperty.albedo,

      specular = obj.mMeshOpticalProperty.specular,
      smoothness = obj.mMeshOpticalProperty.smoothness,
      emission = obj.mMeshOpticalProperty.emission,
      indices_offset = countOfCurrentIndices,
      indices_count = indices.Length // set the index count of the mesh of the current obj
    }
    );



    //public struct TriangularConeMirror
    //{
    //    public Matrix4x4 localToWorldMatrix;
    //    public Vector3 albedo;
    //    public Vector3 specular;
    //    public float smoothness;
    //    public Vector3 emission;
    //    public int indices_offset;
    //    public int indices_count;
    //}

    int stride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                 + sizeof(float) + 2 * sizeof(int);

    // create a computebuffer and set the data to it

    CreateComputeBuffer(ref _triangularConeMirrorBuffer,
                          _triangularConeMirrors, stride);

    CreateComputeBuffer(ref _triangularConeMirrorVertexBuffer,
                       _triangularConeMirrorVertices, 12);
    CreateComputeBuffer(ref _triangularConeMirrorIndexBuffer,
                       _triangularConeMirrorIndices, 4);




  }   // RebuildTriangularConeMirrorBuffer()

  // Build the vertices and the indices of the mesh for the mirror object within the script
  void RebuildPyramidMirrorObjectBuffer() {

    if (!_pyramidMeshObjectsNeedRebuilding) {
      return;
    }

    _pyramidMeshObjectsNeedRebuilding = false;


    // Clear all lists
    _pyramidMirrors.Clear();

    // Loop over all objects and gather their data
    //foreach (RayTracingObject obj in _rayTracingObjects)
    var obj = _pyramidMirrorObjects[0];
    _mirrorType = obj.mMirrorType;



    // Add the object itself
    _pyramidMirrors.Add(new PyramidMirror() {
      localToWorldMatrix = obj.transform.localToWorldMatrix,
      albedo = obj.mMeshOpticalProperty.albedo,

      specular = obj.mMeshOpticalProperty.specular,
      smoothness = obj.mMeshOpticalProperty.smoothness,
      emission = obj.mMeshOpticalProperty.emission,
      height = obj.mPyramidParam.height,
      width = obj.mPyramidParam.width,
      depth = obj.mPyramidParam.depth,
    }
    );



    //    public struct PyramidMirror
    //{
    //    public Matrix4x4 localToWorldMatrix; // the world frame of the pyramid
    //    public Vector3 albedo;
    //    public Vector3 specular;
    //    public float smoothness;
    //    public Vector3 emission;
    //    public float height;
    //    public float width;  // the radius of the base of the cone
    //    public float depth;

    //};


    // stride = sizeof(Matrix4x4) + 4 * sizeof(float) + 3 * sizeof(Vector3) + sizeof(int)

    int pyramidMirrorStride = 16 * sizeof(float) + 3 * 3 * sizeof(float)
                              + 4 * sizeof(float);

    CreateComputeBuffer(ref _pyramidMirrorBuffer, _pyramidMirrors, pyramidMirrorStride);

  }   // RebuildPyramidMirrorObjectBuffer()



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

    //if (!_shaderParameterNeedResetting) {
    //        return;
    //}  // just return if the shaderParameters are already set.  // added by Moon Jung,2020/1/28

    // _shaderParameterNeedResetting = false;


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

    var pixelOffset = new Vector2(Random.value, Random.value);

    RayTracingShader.SetVector("_PixelOffset", pixelOffset);

    //Debug.Log("_PixelOffset =" + pixelOffset);

    float seed = Random.value;

    RayTracingShader.SetFloat("_Seed", seed);

    //Debug.Log("_Seed =" + seed);

    var l = DirectionalLight.transform.forward;
    RayTracingShader.SetVector("_DirectionalLight", new Vector4(l.x, l.y, l.z, DirectionalLight.intensity));

    // Added by Moon Jung, 2020/1/21
    RayTracingShader.SetFloat("_FOV", Mathf.Deg2Rad * _camera.fieldOfView);

    // Added by Moon Jung, 2020/1/29
    RayTracingShader.SetInt("_MaxBounce", _maxNumOfBounce);
    RayTracingShader.SetInt("_MirrorType", _mirrorType);
    //SetComputeBuffer("_Spheres", _sphereBuffer);   commented out by Moon Jung
    SetComputeBuffer("_MeshObjects", _meshObjectBuffer);

    SetComputeBuffer("_Vertices", _vertexBuffer);
    SetComputeBuffer("_Indices", _indexBuffer);
    SetComputeBuffer("_UVs", _texcoordsBuffer);


    // Set the parameters for the mirror object
    SetComputeBuffer("_TriangularConeMirrors", _triangularConeMirrorBuffer);
    SetComputeBuffer("_TriangularConeMirrorVertices", _triangularConeMirrorVertexBuffer);
    SetComputeBuffer("_TriangularConeMirrorIndices", _triangularConeMirrorIndexBuffer);

    //#region debugging
    //RayTracingShader.SetBuffer(0, "_MeshObjectBufferRW", _meshObjectBufferRW);


    RayTracingShader.SetBuffer(0, "_VertexBufferRW", _vertexBufferRW);
    RayTracingShader.SetBuffer(0, "_RayDirectionBuffer", mRayDirectionBuffer);

    RayTracingShader.SetBuffer(0, "_IntersectionBuffer", mIntersectionBuffer);
    RayTracingShader.SetBuffer(0, "_AccumRayEnergyBuffer", mAccumRayEnergyBuffer);
    RayTracingShader.SetBuffer(0, "_EmissionBuffer", mEmissionBuffer);
    RayTracingShader.SetBuffer(0, "_SpecularBuffer", mSpecularBuffer);



  }   //SetShaderParameters()

  void InitRenderTexture() {

    if (_target == null || _target.width != Screen.width || _target.height != Screen.height) {

      // Release render texture if we already have one
      if (_target != null) {    // The current render texture does not have the right size
        _target.Release();
        _converged.Release();
      }

      // Create the camera's render target for Ray Tracing
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
    }  //if

    // else: do nothing

  }  //InitRenderTexture()


  //void Render(RenderTexture) {
  void Render(RenderTexture destination) {
    // Make sure we have a current render target
    InitRenderTexture();     // create _target and _converge renderTexture   only once.

    // Set the target and dispatch the compute shader
    RayTracingShader.SetTexture(0, "Result", _target);     // set the target for every render?

    int threadGroupsX = Mathf.CeilToInt(Screen.width / 8.0f);
    int threadGroupsY = Mathf.CeilToInt(Screen.height / 8.0f);

    RayTracingShader.Dispatch(0, threadGroupsX, threadGroupsY, 1);



    // Blit the result texture to the screen
    if (_addMaterial == null) {
      _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
    }

    _addMaterial.SetFloat("_Sample", _currentSample);
    // TODO: Upscale To 4K and downscale to 1k.
    Graphics.Blit(_target, _converged, _addMaterial);

    // Null the target Texture of the camera sand blit to the null target (which is
    // the framebuffer

    //_camera.targetTexture = null;     // added by Moon Jung,2020/1/28; but not needed because
    // _canera.targetTexture was not set to a render Texture, but remains to  be null.

    // Graphics.Blit(_converged, mainScreenRT);  // destination is null
    //_camera.targetTexture = null;  

    Graphics.Blit(_converged, destination);
    // if the camera's targetTexture is not null,  the the source texture will be written on
    // it, even if the destination is set to null.

    // if destination is null (which is the case in our program), the screen backbuffer is used
    // as the blit destination, except if Camera.main.targetTexture is not null;
    // Set Camera.main.targetTexture to null before calling to Blit to the destination is the framebuffer


    _currentSample++;    // every call of Render, a new location within every pixel area is sampled for 
                         // renderibg => used for anti-aliasing.
                         //Debug.Log("current sample=" + _currentSample);

  }  // Render()

  // added by Moon Jung,2020/1/28
  private void OnPreRender() {
    //  _camera.targetTexture = _mainScreenRT;   // causes what the camera sees is rendered to the RT
    // rather than the framebuffer.

    // The result of ray tracing Rendering by Unity will be go to the specified target Teture
    // which is set in InitRenderTexture() 


  }
  void OnRenderImage(RenderTexture source, RenderTexture destination) {
    // destination may refer to the render target of the camera or null,
    // which means the framebuffer

    //RebuildPyramidMirrorObjectBuffer();        // commented out by Moon Jung

    RebuildTriangularConeMirrorBuffer();
    // If you do not supply a RenderTexture to the Camera's targetTexture Unity will
    //trigger CPU ReadPixels( get data back from GPU) to give you source RenderTexture,
    //which stall the whole GPU until finish.
    // Super slow.

    RebuildMeshObjectBuffer();

    SetShaderParameters();
    //Render(null as RenderTexture);  //  added by Moon Jung, 2020/1/28

    Render(destination);  // This function dispatches the compute shader

    //DebugLogOfRWBuffers();



  } // OnRenderImage()

  //void OnPostRender()
  //{
  //    // destination may refer to the render target of the camera or null,
  //    // which means the framebuffer

  //    // RebuildMirrorObjectBuffers();        // commented out by Moon Jung

  //    // If you do not supply a RenderTexture to the Camera's targetTexture Unity will
  //    //trigger CPU ReadPixels( get data back from GPU) to give you source RenderTexture,
  //    //which stall the whole GPU until finish.
  //    // Super slow.

  //    RebuildMeshObjectBuffers();

  //    SetShaderParameters();
  //    //Render(null as RenderTexture);  //  added by Moon Jung, 2020/1/28

  //    Render();


  //    //#if UNITY_EDITOR
  //    //        // Application.Quit() does not work in the editor so
  //    //        UnityEditor.EditorApplication.isPlaying = false;
  //    //        //UnityEditor.EditorApplication.Exit(0);
  //    //#else
  //    //                   Application.Quit();
  //    //#endif


  //}//OnPostRender()

  //
  //    private void OnGUI()
  //    {
  //        bool quitGame = GUI.Button(new Rect(Screen.width / 2 - 100,
  //                                            Screen.height / 2 - 200,
  //                                            200, 20), "Quit Game");
  //        if (quitGame)
  //        {
  //#if UNITY_EDITOR
  //            // Application.Quit() does not work in the editor so
  //            UnityEditor.EditorApplication.isPlaying = false;
  //            //UnityEditor.EditorApplication.Exit(0);
  //#else
  //                                       Application.Quit();
  //#endif   
  //        }

  //    }   // OnGUI

  void DebugLogOfRWBuffers() {

    // for debugging: print the buffer

    //_vertexBufferRW.GetData(mVertexArray);

    //int meshObjectIndex = 0;
    //foreach (var meshObj in _meshObjects)
    //{
    //    Debug.Log((meshObjectIndex) + "th meshObj");

    //    int indices_count = meshObj.indices_count;
    //    int indices_offset = meshObj.indices_offset;

    //    int triangleIndex = 0;

    //    for (int i = indices_offset; i < indices_offset + indices_count; i += 3)
    //    {
    //        Debug.Log((triangleIndex) + "th triangle:" + mVertexArray[_indices[i] ].ToString("F6"));
    //        Debug.Log((triangleIndex) + "th triangle:" + mVertexArray[_indices[i + 1]  ].ToString("F6"));
    //        Debug.Log((triangleIndex) + "th triangle:" + mVertexArray[ _indices[i + 2]].ToString("F6"));

    //        ++triangleIndex;
    //    }  // for each triangle

    //    ++meshObjectIndex;
    //} // for each meshObj

    // debugging for the ray 0
    mRayDirectionBuffer.GetData(mRayDirectionArray);
    mIntersectionBuffer.GetData(mIntersectionArray);
    mAccumRayEnergyBuffer.GetData(mAccumRayEnergyArray);
    mEmissionBuffer.GetData(mEmissionArray);
    mSpecularBuffer.GetData(mSpecularArray);

    for (int y = 0; y < Screen.height; y += 10) {
      for (int x = 0; x < Screen.width; x += 10) {
        int idx = y * Screen.width + x;


        var myRayDir = mRayDirectionArray[idx];
        var intersection = mIntersectionArray[idx];
        var accumRayEnergy = mAccumRayEnergyArray[idx];
        var emission = mEmissionArray[idx];
        var specular = mSpecularArray[idx];


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
    }


  } //    void DebugLogOfRWBuffers()
}  //RayTracingMaster
