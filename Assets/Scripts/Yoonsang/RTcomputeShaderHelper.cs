using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RTcomputeShaderHelper : MonoBehaviour {
  #region Variables
  /// <summary>
  /// Does need to rebuild RTobject to transfer the data into the RTshader?
  /// </summary>
  public static bool DoesNeedToRebuildRTobjects { get; set; } = false;
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
  #endregion

  void OnDisable() {
    // Check each compute buffers are still valid and release it!    
    DisposeComputeBuffers(ref MeshObjectsAttrsComputeBuf);
    DisposeComputeBuffers(ref VtxColorsComputeBuf);
    DisposeComputeBuffers(ref VtxColorsComputeBuf);
    DisposeComputeBuffers(ref UVsComputeBuf);
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
    DoesNeedToRebuildRTobjects = true;
  }

  /// <summary>
  /// 
  /// </summary>
  /// <param name="obj"></param>
  public static void UnregisterFromRTmeshObjectsList(RTmeshObject obj) {
    if (MeshObjectsList.Contains(obj)) {
      MeshObjectsList.Remove(obj);
      Debug.Log($"obj <{obj.name}> is removed from the RT object list.");
      DoesNeedToRebuildRTobjects = true;
    }
  }

  /// <summary>
  /// Rebuild the entire list that is going to be transferred into the Compute Shader.
  /// (currently : Vertices, Indices, VertexColors, TextureColors, UVs).
  /// </summary>
  public void RebuildMeshObjects() {
    // Check the condition if we need to rebuild all the lists.
    if (!DoesNeedToRebuildRTobjects) {
      return;
    }

    // kill the flag.
    DoesNeedToRebuildRTobjects = false;
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
      int uvStride = UVsList.Count;
      var fwdUV = new List<Vector2>();
      mesh.GetUVs(0, fwdUV);
      UVsList.AddRange(fwdUV);

      // 4. If the element(go) is convertible of 'RTmeshCube' then we need to add more info
      // about the vertices colors.      
      var rtObject = go.GetComponent<RTmeshObject>();

      // 5. Add the mesh object attributes.
      MeshObjectsAttrsList.Add(new RTmeshObjectAttr() {
        Local2WorldMatrix = go.transform.localToWorldMatrix,
        IndicesOffset = indicesStride,
        IndicesCount = indices.Length,
        colorMode = (int)rtObject.ColorMode,
        collided = (int)rtObject.Collidable
      });

      // 6. Set the color mode.
      switch (rtObject.ColorMode) {
        case eColorMode.NONE:
        // Nothing to do!
        break;

        case eColorMode.TEXTURE:
        break;

        case eColorMode.VERTEX_COLOR:
        //VtxColorsList.AddRange(mesh.colors.Select(e => new Vector3(e.r, e.g, e.b)));
        break;
      }

      colorMode = rtObject.ColorMode;
    }

    // 7. Compute buffers go!
    CreateOrBindDataToComputeBuffer(ref MeshObjectsAttrsComputeBuf, MeshObjectsAttrsList, 80); //
    CreateOrBindDataToComputeBuffer(ref VerticesComputeBuf, VerticesList, 12); // float3
    CreateOrBindDataToComputeBuffer(ref IndicesComputeBuf, IndicesList, 4); // int

    if (VtxColorsList.Count > 0) {
      CreateOrBindDataToComputeBuffer(ref VtxColorsComputeBuf, VtxColorsList, 12); // float3
    }

    if (UVsList.Count > 0) {
      CreateOrBindDataToComputeBuffer(ref UVsComputeBuf, UVsList, 8);
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
