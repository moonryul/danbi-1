using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RTcomputeShaderHelper : MonoBehaviour {
  #region Variables
  /// <summary>
  /// Does need to rebuild RTobject to transfer the data into the RTshader?
  /// </summary>
  public static bool DoesNeedToRebuildRTmeshObjects { get; set; } = false;
  /// <summary>
  /// 
  /// </summary>
  public static bool DoesNeedToRebuildRTmeshProjectorQuad { get; set; } = false;
  /// <summary>
  /// Ray tracing objects list that are transferred into the RTshader.
  /// RTobject is based on the polymorphisms (There're others inherited shapes).
  /// </summary>
  public static List<RTmeshObject> MeshObjectsList { get; set; } = new List<RTmeshObject>();

  /// <summary>
  /// The list for Attributes of the all of ray trace-able mesh objects 
  /// </summary>
  public List<RTmeshObjectAttr> MeshObjectsAttrsList = new List<RTmeshObjectAttr>();
  /// <summary>
  /// All of ray trace-able mesh objects compute buffer.
  /// </summary>
  public ComputeBuffer MeshObjectsAttrsComputeBuf;

  /// <summary>
  /// The list for Vertices of the all of ray trace-able mesh objects. 
  /// </summary>
  public List<Vector3> VerticesList = new List<Vector3>();
  /// <summary>
  /// Vertices of the all of ray trace-able mesh objects compute buffer. 
  /// </summary>
  public ComputeBuffer VerticesComputeBuf;

  /// <summary>
  /// The list for Indices of the all of ray trace-able mesh objects. 
  /// </summary>
  public List<int> IndicesList = new List<int>();
  /// <summary>
  /// Indices of the all of ray trace-able mesh objects compute buffer. 
  /// </summary>
  public ComputeBuffer IndicesComputeBuf;

  /// <summary>
  /// 
  /// </summary>
  public List<Vector3> VtxColorsList = new List<Vector3>();
  /// <summary>
  /// 
  /// </summary>
  public ComputeBuffer VtxColorsComputeBuf;

  /// <summary>
  /// 
  /// </summary>
  public List<Vector2> UVsList = new List<Vector2>();
  /// <summary>
  /// 
  /// </summary>
  public ComputeBuffer UVsComputeBuf;

  public static RTmeshProjectorQuad ProjMeshObject;
  public static RTmeshObjectAttr ProjMeshObjectAttr;

  public ComputeBuffer ProjMeshObjectComputeBuf;

  public Vector3[] Dbg_ProjVertices;
  public List<Vector3> ProjVerticesList = new List<Vector3>();
  public ComputeBuffer ProjVerticesComputeBuf;

  public int[] Dbg_ProjIndices;
  public List<int> ProjIndicesList = new List<int>();
  public ComputeBuffer ProjIndicesComputeBuf;

  public Vector2[] Dbg_ProjUVs;
  public List<Vector2> ProjUVsList = new List<Vector2>();
  public ComputeBuffer ProjUVsComputeBuf;
  // 
  //public ComputeBuffer Dbg_DirectionTexture1Buf;
  //public ComputeBuffer Dbg_DirectionTexture2Buf;
  //public Vector3[] Dbg_DirectionTexture1Result;
  //public Vector3[] Dbg_DirectionTexture2Result;
  // 
  public ComputeBuffer Dbg_ProjectorUVBuf;
  public Vector2[] Dbg_ProjectorUVResult;

  public ComputeBuffer Dbg_ProjectorVtxUVBuf;
  public Vector2[] Dbg_ProjectorVtxUVResult;
  #endregion

  void Start() {
    //Dbg_DirectionTexture1Result = new Vector3[Screen.width * Screen.height];
    //Dbg_DirectionTexture2Result = new Vector3[Screen.width * Screen.height];
    //Dbg_ProjectorUVResult = new Vector2[12];
    //Dbg_ProjectorUVBuf = new ComputeBuffer(12, 8);

    //Dbg_ProjectorUVResult = new Vector2[12];
    //Dbg_ProjectorVtxUVBuf = new ComputeBuffer(12, 8);
  }

  void OnDisable() {
    // Check each compute buffers are still valid and release it!    
    DisposeComputeBuffers(ref MeshObjectsAttrsComputeBuf);
    DisposeComputeBuffers(ref VtxColorsComputeBuf);
    DisposeComputeBuffers(ref VtxColorsComputeBuf);
    DisposeComputeBuffers(ref UVsComputeBuf);
    //DisposeComputeBuffers(ref Dbg_DirectionTexture1Buf);
    //DisposeComputeBuffers(ref Dbg_DirectionTexture2Buf);

    //DisposeComputeBuffers(ref Dbg_ProjectorUVBuf);

    //DisposeComputeBuffers(ref ProjVerticesComputeBuf);
    //DisposeComputeBuffers(ref ProjIndicesComputeBuf);
    //DisposeComputeBuffers(ref ProjUVsComputeBuf);
  }

  /// <summary>
  /// Check the compute buffer are still valid and release it!    
  /// </summary>
  /// <param name="buf"></param>
  void DisposeComputeBuffers(ref ComputeBuffer buf) {
    if (!buf.Null()) {
      buf.Release();
      buf = null;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="obj"></param>
  public static void RegisterToRTmeshObjectsList(RTmeshObject obj) {
    MeshObjectsList.Add(obj);
    Debug.Log($"obj <{obj.name}> is added into the RT object list.");
    DoesNeedToRebuildRTmeshObjects = true;
  }

  public static void RegisterToRTmeshProjectorQuad(RTmeshProjectorQuad quad) {
    ProjMeshObject = quad;
    Debug.Log($"Projector quad <{quad.name}> is added into the RT mesh projector quad.");
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="obj"></param>
  public static void UnregisterFromRTmeshObjectsList(RTmeshObject obj) {
    if (MeshObjectsList.Contains(obj)) {
      MeshObjectsList.Remove(obj);
      Debug.Log($"obj <{obj.name}> is removed from the RT object list.");
      DoesNeedToRebuildRTmeshObjects = true;
    }
  }

  public static void UnregisterFromRTmeshProjectorQuad(RTmeshProjectorQuad quad) {
    if (!ProjMeshObject.Null()) {
      ProjMeshObject = null;
      Debug.Log($"projector quad <{quad.name}> is deleted.");
      DoesNeedToRebuildRTmeshProjectorQuad = true;
    }
  }

  public void RebuildMeshProjectorQuad() {
    if (!DoesNeedToRebuildRTmeshProjectorQuad) {
      return;
    }

    if (ProjMeshObject.Null()) {
      return;
    }

    DoesNeedToRebuildRTmeshProjectorQuad = false;

    // Build Reflector vertex input.    
    var projMesh = new Mesh();
    projMesh.Clear();
    projMesh.vertices = new Vector3[] {
      new Vector3(-1.0f, -1.0f, 1.0f),
      new Vector3(1.0f, -1.0f, 1.0f),
      new Vector3(1.0f, 1.0f, 1.0f),
      new Vector3(-1.0f, 1.0f, 1.0f)
    };
        
    projMesh.triangles = new int[] {
      0, 1, 2, 0, 2, 3
    };

    projMesh.uv = new Vector2[] {
      new Vector2(0.0f, 0.0f),
      new Vector2(1.0f, 0.0f),
      new Vector2(1.0f, 1.0f),
      new Vector2(0.0f, 1.0f)
    };

    ProjVerticesList.AddRange(projMesh.vertices);
    int[] fwdIndices = projMesh.GetIndices(0);
    ProjIndicesList.AddRange(fwdIndices);
    var fwdUV = new List<Vector2>();
    projMesh.GetUVs(0, fwdUV);
    ProjUVsList.AddRange(fwdUV);

    var projObjAttr = ProjMeshObject.GetComponent<RTmeshObject>();
    ProjMeshObjectAttr = new RTmeshObjectAttr() {
      Local2WorldMatrix = ProjMeshObject.transform.localToWorldMatrix,
      IndicesOffset = 0,
      IndicesCount = fwdIndices.Length,
      colorMode = (int)ProjMeshObject.ColorMode      
    };

    var list = new List<RTmeshObjectAttr>();
    list.Add(ProjMeshObjectAttr);

    CreateOrBindDataToComputeBuffer(ref ProjMeshObjectComputeBuf, list, 76);
    CreateOrBindDataToComputeBuffer(ref ProjVerticesComputeBuf, ProjVerticesList, 12);
    CreateOrBindDataToComputeBuffer(ref ProjIndicesComputeBuf, ProjIndicesList, 4);
    CreateOrBindDataToComputeBuffer(ref ProjUVsComputeBuf, ProjUVsList, 8);

    //Dbg_ProjVertices = new Vector3[ProjVerticesList.Count];
    //Dbg_ProjIndices = new int[ProjIndicesList.Count];
    //Dbg_ProjUVs = new Vector2[ProjUVsList.Count];
  }

  /// <summary>
  /// Rebuild the entire list that is going to be transferred into the Compute Shader.
  /// (currently : Vertices, Indices, VertexColors, TextureColors, UVs).
  /// </summary>
  public void RebuildMeshObjects() {
    // Check the condition if we need to rebuild all the lists.
    if (!DoesNeedToRebuildRTmeshObjects) {
      return;
    }

    // kill the flag.
    DoesNeedToRebuildRTmeshObjects = false;
    // Clear all lists.
    MeshObjectsAttrsList.Clear();
    VerticesList.Clear();
    IndicesList.Clear();
    VtxColorsList.Clear();

    var colorMode = eColorMode.NONE;

    // Loop over all objects and gather their data into a single list of the vertices,
    // the indices and the mesh objects.
    foreach (var go in MeshObjectsList) {
      // forward the mesh.
      var mesh = go.GetComponent<MeshFilter>().sharedMesh;

      // 1. Vertices.
      // add vertices data first.
      int verticesStride = VerticesList.Count;
      // AddRange() -> Adds the elements of the specified collection to the end of the 'VerticesList'
      VerticesList.AddRange(mesh.vertices);

      // 2. Indices.
      // Add index data - if the vertex compute buffer wasn't empty before,
      // the indices need to push some offsets.
      int indicesStride = IndicesList.Count;
      int[] indices = mesh.GetIndices(0);
      IndicesList.AddRange(indices.Select(e => e + verticesStride));

      // 3. UVs.
      // Add uv data      
      var fwdUV = new List<Vector2>();
      mesh.GetUVs(0, fwdUV);
      UVsList.AddRange(fwdUV);


      //mesh.RecalculateNormals();
      // 4. If the element(go) is convertible of 'RTmeshCube' then we need to add more info
      // about the vertices colors.      
      var rtObject = go.GetComponent<RTmeshObject>();

      // 5. Add the mesh object attributes.
      MeshObjectsAttrsList.Add(new RTmeshObjectAttr() {
        Local2WorldMatrix = go.transform.localToWorldMatrix,
        IndicesOffset = indicesStride,
        IndicesCount = indices.Length,
        colorMode = (int)rtObject.ColorMode        
      });

      // 6. Set the color mode.
      switch (rtObject.ColorMode) {
        case eColorMode.NONE:
        // Nothing to do!
        break;

        case eColorMode.TEXTURE:
        break;

        case eColorMode.VERTEX_COLOR:
        VtxColorsList.AddRange(mesh.colors.Select(e => new Vector3(e.r, e.g, e.b)));
        break;
      }

      colorMode = rtObject.ColorMode;
    }

    // 7. Compute buffers go!
    CreateOrBindDataToComputeBuffer(ref MeshObjectsAttrsComputeBuf, MeshObjectsAttrsList, 76); //
    CreateOrBindDataToComputeBuffer(ref VerticesComputeBuf, VerticesList, 12); // float3
    CreateOrBindDataToComputeBuffer(ref IndicesComputeBuf, IndicesList, 4); // int

    if (VtxColorsList.Count > 0) {
      CreateOrBindDataToComputeBuffer(ref VtxColorsComputeBuf, VtxColorsList, 12); // float3
    }

    if (UVsList.Count > 0) {
      CreateOrBindDataToComputeBuffer(ref UVsComputeBuf, UVsList, 8);
    }

    if (ProjVerticesList.Count > 0) {
      return;
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <param name="buffer"></param>
  /// <param name="data"></param>
  /// <param name="stride"></param>
  void CreateOrBindDataToComputeBuffer<T>(ref ComputeBuffer buffer,
                                          List<T> data,
                                          int stride)
    where T : struct {
    // check if we already have a compute buffer.
    if (!buffer.Null()) {
      // If no data or buffer doesn't match the given condition, release it.
      if (data.Count == 0
        || buffer.count != data.Count
        || buffer.stride != stride) {
        buffer.Release();
        buffer = null;
      }
    }

    if (data.Count != 0) {
      // If the buffer has been released or wasn't there to begin with, create it.
      if (buffer.Null()) {
        buffer = new ComputeBuffer(data.Count, stride);
      }
      // Set data on the buffer.
      buffer.SetData(data);
    }
  }

  void CreateComputeBufferOnly(ref ComputeBuffer buffer, int count, int stride) {
    if (!buffer.Null()) {
      if (buffer.count != count || buffer.stride != stride) {
        buffer.Release();
        buffer = null;
      }
    }

    if (count > 0) {
      if (buffer.Null()) {
        buffer = new ComputeBuffer(count, stride);
      }
    }
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="shader"></param>
  /// <param name="name"></param>
  /// <param name="buffer"></param>
  public static void SetComputeBuffer(ref ComputeShader shader, string name, ComputeBuffer buffer) {
    if (!buffer.Null()) {
      shader.SetBuffer(0, name, buffer);
    }
  }
};
