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
  static List<Vector3> _vertices = new List<Vector3>();
  static List<int> _indices = new List<int>();
  static List<Vector2> _texcoords = new List<Vector2>();

  ComputeBuffer _meshObjectBuffer;
  ComputeBuffer _vertexBuffer;
  ComputeBuffer _indexBuffer;
  ComputeBuffer _texcoordsBuffer;

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

  struct MeshObject {
    public Matrix4x4 localToWorldMatrix;
    public Vector3 albedo;
    public Vector3 specular;
    public float smoothness;
    public Vector3 emission;
    public int indices_offset;
    public int indices_count;
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
  }

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
    foreach (var obj in _rayTracingObjects) {
      var mesh = obj.GetComponent<MeshFilter>().sharedMesh;
      // Ways to get other components (sibling components) of the gameObject to which 
      // this component is attached:
      // this.GetComponent<T>, where this is a component class
      // this.gameObject.GetComponent<T> does the same thing

      // Add vertex data
      int firstVertex = _vertices.Count;
      _vertices.AddRange(mesh.vertices);

      // Add index data - if the vertex buffer wasn't empty before, the
      // indices need to be offset
      int firstIndex = _indices.Count;
      int[] indices = mesh.GetIndices(0);
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
        indices_count = indices.Length
      });
    }   // foreach (RayTracingObject obj in _rayTracingObjects)

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

    CreateComputeBuffer(ref _meshObjectBuffer, _meshObjects, meshObjStride);
    CreateComputeBuffer(ref _vertexBuffer, _vertices, 12);
    CreateComputeBuffer(ref _indexBuffer, _indices, 4);
    CreateComputeBuffer(ref _texcoordsBuffer, _texcoords, 8);
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



  static void CreateComputeBuffer<T>(ref ComputeBuffer buffer, List<T> data, int stride)
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
  }

  void SetComputeBuffer(string name, ComputeBuffer buffer) {
    if (buffer != null) {
      RayTracingShader.SetBuffer(0, name, buffer);
    }
  }

  void SetShaderParameters() {
    RayTracingShader.SetTexture(0, "_SkyboxTexture", SkyboxTexture);
    RayTracingShader.SetTexture(0, "_RoomTexture", _RoomTexture);
    RayTracingShader.SetMatrix("_CameraToWorld", _camera.cameraToWorldMatrix);
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
  }

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

    // Blit the result texture to the screen
    if (_addMaterial == null) {
      _addMaterial = new Material(Shader.Find("Hidden/AddShader"));
    }

    _addMaterial.SetFloat("_Sample", _currentSample);
    // TODO: Upscale To 4K and downscale to 1k.
    Graphics.Blit(_target, _converged, _addMaterial);
    Graphics.Blit(_converged, destination);
    _currentSample++;
  }

  void OnRenderImage(RenderTexture source, RenderTexture destination) {
   // RebuildMirrorObjectBuffers();        // commented out by Moon Jung

    RebuildMeshObjectBuffers();

    SetShaderParameters();
    Render(destination);
  }
}
